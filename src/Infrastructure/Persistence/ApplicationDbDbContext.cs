using Microsoft.EntityFrameworkCore;
using Application;
using Domain.Entities;

namespace Infrastructure.Persistence;

public class ApplicationDbDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbDbContext(DbContextOptions<ApplicationDbDbContext> options) : base(options)
    {
    }

    public DbSet<Constant> Constants => Set<Constant>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationTag> OperationTags => Set<OperationTag>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportConstant> ReportConstants => Set<ReportConstant>();
    public DbSet<User> Users => Set<User>();
}