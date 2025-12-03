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
    public class ZaloPayService
    {
        
       /* private readonly string Appid;
        private readonly string key1;
        private readonly string key2;
        private readonly string base_url;
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
            _httpClient = httpClient;
        }

        public async Task<ZaloPayResponse> CreatePaymentURL(OrderData orderData)
        {
            if (string.IsNullOrEmpty(orderData.Appid)) orderData.Appid = Appid;
            if (string.IsNullOrEmpty(orderData.Appuser)) orderData.Appuser = "EMRS";
            orderData.Apptime=DateTimeHelper.GetTimeStamp();

            

            

            
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

            // 3. Tạo FormContent để gửi đi (Map đúng tên key của ZaloPay)
            var contentParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("app_id", orderData.Appid),
                new KeyValuePair<string, string>("app_user", orderData.Appuser),
                new KeyValuePair<string, string>("app_time", orderData.Apptime.ToString()),
                new KeyValuePair<string, string>("amount", orderData.Amount.ToString()),
                new KeyValuePair<string, string>("app_trans_id", orderData.Apptransid),
                new KeyValuePair<string, string>("embed_data", orderData.Embeddata),
                new KeyValuePair<string, string>("item", orderData.Item),
                new KeyValuePair<string, string>("description", orderData.Description),
                new KeyValuePair<string, string>("bank_code", orderData.Bankcode ?? ""),
                new KeyValuePair<string, string>("mac", orderData.Mac)
            };

            var requestContent = new FormUrlEncodedContent(contentParams);

            // 4. Gọi API
            var response = await _httpClient.PostAsync(_createOrderUrl, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ZaloPay API Error: {responseString}");
            }

            // 5. Trả về kết quả
            return JsonConvert.DeserializeObject<ZaloPayCreateOrderResponse>(responseString);
        }*/
    }
}
