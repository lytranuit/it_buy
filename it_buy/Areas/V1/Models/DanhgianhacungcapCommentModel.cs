using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("danhgianhacungcap_comment")]
    public class DanhgianhacungcapCommentModel
    {
        [Key]
        public int id { get; set; }
        public int danhgianhacungcap_id { get; set; }
        public string? comment { get; set; }


        [ForeignKey("danhgianhacungcap_id")]
        public virtual DanhgianhacungcapModel? danhgianhacungcap { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }
        public virtual List<DanhgianhacungcapCommentFileModel>? files { get; set; }
        public virtual List<DanhgianhacungcapCommentUserModel>? users_related { get; set; }


        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        [NotMapped]

        public bool is_read { get; set; } = false;


    }
}
