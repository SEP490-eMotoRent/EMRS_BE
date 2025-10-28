using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services;

public class InsuranceService:IInsuranceService

{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;
    public InsuranceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
}
