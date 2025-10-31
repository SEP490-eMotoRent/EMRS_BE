using EMRS.Application.Common;
using EMRS.Application.DTOs.ConfigurationDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IConfigurationService
{
    Task<ResultResponse<Configuration>> CreateAsync(ConfigurationCreateRequest entity);
    Task<ResultResponse<Configuration?>> GetByIdAsync(Guid id);
    Task<ResultResponse<List<Configuration>>> GetAllAsync();
    Task<ResultResponse<Configuration>> UpdateAsync(Configuration entity);
    Task<ResultResponse<object>> DeleteAsync(Guid id);
    Task<ResultResponse<string>> RemoveFaceSet(string facesettoken);
    Task<ResultResponse<string>> CreateFaceSet();

}
