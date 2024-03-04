using Newtonsoft.Json;
using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vue.Models
{
    [Table("queue")]
    public class QueueModel
    {
        [Key]
        public int id { get; set; }
        public string? created_by { get; set; }
        public int? status_id { get; set; } = 1;
        public string? json { get; set; }
        public string? type { get; set; }
        public DateTime? created_at { get; set; }
        [NotMapped]
        public virtual QueueValue? valueQ
        {
            get
            {
                //Console.WriteLine(settings);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<QueueValue>(string.IsNullOrEmpty(json) ? "{}" : json);
            }
            set
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }
        }

    }
    public class QueueValue
    {
        public DutruModel? dutru { get; set; }
        public MuahangModel? muahang { get; set; }
    }
}
