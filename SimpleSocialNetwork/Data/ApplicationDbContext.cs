using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GalleryImage> GalleryImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(
                typeBuilder =>
                {
                    typeBuilder.HasMany(u => u.GalleryImages);

                });

            builder.Entity<GalleryImage>(
                typeBuilder =>
                {
                    typeBuilder.HasMany(i => i.FriendsWithAccess);
                });

            builder.Entity<GalleryImage>(
                typeBuilder =>
                {
                    typeBuilder.HasOne(i => i.Owner);
                });
        }
    }
}
