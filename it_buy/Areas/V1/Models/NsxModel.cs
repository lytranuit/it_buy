using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCNHASX")]
    public class NsxModel
    {
        [Key]
        public int id { get; set; }
        [Column("MaNSX")]
        public string mansx { get; set; }
        [Column("TenNSX")]
        public string? tennsx { get; set; }

    }
}
