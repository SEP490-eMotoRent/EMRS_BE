using EMRS.Application.Abstractions.Models.ZaloPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions
{
    public interface IZaloPayService
    {
        Task<ZaloPayResponse> CreatePaymentURL(OrderData orderData);
    }
}
