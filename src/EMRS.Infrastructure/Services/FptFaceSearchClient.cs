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
    private const string CreateEndpoint = "create";
    private const string AddEndpoint = "add";
    private const string SearchEndpoint = "search";

    private const string CollectionField = "collection";
    private const string IdField = "id";
    private const string NameField = "name";
    private const string FileField = "file";
    private const string ThresholdField = "threshold";

    private const string SuccessCode200 = "200";
    private const string SuccessCode201 = "201";

    private const string JpegMimeType = "image/jpeg";

    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public FptFaceSearchClient(HttpClient http)
    {
        _http = http;
        _baseUrl = Environment.GetEnvironmentVariable("FPT_FACE_BASE") ?? "https://api.fpt.ai/dmp/facesearch/v2";
    }

    public async Task<bool> CreateUserAsync(string collection
        , string id, string name
        , CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), CollectionField);
        form.Add(new StringContent(id), IdField);
        form.Add(new StringContent(name), NameField);

        var body = await PostAndDeserializeAsync<object?>(CreateEndpoint, form, ct);
        return body != null && IsSuccessCode(body.code);
    }

    public async Task<bool> AddImageAsync(string collection
        , string id, Stream imageStream, string filename
        , CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), CollectionField);
        form.Add(new StringContent(id), IdField);
        form.Add(CreateImageContent(imageStream), FileField, filename);

        var body = await PostAndDeserializeAsync<object?>(AddEndpoint, form, ct);

        return body != null && IsSuccessCode(body.code);
    }

    public async Task<FaceSearchResult?> SearchAsync(string collection
        , Stream imageStream, string filename
        , double? threshold = null, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(collection), CollectionField);
        if (threshold.HasValue)
        {
            form.Add(new StringContent(threshold.Value.ToString("G", CultureInfo.InvariantCulture)), ThresholdField);
        }
        form.Add(CreateImageContent(imageStream), FileField, filename);

        var body = await PostAndDeserializeAsync<FaceSearchResult>(SearchEndpoint, form, ct);

        if (body != null && IsSuccessCode(body.code))
        {
            return body.data;
        }

        return null;
    }


    private async Task<FptResponse<T>?> PostAndDeserializeAsync<T>(string endpoint
        , MultipartFormDataContent form, CancellationToken ct)
    {
        try
        {
            var resp = await _http.PostAsync($"{_baseUrl}/{endpoint}", form, ct);

            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<FptResponse<T>>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private StreamContent CreateImageContent(Stream imageStream)
    {
        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(JpegMimeType);
        return imageContent;
    }

    private bool IsSuccessCode(string? code)
    {
        return code == SuccessCode200 || code == SuccessCode201;
    }
}
