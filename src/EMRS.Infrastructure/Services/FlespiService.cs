using Elsheimy.Components.Apis.Protrack;
using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.Protrack;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services
{
    public class FlespiService: IFlespiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _masterToken;

        public FlespiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _masterToken = Environment.GetEnvironmentVariable("FLESPI_TOKEN")
                ?? throw new InvalidOperationException("Missing FLESPI_TOKEN");
        }

      
        public async Task<TempTrackingPayload> CreateFlespiAclTokenAsync(Vehicle vehicle, int ttlSeconds = 300)
        {
            try
            {
                if (string.IsNullOrEmpty(vehicle.DeviceId) && string.IsNullOrEmpty(vehicle.DeviceImei))
                    throw new ArgumentException("Vehicle phải có DeviceId hoặc IMEI");

                var body = new
                {
                    ttl = ttlSeconds,
                    acl = new[]
                    {
                    new {
                        uri = "gw/devices",
                        methods = new[] { "GET" },
                        ids = new[] { vehicle.DeviceId ?? vehicle.DeviceImei }
                    }
                }
                };

                var req = new HttpRequestMessage(HttpMethod.Post, "platform/customer/tokens")
                {
                    Content = JsonContent.Create(body)
                };
                req.Headers.Add("Authorization", $"FlespiToken {_masterToken}");

                var resp = await _httpClient.SendAsync(req);
                resp.EnsureSuccessStatusCode();

                var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
                if (doc == null) throw new Exception("Không parse được response từ Flespi");

                var resultArr = doc.RootElement.GetProperty("result");
                if (resultArr.GetArrayLength() == 0)
                    throw new Exception("Flespi trả về result rỗng khi tạo token");

                var tokenKey = resultArr[0].GetProperty("key").GetString();
                if (string.IsNullOrEmpty(tokenKey))
                    throw new Exception("Flespi token key null");

                var exp = DateTimeOffset.UtcNow.AddSeconds(ttlSeconds).ToUnixTimeSeconds();

                return new TempTrackingPayload
                {
                    vehicleId = vehicle.Id,
                    imei = vehicle.DeviceImei ?? "",
                    deviceId = vehicle.DeviceId ?? "",
                    exp = exp
                };
            }
            catch (Exception ex)
            {
                // Log exception here as needed
                throw new Exception("Error creating Flespi ACL token", ex);
            }
        }

    }
}
