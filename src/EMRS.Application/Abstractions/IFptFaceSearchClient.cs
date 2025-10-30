using EMRS.Application.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IFptFaceSearchClient
{
    Task<bool> CreateUserAsync(string collection, string id, string name, CancellationToken ct = default);
    Task<bool> AddImageAsync(string collection, string id, Stream imageStream, string filename, CancellationToken ct = default);
    Task<FaceSearchResult?> SearchAsync(string collection, Stream imageStream, string filename, double? threshold = null, CancellationToken ct = default);
}
