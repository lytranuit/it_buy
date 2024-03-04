using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("muahang_ncc_chitiet")]
    public class MuahangNccChitietModel
    {
        [Key]
        public int id { get; set; }
        public int muahang_ncc_id { get; set; }
        public string? hh_id { get; set; }
        public decimal? soluong { get; set; }
        public decimal? dongia { get; set; }
        public decimal? thanhtien { get; set; }


        [ForeignKey("muahang_ncc_id")]
        public MuahangNccModel? muahang_ncc { get; set; }

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
