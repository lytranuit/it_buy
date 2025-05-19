using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("TBL_DANHMUCPLHANGHOA")]
    public class TBL_DANHMUCPLHANGHOA
    {
        [Key]
        public string pl { get; set; }
        public string tenpl { get; set; }

    }
}
