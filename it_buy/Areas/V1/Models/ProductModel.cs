using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCTHANHPHAM")]
    public class ProductModel
    {
        [Key]
        public int id { get; set; }
        public string mahh { get; set; }
        public string? tenhh { get; set; }
        public string? dvt { get; set; }
        public string? mahh_goc { get; set; }
        public string? tenhh_goc { get; set; }
        public string? mapl { get; set; }
        public string? tenhoatchat { get; set; }
        public string? dangbaoche { get; set; }
        public string? quicachdonggoi { get; set; }
        public double? handung { get; set; }
        public string? sodk { get; set; }
        public DateTime? ngaycapsodk { get; set; }
        public DateTime? ngayhethansodk { get; set; }
        public string? ghichu { get; set; }

    }
}
