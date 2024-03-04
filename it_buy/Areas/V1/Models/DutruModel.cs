using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("dutru")]
    public class DutruModel
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
        public List<DutruChitietModel>? chitiet { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? date { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }

    enum Status
    {
        [Display(Name = "Nháp")]
        Draft = 1,
        [Display(Name = "Xuất PDF")]
        PDF = 2,
        [Display(Name = "Trình ký")]
        Esign = 3,
        [Display(Name = "Đã ký xong")]
        EsignSuccess = 4,
        [Display(Name = "Ký lỗi")]
        EsignError = 5,
        [Display(Name = "Gửi và nhận báo giá")]
        Baogia = 6,
        [Display(Name = "So sánh giá")]
        sosanhgia = 7,
        [Display(Name = "Xuất PDF Mua hàng")]
        MuahangPDF = 8,
        [Display(Name = "Trình ký Mua hàng")]
        MuahangEsign = 9,
        [Display(Name = "Đã ký xong Mua hàng")]
        MuahangEsignSuccess = 10,
        [Display(Name = "Ký lỗi Mua hàng")]
        MuahangEsignError = 11,
    }
}
