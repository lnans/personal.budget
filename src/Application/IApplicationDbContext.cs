using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Operation> Operations { get; }
    DbSet<OperationTag> OperationTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}