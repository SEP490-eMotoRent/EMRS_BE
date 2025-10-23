using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    IEnumerable<string> Roles { get; }
}
