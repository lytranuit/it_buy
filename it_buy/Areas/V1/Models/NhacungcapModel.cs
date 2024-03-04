using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vue.Models
{

    [Table("TBL_DANHMUCNHACC_SC")]
    public class NhacungcapModel
    {
        [Key]
        public int id { get; set; }

        [Column("MaNCC")]
        public string mancc { get; set; }
        [Column("TenNCC")]
        public string? tenncc { get; set; }
        [Column("TENNCC_VN")]
        public string? tenncc_vn { get; set; }

    }
}
