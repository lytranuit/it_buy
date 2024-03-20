using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("danhgianhacungcap_comment_user")]
    public class DanhgianhacungcapCommentUserModel
    {
        [Key]
        public int id { get; set; }
        public int danhgianhacungcap_comment_id { get; set; }


        [ForeignKey("danhgianhacungcap_comment_id")]
        public virtual DanhgianhacungcapCommentModel? comment { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }


    }
}
