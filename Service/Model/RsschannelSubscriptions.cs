using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    [Table("RSSChannelSubscriptions")]
    public partial class RsschannelSubscriptions
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_rsschannel")]
        public int IdRsschannel { get; set; }
        [Column("id_user")]
        public int IdUser { get; set; }

        [ForeignKey(nameof(IdRsschannel))]
        [InverseProperty(nameof(Rsschannels.RsschannelSubscriptions))]
        public virtual Rsschannels IdRsschannelNavigation { get; set; }
        [ForeignKey(nameof(IdUser))]
        [InverseProperty(nameof(Users.RsschannelSubscriptions))]
        public virtual Users IdUserNavigation { get; set; }
    }
}
