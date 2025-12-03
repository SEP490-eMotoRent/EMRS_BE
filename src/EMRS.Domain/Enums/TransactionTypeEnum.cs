using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Enums;
public enum TransactionTypeEnum
{

    // Booking transactions
    BookingDeposit = 11,
    BookingRefund = 13,
    BookingAdditionalPayment = 14,

    // Wallet transactions
    WalletTopUp = 21,
    WalletWithdraw = 22,

    // Insurance Claim transactions
    InsuranceClaimPayment = 31,
    InsuranceClaimRefund = 32,

    BookingReturnRefund = 41,                      
    
}
