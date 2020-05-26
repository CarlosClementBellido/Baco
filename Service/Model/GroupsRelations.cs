using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    public partial class GroupsRelations
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_group")]
        public int IdGroup { get; set; }
        [Column("id_user")]
        public int IdUser { get; set; }

        [ForeignKey(nameof(IdGroup))]
        [InverseProperty(nameof(Groups.GroupsRelations))]
        public virtual Groups IdGroupNavigation { get; set; }
        [ForeignKey(nameof(IdUser))]
        [InverseProperty(nameof(Users.GroupsRelations))]
        public virtual Users IdUserNavigation { get; set; }
    }
}
