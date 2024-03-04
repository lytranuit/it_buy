
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

        public DbSet<UserModel> UserModel { get; set; }
        public DbSet<UserRoleModel> UserRoleModel { get; set; }
        public DbSet<EmailModel> EmailModel { get; set; }
        public DbSet<TokenModel> TokenModel { get; set; }
        public DbSet<UserManagerModel> UserManagerModel { get; set; }
        public DbSet<DepartmentModel> DepartmentModel { get; set; }
        public DbSet<UserDepartmentModel> UserDepartmentModel { get; set; }


        public DbSet<MaterialModel> MaterialModel { get; set; }
        public DbSet<NhacungcapModel> NhacungcapModel { get; set; }
        public DbSet<DutruModel> DutruModel { get; set; }
        public DbSet<DutruChitietModel> DutruChitietModel { get; set; }
        public DbSet<MuahangModel> MuahangModel { get; set; }
        public DbSet<MuahangChitietModel> MuahangChitietModel { get; set; }
        public DbSet<MuahangNccModel> MuahangNccModel { get; set; }
        public DbSet<MuahangNccDinhkemModel> MuahangNccDinhkemModel { get; set; }
        public DbSet<MuahangNccChitietModel> MuahangNccChitietModel { get; set; }
        public DbSet<QueueModel> QueueModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRoleModel>().ToTable("AspNetUserRoles").HasKey(table => new
            {
                table.RoleId,
                table.UserId
            });

            modelBuilder.Entity<UserModel>().HasMany(e => e.list_users).WithOne(e => e.user).HasForeignKey(e => e.userId);

            modelBuilder.Entity<UserModel>().HasOne(e => e.userreport).WithOne(e => e.userManager).HasForeignKey<UserManagerModel>(e => e.userManagerId);


            //modelBuilder.Entity<UserManagerModel>().HasOne(e => e.user);

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
                "AspNetUsers","AspNetUserRoles","emails","Token","user_manager","department","user_department","queue"
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
                "dm_hanghoa","TBL_DANHMUCNHACC_SC"
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
