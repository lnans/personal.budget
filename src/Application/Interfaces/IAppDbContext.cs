using Domain.AccountOperations;
using Domain.Accounts;
using Domain.Tags;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<AccountOperation> AccountOperations { get; }
    DbSet<Tag> Tags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
