using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("muahang_chitiet")]
    public class MuahangChitietModel
    {
        [Key]
        public int id { get; set; }
        public int muahang_id { get; set; }
        public string? hh_id { get; set; }
        public decimal? soluong { get; set; }
        public int? status_id { get; set; }
        public string? note { get; set; }
        public int? dutru_chitiet_id { get; set; }

        [ForeignKey("muahang_id")]
        public MuahangModel? muahang { get; set; }
        [ForeignKey("dutru_chitiet_id")]
        public DutruChitietModel? dutru_chitiet { get; set; }

        //[NotMapped]
        public string? mahh { get; set; }
        //[NotMapped]
        public string? tenhh { get; set; }
        //[NotMapped]
        public string? dvt { get; set; }
        [NotMapped]

        public int? stt { get; set; }

    }
}
