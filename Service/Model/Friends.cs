using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    public partial class Friends
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_petitioner")]
        public int IdPetitioner { get; set; }
        [Column("id_acceptor")]
        public int IdAcceptor { get; set; }
        [Column("accepted")]
        public bool? Accepted { get; set; }

        [ForeignKey(nameof(IdAcceptor))]
        [InverseProperty(nameof(Users.FriendsIdAcceptorNavigation))]
        public virtual Users IdAcceptorNavigation { get; set; }
        [ForeignKey(nameof(IdPetitioner))]
        [InverseProperty(nameof(Users.FriendsIdPetitionerNavigation))]
        public virtual Users IdPetitionerNavigation { get; set; }
    }
}
