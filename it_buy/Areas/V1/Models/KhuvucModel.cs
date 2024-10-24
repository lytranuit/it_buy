using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCKHUVUC")]
    public class KhuvucModel
    {
        [Key]
        public string makhuvuc { get; set; }
        [Column("tenkhuvuc_VN")]
        public string? tenkhuvuc { get; set; }


    }
}
