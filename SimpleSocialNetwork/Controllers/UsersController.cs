using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialNetwork.Data;
using SimpleSocialNetwork.Helpers;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var users = from s in _dbContext.Users.Where(u => u.UserName != HttpContext.User.Identity.Name) select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.LastName.Contains(searchString)
                                               || s.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    users = users.OrderBy(s => s.BirthDate);
                    break;
                case "date_desc":
                    users = users.OrderByDescending(s => s.BirthDate);
                    break;
                default:
                    users = users.OrderBy(s => s.LastName);
                    break;
            }

            const int pageSize = 4;

            return View(await PaginatedList<ApplicationUser>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult AddFriend(string id)
        {
            var identityUser = _userManager.Users.Include("Friends").FirstOrDefault(u => u.UserName == HttpContext.User.Identity.Name);
            var newFriend = _userManager.Users.Include("Friends").First(u => u.Id == id);

            if (identityUser == null)
            {
                return NotFound("Current user was not found.");
            }

            if (identityUser.Id == newFriend.Id || identityUser.Friends.Contains(newFriend))
            {
                return RedirectToAction("UserProfile", "Users", new {newFriend.Id});
            }

            // TODO: move this logic into repository
            identityUser.Friends.Add(newFriend);
            newFriend.Friends.Add(identityUser);
            _dbContext.Users.Update(identityUser);
            _dbContext.Users.Update(newFriend);
            _dbContext.SaveChanges();

            return RedirectToAction("UserProfile", "Users", new { newFriend.Id });
        }

        public IActionResult RemoveFriend(string id)
        {
            var identityUser = _userManager.Users.Include("Friends").FirstOrDefault(u => u.UserName == HttpContext.User.Identity.Name);
            var oldFriend = _userManager.Users.Include("Friends").First(u => u.Id == id);

            if (identityUser == null)
            {
                return NotFound("Current user was not found.");
            }

            if (!identityUser.Friends.Contains(oldFriend))
            {
                return RedirectToAction("UserProfile", "Users", new {identityUser.Id});
            }

            identityUser.Friends.Remove(oldFriend);
            oldFriend.Friends.Remove(identityUser);
            _dbContext.Users.Update(identityUser);
            _dbContext.Users.Update(oldFriend);
            _dbContext.SaveChanges();

            return RedirectToAction("UserProfile", "Users", new { identityUser.Id });
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public FileResult Photo(string id)
        {
            var user = string.IsNullOrEmpty(id) ?
                _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name) :
                _dbContext.Users.First(u => u.Id == id);

            if (user.Avatar != null)
            {
                return new FileContentResult(user.Avatar, "image/jpeg");
            }

            return new VirtualFileResult("/images/noavatar.png", "image/jpeg");
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

            // TODO: recheck possibility of this case
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

            var imageData = GetImageData();

            if (imageData != null)
            {
                identityUser.Avatar = imageData;
            }

            var result = await _userManager.UpdateAsync(identityUser);

            // TODO: reconsider handling errors in async methods
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                                             "Try again, and if the problem persists " +
                                             "see your system administrator.");
            }

            return RedirectToAction("UserProfile", new { user.Id });
        }

        private byte[] GetImageData()
        {
            byte[] imageData = null;

            if (Request.Form.Files.Count > 0)
            {
                IFormFile avatarFile = Request.Form.Files["Avatar"];

                using (var reader = new BinaryReader(avatarFile.OpenReadStream()))
                {
                    // we expect having notification about the Avatar image size on UI
                    imageData = checked(reader.ReadBytes((int) avatarFile.Length));
                }
            }

            return imageData;
        }

        public IActionResult UserProfile(string id)
        {
            var user = _dbContext.Users.Include(u => u.Friends).First(u => u.Id == id);
            ViewBag.isAuthorizedUser = IsAuthorizedUser(user.Id);
            ViewBag.isFriend = IsFriend(user.Id);
            return View(user);
        }

        // TODO: reconsider this method
        protected bool IsAuthorizedUser(string id)
        {
            var user = _dbContext.Users.First(u => u.Email == HttpContext.User.Identity.Name);
            return user.Id == id;
        }

        // TODO: reconsider this method
        protected bool IsFriend(string id)
        {
            var user = _dbContext.Users.Include("Friends").First(u => u.UserName == HttpContext.User.Identity.Name);
            var otherUser = _dbContext.Users.Include("Friends").First(u => u.Id == id);

            return user.Id == id || user.Friends.Contains(otherUser);
        }
    }
}