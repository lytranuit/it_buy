using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Vue.Models
{
    [Table("DTA_HOADON_XUAT")]
    public class DTA_HOADON_XUAT
    {
        [Key]
        public int id { get; set; }


        [Required]
        public string sohd { get; set; } = null!;

        public DateTime? ngaylaphd { get; set; }

        public string? mapl { get; set; }

        public string? makh { get; set; }

        public string? donvi { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public string? mach { get; set; }

        public DateTime? ngaygiaohang { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? tennguoigd { get; set; }
        
        // = 1
        public bool? kt { get; set; }

      

        /// <summary>
        /// 
        /// </summary>
        public string? pl { get; set; }
        /// <summary>
        /// KHO
        /// </summary>
        public string? noixuat { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? deleted_at { get; set; }

        //
        public bool? khoa { get; set; }

        [NotMapped]
        public UserModel? user_created { get; set; }

        [NotMapped]
        public List<DTA_CT_HOADON_XUAT> chitiet { get; set; } = new();
    }
}
