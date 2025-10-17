using EMRS.Application.Abstractions;
using EMRS.Application.Interfaces.Repositories;
using EMRS.Domain.Entities;
using EMRS.Infrastructure.Persistence;
using EMRS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Infrastructure;

public class UnitOfWork :     IDisposable, IUnitOfWork
{

    private readonly EMRSDbContext _context;

    private  IAccountRepository accountRepository;

    private IMembershipRepository membershipRepository;

    public UnitOfWork(EMRSDbContext context,IMembershipRepository membershipRepository, IAccountRepository accountRepository)
    {
        _context = context;
        this.accountRepository = accountRepository;
        this.membershipRepository = membershipRepository;
    }

    public IAccountRepository GetAccountRepository() => accountRepository;

    public IMembershipRepository GetMembershipRepository() => membershipRepository;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

}