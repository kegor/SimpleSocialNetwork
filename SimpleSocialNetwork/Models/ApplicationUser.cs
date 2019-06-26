using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SimpleSocialNetwork.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Patronymic name")]
        public string PatronymicName { get; set; }

        public byte[] Avatar { get; set; }

        [Display(Name = "Birth date")]
        public DateTime? BirthDate { get; set; }

        public string Hobbies { get; set; }

        public ICollection<ApplicationUser> Friends { get; set; }

        [Display(Name = "Image gallery")]
        public ICollection<GalleryImage> GalleryImages { get; set; }
    }
}
