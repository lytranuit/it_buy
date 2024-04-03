using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("dutru_dinhkem")]
    public class DutruDinhkemModel
    {
        [Key]
        public int id { get; set; }
        public int dutru_id { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public string? ext { get; set; }
        public string? mimeType { get; set; }

        public string? created_by { get; set; }
        [ForeignKey("created_by")]
        public UserModel? user_created_by { get; set; }
        public string note { get; set; }
        [ForeignKey("dutru_id")]
        public DutruModel? dutru { get; set; }

        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }
}
