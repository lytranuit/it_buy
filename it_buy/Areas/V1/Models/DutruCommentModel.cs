using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("dutru_comment")]
    public class DutruCommentModel
    {
        public int id { get; set; }
        public int dutru_id { get; set; }
        public string? comment { get; set; }


        [ForeignKey("dutru_id")]
        public virtual DutruModel? dutru { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }
        public virtual List<DutruCommentFileModel>? files { get; set; }

        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        [NotMapped]

        public bool is_read { get; set; } = false;


    }
}
