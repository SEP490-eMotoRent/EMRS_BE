using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public static class FileHelper
{
    public static IFormFile ConvertByteArrayToFormFile(byte[] fileBytes, string fileName, string contentType = "application/pdf")
    {
        var stream = new MemoryStream(fileBytes);
        IFormFile file = new FormFile(stream, 0, fileBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        return file;
    }
    public static async Task<string> ConvertToBase64FastAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return Convert.ToBase64String(ms.ToArray());
    }
    public static IFormFile ConvertPdfBytesToIFormFile(byte[] pdfBytes, string fileName)
    {
        var stream = new MemoryStream(pdfBytes);

        IFormFile file = new FormFile(stream, 0, pdfBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        return file;
    }

    public static async Task<string> ConvertUrlToBase64StreamAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        using (var stream = await client.GetStreamAsync(url)) // stream file directly
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream); // copy to memory stream in chunks
            byte[] fileBytes = memoryStream.ToArray();

            return Convert.ToBase64String(fileBytes);
        }
    }
    public static string GetFilePrefix(IFormFile file)
    {
        var contentType = file.ContentType.ToLower();

        if (contentType.StartsWith("image/"))
            return "img";

        if (contentType.StartsWith("video/"))
            return "media";

        var documentTypes = new[]
        {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    };

        if (documentTypes.Contains(contentType))
            return "doc";

        return "file"; // fallback
    }



}
