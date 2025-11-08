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
    public class ProtrackService: IProtrackService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://www.dangnhapsaoviet.net/api";
        /*  private const string BaseUrl = "http://api.protrack365.com/api";*/
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
                    vehicle.ProtrackPassword, AES_KEY, AES_IV).Trim();
                var account = vehicle.ProtrackAccount.Trim();

                // QUAN TRỌNG: Lấy thời gian HIỆN TẠI
                var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Generate signature
                string innerHash = SecurityHelper.GetMd5Hash(decryptedPassword);
                string combinedString = innerHash + unixTime.ToString();
                string signature = SecurityHelper.GetMd5Hash(combinedString);

                Console.WriteLine($"=== Protrack Login Debug ===");
                Console.WriteLine($"Current UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Account: {account}");
                Console.WriteLine($"Unix Time: {unixTime}");
                Console.WriteLine($"Password length: {decryptedPassword.Length}");
                Console.WriteLine($"Inner hash (MD5 password): {innerHash}");
                Console.WriteLine($"Combined string: {combinedString}");
                Console.WriteLine($"Signature (MD5 combined): {signature}");

                string url = $"{BaseUrl}/authorization?time={unixTime}" +
                    $"&account={account}&signature={signature}";

                Console.WriteLine($"Request URL: {url}");

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response: {json}");
                Console.WriteLine($"=========================");

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
