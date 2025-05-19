using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Vue.Models
{
    [Table("DTA_CT_HOADON_NHAP")]
    public class DTA_CT_HOADON_NHAP
    {
        [Key]
        public int id { get; set; } // Khóa chính

        public int? stt { get; set; } 
        public string? sohd { get; set; } // Số hợp đồng / hóa đơn
        public DateTime? ngaylaphd { get; set; } 

        public string? mahh { get; set; } // Mã hàng hóa
        public string? mancc { get; set; } // Mã ncc

        public decimal? soluong { get; set; } // Số lượng (kiểu money -> decimal)

        public string? ghichu { get; set; } // Ghi chú
        public int? muahang_id { get; set; } // Muahang_id
        [Column("nguoidung")]
        public string? created_by { get; set; } // Người tạo

        //[ForeignKey("sohd")]
        public DTA_HOADON_NHAP? hoadon { get; set; }

        [NotMapped]

        public string? tenhh { get; set; } // Tên hàng hóa
        [NotMapped]
        public string? dvt { get; set; } // dvt hàng hóa
    }
}
