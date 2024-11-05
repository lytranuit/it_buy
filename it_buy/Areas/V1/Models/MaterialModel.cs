using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("dm_hanghoa")]
    public class MaterialModel
    {
        [Key]
        public int id { get; set; }
        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string? dvt { get; set; }

        [Column("manhom")]
        public string? nhom { get; set; }
        public virtual MaterialGroupModel? nhomhang { get; set; }
        public string? masothietke { get; set; }
        public string? grade { get; set; }
        public string? tieuchuan { get; set; }
        [Column("ghichu")]
        public string? note { get; set; }


        public string? mansx { get; set; }

        public virtual NsxModel? nhasanxuat { get; set; }
        public string? mancc { get; set; }
        public virtual NhacungcapModel? nhacungcap { get; set; }





    }
}
