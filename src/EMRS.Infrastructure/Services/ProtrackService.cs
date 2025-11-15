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
    public class ProtrackService : IProtrackService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://api.protrack365.com/api";
        private readonly string AES_KEY;
        private readonly string AES_IV;

        public ProtrackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            AES_KEY = Environment.GetEnvironmentVariable("PROTRACK_AES_KEY")
                      ?? throw new InvalidOperationException("PROTRACK_AES_KEY not set");
            AES_IV = Environment.GetEnvironmentVariable("PROTRACK_AES_IV")
                ?? throw new InvalidOperationException("PROTRACK_AES_IV not set");
        }

        public string EncryptPassword(string password)
        {
            return SecurityHelper.Encrypt(password, AES_KEY, AES_IV);
        }
        public async Task<ProtrackResponse?> LoginVehicleAsync(Vehicle vehicle)
        {
            try
            {
                var decryptedPassword = SecurityHelper.Decrypt(
                    vehicle.ProtrackPassword, AES_KEY, AES_IV);
                var account = vehicle.ProtrackAccount;


                var TimeNow = DateTime.UtcNow;
                long unixTime = SecurityHelper.ToUnixTime(TimeNow);
                // Generate signature

                string signature = SecurityHelper
                    .GetSignature(unixTime, decryptedPassword);



                string url = $"{BaseUrl}/authorization?time={unixTime}" +
                    $"&account={account}&signature={signature}";

                Console.WriteLine($"Request URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {vehicle.ProtrackAccount}");
                Console.WriteLine($"Response: {decryptedPassword}");
                Console.WriteLine($"Response: {json}");


                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var code = root.GetProperty("code").GetInt32();

                if (code == 0)
                {
                    var record = root.GetProperty("record");
                    return new ProtrackResponse
                    {
                        access_token = record.GetProperty("access_token").GetString()!,
                        expires_in = record.TryGetProperty("expires_in", out var exp)
                            ? exp.GetInt64() : 0
                    };
                }

                if (root.TryGetProperty("message", out var msg))
                {
                    Console.WriteLine($"Protrack API Error Code {code}: {msg.GetString()}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
