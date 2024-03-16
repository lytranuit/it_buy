using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("muahang_comment")]
    public class MuahangCommentModel
    {
        [Key]
        public int id { get; set; }
        public int muahang_id { get; set; }
        public string? comment { get; set; }


        [ForeignKey("muahang_id")]
        public virtual MuahangModel? muahang { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }
        public virtual List<MuahangCommentFileModel>? files { get; set; }
        public virtual List<MuahangCommentUserModel>? users_related { get; set; }

        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        [NotMapped]

        public bool is_read { get; set; } = false;


    }
}
