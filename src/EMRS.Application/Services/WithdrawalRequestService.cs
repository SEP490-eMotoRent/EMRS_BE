// EMRS.Application/Services/WithdrawalRequestService.cs
using AutoMapper;
using EMRS.Application.Abstractions;
using EMRS.Application.Common;
using EMRS.Application.DTOs.WithdrawalRequestDTOs;
using EMRS.Application.Interfaces.Services;
using EMRS.Domain.Entities;
using EMRS.Domain.Enums;

namespace EMRS.Application.Services;

public class WithdrawalRequestService : IWithdrawalRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public WithdrawalRequestService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    // ==================== RENTER ACTIONS ====================

    public async Task<ResultResponse<WithdrawalRequestResponse>> CreateWithdrawalRequest(
        WithdrawalRequestCreateRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get current user
            var userId = Guid.Parse(_currentUserService.UserId);

            // Get renter's wallet
            var wallet = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdAsync(userId);
            if (wallet == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Wallet not found");

            // Validate amount
            if (request.Amount <= 0)
                return ResultResponse<WithdrawalRequestResponse>.Failure("Amount must be greater than 0");

            // Check sufficient balance
            if (wallet.Balance < request.Amount)
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    $"Insufficient balance. Current balance: {wallet.Balance:N0} VND");

            // Create withdrawal request
            var withdrawalRequest = new WithdrawalRequest
            {
                Amount = request.Amount,
                BankName = request.BankName,
                BankAccountNumber = request.BankAccountNumber,
                BankAccountName = request.BankAccountName,
                Status = WithdrawalRequestStatusEnum.Pending.ToString(),
                WalletId = wallet.Id
            };

            await _unitOfWork.GetWithdrawalRequestRepository().AddAsync(withdrawalRequest);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<WithdrawalRequestResponse>(withdrawalRequest);
            return ResultResponse<WithdrawalRequestResponse>.SuccessResult(
                "Withdrawal request created successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WithdrawalRequestResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<List<WithdrawalRequestResponse>>> GetMyWithdrawalRequests()
    {
        try
        {
            // Get current user
            var userId = Guid.Parse(_currentUserService.UserId);

            // Get renter's wallet
            var wallet = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdAsync(userId);
            if (wallet == null)
                return ResultResponse<List<WithdrawalRequestResponse>>.NotFound("Wallet not found");

            // Get all withdrawal requests of this wallet
            var requests = await _unitOfWork.GetWithdrawalRequestRepository()
                .GetWithdrawalRequestsByWalletIdAsync(wallet.Id);

            var response = _mapper.Map<List<WithdrawalRequestResponse>>(requests);
            return ResultResponse<List<WithdrawalRequestResponse>>.SuccessResult(
                "Withdrawal requests retrieved successfully", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<List<WithdrawalRequestResponse>>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WithdrawalRequestDetailResponse>> GetWithdrawalRequestDetail(Guid id)
    {
        try
        {
            var request = await _unitOfWork.GetWithdrawalRequestRepository()
                .GetWithdrawalRequestWithDetailsAsync(id);

            if (request == null)
                return ResultResponse<WithdrawalRequestDetailResponse>.NotFound("Withdrawal request not found");

            // Verify ownership (Renter can only see their own requests, Admin can see all)
            var userId = Guid.Parse(_currentUserService.UserId);
            var userRole = _currentUserService.Roles;

            if (userRole.Equals("RENTER"))
            {
                var wallet = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdAsync(userId);
                if (wallet == null || request.WalletId != wallet.Id)
                    return ResultResponse<WithdrawalRequestDetailResponse>.Forbidden(
                        "You can only view your own withdrawal requests");
            }

            var response = _mapper.Map<WithdrawalRequestDetailResponse>(request);
            return ResultResponse<WithdrawalRequestDetailResponse>.SuccessResult(
                "Withdrawal request retrieved successfully", response);
        }
        catch (Exception ex)
        {
            return ResultResponse<WithdrawalRequestDetailResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WithdrawalRequestResponse>> CancelWithdrawalRequest(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get withdrawal request
            var request = await _unitOfWork.GetWithdrawalRequestRepository().FindByIdAsync(id);
            if (request == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Withdrawal request not found");

            // Verify ownership
            var userId = Guid.Parse(_currentUserService.UserId);
            var wallet = await _unitOfWork.GetWalletRepository().GetWalletByRenterIdAsync(userId);
            if (wallet == null || request.WalletId != wallet.Id)
                return ResultResponse<WithdrawalRequestResponse>.Forbidden(
                    "You can only cancel your own withdrawal requests");

            // Check status (can only cancel Pending requests)
            if (request.Status != WithdrawalRequestStatusEnum.Pending.ToString())
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    "Only pending requests can be canceled");

            // Update status
            request.Status = WithdrawalRequestStatusEnum.Canceled.ToString();
            _unitOfWork.GetWithdrawalRequestRepository().Update(request);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<WithdrawalRequestResponse>(request);
            return ResultResponse<WithdrawalRequestResponse>.SuccessResult(
                "Withdrawal request canceled successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WithdrawalRequestResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    // ==================== ADMIN ACTIONS (CHANGED FROM MANAGER) ====================

    public async Task<ResultResponse<PaginationResult<List<WithdrawalRequestDetailResponse>>>> GetAllWithdrawalRequests(
        WithdrawalRequestSearchRequest searchRequest, int pageNum, int pageSize)
    {
        try
        {
            var result = await _unitOfWork.GetWithdrawalRequestRepository()
                .GetWithdrawalRequestsWithFilter(searchRequest, pageSize, pageNum);

            var responseItems = _mapper.Map<List<WithdrawalRequestDetailResponse>>(result.Items);
            var paginationResponse = new PaginationResult<List<WithdrawalRequestDetailResponse>>
            {
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                Items = responseItems
            };

            return ResultResponse<PaginationResult<List<WithdrawalRequestDetailResponse>>>.SuccessResult(
                "Withdrawal requests retrieved successfully", paginationResponse);
        }
        catch (Exception ex)
        {
            return ResultResponse<PaginationResult<List<WithdrawalRequestDetailResponse>>>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WithdrawalRequestResponse>> ApproveWithdrawalRequest(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get withdrawal request with details
            var request = await _unitOfWork.GetWithdrawalRequestRepository()
                .GetWithdrawalRequestWithDetailsAsync(id);

            if (request == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Withdrawal request not found");

            // Check status
            if (request.Status != WithdrawalRequestStatusEnum.Pending.ToString())
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    "Only pending requests can be approved");

            // Get wallet
            var wallet = await _unitOfWork.GetWalletRepository().FindByIdAsync(request.WalletId);
            if (wallet == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Wallet not found");

            // Check balance again (in case balance changed)
            if (wallet.Balance < request.Amount)
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    $"Insufficient balance. Current balance: {wallet.Balance:N0} VND");

            // Deduct balance from wallet
            wallet.Balance -= request.Amount;
            _unitOfWork.GetWalletRepository().Update(wallet);

            // Create transaction record
            var transaction = new Transaction
            {
                TransactionType = TransactionTypeEnum.WalletWithdraw.ToString(),
                Amount = request.Amount,
                DocNo = request.Id,
                Status = TransactionStatusEnum.Success.ToString()
            };
            await _unitOfWork.GetTransactionRepository().AddAsync(transaction);

            // Update withdrawal request status
            request.Status = WithdrawalRequestStatusEnum.Approved.ToString();
            request.ProcessedAt = DateTime.UtcNow;
            _unitOfWork.GetWithdrawalRequestRepository().Update(request);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<WithdrawalRequestResponse>(request);
            return ResultResponse<WithdrawalRequestResponse>.SuccessResult(
                "Withdrawal request approved successfully. Please complete bank transfer.", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WithdrawalRequestResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WithdrawalRequestResponse>> RejectWithdrawalRequest(
        Guid id, string rejectionReason)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get withdrawal request
            var request = await _unitOfWork.GetWithdrawalRequestRepository().FindByIdAsync(id);
            if (request == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Withdrawal request not found");

            // Check status
            if (request.Status != WithdrawalRequestStatusEnum.Pending.ToString())
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    "Only pending requests can be rejected");

            // Validate rejection reason
            if (string.IsNullOrWhiteSpace(rejectionReason))
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    "Rejection reason is required");

            // Update withdrawal request
            request.Status = WithdrawalRequestStatusEnum.Rejected.ToString();
            request.RejectionReason = rejectionReason;
            request.ProcessedAt = DateTime.UtcNow;
            _unitOfWork.GetWithdrawalRequestRepository().Update(request);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<WithdrawalRequestResponse>(request);
            return ResultResponse<WithdrawalRequestResponse>.SuccessResult(
                "Withdrawal request rejected successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WithdrawalRequestResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }

    public async Task<ResultResponse<WithdrawalRequestResponse>> CompleteWithdrawalRequest(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get withdrawal request
            var request = await _unitOfWork.GetWithdrawalRequestRepository().FindByIdAsync(id);
            if (request == null)
                return ResultResponse<WithdrawalRequestResponse>.NotFound("Withdrawal request not found");

            // Check status (must be Approved)
            if (request.Status != WithdrawalRequestStatusEnum.Approved.ToString())
                return ResultResponse<WithdrawalRequestResponse>.Failure(
                    "Only approved requests can be completed");

            // Update status to Completed
            request.Status = WithdrawalRequestStatusEnum.Completed.ToString();
            _unitOfWork.GetWithdrawalRequestRepository().Update(request);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<WithdrawalRequestResponse>(request);
            return ResultResponse<WithdrawalRequestResponse>.SuccessResult(
                "Withdrawal request completed successfully", response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ResultResponse<WithdrawalRequestResponse>.Failure(
                $"An error occurred: {ex.Message}");
        }
    }
}