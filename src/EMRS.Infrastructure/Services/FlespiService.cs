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
            if (vehicle == null)
                throw new ArgumentException("Vehicle null");

            if (string.IsNullOrEmpty(vehicle.GpsDeviceIdent) && vehicle.FlespiDeviceId == null)
                throw new ArgumentException("Vehicle phải có DeviceId hoặc IMEI");

            try
            {
                // **ĐẢM BẢO deviceId là số (long)**
                long deviceId = vehicle.FlespiDeviceId ?? long.Parse(vehicle.GpsDeviceIdent);

                // **BODY CHÍNH XÁC theo Flespi 2023+**
                var body = new
                {
                    type = "temp", // BẮT BUỘC
                    ttl = ttlSeconds,
                    acl = new[]
                    {
                new {
                    uri = "gw/devices",
                    methods = new[] { "GET" },
                    ids = new long[] { deviceId } // **long[] không phải string[]**
                }
            }
                };

                // **ENDPOINT CHÍNH XÁC: /tokens**
                var req = new HttpRequestMessage(HttpMethod.Post, "/tokens")
                {
                    Content = JsonContent.Create(body)
                };

                req.Headers.Add("Authorization", $"FlespiToken {_masterToken}");
                req.Headers.Add("Accept", "application/json");

                // **DEBUG**: In ra URL để kiểm tra
                var fullUrl = $"{_httpClient.BaseAddress}tokens".TrimEnd('/');
                Console.WriteLine($"Calling Flespi API: {fullUrl}");
                Console.WriteLine($"Body: {JsonSerializer.Serialize(body)}");

                var resp = await _httpClient.SendAsync(req);
                var raw = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Flespi error {resp.StatusCode}. URL: {fullUrl}. Response: {raw}");
                }

                var doc = JsonDocument.Parse(raw);
                var resultArr = doc.RootElement.GetProperty("result");

                if (resultArr.GetArrayLength() == 0)
                    throw new Exception("Flespi trả về result rỗng");

                var tokenKey = resultArr[0].GetProperty("key").GetString();

                return new TempTrackingPayload
                {
                    vehicleId = vehicle.Id,
                    imei = vehicle.GpsDeviceIdent ?? "",
                    deviceId = vehicle.FlespiDeviceId,
                    exp = DateTimeOffset.UtcNow.AddSeconds(ttlSeconds).ToUnixTimeSeconds(),
                    tmpToken = tokenKey
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating Flespi ACL token: {ex.Message}", ex);
            }
        }



    }
}
