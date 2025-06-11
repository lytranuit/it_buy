using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("DTA_CT_HOADON_XUAT")]
    public class DTA_CT_HOADON_XUAT
    {
        [Key]
        public int id { get; set; }

        // = 1

        public bool? kt { get; set; }

        public string? sohd { get; set; }

        public DateTime? ngaylaphd { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public string? mach { get; set; }

        public string? mahh { get; set; }

        public string? tenhh { get; set; }

        public string? dvt { get; set; }
        public string? malo { get; set; }
        public string? handung { get; set; }

        public decimal? soluong { get; set; }

        public int? stt { get; set; }

        public string? makm { get; set; }

        public string? ghichu { get; set; }
        public string? mancc { get; set; }


        public bool? kt_kho { get; set; }

        public DTA_HOADON_XUAT hoadon { get; set; }
    }
}
