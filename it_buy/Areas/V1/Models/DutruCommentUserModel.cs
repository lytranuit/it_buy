﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vue.Models;

namespace Vue.Models
{
    [Table("dutru_comment_user")]
    public class DutruCommentUserModel
    {
        [Key]
        public int id { get; set; }
        public int dutru_comment_id { get; set; }


        [ForeignKey("dutru_comment_id")]
        public virtual DutruCommentModel? comment { get; set; }


        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual UserModel? user { get; set; }


    }
}
