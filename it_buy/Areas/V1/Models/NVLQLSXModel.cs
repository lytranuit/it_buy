using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Vue.Models
{

    [Table("dm_hanghoa")]
    public class NVLQLSXModel
    {
        [Key]
        public string mahh { get; set; }
        public string? tenhh { get; set; }
        public string? tenhh_tm { get; set; }
        public string? dvt { get; set; }
        public string? mancc { get; set; }
        public string? mansx { get; set; }
        public string? masothietke { get; set; }

        [ForeignKey("mansx")]
        public NsxModel? nhasanxuat { get; set; }
    }
}
