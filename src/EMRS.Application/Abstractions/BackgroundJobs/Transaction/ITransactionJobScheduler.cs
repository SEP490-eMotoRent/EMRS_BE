using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions.BackgroundJobs.Transaction
{
    public interface ITransactionJobScheduler
    {
        void ScheduleAutoCancel(Guid transactionId, TimeSpan delay);
    }
}
