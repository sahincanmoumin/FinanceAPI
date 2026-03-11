using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EntityLayer.Entities.Domain;

namespace DataAccessLayer.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.TaxNumber).HasMaxLength(20);
            builder.Property(x => x.TaxOffice).HasMaxLength(50);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Companies)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CurrentAccountConfiguration : IEntityTypeConfiguration<CurrentAccount>
    {
        public void Configure(EntityTypeBuilder<CurrentAccount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Company)
                   .WithMany(x => x.CurrentAccounts)
                   .HasForeignKey(x => x.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Company)
                   .WithMany(x => x.Stocks)
                   .HasForeignKey(x => x.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SerialNumber).IsRequired().HasMaxLength(20);

            builder.Property(x => x.Type).IsRequired();

            builder.HasOne(x => x.Company)
                   .WithMany(x => x.Invoices)
                   .HasForeignKey(x => x.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CurrentAccount)
                   .WithMany(x => x.Invoices)
                   .HasForeignKey(x => x.CurrentAccountId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Invoice)
                   .WithMany(x => x.InvoiceDetails)
                   .HasForeignKey(x => x.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Stock)
                   .WithMany(x => x.InvoiceDetails)
                   .HasForeignKey(x => x.StockId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class StockTransConfiguration : IEntityTypeConfiguration<StockTrans>
    {
        public void Configure(EntityTypeBuilder<StockTrans> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Stock)
                   .WithMany(x => x.StockTransactions)
                   .HasForeignKey(x => x.StockId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}