using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleSocialNetwork.Data;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }

            var user = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);
            return RedirectToAction("UserProfile", new { user.Id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult EditProfile(string id)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            return View(user);
        }
        [HttpPost]
        public async Task<ActionResult> EditProfile(ApplicationUser user)
        {
            var identityUser = _userManager.Users.FirstOrDefault(x => x.Id == user.Id);

            if (identityUser == null)
            {
                // TODO: add log here
                return NotFound("User was not found.");
            }

            // TODO: reconsider following lines
            identityUser.FirstName = user.FirstName;
            identityUser.LastName = user.LastName;
            identityUser.PatronymicName = user.PatronymicName;
            identityUser.BirthDate = user.BirthDate;
            identityUser.Hobbies = user.Hobbies;

            byte[] imageData = null;
            if (Request.Form.Files.Count > 0)
            {
                IFormFile avatarFile = Request.Form.Files["Avatar"];

                using (var reader = new BinaryReader(avatarFile.OpenReadStream()))
                {
                    imageData = reader.ReadBytes((int)avatarFile.Length);
                }
            }

            identityUser.Avatar = imageData;

            var result = await _userManager.UpdateAsync(identityUser);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists " +
                                             "see your system administrator.");
            }

            return RedirectToAction("UserProfile", new { user.Id });
        }

        public IActionResult UserProfile(string id)
        {
            var user = _dbContext.Users.First(u => u.Id == id);
            ViewBag.is_auth_user = IsAuthorizedUser(user.Id);
            return View(user);
        }

        // TODO: move to another controller
        public FileResult Photo(string id)
        {
            ApplicationUser user = null;

            if (string.IsNullOrEmpty(id))
            {
                // TODO: validation and other checks
                user = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);
            }
            else
            {
                user = _dbContext.Users.First(u => u.Id == id);
            }


            if (user.Avatar != null)
            {
                return new FileContentResult(user.Avatar, "image/jpeg");
            }
            else
            {
                return new VirtualFileResult("/images/noavatar.png", "image/jpeg");
            }
        }

        // TODO: reconsider this method
        protected bool IsAuthorizedUser(string id)
        {
            var user = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);
            if (user.Id == id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
