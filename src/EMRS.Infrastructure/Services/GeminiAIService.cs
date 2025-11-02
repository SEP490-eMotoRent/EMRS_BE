// EMRS.Infrastructure/Services/GeminiAIService.cs
using EMRS.Application.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMRS.Infrastructure.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public GeminiAIService()
    {
        _httpClient = new HttpClient();
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? "AIzaSyAtvjhg4TOZ8-ZwhnRhdvYIWMGskdLeeEM";
    }

    public async Task<VehicleVerificationResult> VerifyVehicleAsync(
        List<string> handoverImageUrls,
        List<string> returnImageUrls,
        string licensePlate)
    {
        try
        {
            // Tải ảnh từ URLs
            var handoverImages = await DownloadImagesAsBase64Async(handoverImageUrls);
            var returnImages = await DownloadImagesAsBase64Async(returnImageUrls);

            // Tạo prompt chi tiết
            var prompt = $@"
You are an expert vehicle inspector. Compare these two sets of images:

SET 1 - HANDOVER IMAGES (when vehicle was given to customer):
These are the original condition photos taken when the vehicle was handed over.

SET 2 - RETURN IMAGES (when vehicle was returned):
These are the photos taken when the vehicle was returned.

VEHICLE LICENSE PLATE: {licensePlate}

YOUR TASK:
1. Verify this is THE SAME VEHICLE by checking:
   - License plate matches: '{licensePlate}'
   - Overall vehicle appearance (color, model, distinctive features)
   - Unique marks or characteristics

2. Provide a confidence score (0-1) for vehicle verification

3. Explain your reasoning clearly

RESPONSE FORMAT (JSON only):
{{
  ""isVerified"": true/false,
  ""confidence"": 0.0-1.0,
  ""reason"": ""detailed explanation"",
  ""licensePlateMatch"": ""MATCH"" or ""MISMATCH"" or ""UNCLEAR""
}}

Be strict: If you cannot clearly see the license plate or have ANY doubt, set isVerified to false.
";

            var requestBody = BuildGeminiRequest(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIAsync(requestBody);

            return ParseVerificationResponse(response);
        }
        catch (Exception ex)
        {
            return new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                Reason = $"Error during verification: {ex.Message}",
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

            var prompt = @"
You are an expert vehicle damage inspector. Compare these two sets of images:

SET 1 - HANDOVER IMAGES (original condition):
These show the vehicle's condition when it was handed over to the customer.

SET 2 - RETURN IMAGES (current condition):
These show the vehicle's condition when returned.

YOUR TASK:
Identify ANY NEW damages, scratches, dents, or issues that appear in RETURN images but NOT in HANDOVER images.

For each new damage found, provide:
- Location (e.g., ""Front bumper"", ""Left side mirror"", ""Rear panel"")
- Damage type (e.g., ""Scratch"", ""Dent"", ""Missing part"", ""Crack"")
- Severity (""Minor"", ""Moderate"", ""Severe"")
- Confidence (0.0-1.0)
- Description (brief explanation)

RESPONSE FORMAT (JSON only):
{
  ""hasNewDamages"": true/false,
  ""suggestions"": [
    {
      ""location"": ""specific location"",
      ""damageType"": ""type of damage"",
      ""severity"": ""Minor/Moderate/Severe"",
      ""confidence"": 0.0-1.0,
      ""description"": ""brief description""
    }
  ]
}

IMPORTANT:
- Only report NEW damages (not present in handover images)
- Be conservative: If unsure, mention it with lower confidence
- If no new damages found, return empty suggestions array
";

            var requestBody = BuildGeminiRequest(prompt, handoverImages, returnImages);
            var response = await CallGeminiAPIAsync(requestBody);

            return ParseDamageResponse(response);
        }
        catch (Exception ex)
        {
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>
                {
                    new DamageSuggestion
                    {
                        Location = "Unknown",
                        DamageType = "Analysis Error",
                        Severity = "Unknown",
                        Confidence = 0,
                        Description = $"Error during analysis: {ex.Message}"
                    }
                }
            };
        }
    }

    // === HELPER METHODS ===

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
                Console.WriteLine($"Failed to download image from {url}: {ex.Message}");
            }
        }

        return base64Images;
    }

    private object BuildGeminiRequest(string prompt, List<string> handoverImages, List<string> returnImages)
    {
        var contents = new List<object>();

        // Add prompt text
        contents.Add(new
        {
            text = prompt
        });

        // Add handover images
        contents.Add(new { text = "=== HANDOVER IMAGES ===" });
        foreach (var img in handoverImages)
        {
            contents.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = img
                }
            });
        }

        // Add return images
        contents.Add(new { text = "=== RETURN IMAGES ===" });
        foreach (var img in returnImages)
        {
            contents.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = img
                }
            });
        }

        return new
        {
            contents = new[]
            {
                new
                {
                    parts = contents.ToArray()
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                topK = 32,
                topP = 1,
                maxOutputTokens = 2048
            }
        };
    }

    private async Task<string> CallGeminiAPIAsync(object requestBody)
    {
        var url = $"{API_BASE_URL}?key={_apiKey}";
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    private VehicleVerificationResult ParseVerificationResponse(string apiResponse)
    {
        try
        {
            var doc = JsonDocument.Parse(apiResponse);
            var textContent = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Extract JSON from markdown code block if present
            textContent = ExtractJsonFromMarkdown(textContent);

            var result = JsonSerializer.Deserialize<VehicleVerificationResult>(
                textContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                Reason = "Failed to parse AI response",
                LicensePlateMatch = "UNCLEAR"
            };
        }
        catch (Exception ex)
        {
            return new VehicleVerificationResult
            {
                IsVerified = false,
                Confidence = 0,
                Reason = $"Failed to parse response: {ex.Message}",
                LicensePlateMatch = "UNCLEAR"
            };
        }
    }

    private DamageDetectionResult ParseDamageResponse(string apiResponse)
    {
        try
        {
            var doc = JsonDocument.Parse(apiResponse);
            var textContent = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            textContent = ExtractJsonFromMarkdown(textContent);

            var result = JsonSerializer.Deserialize<DamageDetectionResult>(
                textContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>()
            };
        }
        catch (Exception ex)
        {
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>
                {
                    new DamageSuggestion
                    {
                        Location = "Unknown",
                        DamageType = "Parse Error",
                        Severity = "Unknown",
                        Confidence = 0,
                        Description = $"Failed to parse response: {ex.Message}"
                    }
                }
            };
        }
    }

    private string ExtractJsonFromMarkdown(string text)
    {
        // Remove markdown code blocks (```json ... ```)
        text = text.Trim();

        if (text.StartsWith("```json"))
        {
            text = text.Substring(7);
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
        }

        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3);
        }

        return text.Trim();
    }
}