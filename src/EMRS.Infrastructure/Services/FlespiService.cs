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

            
            _flespiToken = Environment.GetEnvironmentVariable("FLESPI_TOKEN")
                ?? throw new InvalidOperationException("FLESPI_TOKEN chưa được set trong .env");

            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("FlespiToken", _flespiToken.Replace("FlespiToken ", ""));
        }

        
        public async Task<string> GetChannelInfoAsync(int channelId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/gw/channels/{channelId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content; 
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

       
        public async Task<string> GetAllDevicesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/gw/devices/all");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content; 
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        
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

        
        public async Task<string> GetLatestMessagesAsync(int deviceId, int count = 10)
        {
            try
            {
                
                var dataQuery = JsonSerializer.Serialize(new
                {
                    reverse = true,  
                    count = count    
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
