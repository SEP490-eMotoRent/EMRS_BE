// EMRS.API/Swagger/FileUploadOperationFilter.cs
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EMRS.API.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formFileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.ModelMetadata != null &&
                       (p.ModelMetadata.ModelType == typeof(IFormFile) ||
                        p.ModelMetadata.ModelType == typeof(IEnumerable<IFormFile>) ||
                        p.ModelMetadata.ModelType == typeof(List<IFormFile>)))
            .ToList();

        if (!formFileParams.Any())
            return;

        // Tạo schema cho multipart/form-data
        var uploadFileSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new HashSet<string>()
        };

        foreach (var param in context.ApiDescription.ParameterDescriptions)
        {
            if (param.ModelMetadata == null)
                continue;

            var paramType = param.ModelMetadata.ModelType;

            if (paramType == typeof(IFormFile))
            {
                uploadFileSchema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else if (paramType == typeof(IEnumerable<IFormFile>) ||
                     paramType == typeof(List<IFormFile>))
            {
                uploadFileSchema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
            else
            {
                // Các property khác (string, int, Guid, etc.)
                uploadFileSchema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = GetOpenApiType(paramType)
                };
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = uploadFileSchema
                }
            }
        };

        // Xóa tất cả parameters cũ
        operation.Parameters.Clear();
    }

    private static string GetOpenApiType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long)) return "integer";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(Guid)) return "string";
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset)) return "string";

        return "string"; // Default
    }
}