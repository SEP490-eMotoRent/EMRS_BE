using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class FptFaceSearchClient : IFptFaceSearchClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public FptFaceSearchClient(HttpClient http)
    {
        _http = http;
        _baseUrl = Environment.GetEnvironmentVariable("FPT_FACE_BASE") ?? "https://api.fpt.ai/dmp/facesearch/v2";

      
    }

    public async Task<bool> CreateUserAsync(string collection, string id, string name, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), "collection");
        form.Add(new StringContent(id), "id");
        form.Add(new StringContent(name), "name");

        var resp = await _http.PostAsync($"{_baseUrl}/create", form, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var body = await resp.Content.ReadFromJsonAsync<FptResponse<object?>>(cancellationToken: ct);
        return body != null && (body.code == "200" || body.code == "201");
    }

    public async Task<bool> AddImageAsync(string collection, string id, Stream imageStream, string filename, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), "collection");
        form.Add(new StringContent(id), "id");

        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(imageContent, "file", filename);

        var resp = await _http.PostAsync($"{_baseUrl}/add", form, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var body = await resp.Content.ReadFromJsonAsync<FptResponse<object?>>(cancellationToken: ct);
        return body != null && (body.code == "200" || body.code == "201");
    }

    public async Task<FaceSearchResult?> SearchAsync(string collection, Stream imageStream, string filename, double? threshold = null, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), "collection");
        if (threshold.HasValue)
            form.Add(new StringContent(threshold.Value.ToString("G", CultureInfo.InvariantCulture)), "threshold");

        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(imageContent, "file", filename);

        var resp = await _http.PostAsync($"{_baseUrl}/search", form, ct);
        if (!resp.IsSuccessStatusCode) return null;

        var body = await resp.Content.ReadFromJsonAsync<FptResponse<FaceSearchResult>>(cancellationToken: ct);
        return body?.data;
    }
}
