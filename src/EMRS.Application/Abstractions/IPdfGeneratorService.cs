using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions
{
    public interface IPdfGeneratorService
    {
        public byte[] GeneratePdf(byte[] templateBytes, List<string> parameters);
    }
}
