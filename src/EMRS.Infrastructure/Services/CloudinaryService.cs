using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EMRS.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Services;

public class CloudinaryService: ICloudinaryService
{
    private readonly Cloudinary cloudinary;
    public CloudinaryService()
    {
        var cloudinaryName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
        var account = new Account(
             cloudinaryName,
             apiKey,
              apiSecret);
        cloudinary = new Cloudinary(account);
    }
    private static string? ExtractPublicIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var file = Path.GetFileNameWithoutExtension(parts.Last());
            return file?.Split('_').Length == 3
                ? string.Join('/', parts.Skip(4).SkipLast(1).Append(file))
                : null;
        }
        catch { return null; }
    }

    public async Task<string?> UploadImageFileAsync(IFormFile file, string fileName, string folderName, string? oldImageUrl = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return null;

            await using var stream = file.OpenReadStream();

            // Nếu có URL cũ thì extract public_id để overwrite
            string? oldPublicId = !string.IsNullOrEmpty(oldImageUrl)
                ? ExtractPublicIdFromUrl(oldImageUrl)
                : null;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = true,
                Folder = folderName,
                FilenameOverride = fileName
            };

            if (!string.IsNullOrEmpty(oldPublicId))
                uploadParams.PublicId = oldPublicId;

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl?.ToString();

            return null;
        }
        catch
        {
            return null;
        }
    }
    public async Task<string?> UploadDocumentFileAsync(IFormFile file, string fileName, string folderName, string? oldFileUrl = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".doc", ".docx", ".pdf" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                return null; 

            await using var stream = file.OpenReadStream();
            string? oldPublicId = !string.IsNullOrEmpty(oldFileUrl)
                ? ExtractPublicIdFromUrl(oldFileUrl)
                : null;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = true,
                Folder = folderName,
                FilenameOverride = fileName
            };

            if (!string.IsNullOrEmpty(oldPublicId))
                uploadParams.PublicId = oldPublicId;

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl?.ToString();

            return null;
        }
        catch
        {
            return null;
        }
    }

}
