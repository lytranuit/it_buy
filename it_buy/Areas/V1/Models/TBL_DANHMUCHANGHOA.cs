using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("TBL_DANHMUCHANGHOA")]
    public class TBL_DANHMUCHANGHOA
    {
        [Key]
        public int id { get; set; }
        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string? dvt { get; set; }

        public string? nhom { get; set; }
        public string? ghichu { get; set; }
        public string? quicach { get; set; }


        public string? mansx { get; set; }
        //public virtual NsxModel? nhasanxuat { get; set; }
        public string? mancc { get; set; }
        //public virtual NhacungcapModel? nhacungcap { get; set; }





    }
}
