using EMRS.Application.Abstractions.BackgroundJobs.Transaction;
using EMRS.Application.Interfaces.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure.BackgroundJobs.Transaction
{
    public class TransactionJobScheduler : ITransactionJobScheduler
    {
        public void ScheduleAutoCancel(Guid transactionId, TimeSpan delay)
        {
            BackgroundJob.Schedule<TransactionBackgroundJob>(
                job => job.AutoCancelPendingTransaction(transactionId),
                delay
            );

            Console.WriteLine($"[Hangfire] Scheduled auto-cancel for Transaction {transactionId} after {delay.TotalMinutes} minutes.");
        }
    }
}
