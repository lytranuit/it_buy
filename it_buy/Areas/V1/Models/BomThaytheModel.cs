using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCBOM_THAYTHE")]
    public class BomThaytheModel
    {
        public string mahh { get; set; }
        public decimal colo { get; set; }
        public string manvl { get; set; }
        public int stt_thaythe { get; set; }

        public string manvl_thaythe { get; set; }
        public string? tennvl_thaythe { get; set; }
        public string? dvtnvl_thaythe { get; set; }
        public double? me_thaythe { get; set; }
        public decimal? soluong_thaythe { get; set; }

    }
}
