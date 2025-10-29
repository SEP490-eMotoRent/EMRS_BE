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
}
