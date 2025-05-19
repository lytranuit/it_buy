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
        public string mahh { get; set; }
        public string tenhh { get; set; }
        public string? dvt { get; set; }

        [Column("manhom")]
        public string? nhom { get; set; }
        public virtual MaterialGroupModel? nhomhang { get; set; }
        public string? masothietke { get; set; }
        public string? grade { get; set; }
        public string? tieuchuan { get; set; }
        [Column("ghichu")]
        public string? note { get; set; }
        public string? leadtime { get; set; }
        public string? moq { get; set; }
        public string? quicach { get; set; }


        public string? group { get; set; }
        public string? mansx { get; set; }

        public virtual NsxModel? nhasanxuat { get; set; }
        public string? mancc { get; set; }
        public virtual NhacungcapModel? nhacungcap { get; set; }


        public string? created_by { get; set; }

        [ForeignKey("created_by")]
        public UserModel? user_created_by { get; set; }

        public DateTime? deleted_at { get; set; }
        public DateTime? created_at { get; set; }

        public string? user_notify { get; set; }


        [NotMapped]
        public List<string>? list_user_notify
        {
            get
            {
                return user_notify != null ? user_notify.Split(",").ToList() : null;
            }
            set
            {
                user_notify = value != null ? string.Join(",", value) : null;
            }
        }

        public decimal? tonkho_duoi { get; set; }
        public string? image_url { get; set; }

        [NotMapped]
        public virtual List<string> list_sp { get; set; }



    }
}
