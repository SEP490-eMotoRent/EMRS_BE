using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.FacePlusPlus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class FacePlusPlusService:IFacePlusPlusService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _faceToken;
    private readonly ILogger<FacePlusPlusService> _logger;
    public FacePlusPlusService(HttpClient http,ILogger<FacePlusPlusService> logger)
    {
        _http = http;
        _logger = logger;
        _faceToken = Environment.GetEnvironmentVariable("FACE_SET_TOKEN")
                 ?? throw new InvalidOperationException("FACE_SET_TOKEN not set");
        _apiKey = Environment.GetEnvironmentVariable("FACEPP_API_KEY")
                  ?? throw new InvalidOperationException("FACEPP_API_KEY not set");
        _apiSecret = Environment.GetEnvironmentVariable("FACEPP_API_SECRET")
                  ?? throw new InvalidOperationException("FACEPP_API_SECRET not set");
    }

   
    public async Task<string?> DetectFaceByUrlAsync(IFormFile file)
    {
        string base64;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            base64 = Convert.ToBase64String(fileBytes);
        }
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
           new KeyValuePair<string, string>("image_base64", base64),

        });

        var response = await _http.PostAsync("detect", form);
        var json = await response.Content.ReadAsStringAsync();
        

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($" Detect failed (HTTP {response.StatusCode}): {json}");
            return null;
        }

        Console.WriteLine($" Detect success: {json}");
        var result = await response.Content.ReadFromJsonAsync<FacePlusPlusDetectResponse>();
        return result?.faces?.FirstOrDefault()?.face_token;
    }

    public async Task<string?> CreateFaceSetAsync()
    {
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
        });
        
        var response = await _http.PostAsync("faceset/create", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($" Create FaceSet failed: {json}");
            return null;
        }
        Console.WriteLine($" success: {json}");
        var result = await response.Content.ReadFromJsonAsync<FacePlusPlusFaceSetResponse>();
        return result?.faceset_token;
    }

    public async Task<bool> AddFaceAsync( string faceToken)
    {
        await Task.Delay(1000);
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
            new KeyValuePair<string, string>("faceset_token", _faceToken),
            new KeyValuePair<string, string>("face_tokens", faceToken)
        });

        var response = await _http.PostAsync("faceset/addface", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($" Add Face failed: {json}");
            return false;
        }
        Console.WriteLine($"  success: {json}");
        var result = await response.Content.ReadFromJsonAsync<FacePlusPlusFaceSetResponse>();
        return true;
    }
    public async Task<FaceSearchResult?> SearchByUrlAsync(
    string imageUrl, int returnResultCount = 1, double confidenceThreshold = 70)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is null or empty", nameof(imageUrl));

        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("image_url", imageUrl),
        new KeyValuePair<string, string>("faceset_token", _faceToken),
        new KeyValuePair<string, string>("return_result_count", returnResultCount.ToString())
    });

        var response = await _http.PostAsync(
            "https://api-us.faceplusplus.com/facepp/v3/search", form);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Search failed: {json}");
            return null;
        }

        var data = await response.Content.ReadFromJsonAsync<FacePlusPlusSearchResponse>();
        if (data?.results == null || data.results.Count == 0)
            return null;

        var best = data.results.OrderByDescending(r => r.confidence).First();

        if (best.confidence is null || best.confidence < confidenceThreshold)
        {
            return new FaceSearchResult
            {
                Id = null,
                Name = best.user_id,
                Score = best.confidence ?? 0,
                Message = $"No match: confidence {best.confidence} < threshold {confidenceThreshold}",
                IsMatch = false
            };
        }

        return new FaceSearchResult
        {
            Id = best.face_token,
            Name = best.user_id,
            Score = best.confidence ?? 0,
            Message = "Face returned",
            IsMatch = true
        };
    }

    public async Task<FaceSearchResult?> SearchByFileAsync(
        IFormFile file, int returnResultCount = 1, double confidenceThreshold = 70)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty", nameof(file));


        string base64;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            base64 = Convert.ToBase64String(fileBytes);
        }
        /*string base64;
        using (var inputStream = file.OpenReadStream())
        {
            using (var originalBitmap = SKBitmap.Decode(inputStream))
            {
                if (originalBitmap == null)
                {
                    _logger.LogError("Failed to decode image via SkiaSharp.");
                    return null;
                }

                int maxWidth = 1024;
                int newWidth = originalBitmap.Width;
                int newHeight = originalBitmap.Height;
                if (originalBitmap.Width > maxWidth)
                {
                    float ratio = (float)maxWidth / originalBitmap.Width;
                    newWidth = maxWidth;
                    newHeight = (int)(originalBitmap.Height * ratio);
                }

                var info = new SKImageInfo(newWidth, newHeight);
                using (var resizedBitmap = originalBitmap.Resize(info, SKFilterQuality.High))
                using (var image = SKImage.FromBitmap(resizedBitmap))
                using (var dataFromImage = image.Encode(SKEncodedImageFormat.Jpeg, 75)) // Nén 75%
                {
                    byte[] imageBytes = dataFromImage.ToArray();

                    _logger.LogInformation("Image processed: {OldSize} -> {NewSize} bytes", file.Length, imageBytes.Length);

                    base64 = Convert.ToBase64String(imageBytes);
                }
            }
        }*/
        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("image_base64", base64),
        new KeyValuePair<string, string>("faceset_token", _faceToken),
        new KeyValuePair<string, string>("return_result_count", returnResultCount.ToString())
    });

        var response = await _http.PostAsync("https://api-us.faceplusplus.com/facepp/v3/search", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Search failed: {json}");
            return null;
        }

        var data = await response.Content.ReadFromJsonAsync<FacePlusPlusSearchResponse>();
        if (data?.results == null || data.results.Count == 0)
            return null;

        var best = data.results.OrderByDescending(r => r.confidence).First();

        if (best.confidence is null || best.confidence < confidenceThreshold)
        {
            return new FaceSearchResult
            {
                Id = null,
                Name = best.user_id,
                Score = best.confidence ?? 0,
                Message = $"No match: confidence {best.confidence} < threshold {confidenceThreshold}",
                IsMatch = false
            };
        }
        else
        {
            return new FaceSearchResult
            {
                Id = best.face_token,
                Name = best.user_id,
                Score = best.confidence ?? 0,
                Message = "Face returned",
                IsMatch = true
            };
        }
    }
    /*public async Task<FaceSearchResult?> SearchByFileAsync(
        IFormFile file, int returnResultCount = 1, double confidenceThreshold = 70)
    {
        // 1. Log Input: Ghi lại thông tin file đầu vào
        _logger.LogInformation("Processing File: {FileName} | Size: {Size} bytes", file?.FileName, file?.Length);

        if (file == null || file.Length == 0)
        {
            _logger.LogError("File validation failed: Null or Empty.");
            throw new ArgumentException("File is null or empty", nameof(file));
        }
        string base64;
        using (var inputStream = file.OpenReadStream())
        {
            using (var originalBitmap = SKBitmap.Decode(inputStream))
            {
                if (originalBitmap == null)
                {
                    _logger.LogError("Failed to decode image via SkiaSharp.");
                    return null;
                }

                int maxWidth = 1024;
                int newWidth = originalBitmap.Width;
                int newHeight = originalBitmap.Height;
                if (originalBitmap.Width > maxWidth)
                {
                    float ratio = (float)maxWidth / originalBitmap.Width;
                    newWidth = maxWidth;
                    newHeight = (int)(originalBitmap.Height * ratio);
                }

                var info = new SKImageInfo(newWidth, newHeight);
                using (var resizedBitmap = originalBitmap.Resize(info, SKFilterQuality.High))
                using (var image = SKImage.FromBitmap(resizedBitmap))
                using (var dataFromImage = image.Encode(SKEncodedImageFormat.Jpeg, 75)) // Nén 75%
                {
                    byte[] imageBytes = dataFromImage.ToArray();

                    _logger.LogInformation("Image processed: {OldSize} -> {NewSize} bytes", file.Length, imageBytes.Length);

                    base64 = Convert.ToBase64String(imageBytes);
                }
            }
        }

        // 3. Chuẩn bị Form Data
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
            new KeyValuePair<string, string>("image_base64", base64),
            new KeyValuePair<string, string>("faceset_token", _faceToken),
            new KeyValuePair<string, string>("return_result_count", returnResultCount.ToString())
        });

        _logger.LogInformation("Calling Face++ API...");

        // 4. Gọi API
        var response = await _http.PostAsync("https://api-us.faceplusplus.com/facepp/v3/search", form);
        var json = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Face++ API Response (LOOK AT THIS): {Json}", json);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Face++ API Error. Status: {Status}. Response: {Json}", response.StatusCode, json);
            return null;
        }

        var data = await response.Content.ReadFromJsonAsync<FacePlusPlusSearchResponse>();

        if (data?.results == null || data.results.Count == 0)
        {
            _logger.LogWarning("API OK but No Face Found in image.");
            return null;
        }

        var best = data.results.OrderByDescending(r => r.confidence).First();

        // 7. So sánh độ tin cậy (Confidence)
        if (best.confidence is null || best.confidence < confidenceThreshold)
        {
            _logger.LogWarning("Low Confidence: {Score} < {Threshold}. User: {User}",
                best.confidence, confidenceThreshold, best.user_id);

            return new FaceSearchResult
            {
                Id = null,
                Name = best.user_id,
                Score = best.confidence ?? 0,
                Message = $"No match: confidence {best.confidence} < threshold {confidenceThreshold}",
                IsMatch = false
            };
        }

        // 8. Thành công
        _logger.LogInformation("Match Found! User: {User} - Score: {Score}", best.user_id, best.confidence);

        return new FaceSearchResult
        {
            Id = best.face_token,
            Name = best.user_id,
            Score = best.confidence ?? 0,
            Message = "Face returned",
            IsMatch = true
        };
    }*/

    public async Task<bool> RemoveFaceAsync( string faceToken)
    {
        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("faceset_token", _faceToken),
        new KeyValuePair<string, string>("face_tokens", faceToken)
    });

        var response = await _http.PostAsync("faceset/removeface", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($" Remove Face failed: {json}");
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<FacePlusPlusRemoveFaceResponse>();

        if (result?.faceset_token != null)
        {
            Console.WriteLine($" Remove Face success: face_removed={result.face_removed}, face_count={result.face_count}");
            return true;
        }

        Console.WriteLine($" Remove Face response invalid: {json}");
        return false;
    }

    public async Task<bool> DeleteFaceSetAsync(string facesetToken)
    {
        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),   
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("faceset_token", facesetToken),
        new KeyValuePair<string, string>("check_empty", "0") 
    });

        var response = await _http.PostAsync("faceset/delete", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($" Delete FaceSet failed: {json}");
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<FacePlusPlusDeleteResponse>();

        if (result?.faceset_token != null)
        {
            Console.WriteLine($" Delete success: faceset_token={result.faceset_token}");
            return true;
        }

        Console.WriteLine($" Delete response invalid: {json}");
        return false;
    }

}