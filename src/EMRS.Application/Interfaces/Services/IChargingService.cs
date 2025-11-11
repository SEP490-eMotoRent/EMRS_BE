using EMRS.Application.Common;
using EMRS.Application.DTOs.ChargingRecordDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Interfaces.Services
{
    public interface IChargingService
    {

        Task<ResultResponse<BookingChargingSearchResponse>> SearchBookingByLicensePlate(string licensePlate);

        Task<ResultResponse<ChargingRateResponse>> GetChargingRate(ChargingRateRequest request);

        Task<ResultResponse<ChargingRecordResponse>> CreateChargingRecord(ChargingRecordCreateRequest request);

        Task<ResultResponse<List<ChargingRecordListResponse>>> GetChargingRecordsByRenter();

        Task<ResultResponse<List<ChargingRecordListResponse>>> GetChargingRecordsByBookingId(Guid bookingId);
    }
}
