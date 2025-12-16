// EMRS.Infrastructure/Services/GeminiAIService.cs (BULLETPROOF VERSION - NO JSON PARSING)
using EMRS.Application.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EMRS.Infrastructure.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _apiKeys;
    private int _currentKeyIndex = 0;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private const int MAX_RETRIES = 3;
    private const int RETRY_DELAY_MS = 1000;

    public GeminiAIService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(2);

        _apiKeys = new List<string>();

        var key1 = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var key2 = Environment.GetEnvironmentVariable("GEMINI_API_KEY_2");
        var key3 = Environment.GetEnvironmentVariable("GEMINI_API_KEY_3");

        if (!string.IsNullOrWhiteSpace(key1)) _apiKeys.Add(key1);
        if (!string.IsNullOrWhiteSpace(key2)) _apiKeys.Add(key2);
        if (!string.IsNullOrWhiteSpace(key3)) _apiKeys.Add(key3);

        if (_apiKeys.Count == 0)
        {
            throw new InvalidOperationException("No Gemini API keys configured.");
        }

        Console.WriteLine($"Loaded {_apiKeys.Count} Gemini API key(s)");
    }

    public async Task<VehicleVerificationResult> VerifyVehicleAsync(
        List<string> handoverImageUrls,
        List<string> returnImageUrls,
        string licensePlate)
    {
        try
        {
            var handoverImages = await DownloadImagesAsBase64Async(handoverImageUrls);
            var returnImages = await DownloadImagesAsBase64Async(returnImageUrls);

            if (handoverImages.Count == 0 || returnImages.Count == 0)
            {
                return new VehicleVerificationResult
                {
                    IsVerified = false,
                    Confidence = 0,
                    Reason = "Không thể tải ảnh để so sánh",
                    LicensePlateMatch = "UNCLEAR"
                };
            }

            var prompt = BuildVerificationPrompt(licensePlate);
            var requestBody = BuildGeminiRequest(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIWithRetryAsync(requestBody);

            return ParseVerificationFromText(response, licensePlate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in VerifyVehicleAsync: {ex.Message}");
            return new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                Reason = $"Lỗi: {ex.Message}",
                LicensePlateMatch = "UNCLEAR"
            };
        }
    }

    public async Task<DamageDetectionResult> DetectDamagesAsync(
        List<string> handoverImageUrls,
        List<string> returnImageUrls)
    {
        try
        {
            var handoverImages = await DownloadImagesAsBase64Async(handoverImageUrls);
            var returnImages = await DownloadImagesAsBase64Async(returnImageUrls);

            if (handoverImages.Count == 0 || returnImages.Count == 0)
            {
                return new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>()
                };
            }

            var prompt = BuildDamageDetectionPrompt();
            var requestBody = BuildGeminiRequest(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIWithRetryAsync(requestBody);

            return ParseDamagesFromText(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DetectDamagesAsync: {ex.Message}");
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };
        }
    }

    

    private string BuildVerificationPrompt(string licensePlate)
    {
        return $@"
Bạn là chuyên gia giám định xe. So sánh ảnh BÀN GIAO với ảnh TRẢ XE.

Biển số xe dự kiến: {licensePlate}

Trả lời theo format sau (CHÍNH XÁC, KHÔNG THÊM GÌ KHÁC):

VERIFIED: YES hoặc NO
CONFIDENCE: số từ 0.0 đến 1.0
LICENSE_PLATE: MATCH hoặc MISMATCH hoặc UNCLEAR
REASON: giải thích ngắn gọn (1 dòng, tối đa 100 chữ)

Ví dụ:
VERIFIED: YES
CONFIDENCE: 0.9
LICENSE_PLATE: MATCH
REASON: Cùng màu đỏ, cùng kiểu dáng Honda Wave, biển số khớp nhau

Bây giờ hãy phân tích:
";
    }

    private string BuildDamageDetectionPrompt()
    {
        return @"
Bạn là chuyên gia giám định hư hỏng xe. So sánh ảnh BÀN GIAO với ảnh TRẢ XE.

Tìm hư hỏng MỚI (có trong ảnh TRẢ XE nhưng KHÔNG CÓ trong ảnh BÀN GIAO).

Trả lời theo format sau (CHÍNH XÁC):

HAS_DAMAGES: YES hoặc NO

Nếu có hư hỏng, liệt kê từng cái (mỗi hư hỏng trên 1 dòng):
DAMAGE|Vị trí|Loại hư hỏng|Mức độ|Độ tin cậy 0-1|Mô tả ngắn

Ví dụ:
HAS_DAMAGES: YES
DAMAGE|Mặt trước xe|Vết nứt|Nghiêm trọng|0.95|Mặt trước bị nứt vỡ lớn ở giữa
DAMAGE|Gương trái|Vết xước|Nhỏ|0.7|Vết xước nhẹ trên gương

Hoặc nếu không có hư hỏng:
HAS_DAMAGES: NO

Bây giờ hãy phân tích:
";
    }

    

    private VehicleVerificationResult ParseVerificationFromText(string response, string licensePlate)
    {
        try
        {
            Console.WriteLine($"AI Response:\n{response}\n");

            // Default values
            bool isVerified = false;
            double confidence = 0.0;
            string licensePlateMatch = "UNCLEAR";
            string reason = "Không thể phân tích kết quả";

            // Parse VERIFIED
            var verifiedMatch = Regex.Match(response, @"VERIFIED:\s*(YES|NO)", RegexOptions.IgnoreCase);
            if (verifiedMatch.Success)
            {
                isVerified = verifiedMatch.Groups[1].Value.ToUpper() == "YES";
            }

            // Parse CONFIDENCE
            var confidenceMatch = Regex.Match(response, @"CONFIDENCE:\s*([\d.]+)");
            if (confidenceMatch.Success && double.TryParse(confidenceMatch.Groups[1].Value, out double conf))
            {
                confidence = Math.Clamp(conf, 0, 1);
            }

            // Parse LICENSE_PLATE
            var plateMatch = Regex.Match(response, @"LICENSE_PLATE:\s*(MATCH|MISMATCH|UNCLEAR)", RegexOptions.IgnoreCase);
            if (plateMatch.Success)
            {
                licensePlateMatch = plateMatch.Groups[1].Value.ToUpper();
            }

            // Parse REASON
            var reasonMatch = Regex.Match(response, @"REASON:\s*(.+?)(?:\n|$)", RegexOptions.Singleline);
            if (reasonMatch.Success)
            {
                reason = reasonMatch.Groups[1].Value.Trim();
                // Giới hạn độ dài
                if (reason.Length > 200)
                {
                    reason = reason.Substring(0, 197) + "...";
                }
            }

            Console.WriteLine($"Parsed: verified={isVerified}, confidence={confidence}, plate={licensePlateMatch}");

            return new VehicleVerificationResult
            {
                IsVerified = isVerified,
                Confidence = confidence,
                LicensePlateMatch = licensePlateMatch,
                Reason = reason
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parse error: {ex.Message}");
            return new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                LicensePlateMatch = "UNCLEAR",
                Reason = "Lỗi phân tích kết quả"
            };
        }
    }

    private DamageDetectionResult ParseDamagesFromText(string response)
    {
        try
        {
            Console.WriteLine($"📝 AI Response:\n{response}\n");

            var result = new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };

            
            var hasDamagesMatch = Regex.Match(response, @"HAS_DAMAGES:\s*(YES|NO)", RegexOptions.IgnoreCase);
            if (hasDamagesMatch.Success)
            {
                result.HasNewDamages = hasDamagesMatch.Groups[1].Value.ToUpper() == "YES";
            }

            if (!result.HasNewDamages)
            {
                Console.WriteLine("✅ No damages detected");
                return result;
            }

            
            var damageMatches = Regex.Matches(response, @"DAMAGE\|([^|]+)\|([^|]+)\|([^|]+)\|([\d.]+)\|(.+?)(?:\n|$)");

            foreach (Match match in damageMatches)
            {
                if (match.Groups.Count >= 6)
                {
                    var location = match.Groups[1].Value.Trim();
                    var damageType = match.Groups[2].Value.Trim();
                    var severity = match.Groups[3].Value.Trim();
                    var confidenceStr = match.Groups[4].Value.Trim();
                    var description = match.Groups[5].Value.Trim();

                    double confidence = 0;
                    if (double.TryParse(confidenceStr, out double conf))
                    {
                        confidence = Math.Clamp(conf, 0, 1);
                    }

                    result.Suggestions.Add(new DamageSuggestion
                    {
                        Location = location,
                        DamageType = damageType,
                        Severity = severity,
                        Confidence = confidence,
                        Description = description
                    });
                }
            }

            Console.WriteLine($"✅ Parsed {result.Suggestions.Count} damage(s)");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parse error: {ex.Message}");
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };
        }
    }

    // ===== GEMINI API CALLS =====

    private object BuildGeminiRequest(string prompt, List<string> handoverImages, List<string> returnImages)
    {
        var parts = new List<object>();

        parts.Add(new { text = prompt });
        parts.Add(new { text = "\n\n=== ẢNH BÀN GIAO ===\n" });

        for (int i = 0; i < handoverImages.Count; i++)
        {
            parts.Add(new { text = $"Ảnh bàn giao {i + 1}:" });
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = handoverImages[i]
                }
            });
        }

        parts.Add(new { text = "\n\n=== ẢNH TRẢ XE ===\n" });

        for (int i = 0; i < returnImages.Count; i++)
        {
            parts.Add(new { text = $"Ảnh trả xe {i + 1}:" });
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = returnImages[i]
                }
            });
        }

        return new
        {
            contents = new[]
            {
                new { parts = parts.ToArray() }
            },
            generationConfig = new
            {
                temperature = 0.1,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 1024
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
            }
        };
    }

    private async Task<string> CallGeminiAPIWithRetryAsync(object requestBody)
    {
        var lastException = new Exception("Unknown error");
        var keysAttempted = new HashSet<int>();

        for (int attempt = 0; attempt < MAX_RETRIES * _apiKeys.Count; attempt++)
        {
            try
            {
                var currentKey = _apiKeys[_currentKeyIndex];
                var response = await CallGeminiAPISingleAttemptAsync(requestBody, currentKey);

                if (keysAttempted.Count > 1)
                {
                    _currentKeyIndex = 0;
                }

                return response;
            }
            catch (GeminiOverloadException)
            {
                keysAttempted.Add(_currentKeyIndex);
                SwitchToNextKey();

                if (keysAttempted.Count >= _apiKeys.Count)
                {
                    await Task.Delay(RETRY_DELAY_MS);
                    keysAttempted.Clear();
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                SwitchToNextKey();

                if (keysAttempted.Count >= _apiKeys.Count)
                {
                    throw;
                }
            }
        }

        throw new Exception($"Failed after {MAX_RETRIES * _apiKeys.Count} attempts. Last error: {lastException.Message}", lastException);
    }

    private async Task<string> CallGeminiAPISingleAttemptAsync(object requestBody, string apiKey)
    {
        var url = $"{API_BASE_URL}?key={apiKey}";
        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = false });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorDoc = JsonDocument.Parse(responseBody);
                    if (errorDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        var errorCode = error.TryGetProperty("code", out var code) ? code.GetInt32() : 0;
                        var errorMessage = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown";
                        var errorStatus = error.TryGetProperty("status", out var status) ? status.GetString() : "";

                        if (errorCode == 503 || errorStatus == "UNAVAILABLE" ||
                            errorMessage.Contains("overloaded", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new GeminiOverloadException(errorMessage);
                        }
                    }
                }
                catch (GeminiOverloadException) { throw; }
                catch { }

                throw new Exception($"API error {response.StatusCode}: {responseBody}");
            }

            // Extract text content từ response
            var doc = JsonDocument.Parse(responseBody);
            var textContent = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textContent ?? "";
        }
        catch (TaskCanceledException)
        {
            throw new Exception("API request timed out");
        }
    }

    private void SwitchToNextKey()
    {
        _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Count;
        Console.WriteLine($"Switched to key #{_currentKeyIndex + 1}");
    }

    // ===== HELPER METHODS =====

    private async Task<List<string>> DownloadImagesAsBase64Async(List<string> imageUrls)
    {
        var base64Images = new List<string>();

        foreach (var url in imageUrls)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                var base64 = Convert.ToBase64String(imageBytes);
                base64Images.Add(base64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download image: {ex.Message}");
            }
        }

        return base64Images;
    }
}

public class GeminiOverloadException : Exception
{
    public GeminiOverloadException(string message) : base(message) { }
}