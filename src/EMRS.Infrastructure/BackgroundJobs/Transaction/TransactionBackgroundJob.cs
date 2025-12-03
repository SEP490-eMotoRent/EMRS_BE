using EMRS.Application.Abstractions;
using EMRS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.Transaction
{
    public class TransactionBackgroundJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBackgroundJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AutoCancelPendingTransaction(Guid transactionId)
        {
            try
            {
                var transaction = await _unitOfWork.GetTransactionRepository()
                    .FindByIdAsync(transactionId);

                if (transaction == null)
                {
                    Console.WriteLine($"[Hangfire] Transaction {transactionId} not found");
                    return;
                }

                // Chỉ cancel nếu còn Pending
                if (transaction.Status == TransactionStatusEnum.Pending.ToString())
                {
                    transaction.Status = TransactionStatusEnum.Failed.ToString();
                    _unitOfWork.GetTransactionRepository().Update(transaction);
                    await _unitOfWork.SaveChangesAsync();

                    Console.WriteLine($"[Hangfire] Transaction {transactionId} auto-cancelled successfully due to timeout (15 minutes)");
                }
                else
                {
                    Console.WriteLine($"[Hangfire] Transaction {transactionId} already processed (Status: {transaction.Status}), skipping");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire] Error auto-cancelling Transaction {transactionId}: {ex.Message}");
            }
        }
    }
}
