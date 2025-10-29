using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.InsurancePackageDTOs;

public class InsurancePackageUpdateRequest
{
    public string PackageName { get; set; }
    public decimal PackageFee { get; set; }
    public decimal CoveragePersonLimit { get; set; }
    public decimal CoveragePropertyLimit { get; set; }
    public decimal CoverageVehiclePercentage { get; set; }
    public decimal CoverageTheft { get; set; }
    public decimal DeductibleAmount { get; set; }
    public string Description { get; set; }
}
