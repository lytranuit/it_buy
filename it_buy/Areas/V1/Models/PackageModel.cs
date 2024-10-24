using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vue.Models;
namespace it_report.QLSX.Models
{

    [Table("TBL_DANHMUCKIENHANG")]
    public class PackageModel
    {
        [Key]
        public string qrcode { get; set; }
        public string qrcode_image { get; set; }
        public string code { get; set; }
        public string mahh { get; set; }

        public string mansx { get; set; }

        public string mancc { get; set; }
        public string ngaysx { get; set; }
        public string tinhtrang { get; set; }


        public decimal? soluongbandau { get; set; }
        public decimal? soluonglo { get; set; }
        public decimal soluong { get; set; }
        public string tenhh { get; set; }
        public string malo { get; set; }
        public string handung { get; set; }

        public string sopkn { get; set; }
        public string masothietke { get; set; }
        public string makho { get; set; }
        public string dvt { get; set; }
        public string quicach { get; set; }

        public string sophieuTN { get; set; }
        public int? tiepnhan_id { get; set; }

        public string mavt { get; set; }
        public string mota { get; set; }
        public string phanloai { get; set; }

        [NotMapped]
        public string ngayhandung { get; set; }
        public DateTime? ngaychapnhan { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
