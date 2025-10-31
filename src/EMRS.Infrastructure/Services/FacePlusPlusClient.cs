using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class FacePlusPlusClient:IFacePlusPlusClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _apiSecret;

    public FacePlusPlusClient(HttpClient http)
    {
        _http = http;
        _apiKey = Environment.GetEnvironmentVariable("FACEPP_API_KEY")
                  ?? throw new InvalidOperationException("FACEPP_API_KEY not set");
        _apiSecret = Environment.GetEnvironmentVariable("FACEPP_API_SECRET")
                  ?? throw new InvalidOperationException("FACEPP_API_SECRET not set");
    }

   
    public async Task<string?> DetectFaceByUrlAsync(string imageUrl)
    {
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
            new KeyValuePair<string, string>("image_url", imageUrl)
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

    public async Task<bool> AddFaceAsync(string facesetToken, string faceToken)
    {
        using var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("api_secret", _apiSecret),
            new KeyValuePair<string, string>("faceset_token", facesetToken),
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
        return result?.faceset_token!=null;
    }

    public async Task<FaceSearchResult?> SearchByFileAsync(IFormFile file, string facesetToken, int returnResultCount = 1)
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

        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("image_base64", base64),
        new KeyValuePair<string, string>("faceset_token", facesetToken),
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
        return new FaceSearchResult
        {
            Id = best.face_token,
            Name = best.user_id,
            Score = best.confidence ?? 0
        };
    }


    public async Task<bool> RemoveFaceAsync(string facesetToken, string faceToken)
    {
        using var form = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("api_key", _apiKey),
        new KeyValuePair<string, string>("api_secret", _apiSecret),
        new KeyValuePair<string, string>("faceset_token", facesetToken),
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