using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Only Admin can create new accounts (Member, Staff, Librarian, Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsAdmin = false,
                IsApproved = true // Admin-created accounts are trusted immediately
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = $"Account created successfully for {model.FullName} ({model.Role}).";
                return RedirectToAction(nameof(Register));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // GET: Account/SelfRegister — public "Become a Member" registration
        [HttpGet]
        public IActionResult SelfRegister()
        {
            return View();
        }

        // POST: Account/SelfRegister — creates an unapproved Member account
        [HttpPost]
        public async Task<IActionResult> SelfRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsAdmin = false,
                IsApproved = false // must be approved by a Librarian/Admin before login works
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                TempData["Success"] = "Your registration has been submitted and is pending Librarian approval. You will be able to log in once approved.";
                return RedirectToAction("BecomeAMember", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsApproved)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError("", "Your account is pending Librarian approval. Please check back later.");
                    return View(model);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // GET: Account/PendingRegistrations (Librarian/Admin)
        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet]
        public async Task<IActionResult> PendingRegistrations()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            var pending = members.Where(m => !m.IsApproved).OrderBy(m => m.FullName).ToList();
            return View(pending);
        }

        // POST: Account/ApproveRegistration/{id} (Librarian/Admin)
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRegistration(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} has been approved and can now log in.";
            return RedirectToAction(nameof(PendingRegistrations));
        }

        // POST: Account/RejectRegistration/{id} (Librarian/Admin)
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRegistration(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);

            TempData["Success"] = "Registration rejected and removed.";
            return RedirectToAction(nameof(PendingRegistrations));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}