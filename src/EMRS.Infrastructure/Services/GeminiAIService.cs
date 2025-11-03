// EMRS.Infrastructure/Services/GeminiAIService.cs (CẬP NHẬT)
using EMRS.Application.Abstractions;
using System.Text;
using System.Text.Json;

namespace EMRS.Infrastructure.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public GeminiAIService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(2); // Tăng timeout
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

            if (handoverImages.Count == 0 || returnImages.Count == 0)
            {
                return new VehicleVerificationResult
                {
                    IsVerified = false,
                    Confidence = 0,
                    Reason = "Failed to load images for comparison",
                    LicensePlateMatch = "UNCLEAR"
                };
            }

            // ===== CẢI THIỆN PROMPT - RÕ RÀNG HƠN =====
            var prompt = $@"
You are a professional vehicle inspector performing a vehicle return verification.

**CRITICAL CONTEXT:**
- Expected License Plate: **{licensePlate}**
- This is a vehicle rental return inspection
- You must verify if the RETURN photos show the SAME vehicle as the HANDOVER photos

**YOUR TASK:**
Compare the HANDOVER images (when vehicle was given to customer) with the RETURN images (when vehicle came back).

**VERIFICATION CHECKLIST:**
1. ✓ License Plate Visibility and Match
   - Can you clearly see the license plate in BOTH sets?
   - Does the plate match: '{licensePlate}'?
   
2. ✓ Vehicle Model and Color
   - Same body shape and design?
   - Same color scheme?
   - Same wheel design?
   
3. ✓ Distinctive Features
   - Any unique scratches, stickers, or marks visible in BOTH sets?
   - Same brand/model logos visible?

**IMPORTANT RULES:**
- If you see THE SAME scooter model with THE SAME color in BOTH sets → likely the SAME vehicle
- License plate might not be clearly visible in all angles (front/side views) - this is NORMAL for scooters
- Focus on OVERALL vehicle appearance, not just license plate
- If 80%+ features match → isVerified = true

**RESPONSE FORMAT (JSON only, no explanation outside JSON):**
{{
  ""isVerified"": true or false,
  ""confidence"": 0.0 to 1.0,
  ""reason"": ""detailed explanation of your decision"",
  ""licensePlateMatch"": ""MATCH"" or ""MISMATCH"" or ""UNCLEAR""
}}

**ANALYZE THE IMAGES BELOW:**
";

            var requestBody = BuildImprovedGeminiRequest(prompt, handoverImages, returnImages);
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

            if (handoverImages.Count == 0 || returnImages.Count == 0)
            {
                return new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>
                    {
                        new DamageSuggestion
                        {
                            Location = "Unknown",
                            DamageType = "Image Load Error",
                            Severity = "Unknown",
                            Confidence = 0,
                            Description = "Failed to load images for damage detection"
                        }
                    }
                };
            }

            // ===== CẢI THIỆN PROMPT CHO DAMAGE DETECTION =====
            var prompt = @"
You are an expert vehicle damage inspector for a rental company.

**YOUR TASK:**
Compare HANDOVER images (original condition) with RETURN images (current condition) to find NEW damages.

**WHAT TO LOOK FOR:**
- Scratches (small surface marks)
- Dents (deformations in body panels)
- Cracks (breaks in plastic/mirror parts)
- Missing parts (mirrors, covers, logos)
- Broken lights
- Tire damage
- Seat tears or damage

**CRITICAL RULES:**
- ONLY report damages that are CLEARLY VISIBLE in RETURN images but NOT in HANDOVER images
- Ignore lighting differences, camera angles, or minor reflections
- If uncertain (confidence < 0.6), still report but mark as ""Minor"" severity with lower confidence
- If images are identical or very similar, return empty suggestions array

**DAMAGE SEVERITY GUIDE:**
- ""Minor"": Small scratches, minor scuffs (< 5cm)
- ""Moderate"": Visible scratches/dents, broken minor parts (5-15cm)
- ""Severe"": Large damage, broken major parts, safety issues (> 15cm)

**RESPONSE FORMAT (JSON only):**
{
  ""hasNewDamages"": true or false,
  ""suggestions"": [
    {
      ""location"": ""exact location (e.g., Front Left Panel, Right Mirror, Rear Mudguard)"",
      ""damageType"": ""Scratch / Dent / Crack / Missing Part / Broken Light / Other"",
      ""severity"": ""Minor / Moderate / Severe"",
      ""confidence"": 0.0 to 1.0,
      ""description"": ""brief specific description""
    }
  ]
}

**EXAMPLES:**
Good: {""location"": ""Front Left Panel"", ""damageType"": ""Scratch"", ""severity"": ""Minor"", ""confidence"": 0.85, ""description"": ""Vertical scratch about 8cm long near headlight""}
Bad: {""location"": ""Front"", ""damageType"": ""Damage"", ""severity"": ""Unknown"", ""confidence"": 0.5, ""description"": ""Something wrong""}

**ANALYZE THE IMAGES BELOW:**
";

            var requestBody = BuildImprovedGeminiRequest(prompt, handoverImages, returnImages);
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

    // ===== CẢI THIỆN HELPER METHODS =====

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
                Console.WriteLine($"⚠️ Failed to download image from {url}: {ex.Message}");
                // Continue với ảnh khác, không throw exception
            }
        }

        return base64Images;
    }

    private object BuildImprovedGeminiRequest(string prompt, List<string> handoverImages, List<string> returnImages)
    {
        var parts = new List<object>();

        // 1. Add instruction prompt
        parts.Add(new { text = prompt });

        // 2. Add HANDOVER images with clear labels
        parts.Add(new { text = "\n\n📸 **HANDOVER IMAGES (Original Condition):**\n" });

        for (int i = 0; i < handoverImages.Count; i++)
        {
            parts.Add(new { text = $"**Handover Image {i + 1}/{handoverImages.Count}:**" });
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = handoverImages[i]
                }
            });
        }

        // 3. Add clear separator
        parts.Add(new { text = "\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" });

        // 4. Add RETURN images with clear labels
        parts.Add(new { text = "📸 **RETURN IMAGES (Current Condition):**\n" });

        for (int i = 0; i < returnImages.Count; i++)
        {
            parts.Add(new { text = $"**Return Image {i + 1}/{returnImages.Count}:**" });
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = "image/jpeg",
                    data = returnImages[i]
                }
            });
        }

        // 5. Final instruction
        parts.Add(new { text = "\n\n**NOW PROVIDE YOUR ANALYSIS IN JSON FORMAT:**" });

        return new
        {
            contents = new[]
            {
                new
                {
                    parts = parts.ToArray()
                }
            },
            generationConfig = new
            {
                temperature = 0.2,  // Giảm temperature để ổn định hơn
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 2048,
                responseMimeType = "application/json"  // Bắt buộc trả JSON
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

    private async Task<string> CallGeminiAPIAsync(object requestBody)
    {
        var url = $"{API_BASE_URL}?key={_apiKey}";
        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Gemini API Error: {response.StatusCode}");
                Console.WriteLine($"Response: {responseBody}");
                throw new Exception($"Gemini API returned {response.StatusCode}: {responseBody}");
            }

            return responseBody;
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Gemini API request timed out after 2 minutes");
        }
    }

    private VehicleVerificationResult ParseVerificationResponse(string apiResponse)
    {
        try
        {
            var doc = JsonDocument.Parse(apiResponse);

            // Check for error in response
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errorMessage = error.GetProperty("message").GetString();
                return new VehicleVerificationResult
                {
                    IsVerified = false,
                    Confidence = 0,
                    Reason = $"API Error: {errorMessage}",
                    LicensePlateMatch = "UNCLEAR"
                };
            }

            var textContent = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Extract JSON from markdown code block if present
            textContent = ExtractJsonFromMarkdown(textContent);

            // Parse JSON response
            var result = JsonSerializer.Deserialize<VehicleVerificationResult>(
                textContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result == null)
            {
                return new VehicleVerificationResult
                {
                    IsVerified = false,
                    Confidence = 0,
                    Reason = "Failed to parse AI response - result is null",
                    LicensePlateMatch = "UNCLEAR"
                };
            }

            // Validate parsed result
            if (result.Confidence < 0 || result.Confidence > 1)
            {
                result.Confidence = Math.Clamp(result.Confidence, 0, 1);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parse Error: {ex.Message}");
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

            // Check for error
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var errorMessage = error.GetProperty("message").GetString();
                return new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>
                    {
                        new DamageSuggestion
                        {
                            Location = "API Error",
                            DamageType = "Error",
                            Severity = "Unknown",
                            Confidence = 0,
                            Description = $"API Error: {errorMessage}"
                        }
                    }
                };
            }

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

            if (result == null)
            {
                return new DamageDetectionResult
                {
                    HasNewDamages = false,
                    Suggestions = new List<DamageSuggestion>()
                };
            }

            // Validate confidence scores
            if (result.Suggestions != null)
            {
                foreach (var suggestion in result.Suggestions)
                {
                    suggestion.Confidence = Math.Clamp(suggestion.Confidence, 0, 1);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parse Error: {ex.Message}");
            return new DamageDetectionResult
            {
                HasNewDamages = false,
                Suggestions = new List<DamageSuggestion>
                {
                    new DamageSuggestion
                    {
                        Location = "Parse Error",
                        DamageType = "Error",
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
        if (string.IsNullOrWhiteSpace(text))
            return "{}";

        text = text.Trim();

        // Remove markdown code blocks
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