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
    public class RepairRequestRepository:GenericRepository<RepairRequest>, IRepairRequestRepository
    {
        private readonly EMRSDbContext _context;
        public RepairRequestRepository(EMRSDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<PaginationResult<List<RepairRequest>>> GetAllPaginatedAsync(
    int pageSize, int pageNum, bool orderByDesc)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query();

            query = orderByDesc
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<RepairRequest>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalCount,
                Items = items
            };
        }
        public async Task<PaginationResult<List<RepairRequest>>> GetByTechnicianIdPaginatedAsync(
    Guid technicianId, int pageSize, int pageNum, bool orderByDesc)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query().Where(r => r.TechnicianId == technicianId);

            query = orderByDesc
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<RepairRequest>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalCount,
                Items = items
            };
        }

        public async Task<PaginationResult<List<RepairRequest>>> GetByBranchIdPaginatedAsync(
    Guid branchId, int pageSize, int pageNum, bool orderByDesc)
        {
            if (pageNum <= 0) pageNum = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = Query()
                .Where(r => r.Vehicle.BranchId == branchId); 

            query = orderByDesc
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var items = await query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<List<RepairRequest>>
            {
                CurrentPage = pageNum,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalCount,
                Items = items
            };
        }

    }
}
