using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.DTOs.WalletDTOs;

public class WalletResponse
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }

    public Guid RenterId { get; set; }

}
