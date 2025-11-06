using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.Protrack;
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
    public class ProtrackService: IProtrackService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.protrack365.com/api";
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
            return SecurityHelper.Encrypt(password, AES_KEY, AES_KEY);
        }
        public async Task<string?> LoginVehicleAsync(Vehicle vehicle)
        {
            try
            {
                var decryptedPassword = SecurityHelper.Decrypt(vehicle.ProtrackPassword,AES_KEY,AES_IV);
                long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string innerHash = SecurityHelper.GetMd5Hash(decryptedPassword);
                string signature = SecurityHelper.GetMd5Hash(innerHash + time);

                string url = $"{BaseUrl}/authorization?time={time}&account={vehicle.ProtrackAccount}&signature={signature}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.GetProperty("code").GetInt32() == 0)
                {
                    var record = root.GetProperty("record");
                    return record.GetProperty("access_token").GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Protrack login failed: {ex.Message}");
                return null;
            }
        }
    }
}
