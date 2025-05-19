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
        public decimal? quidoi { get; set; }
        public decimal? soluong_nhanhang { get; set; }
        public int? status_id { get; set; }
        public string? note { get; set; }
        public int dutru_chitiet_id { get; set; }
        public string? note_nhanhang { get; set; }
        public int? status_nhanhang { get; set; }
        public DateTime? date_nhanhang { get; set; }
        public string? user_nhanhang_id { get; set; }
        [ForeignKey("user_nhanhang_id")]
        public virtual UserModel? user_nhanhang { get; set; }

        [ForeignKey("muahang_id")]
        public virtual MuahangModel? muahang { get; set; }
        [ForeignKey("dutru_chitiet_id")]
        public virtual DutruChitietModel? dutru_chitiet { get; set; }

        public virtual List<MuahangNccChitietModel> muahang_ncc_chitiet { get; set; }
        //[NotMapped]
        public string? mahh { get; set; }
        //[NotMapped]
        public string? tenhh { get; set; }
        //[NotMapped]
        public string? dvt { get; set; }
        public string? dvt_dutru { get; set; }
        public string? grade { get; set; }
        public string? nhasx { get; set; }
        public string? mansx { get; set; }
        public string? leadtime { get; set; }
        public string? moq { get; set; }
        [NotMapped]

        public int? stt { get; set; }

        [NotMapped]
        public decimal? soluong_quidoi
        {
            get
            {
                return soluong * quidoi;
            }
        }

    }
}
