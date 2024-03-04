using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("dm_hanghoa")]
    public class MaterialModel
    {
        [Key]
        public int id { get; set; }
        public string? mahh { get; set; }
        public string? tenhh { get; set; }
        public string? dvt { get; set; }
        public string? masothietke { get; set; }
    }
}
