using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("muahang_comment_user")]
    public class MuahangCommentUserModel
    {
        [Key]
        public int id { get; set; }
        public int muahang_comment_id { get; set; }


        [ForeignKey("muahang_comment_id")]
        public virtual MuahangCommentModel? comment { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }


    }
}
