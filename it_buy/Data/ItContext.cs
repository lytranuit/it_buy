
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using Vue.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DiagnosticAdapter;
using System.Data.Common;
using workflow.Models;
using System.Reflection.Metadata;
using System.Xml;
using it_report.QLSX.Models;

namespace Vue.Data
{
    public class ItContext : DbContext
    {
        private IActionContextAccessor actionAccessor;
        private UserManager<UserModel> UserManager;
        public ItContext(DbContextOptions<ItContext> options, UserManager<UserModel> UserMgr, IActionContextAccessor ActionAccessor) : base(options)
        {
            actionAccessor = ActionAccessor;
            UserManager = UserMgr;

        }

        public DbSet<AuditTrailsModel> AuditTrailsModel { get; set; }

        public virtual DbSet<UserModel> UserModel { get; set; }
        public DbSet<UserRoleModel> UserRoleModel { get; set; }
        public DbSet<EmailModel> EmailModel { get; set; }
        public DbSet<TokenModel> TokenModel { get; set; }
        public DbSet<UserManagerModel> UserManagerModel { get; set; }
        public DbSet<DepartmentModel> DepartmentModel { get; set; }
        public DbSet<UserDepartmentModel> UserDepartmentModel { get; set; }


        //public DbSet<NhacungcapQLSXModel> NhacungcapQLSXModel { get; set; }
        public DbSet<NVLQLSXModel> NVLQLSXModel { get; set; }
        //public DbSet<NVLRDQLSXModel> NVLRDQLSXModel { get; set; }
        public DbSet<KhuvucModel> KhuvucModel { get; set; }
        public DbSet<NsxModel> NsxModel { get; set; }
        public DbSet<BomModel> BomModel { get; set; }
        public DbSet<PackageModel> PackageModel { get; set; }
        public DbSet<BomThaytheModel> BomThaytheModel { get; set; }
        public DbSet<ProductModel> ProductModel { get; set; }

        public DbSet<MaterialGroupModel> MaterialGroupModel { get; set; }
        public DbSet<MaterialModel> MaterialModel { get; set; }
        public DbSet<MaterialDinhkemModel> MaterialDinhkemModel { get; set; }
        public DbSet<NhacungcapModel> NhacungcapModel { get; set; }
        public DbSet<DutruModel> DutruModel { get; set; }
        public DbSet<DutruChitietModel> DutruChitietModel { get; set; }
        public DbSet<DutruChitietDinhkemModel> DutruChitietDinhkemModel { get; set; }
        public DbSet<DutruCommentModel> DutruCommentModel { get; set; }
        public DbSet<DutruDinhkemModel> DutruDinhkemModel { get; set; }
        public DbSet<DutruCommentUserModel> DutruCommentUserModel { get; set; }
        public DbSet<DutruCommentFileModel> DutruCommentFileModel { get; set; }

        public DbSet<DanhgianhacungcapModel> DanhgianhacungcapModel { get; set; }
        public DbSet<DanhgianhacungcapCommentModel> DanhgianhacungcapCommentModel { get; set; }
        public DbSet<DanhgianhacungcapDinhkemModel> DanhgianhacungcapDinhkemModel { get; set; }
        public DbSet<DanhgianhacungcapDanhgiaModel> DanhgianhacungcapDanhgiaModel { get; set; }
        public DbSet<DanhgianhacungcapCommentUserModel> DanhgianhacungcapCommentUserModel { get; set; }
        public DbSet<DanhgianhacungcapCommentFileModel> DanhgianhacungcapCommentFileModel { get; set; }

        public virtual DbSet<MuahangModel> MuahangModel { get; set; }
        public DbSet<MuahangChitietModel> MuahangChitietModel { get; set; }
        public DbSet<MuahangCommentModel> MuahangCommentModel { get; set; }
        public DbSet<MuahangCommentUserModel> MuahangCommentUserModel { get; set; }
        public DbSet<MuahangCommentFileModel> MuahangCommentFileModel { get; set; }
        public DbSet<MuahangNccModel> MuahangNccModel { get; set; }
        public DbSet<MuahangNccDinhkemModel> MuahangNccDinhkemModel { get; set; }
        public DbSet<MuahangDinhkemModel> MuahangDinhkemModel { get; set; }
        public DbSet<MuahangUynhiemchiModel> MuahangUynhiemchiModel { get; set; }
        public DbSet<MuahangNccChitietModel> MuahangNccChitietModel { get; set; }
        public DbSet<QueueModel> QueueModel { get; set; }
        public DbSet<RelatedEsignModel> RelatedEsignModel { get; set; }



        public DbSet<DocumentModel> DocumentModel { get; set; }
        public DbSet<DocumentFileModel> DocumentFileModel { get; set; }

        public DbSet<DocumentAttachmentModel> DocumentAttachmentModel { get; set; }
        public DbSet<DocumentSignatureModel> DocumentSignatureModel { get; set; }
        public DbSet<DocumentUserReceiveModel> DocumentUserReceiveModel { get; set; }
        public DbSet<DocumentEventModel> DocumentEventModel { get; set; }
        public DbSet<DocumentTypeModel> DocumentTypeModel { get; set; }
        public DbSet<DocumentTypeReceiveModel> DocumentTypeReceiveModel { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRoleModel>().ToTable("AspNetUserRoles").HasKey(table => new
            {
                table.RoleId,
                table.UserId
            });

            modelBuilder.Entity<BomModel>()
                .HasKey(e => new { e.mahh, e.colo, e.manvl, e.stt });
            modelBuilder.Entity<BomThaytheModel>()
                .HasKey(e => new { e.mahh, e.colo, e.manvl, e.stt_thaythe });

            modelBuilder.Entity<UserModel>().HasMany(e => e.list_users).WithOne(e => e.user).HasForeignKey(e => e.userId);

            modelBuilder.Entity<UserModel>().HasOne(e => e.userreport).WithOne(e => e.userManager).HasForeignKey<UserManagerModel>(e => e.userManagerId);

            modelBuilder.Entity<MaterialModel>()
            .HasOne(s => s.nhacungcap)
            .WithMany()
            .HasForeignKey(e => e.mancc).HasPrincipalKey(d => d.mancc);

            modelBuilder.Entity<MaterialModel>()
            .HasOne(s => s.nhasanxuat)
            .WithMany()
            .HasForeignKey(e => e.mansx).HasPrincipalKey(d => d.mansx);

            modelBuilder.Entity<MaterialModel>()
              .HasOne(s => s.nhomhang)
              .WithMany(d => d.items)
              .HasForeignKey(e => e.nhom).HasPrincipalKey(d => d.manhom);


            modelBuilder.Entity<MuahangModel>()
              .HasOne(s => s.muahang_chonmua)
              .WithMany()
              .HasForeignKey(e => e.muahang_chonmua_id);
            //modelBuilder.Entity<UserManagerModel>().HasOne(e => e.user);

            //modelBuilder.Entity<DanhgianhacungcapModel>()
            //.HasOne(s => s.nhacungcap)
            //.WithMany()
            //.HasForeignKey(e => e.mancc).HasPrincipalKey(d => d.id);

            //modelBuilder.Entity<DanhgianhacungcapModel>()
            //.HasOne(s => s.nhasanxuat)
            //.WithMany()
            //.HasForeignKey(e => e.mansx).HasPrincipalKey(d => d.id);

        }
        public override int SaveChanges()
        {
            OnBeforeSaveChanges();
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
                if (entry.Entity is AuditTrailsModel || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
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
                AuditTrailsModel.Add(auditEntry.ToAudit());
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
        }
    }
    public class CommandInterceptor
    {
        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId, Guid connectionId, bool async, DateTimeOffset startTime)
        {

            var list_talbe = new List<string>()
            {
                "AspNetUsers","AspNetUserRoles","emails","Token","user_manager","department","user_department","queue",
                "document","document_event","document_attachment","document_file","document_user_receive","document_signature","document_type","document_type_receive","related_esign"
            };
            //var tableName = "AspNetUsers";
            foreach (var tableName in list_talbe)
            {
                var secondaryDatabaseName = "OrgData";
                var schemaName = "dbo";
                command.CommandText = command.CommandText.Replace($" [{tableName}]", $" [{schemaName}].[{tableName}]")
                                                     .Replace($" [{schemaName}].[{tableName}]", $" [{secondaryDatabaseName}].[{schemaName}].[{tableName}]");
            }
            var list_talbe2 = new List<string>()
            {
                "dm_hanghoa","TBL_DANHMUCNHACC","TBL_DANHMUCHANGHOA_MUAHANG","TBL_DANHMUCHANGHOA_DINHKEM","TBL_DANHMUCNHASX","TBL_DANHMUCNHOMHANG","TBL_DANHMUCTHANHPHAM","TBL_DANHMUCKHUVUC","TBL_DANHMUCBOM","TBL_DANHMUCBOM_THAYTHE","TBL_DANHMUCKIENHANG"
            };
            //var tableName = "AspNetUsers";
            foreach (var tableName in list_talbe2)
            {
                var secondaryDatabaseName = "QLSX";
                var schemaName = "dbo";
                command.CommandText = command.CommandText.Replace($" [{tableName}]", $" [{schemaName}].[{tableName}]")
                                                     .Replace($" [{schemaName}].[{tableName}]", $" [{secondaryDatabaseName}].[{schemaName}].[{tableName}]");
            }

        }
    }
}
