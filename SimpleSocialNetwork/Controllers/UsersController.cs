using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialNetwork.Data;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [Authorize]
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

            var users = from s in _dbContext.Users select s;

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

            int pageSize = 4;

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

            if (identityUser.Friends == null)
            {
                identityUser.Friends = new List<ApplicationUser>();
            }
            if (newFriend.Friends == null)
            {
                newFriend.Friends = new List<ApplicationUser>();
            }
            if (identityUser.Id != newFriend.Id && !identityUser.Friends.Contains(newFriend))
            {
                identityUser.Friends.Add(newFriend);
                newFriend.Friends.Add(identityUser);
                _dbContext.Users.Update(identityUser);
                _dbContext.Users.Update(newFriend);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("UserProfile", "Home", new { newFriend.Id });
        }
    }
}