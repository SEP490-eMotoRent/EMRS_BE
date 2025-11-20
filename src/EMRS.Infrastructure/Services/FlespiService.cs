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


        public async Task<TempTrackingPayload> CreateFlespiAclTokenAsync(
     Vehicle vehicle,
     int ttlSeconds,
     int minutes)
        {
            if (vehicle == null)
                throw new ArgumentException("Vehicle null");

            if (string.IsNullOrEmpty(vehicle.GpsDeviceIdent) && vehicle.FlespiDeviceId == null)
                throw new ArgumentException("Vehicle phải có DeviceId hoặc IMEI");

            try
            {
                long deviceId = vehicle.FlespiDeviceId ?? long.Parse(vehicle.GpsDeviceIdent);

                long expireUnix = DateTime.UtcNow
                    .AddMinutes(minutes)
                    .ToUnixTime();

                var body = new[]
                {
            new
            {
                expire = expireUnix,
                enabled = true,
                ttl = ttlSeconds,
                access = new
                {
                    acl = new object[]
                    {
                       
                        new {
                            uri = "gw/devices",
                            methods = new[] { "GET" },
                            ids = new long[] { deviceId }
                        }
                    },
                    type = 2  
                }
            }
        };

                var req = new HttpRequestMessage(HttpMethod.Post, "platform/tokens")
                {
                    Content = JsonContent.Create(body)
                };

                req.Headers.Add("Authorization", $"{_masterToken}");
                req.Headers.Add("Accept", "application/json");

                var resp = await _httpClient.SendAsync(req);
                var raw = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Flespi error {resp.StatusCode}. Response: {raw}"
                    );
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
                    exp = expireUnix,
                    tmpToken = tokenKey
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error creating Flespi ACL token: {ex.Message}", ex
                );
            }
        }




    }
}
