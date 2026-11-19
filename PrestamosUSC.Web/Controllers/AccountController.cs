using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Web.Models;

namespace PrestamosUSC.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.RememberMe, lockoutOnFailure: false);
        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                return Redirect(modelo.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(modelo);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var usuario = new ApplicationUser
        {
            UserName = modelo.Email,
            Email = modelo.Email,
            EmailConfirmed = true,
            NombreCompleto = modelo.NombreCompleto,
            Identificacion = modelo.Identificacion
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Password);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(modelo);
        }

        // Todo usuario que se registra queda con el rol Estudiante
        await _userManager.AddToRoleAsync(usuario, RolesApp.Estudiante);
        await _signInManager.SignInAsync(usuario, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
