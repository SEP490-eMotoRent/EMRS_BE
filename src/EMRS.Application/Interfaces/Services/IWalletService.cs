using EMRS.Application.Common;
using EMRS.Application.DTOs.WalletDTOs;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services;

public interface IWalletService
{
    Task<bool> TransferMoneyAsync(Wallet fromWallet, Wallet toWallet, decimal amount);
    Task<ResultResponse<WalletResponse>> CreateWalletAsync();

}
