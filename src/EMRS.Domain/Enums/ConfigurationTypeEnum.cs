using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Enums;

public enum ConfigurationTypeEnum
{
/*    FacePlusPlus=1,*/
    RentingDurationRate = 2,
    OffPeakChargingPrice = 11,
    NormalChargingPrice = 12,
    PeakChargingPrice = 13,
    DepositRate = 5,
    AdditionalFee=6,
    RefundRate= 7,
    EconomyDepositPrice=8,
    StandardDepositPrice=9,
    PremiumDepositPrice= 10,
    RentalContractTemplate = 17,
    LateReturnPrice=18,
    CleaningPrice=19,
    DamagePrice=20,
    CrossBranchPrice=21,
    EconomyExcessKmPrice=22,
    StandardExcessKmPrice=23,
    PreniumExcessKmPrice=24,
    EconomyExcessKmLimit=25,
    StandardExcessKmLimit=26,
    PreniumExcessKmLimit=27
}
