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
        public string? note_huy { get; set; }
        public DateTime? date_huy { get; set; }
        public string? user_id { get; set; }

        [ForeignKey("user_id")]
        public UserModel? user { get; set; }
        public string? tags { get; set; }

        public List<string> list_tag
        {
            get
            {
                return tags != null ? tags.Split(",").ToList() : new List<string>();
            }
        }
        [ForeignKey("dutru_id")]
        public DutruModel? dutru { get; set; }
        public List<MuahangChitietModel> muahang_chitiet { get; set; }
        public List<DutruChitietDinhkemModel> dinhkem { get; set; }

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
        public string? mansx { get; set; }
        [NotMapped]

        public int? stt { get; set; }
        [NotMapped]

        public bool? can_huy { get; set; } = false;

    }
}
