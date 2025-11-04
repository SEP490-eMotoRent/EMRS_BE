using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.VNPay;
using EMRS.Infrastructure.Helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace EMRS.Infrastructure.Services;

public class VNPayService:IVNPayService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly string hash_secret;
    private readonly string tmn_code;
    private readonly string base_url;
    private readonly string return_url;
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>();
    private VNPayConfig _config;
    public VNPayService(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _config = new VNPayConfig();
        _httpClient = http;
        _httpContextAccessor = httpContextAccessor;
        hash_secret = Environment.GetEnvironmentVariable("HASH_SECRET")
                 ?? throw new InvalidOperationException("HASH_SECRET not set");
        tmn_code = Environment.GetEnvironmentVariable("TMN_CODE")
                  ?? throw new InvalidOperationException("TMN_CODE not set");
        base_url= Environment.GetEnvironmentVariable("BASE_URL")
                              ?? throw new InvalidOperationException("BASE_URL not set");
        return_url= Environment.GetEnvironmentVariable("RETURN_URL")
                              ?? throw new InvalidOperationException("RETURN_URL not set");

    }
    public string CreatePaymentUrl(VNPayRequestData vNPayRequestData)
    {
        try
        {
            _config.Url= base_url;
            _config.TmnCode= tmn_code;
            _config.HashSecret= hash_secret;
            vNPayRequestData.ReturnUrl= return_url;
            var vnTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
           
            var createdDatetimet = DateTime.UtcNow;
            AddRequestData("vnp_Version", _config.Version);
            AddRequestData("vnp_Command", _config.Command);
            AddRequestData("vnp_TmnCode", _config.TmnCode);
            AddRequestData("vnp_Amount", ((long)(vNPayRequestData.Amount * 100)).ToString());
            AddRequestData("vnp_CreateDate", vnTime.ToString("yyyyMMddHHmmss"));
            AddRequestData("vnp_CurrCode", _config.CurrencyCode);
            AddRequestData("vnp_IpAddr", vNPayRequestData.IpAddress);
            AddRequestData("vnp_Locale", vNPayRequestData.Locale ?? "vn");
            AddRequestData("vnp_OrderInfo", vNPayRequestData.OrderDescription);
            AddRequestData("vnp_OrderType", _config.OrderType);
            AddRequestData("vnp_ReturnUrl", vNPayRequestData.ReturnUrl);
            AddRequestData("vnp_TxnRef", vNPayRequestData.OrderId);
            if (!string.IsNullOrEmpty(vNPayRequestData.BankCode))
            {
                AddRequestData("vnp_BankCode", vNPayRequestData.BankCode);
            }

            if (vNPayRequestData.ExpireDate.HasValue)
            {
                var expireTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(vNPayRequestData.ExpireDate.Value, "SE Asia Standard Time");
                AddRequestData("vnp_ExpireDate", expireTime.ToString("yyyyMMddHHmmss"));
            }
            return CreateRequestUrl(_config.Url, _config.HashSecret);
        }
        catch(Exception ex) 
        {
            throw new InvalidOperationException("Error creating VNPay payment URL", ex);
        }
    }
    public VNPayResponseData ProcessResponse()
    {
        var queryParams = HttpRequestHelper.GetQueryParams(_httpContextAccessor);
        if (queryParams == null || queryParams.Count == 0)
            throw new ArgumentException("No VNPay query parameters found");

        foreach (var param in queryParams.Where(p => p.Key.StartsWith("vnp_")))
        {
            AddResponseData(param.Key, param.Value);
        }

        var secureHash = queryParams.GetValueOrDefault("vnp_SecureHash", string.Empty);
        var isValidSignature = ValidateSignature(secureHash, _config.HashSecret);

        if (!isValidSignature)
        {
            return new VNPayResponseData
            {
                IsSuccess = false,
                Message = "Invalid signature",
                ResponseCode = "97",
                RawData = queryParams
            };
        }

        var responseCode = GetResponseData("vnp_ResponseCode");
        var orderId = GetResponseData("vnp_TxnRef");
        var transactionId = GetResponseData("vnp_TransactionNo");
        var amountStr = GetResponseData("vnp_Amount");
        var bankCode = GetResponseData("vnp_BankCode");
        var bankTranNo = GetResponseData("vnp_BankTranNo");
        var cardType = GetResponseData("vnp_CardType");
        var payDateStr = GetResponseData("vnp_PayDate");

        var response = new VNPayResponseData
        {
            IsSuccess = responseCode == "00",
            OrderId = orderId,
            TransactionId = transactionId,
            ResponseCode = responseCode,
            Message = GetResponseMessage(responseCode),
            BankCode = bankCode,
            BankTransactionNo = bankTranNo,
            CardType = cardType,
            RawData = queryParams
        };

        if (long.TryParse(amountStr, out var amount))
            response.Amount = amount / 100m;

        if (!string.IsNullOrEmpty(payDateStr) && payDateStr.Length == 14 &&
            DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var payDate))
        {
            response.TransactionDate = payDate;
        }

        return response;
    }

    public string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày",
            "75" => "Ngân hàng thanh toán đang bảo trì",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch",
            "99" => "Các lỗi khác (lỗi còn lại, không có trong danh sách mã lỗi đã liệt kê)",
            _ => "Lỗi không xác định"
        };
    }



    //HELPER
    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var dataToHash = _responseData
            .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
            .ToDictionary(x => x.Key, x => x.Value);

        var myChecksum = CreateSecureHash(new SortedList<string, string>(dataToHash), secretKey);

        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }
    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }
    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var queryString = BuildQueryString(_requestData);
        var secureHash = CreateSecureHash(_requestData, hashSecret);
        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
    }
    private string BuildQueryString(IDictionary<string, string> data)
    {
        var parts = new List<string>();
        foreach (var item in data)
        {
            if (string.IsNullOrEmpty(item.Value))
                continue;

            var value = Uri.EscapeDataString(item.Value);
            parts.Add($"{item.Key}={value}");
        }
        return string.Join("&", parts);
    }

    private string CreateSecureHash(SortedList<string, string> data, string secretKey)
    {
        var sortedData = new SortedList<string, string>(data); 

        var signData = string.Join("&", sortedData
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")); 

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }



}
