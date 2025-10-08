using Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
}
