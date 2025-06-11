using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("XUAT_NVL_CHITIET")]
    public class XuatNVLChitietModel
    {
        [Key]
        public int id { get; set; }
        public int xuat_id { get; set; }
        public decimal soluong { get; set; }

        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string dvt { get; set; }

        public int stt { get; set; }

        public string? note { get; set; }


        [ForeignKey("xuat_id")]
        public XuatNVLModel? xuat { get; set; }
    }


}
