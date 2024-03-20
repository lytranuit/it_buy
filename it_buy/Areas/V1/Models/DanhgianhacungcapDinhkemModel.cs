using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("danhgianhacungcap_dinhkem")]
    public class DanhgianhacungcapDinhkemModel
    {
        [Key]
        public int id { get; set; }
        public int danhgianhacungcap_id { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public string? ext { get; set; }
        public string? mimeType { get; set; }

        public string? created_by { get; set; }
        public string note { get; set; }
        [ForeignKey("danhgianhacungcap_id")]
        public DanhgianhacungcapModel? danhgianhacungcap { get; set; }

        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }
}
