using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationCode, int minutesToExpire);
}
