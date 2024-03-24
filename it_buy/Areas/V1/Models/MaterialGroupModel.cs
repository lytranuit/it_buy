using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("TBL_DANHMUCNHOMHANG")]
    public class MaterialGroupModel
    {
        [Key]
        public string manhom { get; set; }
        public string? tennhom { get; set; }

        public virtual List<MaterialModel>? items { get; set; }
    }
}
