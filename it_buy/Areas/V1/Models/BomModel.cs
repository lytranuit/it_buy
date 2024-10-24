using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{

    [Table("TBL_DANHMUCBOM")]
    public class BomModel
    {
        public string mahh { get; set; }
        public string? tenhh { get; set; }
        public string? dvt { get; set; }
        public string? mahh_goc { get; set; }
        public string? tenhh_goc { get; set; }
        public string? dvt_goc { get; set; }
        public decimal? colo { get; set; }
        public int? stt { get; set; }
        public string? manvl { get; set; }
        public string? tennvl { get; set; }
        public string? dvtnvl { get; set; }
        public double? me { get; set; }
        public decimal? soluong { get; set; }

    }
}
