using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialNetwork.Data;
using SimpleSocialNetwork.Models;

namespace SimpleSocialNetwork.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            //if (!HttpContext.User.Identity.IsAuthenticated)
            //{
            //    RedirectToAction("Index", "Home");
            //}

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

            int pageSize = 3;
            return View(await PaginatedList<ApplicationUser>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}