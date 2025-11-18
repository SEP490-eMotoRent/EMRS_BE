using EMRS.Application.Common;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.Persistence.Repositories
{
    public class TicketRepository:GenericRepository<Ticket>, ITicketRepository
    {
        private readonly EMRSDbContext _context;
        public TicketRepository(EMRSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginationResult<List<Ticket>>> GetAllTicketsAsync(
            int pageSize, int pageNum, bool orderByDescending = true)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query();

           
            query = orderByDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<Ticket>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = items,
                TotalItems = totalCount
            };
        }
        public async Task<PaginationResult<List<Ticket>>> GetAllTicketsByStaffIdAsync(Guid StaffId,
         int pageSize, int pageNum, bool orderByDescending = true)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query().Where(a=>a.StaffId== StaffId);


            query = orderByDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<Ticket>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = items,
                TotalItems = totalCount
            };
        }
        public async Task<PaginationResult<List<Ticket>>> GetAllTicketsByBookingIdAsync(Guid BookingId,
        int pageSize, int pageNum, bool orderByDescending = true)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query().Where(a => a.BookingId == BookingId);


            query = orderByDescending
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<Ticket>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = items,
                TotalItems = totalCount
            };
        }
    }
}
