using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("muahang")]
    public class MuahangModel
    {
        [Key]
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? created_by { get; set; }

        [ForeignKey("created_by")]
        public UserModel? user_created_by { get; set; }
        public int? type_id { get; set; }
        public int? status_id { get; set; } = 1;
        public int? activeStep { get; set; }
        public string? note { get; set; }
        public string? pdf { get; set; }
        public int? esign_id { get; set; }
        public int? muahang_chonmua_id { get; set; }
        public bool? is_dathang { get; set; }
        public bool? is_thanhtoan { get; set; }

        public string? dondathang { get; set; }
        public string? loaithanhtoan { get; set; } // Trả trước or trả sau
        //[ForeignKey("muahang_chonmua_id")]
        [NotMapped]
        public MuahangNccModel? muahang_chonmua { get; set; }
        public List<MuahangDinhkemModel>? list_thanhtoan { get; set; }
        public List<MuahangChitietModel>? chitiet { get; set; }
        public List<MuahangNccModel>? nccs { get; set; }

        public DateTime? date_finish { get; set; }     ///Ngày hoàn thành
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? date { get; set; }     ///Ngày giao hàng dự kiến
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }

}
