using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    public partial class Users
    {
        public Users()
        {
            FriendsIdAcceptorNavigation = new HashSet<Friends>();
            FriendsIdPetitionerNavigation = new HashSet<Friends>();
            GroupsRelations = new HashSet<GroupsRelations>();
            RsschannelSubscriptions = new HashSet<RsschannelSubscriptions>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("nick")]
        [StringLength(16)]
        public string Nick { get; set; }
        [Required]
        [Column("mail")]
        [StringLength(255)]
        public string Mail { get; set; }
        [Required]
        [Column("pass_hash")]
        [StringLength(60)]
        public string PassHash { get; set; }

        [InverseProperty(nameof(Friends.IdAcceptorNavigation))]
        public virtual ICollection<Friends> FriendsIdAcceptorNavigation { get; set; }
        [InverseProperty(nameof(Friends.IdPetitionerNavigation))]
        public virtual ICollection<Friends> FriendsIdPetitionerNavigation { get; set; }
        [InverseProperty("IdUserNavigation")]
        public virtual ICollection<GroupsRelations> GroupsRelations { get; set; }
        [InverseProperty("IdUserNavigation")]
        public virtual ICollection<RsschannelSubscriptions> RsschannelSubscriptions { get; set; }
    }
}
