using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface ICloudinaryService
{
    Task<bool> DeleteImageFileByUrlAsync(string fileUrl, string folderName);
    Task<bool> DeleteDocFileByUrlAsync(string fileUrl, string folderName);
        Task<string?> UploadDocumentFileAsync(IFormFile file, string fileName, string folderName, string? oldFileUrl = null);
    Task<string?> UploadImageFileAsync(IFormFile file, string fileName, string folderName, string? oldImageUrl = null);
}
