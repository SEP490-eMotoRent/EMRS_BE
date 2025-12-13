using EMRS.Application.Abstractions.Models.FacePlusPlus;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IFacePlusPlusService
{

    Task<FaceSearchResult?> SearchByUrlAsync(
    string imageUrl, int returnResultCount = 1, double confidenceThreshold = 70);
    Task<string?> DetectFaceByUrlAsync(IFormFile file);
    Task<string?> CreateFaceSetAsync();
    Task<bool> RemoveFaceAsync(string faceToken);

    Task<bool> AddFaceAsync(string faceToken);


    Task<FaceSearchResult?> SearchByFileAsync(
       IFormFile file, int returnResultCount = 1, double confidenceThreshold=70);
    Task<bool> DeleteFaceSetAsync(string facesetToken);


}
