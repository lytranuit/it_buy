using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("muahang_ncc_dinhkem")]
    public class MuahangNccDinhkemModel
    {
        [Key]
        public int id { get; set; }
        public int muahang_ncc_id { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public string? ext { get; set; }
        public string? mimeType { get; set; }

        public string? created_by { get; set; }

        [ForeignKey("muahang_ncc_id")]
        public MuahangNccModel? muahang_ncc { get; set; }

        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }
}
