using EMRS.Application.Abstractions.Models.Protrack;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

    public interface IFlespiService
    {
    Task<TempTrackingPayload> CreateFlespiAclTokenAsync(Vehicle vehicle, int ttlSeconds = 300);
}
