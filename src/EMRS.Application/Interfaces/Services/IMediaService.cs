using EMRS.Application.Common;
using EMRS.Application.DTOs.MediaDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IMediaService
{
    Task<ResultResponse<MediaResponse>> UpdateSingleMediaAsync(MediaUpdateRequest updateRequest);
    Task<ResultResponse<MediaResponse>> AddMediaAsync(AddMediaRequest request);


}
