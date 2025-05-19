using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Vue.Models
{
    [Table("DTA_VATTU_NHAP")]
    public class VattuNhapModel
    {
        //[Column("NGAYLAPHD")]
        public DateTime? ngaylap { get; set; } // Ngày lập phiếu

        //[Column("NHAPTAIKHO")]
        public string? makho { get; set; } // Mã kho

        public string? ghichu { get; set; } // Ghi chú

        [Required]
        public string sohd { get; set; } // Số hợp đồng / hóa đơn
        //public string? mapl { get; set; } = "MUA";

        [Key]
        public int id { get; set; } // ID chính

        public DateTime? created_at { get; set; } // Thời điểm tạo

        public DateTime? deleted_at { get; set; } // Thời điểm xóa

        //[Column("nguoidung")]
        public string? created_by { get; set; } // Người tạo
        [NotMapped]
        public UserModel? user_created { get; set; }

        public List<VattuNhapChiTietModel>? chitiet { get; set; }
    }
}
