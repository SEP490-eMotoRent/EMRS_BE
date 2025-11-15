using EMRS.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services
{
    public class FlespiService : IFlespiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flespiToken;

        public FlespiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://flespi.io")
            };

            // Lấy token từ .env
            _flespiToken = Environment.GetEnvironmentVariable("FLESPI_TOKEN")
                ?? throw new InvalidOperationException("FLESPI_TOKEN chưa được set trong .env");

            // Set Authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("FlespiToken", _flespiToken.Replace("FlespiToken ", ""));
        }

        // Lấy thông tin channel
        public async Task<string> GetChannelInfoAsync(int channelId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/gw/channels/{channelId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content; // Trả về JSON raw để xem
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        // Lấy danh sách tất cả devices
        public async Task<string> GetAllDevicesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/gw/devices/all");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content; // Trả về JSON raw
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        // Lấy thông tin device cụ thể
        public async Task<string> GetDeviceInfoAsync(int deviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/gw/devices/{deviceId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        // Lấy messages mới nhất
        public async Task<string> GetLatestMessagesAsync(int deviceId, int count = 10)
        {
            try
            {
                // Tạo query parameter
                var dataQuery = JsonSerializer.Serialize(new
                {
                    reverse = true,  // Lấy từ mới nhất
                    count = count    // Số lượng messages
                });

                var encodedQuery = Uri.EscapeDataString(dataQuery);
                var url = $"/gw/devices/{deviceId}/messages?data={encodedQuery}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }


    }
}
