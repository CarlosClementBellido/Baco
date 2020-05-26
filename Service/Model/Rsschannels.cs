using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    [Table("RSSChannels")]
    public partial class Rsschannels
    {
        public Rsschannels()
        {
            RsschannelSubscriptions = new HashSet<RsschannelSubscriptions>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("rss")]
        [StringLength(64)]
        public string Rss { get; set; }
        [Required]
        [Column("name")]
        [StringLength(24)]
        public string Name { get; set; }

        [InverseProperty("IdRsschannelNavigation")]
        public virtual ICollection<RsschannelSubscriptions> RsschannelSubscriptions { get; set; }
    }
}
