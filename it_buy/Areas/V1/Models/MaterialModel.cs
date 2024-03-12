using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("material")]
    public class MaterialModel
    {
        [Key]
        public int id { get; set; }
        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string? dvt { get; set; }
        public string? masothietke { get; set; }
        public string? grade { get; set; }
        public string? nhasx { get; set; }
        public string? tensp { get; set; }
        public string? dangbaoche { get; set; }
        public string? note { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? created_at { get; set; }
    }
}
