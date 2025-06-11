using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("XUAT_NVL")]
    public class XuatNVLModel
    {
        [Key]
        public int id { get; set; }
        public string? code { get; set; }
        public string? bophan_id { get; set; }
        public string? created_by { get; set; }
        public int? status_id { get; set; } = 1;
        public string? note { get; set; }
        public string? pdf { get; set; }
        public int? esign_id { get; set; }
        public virtual List<XuatNVLChitietModel>? chitiet { get; set; }

        public DateTime? date_finish { get; set; }     ///Ngày hoàn thành
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? date { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }


}
