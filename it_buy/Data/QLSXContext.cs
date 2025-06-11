using Vue.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Vue.Data
{
    public class QLSXContext : DbContext
    {
        private IActionContextAccessor actionAccessor;
        private UserManager<UserModel> UserManager;
        private ItContext _context;
        public QLSXContext(DbContextOptions<QLSXContext> options, ItContext context, UserManager<UserModel> UserMgr, IActionContextAccessor ActionAccessor) : base(options)
        {
            actionAccessor = ActionAccessor;
            UserManager = UserMgr;
            _context = context;
        }

        public DbSet<KhoModel> KhoModel { get; set; }
        public DbSet<VattuNhapModel> VattuNhapModel { get; set; }
        public DbSet<XuatVattuModel> XuatVattuModel { get; set; }
        public DbSet<XuatNVLModel> XuatNVLModel { get; set; }
        public DbSet<VattuNhapChiTietModel> VattuNhapChiTietModel { get; set; }
        public DbSet<VattuDieuchuyenModel> VattuDieuchuyenModel { get; set; }
        public DbSet<VattuDieuchuyenChiTietModel> VattuDieuchuyenChiTietModel { get; set; }


        public DbSet<DTA_HOADON_NHAP> DTA_HOADON_NHAP { get; set; }
        public DbSet<DTA_CT_HOADON_NHAP> DTA_CT_HOADON_NHAP { get; set; }
        public DbSet<DTA_HOADON_XUAT> DTA_HOADON_XUAT { get; set; }
        public DbSet<DTA_CT_HOADON_XUAT> DTA_CT_HOADON_XUAT { get; set; }

        public DbSet<TBL_DANHMUCHANGHOA> TBL_DANHMUCHANGHOA { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // VattuNhap -> VattuNhapChiTiet (1-N)
            modelBuilder.Entity<VattuNhapModel>()
                .HasMany(v => v.chitiet)
                .WithOne(c => c.hoadon)
                .HasForeignKey(c => new { c.sohd })
                .HasPrincipalKey(v => new { v.sohd });

            // VattuDieuchuyen -> VattuDieuchuyenChiTiet (1-N)
            modelBuilder.Entity<VattuDieuchuyenModel>()
                .HasMany(v => v.chitiet)
                .WithOne(c => c.hoadon)
                .HasForeignKey(c => new { c.sohd })
                .HasPrincipalKey(v => new { v.sohd });

            modelBuilder.Entity<DTA_HOADON_XUAT>()
                .HasMany(v => v.chitiet)
                .WithOne(c => c.hoadon)
                .HasForeignKey(c => new { c.sohd, c.ngaylaphd })
                .HasPrincipalKey(v => new { v.sohd, v.ngaylaphd });

            modelBuilder.Entity<DTA_HOADON_NHAP>()
               .HasMany(v => v.chitiet)
               .WithOne(c => c.hoadon)
               .HasForeignKey(c => new { c.sohd, c.ngaylaphd })
               .HasPrincipalKey(v => new { v.sohd, v.ngaylap });

        }
        // EF Core 6
        protected override void ConfigureConventions(
            ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 5);
        }
        public override int SaveChanges()
        {
            OnBeforeSaveChanges();
            return base.SaveChanges();
        }
        public int Save()
        {
            return base.SaveChanges();
        }

        private void OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            var user_http = actionAccessor.ActionContext.HttpContext.User;
            var user_id = UserManager.GetUserId(user_http);
            var changes = ChangeTracker.Entries();
            foreach (var entry in changes)
            {
                if (entry.Entity is AuditTrailsModel || entry.Entity is EmailModel || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = user_id;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                var Original = entry.GetDatabaseValues().GetValue<object>(propertyName);
                                var Current = property.CurrentValue;
                                if (JsonConvert.SerializeObject(Original) == JsonConvert.SerializeObject(Current))
                                    continue;
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = Original;
                                auditEntry.NewValues[propertyName] = Current;

                            }
                            break;
                    }

                }
            }
            foreach (var auditEntry in auditEntries)
            {
                _context.AuditTrailsModel.Add(auditEntry.ToAudit());
            }
            /////
            Program.description = "";
            _context.SaveChanges();
        }
    }
}
