using System;
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
        public int? material_id { get; set; }
        [ForeignKey("material_id")]
        public virtual MaterialModel? material { get; set; }
        public int? ncc_id { get; set; }
        [ForeignKey("ncc_id")]
        public virtual NhacungcapModel? ncc { get; set; }
        public string? created_by { get; set; }

        [ForeignKey("created_by")]
        public virtual UserModel? user_created_by { get; set; }

        public string? user_chapnhan_id { get; set; }
        [ForeignKey("user_chapnhan_id")]
        public virtual UserModel? user_chapnhan { get; set; }
        public bool? is_chapnhan { get; set; }
        public DateTime? date_chapnhan { get; set; }     ///Ngày hoàn thành
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }

}
