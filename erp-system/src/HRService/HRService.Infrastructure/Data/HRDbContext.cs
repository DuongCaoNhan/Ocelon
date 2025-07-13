using Microsoft.EntityFrameworkCore;
using HRService.Domain.Entities;

namespace HRService.Infrastructure.Data;

public class HRDbContext : DbContext
{
    public HRDbContext(DbContextOptions<HRDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Position).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");

            // Owned types for Address and EmergencyContact
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.State).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
                address.Property(a => a.Country).HasMaxLength(100);
            });

            entity.OwnsOne(e => e.EmergencyContact, contact =>
            {
                contact.Property(c => c.Name).HasMaxLength(200);
                contact.Property(c => c.Relationship).HasMaxLength(50);
                contact.Property(c => c.PhoneNumber).HasMaxLength(20);
                contact.Property(c => c.Email).HasMaxLength(255);
            });

            // Navigation properties
            entity.HasMany(e => e.PayrollRecords)
                  .WithOne(p => p.Employee)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.LeaveRequests)
                  .WithOne(l => l.Employee)
                  .HasForeignKey(l => l.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PayrollRecord configuration
        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.GrossPay).HasColumnType("decimal(18,2)");
            entity.Property(p => p.NetPay).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TaxWithheld).HasColumnType("decimal(18,2)");
            entity.Property(p => p.OtherDeductions).HasColumnType("decimal(18,2)");
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Reason).HasMaxLength(500);
            entity.Property(l => l.ApproverComments).HasMaxLength(500);
        });

        // Global query filter for soft delete
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PayrollRecord>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(l => !l.IsDeleted);
    }
}
