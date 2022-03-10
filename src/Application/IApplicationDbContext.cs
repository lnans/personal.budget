using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application;

public interface IApplicationDbContext
{
    DbSet<Constant> Constants { get; }
    DbSet<Operation> Operations { get; }
    DbSet<OperationTag> OperationTags { get; }
    DbSet<Report> Reports { get; }
    DbSet<ReportConstant> ReportConstants { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}