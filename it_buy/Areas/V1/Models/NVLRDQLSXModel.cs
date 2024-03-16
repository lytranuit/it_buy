using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vue.Models
{

    [Table("TBL_DANHMUCHANGHOA")]
    public class NVLRDQLSXModel
    {
        [Key]
        [Column("MAHH")]
        public string mahh { get; set; }
        [Column("TENHH")]
        public string? tenhh { get; set; }
        [Column("DVT")]
        public string? dvt { get; set; }
        public string? mancc { get; set; }
        public string? mansx { get; set; }
        [ForeignKey("mansx")]
        public NsxModel? nhasanxuat { get; set; }

    }
}
