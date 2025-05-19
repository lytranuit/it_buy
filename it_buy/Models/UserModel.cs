using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using workflow.Models;

namespace Vue.Models
{

    [Table("AspNetUsers")]
    public class UserModel : IdentityUser
    {
        public string FullName { get; set; }
        public string? image_url { get; set; }
        public string? image_sign { get; set; }
        public string? signature { get; set; }
        public DateTime? last_login { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
        public List<UserDepartmentModel> departments { get; set; }
        [NotMapped]
        public List<UserManagerModel> list_users { get; set; }
        [NotMapped]
        public UserManagerModel userreport { get; set; }
        public string? list_warehouse { get; set; }
        [NotMapped]
        public virtual List<string>? warehouses
        {
            get
            {
                //Console.WriteLine(settings);
                return JsonSerializer.Deserialize<List<string>>(string.IsNullOrEmpty(list_warehouse) ? "[]" : list_warehouse);
            }
            set
            {
                list_warehouse = JsonSerializer.Serialize(value);
            }
        }
        public string? list_warehouse_vt { get; set; }
        [NotMapped]
        public virtual List<string>? warehouses_vt
        {
            get
            {
                //Console.WriteLine(settings);
                return JsonSerializer.Deserialize<List<string>>(string.IsNullOrEmpty(list_warehouse_vt) ? "[]" : list_warehouse_vt);
            }
            set
            {
                list_warehouse_vt = JsonSerializer.Serialize(value);
            }
        }
    }
}
