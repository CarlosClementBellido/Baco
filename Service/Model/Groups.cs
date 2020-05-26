using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.Model
{
    public partial class Groups
    {
        public Groups()
        {
            GroupsRelations = new HashSet<GroupsRelations>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name")]
        [StringLength(24)]
        public string Name { get; set; }

        [InverseProperty("IdGroupNavigation")]
        public virtual ICollection<GroupsRelations> GroupsRelations { get; set; }
    }
}
