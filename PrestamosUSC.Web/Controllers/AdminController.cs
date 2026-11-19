using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Structures;
using PrestamosUSC.Web.Models;

namespace PrestamosUSC.Web.Controllers;

[Authorize(Roles = RolesApp.Administrador)]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHistorialActividad _historial;
    private readonly IColaEspera _cola;

    public AdminController(UserManager<ApplicationUser> userManager, IHistorialActividad historial, IColaEspera cola)
    {
        _userManager = userManager;
        _historial = historial;
        _cola = cola;
    }

    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var lista = new List<UsuarioViewModel>();
        foreach (var u in usuarios)
        {
            lista.Add(new UsuarioViewModel
            {
                Id = u.Id,
                Nombre = u.NombreCompleto,
                Email = u.Email ?? string.Empty,
                EsAdministrador = await _userManager.IsInRoleAsync(u, RolesApp.Administrador)
            });
        }
        return View(lista);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "No puedes cambiar tu propio rol.";
            return RedirectToAction(nameof(Usuarios));
        }

        if (await _userManager.IsInRoleAsync(usuario, RolesApp.Administrador))
        {
            await _userManager.RemoveFromRoleAsync(usuario, RolesApp.Administrador);
            await _userManager.AddToRoleAsync(usuario, RolesApp.Estudiante);
        }
        else
        {
            await _userManager.RemoveFromRoleAsync(usuario, RolesApp.Estudiante);
            await _userManager.AddToRoleAsync(usuario, RolesApp.Administrador);
        }

        TempData["Mensaje"] = $"Rol de {usuario.Email} actualizado.";
        return RedirectToAction(nameof(Usuarios));
    }

    public IActionResult Estructuras() => View(new EstructurasViewModel
    {
        Resultados = ComparadorEstructuras.Ejecutar(),
        Historial = _historial.Recientes(15),
        ColaEspera = _cola.Listar()
    });
}
