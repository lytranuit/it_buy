using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Vue.Models
{
    [Table("DTA_VATTU_DIEUCHUYEN_CT")]
    public class VattuDieuchuyenChiTietModel
    {
        [Key]
        public int id { get; set; }

        //public int? stt { get; set; }
        public string? sohd { get; set; }

        //public DateTime? ngaylaphd { get; set; }

        public string? mahh { get; set; }
        public string? mancc { get; set; } // Mã ncc

        public string? malo { get; set; }
        public DateTime? handung { get; set; }
        public decimal? soluong { get; set; }
        public decimal? tonkho { get; set; }

        //[Column("MaCH")]
        //public string? created_by { get; set; }
        //[Column("KT")]
        public bool? kt_xuat { get; set; }

        //[Column("KT_KHO")]
        public bool? kt_nhap { get; set; }

        public string? ghichu { get; set; }

        public string? user_xuat { get; set; }

        public string? user_nhap { get; set; }

        // Optional: Navigation property if used with EF Core
        //[ForeignKey("sohd")]
        public VattuDieuchuyenModel? hoadon { get; set; }
        [NotMapped]

        public string? tenhh { get; set; } // Tên hàng hóa
        [NotMapped]
        public string? dvt { get; set; } // dvt hàng hóa
    }
}
