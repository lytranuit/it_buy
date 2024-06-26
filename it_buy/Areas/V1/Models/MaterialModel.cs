using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("TBL_DANHMUCHANGHOA_MUAHANG")]
    public class MaterialModel
    {
        [Key]
        public int id { get; set; }
        [Column("MAHH")]
        public string mahh { get; set; }
        [Column("TENHH")]
        public string tenhh { get; set; }
        [Column("DVT")]
        public string? dvt { get; set; }

        public string? nhom { get; set; }
        public virtual MaterialGroupModel? nhomhang { get; set; }
        public string? masothietke { get; set; }
        public string? grade { get; set; }
        //public string? nhasx { get; set; }
        public string? tensp { get; set; }
        public string? dangbaoche { get; set; }
        public string? tieuchuan { get; set; }
        [Column("Ghichu")]
        public string? note { get; set; }

        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }

        public string? mansx { get; set; }

        public virtual NsxModel? nhasanxuat { get; set; }
        public string? mancc { get; set; }
        public virtual NhacungcapModel? nhacungcap { get; set; }
        public virtual List<MaterialDinhkemModel>? dinhkem { get; set; }





    }
}
