using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.MembershipDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Services
{
    public class MembershipService: IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MembershipService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        private MembershipResponse MapToResponse(Membership membership)
        {
            return new MembershipResponse
            {
                Id = membership.Id,
                TierName = membership.TierName,
                MinBookings = membership.MinBookings,
                DiscountPercentage = membership.DiscountPercentage,
                Description = membership.Description,
                CreatedAt = membership.CreatedAt,
                UpdatedAt = membership.UpdatedAt
            };
        }
        public async Task<ResultResponse<MembershipResponse>> UpdateAsync(UpdateMembershipRequest request)
        {
            if (request.Id == Guid.Empty)
                return ResultResponse<MembershipResponse>.Failure("ID không hợp lệ");

          
            try
            {
                var membership = await _unitOfWork.GetMembershipRepository().FindByIdAsync(request.Id);

                if (membership == null||membership.IsDeleted)
                    return ResultResponse<MembershipResponse>.NotFound("Không tìm thấy gói thành viên");

                membership.TierName = request.TierName;
                membership.Description = request.Description;
                membership.MinBookings = request.MinBookings;
                membership.DiscountPercentage = request.DiscountPercentage;

                _unitOfWork.GetMembershipRepository().Update(membership);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToResponse(membership);
                return ResultResponse<MembershipResponse>.SuccessResult("Cập nhật thành công", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<MembershipResponse>.Failure($"Lỗi khi cập nhật: {ex.Message}");
            }
        }
        public async Task<ResultResponse<Membership>> CreateMembership(CreateMembershipRequest createMembershipRequest)
        {

            var newMembership = new Membership
            {
                TierName = createMembershipRequest.TierName,
                Description = createMembershipRequest.Description,
                DiscountPercentage = createMembershipRequest.DiscountPercentage,
                MinBookings = createMembershipRequest.MinBookings
            };
            await _unitOfWork.GetMembershipRepository().AddAsync(newMembership);
            await _unitOfWork.SaveChangesAsync();
            return ResultResponse<Membership>.SuccessResult("Membership created", newMembership);


        }
      
        public async Task<ResultResponse<MembershipResponse>> GetMembershipByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return ResultResponse<MembershipResponse>.Failure("Invalid ID");

                var membership = await _unitOfWork.GetMembershipRepository().FindByIdAsync(id);

                if (membership == null||membership.IsDeleted)
                    return ResultResponse<MembershipResponse>.NotFound("Membership not found");

                var response = MapToResponse(membership);
                return ResultResponse<MembershipResponse>.SuccessResult("Membership found", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<MembershipResponse>.Failure($"Error retrieving membership: {ex.Message}");
            }
        }
        public async Task<ResultResponse<bool>> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                return ResultResponse<bool>.Failure("ID không hợp lệ");

            try
            {
                var membership = await _unitOfWork.GetMembershipRepository().FindByIdAsync(id);

                if (membership == null || membership.IsDeleted)
                    return ResultResponse<bool>.NotFound("Không tìm thấy gói thành viên");

                if (membership.Renters?.Any(r => !r.IsDeleted) == true)
                    return ResultResponse<bool>.Failure("Không thể xóa gói thành viên đang có người thuê");
                membership.IsDeleted = true;
                _unitOfWork.GetMembershipRepository().Update(membership);
                await _unitOfWork.SaveChangesAsync();

                return ResultResponse<bool>.SuccessResult("Xóa thành công", true);
            }
            catch (Exception ex)
            {
                return ResultResponse<bool>.Failure($"Lỗi khi xóa: {ex.Message}");
            }
        }
        public async Task<ResultResponse<List<MembershipResponse>>> GetAllMembershipsAsync()
        {
            try
            {
                var memberships = await _unitOfWork.GetMembershipRepository().GetAllAsync();

                if (memberships == null || !memberships.Any())
                    return ResultResponse<List<MembershipResponse>>.SuccessResult("No memberships found", new List<MembershipResponse>());

                var response = memberships.Select(MapToResponse).ToList();
                return ResultResponse<List<MembershipResponse>>.SuccessResult("Memberships retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return ResultResponse<List<MembershipResponse>>.Failure($"Error retrieving memberships: {ex.Message}");
            }
        }
    }
}
