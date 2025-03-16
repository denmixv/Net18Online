using Enums.Users;
using Everything.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using WebPortalEverthing.Controllers.AuthAttributes;
using WebPortalEverthing.Models.Admin;
using WebPortalEverthing.Services;

namespace WebPortalEverthing.Controllers
{
    [IsAdmin]
    public class SurveysAdminController : Controller
    {
        private readonly IUserRepositryReal _userRepositryReal;
        private readonly EnumHelper _enumHelper;

        public SurveysAdminController(
            IUserRepositryReal userRepositryReal, 
            EnumHelper enumHelper)
        {
            _userRepositryReal = userRepositryReal;
            _enumHelper = enumHelper;
        }

        public IActionResult Users()
        {
            var users = _userRepositryReal
                .GetAll()
                .Select(x => new UserViewModel
                {
                    Id = x.Id,
                    Name = x.Login,
                    Role = x.Role,
                    Roles = _enumHelper.GetNames(x.Role)
                })
                .ToList();

            var viewModel = new AdminUserViewModel
            {
                Users = users,
                Roles = _enumHelper.GetSelectListItems<Role>()
            };

            return View(viewModel);
        }

        public IActionResult DeleteUser(int id)
        {
            _userRepositryReal.Delete(id);
            return RedirectToAction(nameof(Users));
        }

        public IActionResult UpdateRole(Role role, int userId)
        {
            _userRepositryReal.UpdateRole(userId, role);
            return RedirectToAction(nameof(Users));
        }
    }
}
