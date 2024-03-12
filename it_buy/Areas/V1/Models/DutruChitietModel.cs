using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("dutru_chitiet")]
    public class DutruChitietModel
    {
        [Key]
        public int id { get; set; }
        public int dutru_id { get; set; }
        public string? hh_id { get; set; }
        public decimal? soluong { get; set; }
        public int? status_id { get; set; }
        public string? note { get; set; }

        [ForeignKey("dutru_id")]
        public DutruModel? dutru { get; set; }
        public List<MuahangChitietModel> muahang_chitiet { get; set; }

        //[NotMapped]
        public string? mahh { get; set; }
        //[NotMapped]
        public string? tenhh { get; set; }
        //[NotMapped]
        public string? dvt { get; set; }
        public string? masothietke { get; set; }
        public string? grade { get; set; }
        public string? nhasx { get; set; }
        public string? tensp { get; set; }
        public string? dangbaoche { get; set; }
        [NotMapped]

        public int? stt { get; set; }

    }
}
