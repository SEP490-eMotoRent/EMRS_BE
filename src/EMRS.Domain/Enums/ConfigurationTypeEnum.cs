using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Enums;

public enum ConfigurationTypeEnum
{
    FacePlusPlus=1,
    RentingDurationRate = 2,
    ChargingRate = 3,
    BookingDiscountRate = 4,
    DepositRate = 5,
    AdditionalFee=6,
    RefundRate= 7,
    EconomyDepositPrice=8,
    StandardDepositPrice=9,
    PremiumDepositPrice= 10,
    // Additional Fee: "DAMAGE", "CLEANING", "LATE_RETURN", "CROSS_BRANCH", "EXCESS_KM". "EARLY_HANDOVER"
    LateReturnFee = 11,
    CleaningFee = 12,
    DamageFee = 13,
    CrossBranchFee = 14,
    ExcessKmFee = 15,
    EarlyHandoverFee = 16
}
