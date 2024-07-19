using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("danhgianhacungcap_danhgia")]
    public class DanhgianhacungcapDanhgiaModel
    {
        [Key]
        public int id { get; set; }
        public int danhgianhacungcap_id { get; set; }
        public DateTime? date_dealine { get; set; }
        public DateTime? date_accept { get; set; }

        public string? bophan { get; set; }
        public string? user_id { get; set; }
        //public string? comment { get; set; }

        [ForeignKey("user_id")]
        public UserModel? user { get; set; }
        public string? note { get; set; }
        [ForeignKey("danhgianhacungcap_id")]
        public DanhgianhacungcapModel? danhgianhacungcap { get; set; }

    }
}
