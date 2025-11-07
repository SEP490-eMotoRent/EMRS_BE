using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IProtrackService
{
    Task<ProtrackResponse?> LoginVehicleAsync(Vehicle vehicle);
    string EncryptPassword(string password);
}
