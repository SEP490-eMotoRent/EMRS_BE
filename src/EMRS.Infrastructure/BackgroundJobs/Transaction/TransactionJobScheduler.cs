using EMRS.Application.Abstractions.BackgroundJobs.Transaction;
using EMRS.Application.Interfaces.Services;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TransactionJobScheduler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void ScheduleAutoCancel(Guid transactionId, TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                using var scope = _serviceScopeFactory.CreateScope();
                var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
                await walletService.AutoCancelTopUpRequestAsync(transactionId);
            });
        }
    }
}
