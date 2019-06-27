using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleSocialNetwork.Data;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Controllers
{
    public class ImageGalleryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ImageGalleryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        public IActionResult Index(string userId)
        {
            var user = _dbContext.Users.Include(u => u.GalleryImages).ThenInclude(i => i.FriendsWithAccess).FirstOrDefault(u => u.Id == userId);
            var identityUser = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);

            if (user == null)
            {
                return NotFound("User was not found");
            }

            var images = user.GalleryImages;

            if (user != identityUser)
            {
                images = images.Where(i => i.FriendsWithAccess.Contains(identityUser)).ToList();
            }

            return View(images);
        }

        [HttpPost]
        public IActionResult Index()
        {
            byte[] imageData = null;
            GalleryImage galleryImage = null;
            if (Request.Form.Files.Count > 0)
            {
                IFormFile imageFile = Request.Form.Files["Image"];

                using (var reader = new BinaryReader(imageFile.OpenReadStream()))
                {
                    imageData = reader.ReadBytes((int)imageFile.Length);
                }
            }

            var identityUser = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);

            if (imageData != null)
            {
                galleryImage = new GalleryImage()
                {
                    Image = imageData,
                    Owner = identityUser
                };
            }

            if (identityUser.GalleryImages == null)
            {
                identityUser.GalleryImages = new List<GalleryImage>();
            }

            identityUser.GalleryImages.Add(galleryImage);
            _dbContext.Users.Update(identityUser);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", new {userid = identityUser.Id});
        }

        public FileResult ImageFile(int id)
        {
            // TODO: validation and other checks
            var galleryImage = _dbContext.GalleryImages.Include("FriendsWithAccess").First(u => u.Id == id);

            if (galleryImage != null)
            {
                return new FileContentResult(galleryImage.Image, "model/jpeg");
            }

            return null;
        }

        public IActionResult Image(int id)
        {
            var galleryImage = _dbContext.GalleryImages.Include(i => i.Owner).ThenInclude(u => u.Friends).First(i => i.Id == id);

            var user = galleryImage.Owner;

            var friendsListToShare = new List<SelectListItem>();

            foreach (var friend in (galleryImage.FriendsWithAccess != null ? user.Friends.Except(galleryImage.FriendsWithAccess) : user.Friends))
            {
                friendsListToShare.Add(new SelectListItem()
                {
                    Text = friend.FirstName,
                    Value = friend.Id
                });
            }

            ViewBag.FriendsListToShare = friendsListToShare;
            ViewBag.IsOwner = user.UserName == HttpContext.User.Identity.Name;

            return View(galleryImage);
        }

        [HttpPost]
        public IActionResult Image(GalleryImage model)
        {
            string[] userIds = Request.Form["Friend"];

            if (userIds.Any())
            {
                var users = _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToList();

                if (users.Any())
                {
                    var galleryImage = _dbContext.GalleryImages.Include(i => i.FriendsWithAccess).First(u => u.Id == model.Id);

                    users.ForEach(u => galleryImage.FriendsWithAccess.Add(u));
                    _dbContext.GalleryImages.Update(galleryImage);
                    _dbContext.SaveChanges();

                    return RedirectToAction("Image", new { id = galleryImage.Id });
                }
            }

            return View("Image", new {id = model.Id});
        }

        public IActionResult Delete(int id)
        {
            var galleryImage = _dbContext.GalleryImages.First(u => u.Id == id);
            var identityUser = _dbContext.Users.Include(u => u.GalleryImages).First(u => u.Email == HttpContext.User.Identity.Name);

            identityUser.GalleryImages.Remove(galleryImage);
            // TODO: try to use Async
            _dbContext.SaveChanges();

            return RedirectToAction("Index", new {userid = identityUser.Id });
        }

        public IActionResult RemoveSharing(int id, string friendId)
        {
            var galleryImage = _dbContext.GalleryImages.Include("FriendsWithAccess").First(u => u.Id == id);
            var friendWithAccess = _dbContext.Users.FirstOrDefault(u => u.Id == friendId);
            galleryImage.FriendsWithAccess.Remove(friendWithAccess);

            _dbContext.GalleryImages.Update(galleryImage);
            _dbContext.SaveChanges();

            return RedirectToAction("Image", new { id = galleryImage.Id });
        }
    }
}