﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("muahang_ncc")]
    public class MuahangNccModel
    {
        [Key]
        public int id { get; set; }
        public int? muahang_id { get; set; }
        public int? ncc_id { get; set; }
        public bool? chonmua { get; set; }
        public bool? dapung { get; set; }
        public string? thoigiangiaohang { get; set; }
        public string? baohanh { get; set; }
        public string? thanhtoan { get; set; }
        public decimal? tonggiatri { get; set; }
        public List<MuahangNccChitietModel>? chitiet { get; set; }
        public List<MuahangNccDinhkemModel>? dinhkem { get; set; }

        [ForeignKey("muahang_id")]
        public MuahangModel? muahang { get; set; }
        [ForeignKey("ncc_id")]
        public NhacungcapModel? ncc { get; set; }

    }

}