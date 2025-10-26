using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IFaceAPIService
{
    Task<(bool IsIdentical, double Confidence)> VerifyFaceAsync(string cccdUrl, string selfieUrl);
}

