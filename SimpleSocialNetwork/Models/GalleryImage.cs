using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleSocialNetwork.Models
{
    public class GalleryImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ApplicationUser Owner { get; set; }

        public byte[] Image { get; set; }

        public ICollection<ApplicationUser> FriendsWithAccess { get; set; }
    }
}
