using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Vision.Face;
using EMRS.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace EMRS.Infrastructure.Services
{
    public class FaceAPIService : IFaceAPIService
    {
        private readonly FaceClient _faceClient;

        public FaceAPIService(IConfiguration config)
        {
            var endpoint = Environment.GetEnvironmentVariable("FACE_ENDPOINT") ?? config["FACE_ENDPOINT"];
            var key = Environment.GetEnvironmentVariable("FACE_APIKEY") ?? config["FACE_APIKEY"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
                throw new InvalidOperationException("FACE_ENDPOINT hoặc FACE_APIKEY chưa được cấu hình.");

            _faceClient = new FaceClient(new Uri(endpoint), new AzureKeyCredential(key));
        }

        public async Task<(bool IsIdentical, double Confidence)> VerifyFaceAsync(string cccdUrl, string selfieUrl)
        {
            try
            {
                // Detect hai ảnh từ URL
                var cccdResponse = await _faceClient.DetectAsync(
                    new Uri(cccdUrl),
                    FaceDetectionModel.Detection03,
                    FaceRecognitionModel.Recognition04,
                    returnFaceId: true,
                    returnFaceAttributes: new[] { FaceAttributeType.QualityForRecognition }
                );

                var selfieResponse = await _faceClient.DetectAsync(
                    new Uri(selfieUrl),
                    FaceDetectionModel.Detection03,
                    FaceRecognitionModel.Recognition04,
                    returnFaceId: true,
                    returnFaceAttributes: new[] { FaceAttributeType.QualityForRecognition }
                );

                var cccdFaces = cccdResponse.Value;
                var selfieFaces = selfieResponse.Value;

                if (!cccdFaces.Any() || !cccdFaces[0].FaceId.HasValue)
                    return (false, 0);

                if (!selfieFaces.Any() || !selfieFaces[0].FaceId.HasValue)
                    return (false, 0);

                Guid faceId1 = cccdFaces[0].FaceId.Value;
                Guid faceId2 = selfieFaces[0].FaceId.Value;

                // Verify
                Response<FaceVerificationResult> verifyResponse = await _faceClient.VerifyFaceToFaceAsync(faceId1, faceId2);
                FaceVerificationResult result = verifyResponse.Value;

                return (result.IsIdentical, result.Confidence);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FaceAPIService] Lỗi xác minh khuôn mặt: {ex.Message}");
                return (false, 0);
            }
        }
    }
}
