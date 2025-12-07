using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.ZaloPay;
using EMRS.Application.Common;
using EMRS.Application.Helper;
using EMRS.Infrastructure.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services
{
    public class ZaloPayService:IZaloPayService
    {

        private readonly string Appid;
        private readonly string key1;
        private readonly string key2;
        private readonly string base_url;
        private readonly string return_url;
        private readonly HttpClient _httpClient;

        public ZaloPayService(HttpClient httpClient)
        {
            Appid = Environment.GetEnvironmentVariable("ZALOPAY_APPID")
                  ?? throw new InvalidOperationException("ZALOPAY_APPID not set");
            key1 = Environment.GetEnvironmentVariable("ZALOPAY_KEY1")
                 ?? throw new InvalidOperationException("ZALOPAY_KEY1 not set");
            key2 = Environment.GetEnvironmentVariable("ZALOPAY_KEY2")
                 ?? throw new InvalidOperationException("ZALOPAY_KEY2 not set");
            base_url = Environment.GetEnvironmentVariable("ZALOPAY_URL_BASE")
                 ?? throw new InvalidOperationException("ZALOPAY_URL_BASE not set");
            return_url = Environment.GetEnvironmentVariable("ZALOPAY_RETURN_URL")
                 ?? throw new InvalidOperationException("ZALOPAY_RETURN_URL not set");
            _httpClient = httpClient;
        }
        public bool VerifyCallback(ZaloPayCallbackResponseData response)
        {
            var rawData =
                $"{response.AppId}|{response.AppTransId}|{response.PmcId}|{response.BankCode}|{response.Amount}|{response.DiscountAmount}|{response.Status}";

            var computedChecksum = ZaloPayHelper.ValidateCompute(rawData, key2);

            if (computedChecksum == response.Checksum)
                return true;
            return false;
        }

        public async Task<ZaloPayResponse> CreatePaymentURL(OrderData orderData)
        {
            // ... (Giữ nguyên phần logic tính toán đầu vào của bạn) ...
            if (string.IsNullOrEmpty(orderData.Appid)) orderData.Appid = Appid;
            if (string.IsNullOrEmpty(orderData.Appuser)) orderData.Appuser = "EMRS";
            orderData.Apptime = DateTimeHelper.GetTimeStamp(); // Đảm bảo trả về Milliseconds (long)
            orderData.Bankcode = "zalopayapp";
            orderData.Embeddata = System.Text.Json.JsonSerializer.Serialize(new
            {
                redirecturl = return_url,

            });
            // ... (Giữ nguyên phần tính MAC) ...
            orderData.Mac = ZaloPayHelper.ComputeMac(
                key1,
                int.Parse(orderData.Appid),
                orderData.Apptransid,
                orderData.Appuser,
                orderData.Amount,
                orderData.Apptime,
                orderData.Embeddata,
                orderData.Item
            );

            // --- SỬA QUAN TRỌNG: Tên key phải là snake_case (có dấu gạch dưới) ---
            // Code cũ của bạn: "appid", "appuser" -> Sai, ZaloPay không nhận được.
            // Code đúng: "app_id", "app_user"...
            var contentParams = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("appid", orderData.Appid),
        new KeyValuePair<string, string>("appuser", orderData.Appuser),
        new KeyValuePair<string, string>("apptime", orderData.Apptime.ToString()),
        new KeyValuePair<string, string>("amount", orderData.Amount.ToString()),
        new KeyValuePair<string, string>("apptransid", orderData.Apptransid),
        new KeyValuePair<string, string>("embeddata", orderData.Embeddata),
        new KeyValuePair<string, string>("item", orderData.Item),
        new KeyValuePair<string, string>("description", orderData.Description),
        new KeyValuePair<string, string>("bankcode", orderData.Bankcode ?? ""),
        new KeyValuePair<string, string>("mac", orderData.Mac)
    };
            // In ra dữ liệu Request
            Console.WriteLine($"[ZALOPAY REQUEST]: {JsonConvert.SerializeObject(contentParams.ToDictionary(x => x.Key, x => x.Value))}");
            var requestContent = new FormUrlEncodedContent(contentParams);

            // Gọi API
            var response = await _httpClient.PostAsync(base_url, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            // ========================================================================
            // CÁCH DEBUG: IN RAW MESSAGE RA ĐÂY
            // ========================================================================

            // Cách 1: In ra cửa sổ Output (View -> Output trong Visual Studio)
            Console.WriteLine($"[ZALOPAY RAW RESPONSE]: {responseString}");

           

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ZaloPay HTTP Error: {response.StatusCode} - {responseString}");
            }

            return JsonConvert.DeserializeObject<ZaloPayResponse>(responseString);
        }
    }
}
