using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCKHO")]
    public class KhoModel
    {
        [Key]
        public int id { get; set; }
        public string makho { get; set; }
        public string? tenkho { get; set; }


    }
}
