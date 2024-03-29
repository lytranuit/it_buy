﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("danhgianhacungcap")]
    public class DanhgianhacungcapModel
    {
        [Key]
        public int id { get; set; }
        public string? mahh { get; set; }
        public string? tenhh { get; set; }

        public string? dvt { get; set; }

        public string? masothietke { get; set; }
        public string? grade { get; set; }

        public string? quicach { get; set; }

        public string? created_by { get; set; }

        [ForeignKey("created_by")]
        public virtual UserModel? user_created_by { get; set; }

        public string? user_chapnhan_id { get; set; }
        [ForeignKey("user_chapnhan_id")]
        public virtual UserModel? user_chapnhan { get; set; }
        public bool? is_thongbao { get; set; }
        public bool? is_chapnhan { get; set; }
        public DateTime? date_chapnhan { get; set; }     ///Ngày hoàn thành
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }


        public int? mansx { get; set; }

        public virtual NsxModel? nhasanxuat { get; set; }
        public int? mancc { get; set; }
        public virtual NhacungcapModel? nhacungcap { get; set; }
    }

}
