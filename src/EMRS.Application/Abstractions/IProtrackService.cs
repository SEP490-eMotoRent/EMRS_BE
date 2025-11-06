using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IProtrackService
{
    Task<string?> LoginVehicleAsync(Vehicle vehicle);
    string EncryptPassword(string password);
}
