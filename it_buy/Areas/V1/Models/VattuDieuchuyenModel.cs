using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Vue.Models
{
    [Table("DTA_VATTU_DIEUCHUYEN")]
    public class VattuDieuchuyenModel
    {
        [Key]
        public int id { get; set; }

        //[Column("NgayLapHD")]
        public DateTime? ngaylap { get; set; }


        //[Column("noixuat")]
        public string? noidi { get; set; }

        //[Column("MAKH")]
        public string? noiden { get; set; }

        //[Column("TENNGUOIGD")]
        public string? ghichu { get; set; }

        public string? pl { get; set; } = null!;
        [Required]
        public string sohd { get; set; } = null!;

        public DateTime? created_at { get; set; }

        public DateTime? deleted_at { get; set; }

        //[Column("MACH")]
        public string? created_by { get; set; }
        [NotMapped]
        public UserModel? user_created { get; set; }

        public List<VattuDieuchuyenChiTietModel> chitiet { get; set; }
    }
}
