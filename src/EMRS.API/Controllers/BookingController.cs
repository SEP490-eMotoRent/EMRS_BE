using EMRS.Application.Abstractions;
using EMRS.Application.Abstractions.Models.VNPay;
using EMRS.Application.Abstractions.Models.ZaloPay;
using EMRS.Application.DTOs.BookingDTOs;
using EMRS.Application.DTOs.BranchDTOs;
using EMRS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {

        private readonly IBookingService _bookingService;
        private readonly IVNPayService _vNPayService;
        public BookingController(IVNPayService vNPayService,IBookingService bookingService)
        {
            _bookingService = bookingService;
            _vNPayService = vNPayService;
        }
        [Authorize(Roles ="RENTER")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BookingCreateRequest request)
        {

            var result = await _bookingService.CreateBooking(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "RENTER")]
        [HttpPost("vnpay")]
        public async Task<IActionResult> CreateVnpayBooking([FromBody] BookingCreateRequest request)
        {

            var result = await _bookingService.CreateBookingWithoutWallet(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPut("vnpay/callback")]
        public async Task<IActionResult> VnPayCallback([FromBody] VNPayResponseData vNPayResponseData)
        {
            var result = await _bookingService.ProcessCallBack(vNPayResponseData);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {

                return BadRequest(result.Message);
            }
        }
        [Authorize(Roles = "RENTER")]
        [HttpPost("zalopay")]
        public async Task<IActionResult> CreateZalopayBooking([FromBody] BookingCreateRequest request)
        {

            var result = await _bookingService.CreateBookingWithoutWalletZalo(request);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [HttpPost("zalopay/callback")]
        public async Task<IActionResult> ZaloPayCallback([FromBody] ZaloPayCallbackResponseData zaloPayCallbackResponseData)
        {
            var result = await _bookingService.ProcessCallBackZaloPay(zaloPayCallbackResponseData);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {

                return BadRequest(result.Message);
            }
        }
        [Authorize(Roles = "RENTER")]
        [HttpGet("renter/get")]
        public async Task<IActionResult> GetAll()
        {

            var result = await _bookingService.GetAllBookingsByRenterIdAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "STAFF,ADMIN")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllBooking( Guid? BranchId, Guid? VehicleModelId, Guid? RenterId ,string? BookingStatus,DateOnly? Date,int PageNum, int PageSize )
        {
            var request = new BookingSearchRequest
            {
                BranchId = BranchId,
                VehicleModelId = VehicleModelId,
                RenterId = RenterId,
                BookingStatus = BookingStatus,
                Date= Date
            };
            var result = await _bookingService.GetAllBookings(request,PageNum,PageSize);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        /// <summary>
        /// VNPay IPN callback
        ///
        /// </summary>
       
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingDetail(Guid bookingId)
        {
        
            var result = await _bookingService.GetBookingDetailAsync(bookingId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "RENTER")]
        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {

            var result = await _bookingService.CancelBookingByCustomerAsync(bookingId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        /// <summary>
        /// Staff hủy booking
        /// </summary>
        [Authorize(Roles = "STAFF")]
        [HttpPut("staff/cancel/{bookingId}")]
        public async Task<IActionResult> CancelByStaff(Guid bookingId)
        {
            var result = await _bookingService.CancelBookingByStaffAsync(bookingId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
       
        [Authorize(Roles = "STAFF")]
        [HttpPut("vehicle/assign/{bookingId}/{vehicleId}")]
        public async Task<IActionResult> AssignVehicleForBooking(Guid bookingId,Guid vehicleId)
        {
           
            var result = await _bookingService.AssignVehicleForBookingIfBooked(bookingId,vehicleId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
        [Authorize(Roles = "STAFF")]
        [HttpPut("vehicle/{bookingId}/{vehicleId}")]
        public async Task<IActionResult> EditVehicleModelForBooking(Guid bookingId, Guid vehicleId)
        {

            var result = await _bookingService.UpdateVehicleForBooking(bookingId, vehicleId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
       /* [Authorize(Roles = "MANAGER")]*/
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetBookingByBranchID(Guid branchId,int PageNum, int PageSize, bool orderByDescending)
        {

            var result = await _bookingService.GetBookingByHandoverIdAsync(branchId,PageNum,PageSize, orderByDescending);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }


        }
    }
}
