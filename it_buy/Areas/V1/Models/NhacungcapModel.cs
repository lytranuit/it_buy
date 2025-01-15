using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vue.Models
{

    [Table("TBL_DANHMUCNHACC")]
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
        [Column("DiachiNCC")]
        public string? diachincc { get; set; }
        [Column("DienthoaiNCC")]
        public string? dienthoaincc { get; set; }
        [Column("EmailNCC")]
        public string? emailncc { get; set; }
        [Column("NguoiLienHe")]
        public string? nguoilienhe { get; set; }
        [Column("Dienthoai")]
        public string? dienthoai { get; set; }
        [Column("Email")]
        public string? email { get; set; }
        [Column("chucvu")]
        public string? chucvu { get; set; }
        [Column("taikhoannh")]
        public string? taikhoannh { get; set; }
        [Column("masothue")]
        public string? masothue { get; set; }
        [Column("nhom")]
        public string? nhom { get; set; }

        public string? nguoiphutrach { get; set; }
        public int? danhgia { get; set; }
        public int? type { get; set; }
        //public virtual List<MaterialModel> Materials { get; set; }

        [NotMapped]
        public int? start_c { get; set; }

    }
}
