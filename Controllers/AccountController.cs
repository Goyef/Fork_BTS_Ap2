using System.Security.Claims;
using ASPBookProject.Data;
using ASPBookProject.Models;
using ASPBookProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{

    private readonly UserManager<Medecin> _userManager;
    private IPasswordHasher<Medecin> passwordHasher;




    private readonly SignInManager<Medecin> _signInManager; 


    public AccountController(SignInManager<Medecin> signInManager, UserManager<Medecin> userManager, IPasswordHasher<Medecin> passwordHash)
    {
        _signInManager = signInManager; 
        _userManager = userManager;
        passwordHasher = passwordHash;

    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Erreur lors du login");
        }

        return View(model);
    }
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        DateTime Min = DateTime.Now.AddYears(-18);
        if (ModelState.IsValid)
        {
            if (model.Date >= Min)
            {
                ModelState.AddModelError("", "Le médecin doit avoir au moins 18 ans.");
            }
            else
            {
                var medecin = new Medecin
                {
                    UserName = model.UserName,
                    Role = model.Role,
                    Date_naissance_m = model.Date,
                };

                var result = await _userManager.CreateAsync(medecin, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(medecin, isPersistent: false);
                    return RedirectToAction("Index", "Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }
        return View(model);
    }



    [HttpPost]
    public async Task<IActionResult> Edit(string id, string UserName, string Role, DateTime Date_naissance_m, string PasswordHash)
    {
        DateTime Min = DateTime.Now.AddYears(-18);
        Medecin user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(UserName))
                user.UserName = UserName;
            else
                ModelState.AddModelError("", "UserName ne peut pas être vide");
            if (!string.IsNullOrEmpty(Role))
                user.Role = Role;
            else
                ModelState.AddModelError("", "Role ne peut pas être vide");
            if (Date_naissance_m < Min)
                user.Date_naissance_m = Date_naissance_m;
            else
                ModelState.AddModelError("", "Le médecin a minumun 18 ans");

            if (!ModelState.IsValid)
            {
                return View(user);
            }
            if (!string.IsNullOrEmpty(PasswordHash))
            {
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, PasswordHash);
                if (!passwordChangeResult.Succeeded)
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
            }

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

        }
        return View(user);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Medecin user = _userManager.FindByIdAsync(userId).Result;
        user.PasswordHash = "";

        return View(user);
    }

    [HttpPost]
    public IActionResult ThrowException()
    {
        throw new Exception("Une exception s'est produite, nous testons la page d'exception pour les développeurs.");
    }

   
}
