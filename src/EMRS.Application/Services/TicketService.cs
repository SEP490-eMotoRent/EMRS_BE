using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.MediaDTOs;
using EMRS.Application.DTOs.RentalReceiptDTOs;
using EMRS.Application.DTOs.TicketDTOs;
using EMRS.Application.DTOs.VehicleDTOs;
using EMRS.Application.Helper;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class TicketService:ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        public TicketService(ICloudinaryService cloudinaryService,IUnitOfWork unitOfWork) 
        {
            _cloudinaryService = cloudinaryService;
            _unitOfWork = unitOfWork;
        }  
        public async Task<ResultResponse<TicketDetailResponse>> CreateTicketAsync(TicketCreateRequest ticketCreateRequest)
        {
            try
            {
                var ticket = new Ticket
                {
                    Title = ticketCreateRequest.Title,
                    Description = ticketCreateRequest.Description,
                    TicketType = ticketCreateRequest.TicketType.ToString(),
                    BookingId = ticketCreateRequest.BookingId,
                    
                    Status = TicketStatusEnum.Pending.ToString(),
                };
                List<Media> savedMedias = new List<Media>();
                if (ticketCreateRequest.Attachments != null && ticketCreateRequest.Attachments.Count > 0)
                {
                var uploadTasks = ticketCreateRequest.Attachments.Select(async file =>
                {
                string contentType = file.ContentType.ToLower();

                string prefix = contentType switch
                {
                    var x when x.StartsWith("image/") => "img",
                    var x when x.StartsWith("video/") => "media",
                    var x when x.StartsWith("application/") => "doc",
                    _ => "media"
                };

                string mediaType = contentType switch
                {
                    var x when x.StartsWith("image/") => MediaTypeEnum.Image.ToString(),
                    var x when x.StartsWith("video/") => MediaTypeEnum.Video.ToString(),
                    var x when x.Contains("pdf") || x.Contains("msword") || x.Contains("spreadsheet") => MediaTypeEnum.Document.ToString(),
                    _ => MediaTypeEnum.Document.ToString()
                };

                string filename =  $"{prefix}_{Generator.PublicIdGenerate()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

                string url = await _cloudinaryService.UploadImageFileAsync(
                    file,
                    filename,
                    "Ticket"
                );

                return new Media
                {
                    EntityType = MediaEntityTypeEnum.Ticket.ToString(),
                    FileUrl = url,
                    DocNo = ticket.Id,
                    MediaType = mediaType
                };
                }).ToList();

                var medias = (await Task.WhenAll(uploadTasks)).ToList();
                await _unitOfWork.GetMediaRepository().AddRangeAsync(medias);
                    savedMedias = medias;
                }
             
                await _unitOfWork.GetTicketRepository().AddAsync(ticket);
                await _unitOfWork.SaveChangesAsync();
                var attachmentsResponse = savedMedias.Any()
          ? savedMedias.Select(m => new MediaResponse
          {
              EntityType = m.EntityType,
              DocNo = m.DocNo,
              FileUrl = m.FileUrl,
              MediaType = m.MediaType
          }).ToList()
          : null;
                return ResultResponse<TicketDetailResponse>.SuccessResult("Ticket created successfully", new TicketDetailResponse
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,
                    BookingId = ticket.BookingId,
                    StaffId = ticket.StaffId,
                    TicketType = ticket.TicketType,  
                    Attachments= attachmentsResponse
                });
            }
            catch (Exception ex)
            {
                return ResultResponse<TicketDetailResponse>.Failure(
               $"An error occurred while creating : {ex.Message}"
           );
            }
        }
        public async Task<ResultResponse<TicketResponse>> UpdateTicketAsync(TicketUpdateRequest ticketUpdateRequest)
        {
            try
            {
               var ticket = await _unitOfWork.GetTicketRepository().FindByIdAsync(ticketUpdateRequest.Id);
                if(ticket == null)
                {
                    return ResultResponse<TicketResponse>.NotFound("Ticket not found");
                }
                ticket.Status = ticketUpdateRequest.Status.ToString();
                if (ticketUpdateRequest.StaffId != null)
                {
                    ticket.StaffId = ticketUpdateRequest.StaffId;
                }
                 _unitOfWork.GetTicketRepository().Update(ticket);
                await _unitOfWork.SaveChangesAsync();
                return ResultResponse<TicketResponse>.SuccessResult("Ticket updated successfully", new TicketResponse
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,
                    BookingId = ticket.BookingId,
                    StaffId = ticket.StaffId,
                    TicketType = ticket.TicketType
                });
            }
            catch (Exception ex)
            {
                return ResultResponse<TicketResponse>.Failure(
               $"An error occurred while creating : {ex.Message}"
           );
            }
        }

        public async Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketAsync(
     int pageSize, int pageNum, bool orderByDescending = true)
        {
            try
            {
                var pagedTickets = await _unitOfWork.GetTicketRepository()
                    .GetAllTicketsAsync(pageSize, pageNum, orderByDescending);

                if (pagedTickets == null || pagedTickets.Items == null)
                {
                    return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                        "No tickets found.");
                }

                var mappedItems = pagedTickets.Items.Select(t => new TicketResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    BookingId = t.BookingId,
                    StaffId = t.StaffId,
                    TicketType = t.TicketType
                }).ToList();

                var result = new PaginationResult<List<TicketResponse>>
                {
                    CurrentPage = pagedTickets.CurrentPage,
                    PageSize = pagedTickets.PageSize,
                    TotalPages = pagedTickets.TotalPages,
                    TotalItems = pagedTickets.TotalItems,
                    Items = mappedItems
                };

                return ResultResponse<PaginationResult<List<TicketResponse>>>.SuccessResult(
                    "Get all tickets successfully", result);
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                    $"An error occurred while getting tickets: {ex.Message}");
            }
        }
        public async Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketByStaffIdAsync(
    Guid StaffId, int pageSize, int pageNum, bool orderByDescending = true)
        {
            try
            {
                var pagedTickets = await _unitOfWork.GetTicketRepository()
                    .GetAllTicketsByStaffIdAsync(StaffId,pageSize, pageNum, orderByDescending);

                if (pagedTickets == null || pagedTickets.Items == null)
                {
                    return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                        "No tickets found.");
                }

                var mappedItems = pagedTickets.Items.Select(t => new TicketResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    BookingId = t.BookingId,
                    StaffId = t.StaffId,
                    TicketType = t.TicketType
                }).ToList();

                var result = new PaginationResult<List<TicketResponse>>
                {
                    CurrentPage = pagedTickets.CurrentPage,
                    PageSize = pagedTickets.PageSize,
                    TotalPages = pagedTickets.TotalPages,
                    TotalItems = pagedTickets.TotalItems,
                    Items = mappedItems
                };

                return ResultResponse<PaginationResult<List<TicketResponse>>>.SuccessResult(
                    "Get all tickets successfully", result);
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                    $"An error occurred while getting tickets: {ex.Message}");
            }
        }
        public async Task<ResultResponse<PaginationResult<List<TicketResponse>>>> GetAllTicketByBookingIdAsync(
   Guid BookingId, int pageSize, int pageNum, bool orderByDescending = true)
        {
            try
            {
                var pagedTickets = await _unitOfWork.GetTicketRepository()
                    .GetAllTicketsByBookingIdAsync(BookingId, pageSize, pageNum, orderByDescending);

                if (pagedTickets == null || pagedTickets.Items == null)
                {
                    return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                        "No tickets found.");
                }

                var mappedItems = pagedTickets.Items.Select(t => new TicketResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    BookingId = t.BookingId,
                    StaffId = t.StaffId,
                    TicketType = t.TicketType
                }).ToList();

                var result = new PaginationResult<List<TicketResponse>>
                {
                    CurrentPage = pagedTickets.CurrentPage,
                    PageSize = pagedTickets.PageSize,
                    TotalPages = pagedTickets.TotalPages,
                    TotalItems = pagedTickets.TotalItems,
                    Items = mappedItems
                };

                return ResultResponse<PaginationResult<List<TicketResponse>>>.SuccessResult(
                    "Get all tickets successfully", result);
            }
            catch (Exception ex)
            {
                return ResultResponse<PaginationResult<List<TicketResponse>>>.Failure(
                    $"An error occurred while getting tickets: {ex.Message}");
            }
        }
        public async Task<ResultResponse<TicketDetailResponse>> GetTicketByIdAsync(Guid ticketId)
        {
            try
            {
                var ticketEntity = await _unitOfWork.GetTicketRepository()
                    .FindByIdAsync(ticketId);

                if (ticketEntity == null)
                {
                    return ResultResponse<TicketDetailResponse>.Failure("Ticket not found");
                }

                var medias = await _unitOfWork.GetMediaRepository()
                    .GetMediasByEntityIdAsync(ticketEntity.Id);

                var mappedMedias = medias != null
                    ? medias.Select(m => new MediaResponse
                    {
                        EntityType = m.EntityType,
                        DocNo = m.DocNo,
                        FileUrl = m.FileUrl,
                        MediaType = m.MediaType
                    }).ToList()
                    : null;

                var response = new TicketDetailResponse
                {
                    Id = ticketEntity.Id,
                    Title = ticketEntity.Title,
                    Description = ticketEntity.Description,
                    Status = ticketEntity.Status,
                    CreatedAt = ticketEntity.CreatedAt,
                    BookingId = ticketEntity.BookingId,
                    StaffId = ticketEntity.StaffId,
                    TicketType = ticketEntity.TicketType,
                    Attachments = mappedMedias
                };

                return ResultResponse<TicketDetailResponse>.SuccessResult(
                    "Get ticket successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<TicketDetailResponse>.Failure(
                    $"An error occurred while getting ticket: {ex.Message}");
            }
        }

    }
}
