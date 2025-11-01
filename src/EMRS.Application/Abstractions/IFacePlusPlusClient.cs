using EMRS.Application.Abstractions.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IFacePlusPlusClient
{


    Task<string?> DetectFaceByUrlAsync(IFormFile file);
    Task<string?> CreateFaceSetAsync();
    Task<bool> RemoveFaceAsync(string facesetToken, string faceToken);

    Task<bool> AddFaceAsync(string facesetToken, string faceToken);


    Task<FaceSearchResult?> SearchByFileAsync(IFormFile file, string facesetToken, int returnResultCount = 1);
    Task<bool> DeleteFaceSetAsync(string facesetToken);


}
