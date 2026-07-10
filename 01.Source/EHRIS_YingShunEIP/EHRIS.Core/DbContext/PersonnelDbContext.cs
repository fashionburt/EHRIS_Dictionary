namespace EHRIS.Core.DbContext;

using EHRIS.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class PersonnelDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public PersonnelDbContext(DbContextOptions<PersonnelDbContext> options)
        : base(options)
    {
    }

    public DbSet<AcpLeave> AcpLeaves { get; set; }
    public DbSet<AcpOvertime> AcpOvertimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcpLeave>().ToTable("acp_leave");
        modelBuilder.Entity<AcpOvertime>().ToTable("acp_overtime");
    }
}