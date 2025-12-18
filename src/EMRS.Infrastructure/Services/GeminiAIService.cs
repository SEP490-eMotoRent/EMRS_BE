// EMRS.Infrastructure/Services/GeminiAIService.cs (HYBRID: JSON SPEED + TEXT PARSING SAFETY)
using EMRS.Application.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

namespace EMRS.Infrastructure.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _apiKeys;
    private int _currentKeyIndex = 0;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private const int MAX_RETRIES = 2;
    private const int RETRY_DELAY_MS = 500;

    public GeminiAIService()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10,
            EnableMultipleHttp2Connections = true
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(90)
        };

        _httpClient.DefaultRequestHeaders.ConnectionClose = false;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
    }

    public async Task<VehicleVerificationResult> VerifyVehicleAsync(
        List<string> handoverImageUrls,
        List<string> returnImageUrls,
        string licensePlate)
    {
        try
        {
            var downloadTask = Task.WhenAll(
                DownloadImagesAsBase64ParallelAsync(handoverImageUrls),
                DownloadImagesAsBase64ParallelAsync(returnImageUrls)
            );

            var results = await downloadTask.ConfigureAwait(false);
            var handoverImages = results[0];
            var returnImages = results[1];

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
            var requestBody = BuildGeminiRequestWithJSON(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIWithRetryAsync(requestBody).ConfigureAwait(false);

            return ParseVerificationHybrid(response);
        }
        catch (Exception ex)
        {
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
            var downloadTask = Task.WhenAll(
                DownloadImagesAsBase64ParallelAsync(handoverImageUrls),
                DownloadImagesAsBase64ParallelAsync(returnImageUrls)
            );

            var results = await downloadTask.ConfigureAwait(false);
            var handoverImages = results[0];
            var returnImages = results[1];

            if (handoverImages.Count == 0 || returnImages.Count == 0)
            {
                return new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>()
                };
            }

            var prompt = BuildDamageDetectionPrompt();
            var requestBody = BuildGeminiRequestWithJSON(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIWithRetryAsync(requestBody).ConfigureAwait(false);

            return ParseDamagesHybrid(response);
        }
        catch
        {
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };
        }
    }

    // ===== PROMPTS (OPTIMIZED FOR JSON) =====

    private string BuildVerificationPrompt(string licensePlate)
    {
        return $@"So sánh ảnh BÀN GIAO với ảnh TRẢ XE. Biển số: {licensePlate}

Trả về JSON (KHÔNG có markdown, KHÔNG có backticks):
{{
  ""isVerified"": true/false,
  ""confidence"": 0.0-1.0,
  ""licensePlateMatch"": ""MATCH""/""MISMATCH""/""UNCLEAR"",
  ""reason"": ""giải thích ngắn gọn""
}}";
    }

    private string BuildDamageDetectionPrompt()
    {
        return @"Tìm hư hỏng MỚI (có trong ảnh TRẢ, không có trong ảnh GIAO).

Trả về JSON (KHÔNG có markdown, KHÔNG có backticks):
{
  ""hasNewDamages"": true/false,
  ""suggestions"": [
    {
      ""location"": ""vị trí"",
      ""damageType"": ""loại"",
      ""severity"": ""mức độ"",
      ""confidence"": 0.0-1.0,
      ""description"": ""mô tả""
    }
  ]
}";
    }

    // ===== HYBRID PARSING (JSON + FALLBACK TO REGEX) =====

    private VehicleVerificationResult ParseVerificationHybrid(string response)
    {
        // TRY 1: Parse as JSON (nhanh nhất)
        try
        {
            var cleaned = CleanJsonResponse(response);
            var result = JsonSerializer.Deserialize<VehicleVerificationResult>(
                cleaned,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                }
            );

            if (result != null)
            {
                result.Confidence = Math.Clamp(result.Confidence, 0, 1);
                return result;
            }
        }
        catch
        {
            // JSON parse failed - fallback to regex
        }

        // TRY 2: Fallback to REGEX parsing (an toàn)
        try
        {
            return ParseVerificationFromText(response);
        }
        catch
        {
            // Ultimate fallback
            return new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                LicensePlateMatch = "UNCLEAR",
                Reason = "Không phân tích được"
            };
        }
    }

    private DamageDetectionResult ParseDamagesHybrid(string response)
    {
        // TRY 1: Parse as JSON
        try
        {
            var cleaned = CleanJsonResponse(response);
            var result = JsonSerializer.Deserialize<DamageDetectionResult>(
                cleaned,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                }
            );

            if (result != null)
            {
                if (result.Suggestions != null)
                {
                    foreach (var s in result.Suggestions)
                    {
                        s.Confidence = Math.Clamp(s.Confidence, 0, 1);
                    }
                }
                return result;
            }
        }
        catch
        {
            // JSON parse failed
        }

        // TRY 2: Fallback to REGEX
        try
        {
            return ParseDamagesFromText(response);
        }
        catch
        {
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };
        }
    }

    // ===== REGEX FALLBACK PARSERS =====

    private VehicleVerificationResult ParseVerificationFromText(string response)
    {
        bool isVerified = false;
        double confidence = 0.0;
        string licensePlateMatch = "UNCLEAR";
        string reason = "Không phân tích được";

        var verifiedMatch = Regex.Match(response, @"[""']?isVerified[""']?\s*:\s*(true|false)", RegexOptions.IgnoreCase);
        if (verifiedMatch.Success)
        {
            isVerified = verifiedMatch.Groups[1].Value.ToLower() == "true";
        }

        var confidenceMatch = Regex.Match(response, @"[""']?confidence[""']?\s*:\s*([\d.]+)");
        if (confidenceMatch.Success && double.TryParse(confidenceMatch.Groups[1].Value, out double conf))
        {
            confidence = Math.Clamp(conf, 0, 1);
        }

        var plateMatch = Regex.Match(response, @"[""']?licensePlateMatch[""']?\s*:\s*[""'](MATCH|MISMATCH|UNCLEAR)[""']", RegexOptions.IgnoreCase);
        if (plateMatch.Success)
        {
            licensePlateMatch = plateMatch.Groups[1].Value.ToUpper();
        }

        var reasonMatch = Regex.Match(response, @"[""']?reason[""']?\s*:\s*[""']([^""']+)[""']");
        if (reasonMatch.Success)
        {
            reason = reasonMatch.Groups[1].Value.Trim();
        }

        return new VehicleVerificationResult
        {
            IsVerified = isVerified,
            Confidence = confidence,
            LicensePlateMatch = licensePlateMatch,
            Reason = reason
        };
    }

    private DamageDetectionResult ParseDamagesFromText(string response)
    {
        var result = new DamageDetectionResult
        {
            HasNewDamages = false,
            Suggestions = new List<DamageSuggestion>()
        };

        var hasDamagesMatch = Regex.Match(response, @"[""']?hasNewDamages[""']?\s*:\s*(true|false)", RegexOptions.IgnoreCase);
        if (hasDamagesMatch.Success)
        {
            result.HasNewDamages = hasDamagesMatch.Groups[1].Value.ToLower() == "true";
        }

        // Parse suggestions array (simplified)
        var suggestionsMatch = Regex.Match(response, @"[""']?suggestions[""']?\s*:\s*\[(.*?)\]", RegexOptions.Singleline);
        if (suggestionsMatch.Success)
        {
            var suggestionsText = suggestionsMatch.Groups[1].Value;

            // Try to parse individual suggestion objects
            var objectMatches = Regex.Matches(suggestionsText, @"\{([^}]+)\}");
            foreach (Match objMatch in objectMatches)
            {
                var obj = objMatch.Groups[1].Value;

                var location = ExtractJsonValue(obj, "location");
                var damageType = ExtractJsonValue(obj, "damageType");
                var severity = ExtractJsonValue(obj, "severity");
                var confidenceStr = ExtractJsonValue(obj, "confidence");
                var description = ExtractJsonValue(obj, "description");

                if (double.TryParse(confidenceStr, out double confidence))
                {
                    result.Suggestions.Add(new DamageSuggestion
                    {
                        Location = location,
                        DamageType = damageType,
                        Severity = severity,
                        Confidence = Math.Clamp(confidence, 0, 1),
                        Description = description
                    });
                }
            }
        }

        return result;
    }

    private string ExtractJsonValue(string json, string key)
    {
        var match = Regex.Match(json, $@"[""']?{key}[""']?\s*:\s*[""']?([^,""'}}]+)[""']?");
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    // ===== JSON CLEANING =====

    private string CleanJsonResponse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "{}";

        text = text.Trim();

        // Remove markdown
        text = Regex.Replace(text, @"^```(?:json)?\s*", "");
        text = Regex.Replace(text, @"\s*```$", "");
        text = text.Trim();

        // Remove trailing commas before } or ]
        text = Regex.Replace(text, @",(\s*[}\]])", "$1");

        // Fix common issues
        text = Regex.Replace(text, @"\s{2,}", " ");

        return text;
    }

    // ===== OPTIMIZED IMAGE DOWNLOAD =====

    private async Task<List<string>> DownloadImagesAsBase64ParallelAsync(List<string> imageUrls)
    {
        if (imageUrls == null || imageUrls.Count == 0)
            return new List<string>();

        var tasks = imageUrls.Select(url => DownloadSingleImageAsync(url)).ToArray();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results.Where(r => r != null).ToList()!;
    }

    private async Task<string?> DownloadSingleImageAsync(string imageUrl)
    {
        try
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl).ConfigureAwait(false);
            return Convert.ToBase64String(imageBytes);
        }
        catch
        {
            return null;
        }
    }

    // ===== OPTIMIZED API REQUEST (WITH JSON MODE) =====

    private object BuildGeminiRequestWithJSON(string prompt, List<string> handoverImages, List<string> returnImages)
    {
        var parts = new List<object>(capacity: 2 + handoverImages.Count + returnImages.Count + 2)
        {
            new { text = prompt },
            new { text = "\n\nẢNH BÀN GIAO:\n" }
        };

        for (int i = 0; i < handoverImages.Count; i++)
        {
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = handoverImages[i]
                }
            });
        }

        parts.Add(new { text = "\nẢNH TRẢ XE:\n" });

        for (int i = 0; i < returnImages.Count; i++)
        {
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
                maxOutputTokens = 1024,
                responseMimeType = "application/json"  // JSON MODE for speed
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

    // ===== API CALLS =====

    private async Task<string> CallGeminiAPIWithRetryAsync(object requestBody)
    {
        Exception? lastException = null;
        var keysAttempted = new HashSet<int>();

        for (int attempt = 0; attempt < MAX_RETRIES * _apiKeys.Count; attempt++)
        {
            try
            {
                var currentKey = _apiKeys[_currentKeyIndex];
                var response = await CallGeminiAPISingleAttemptAsync(requestBody, currentKey).ConfigureAwait(false);

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
                    await Task.Delay(RETRY_DELAY_MS).ConfigureAwait(false);
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

        throw new Exception($"Failed after {MAX_RETRIES * _apiKeys.Count} attempts", lastException);
    }

    private async Task<string> CallGeminiAPISingleAttemptAsync(object requestBody, string apiKey)
    {
        var url = $"{API_BASE_URL}?key={apiKey}";

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                if (responseBody.Contains("\"code\": 503") ||
                    responseBody.Contains("UNAVAILABLE") ||
                    responseBody.Contains("overloaded"))
                {
                    throw new GeminiOverloadException("API overloaded");
                }

                throw new Exception($"API error {response.StatusCode}");
            }

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
            throw new Exception("Timeout");
        }
    }

    private void SwitchToNextKey()
    {
        _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Count;
    }
}

public class GeminiOverloadException : Exception
{
    public GeminiOverloadException(string message) : base(message) { }
}