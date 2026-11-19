using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Enums;
using PrestamosUSC.Core.Exceptions;
using PrestamosUSC.Core.Extensions;
using PrestamosUSC.Core.Services;
using PrestamosUSC.Core.Structures;
using PrestamosUSC.Web.Models;

namespace PrestamosUSC.Web.Controllers;

[Authorize]
public class PrestamosController : Controller
{
    private readonly IPrestamoService _prestamos;
    private readonly IRecursoService _recursos;
    private readonly IColaEspera _cola;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrestamosController(IPrestamoService prestamos, IRecursoService recursos, IColaEspera cola, UserManager<ApplicationUser> userManager)
    {
        _prestamos = prestamos;
        _recursos = recursos;
        _cola = cola;
        _userManager = userManager;
    }

    // Todos los préstamos (solo Administrador)
    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> Index(EstadoPrestamo? estado)
    {
        ViewBag.Estado = estado;
        return View(await _prestamos.ListarTodosAsync(estado));
    }

    // Préstamos del usuario que inició sesión
    public async Task<IActionResult> MisPrestamos()
    {
        var usuarioId = _userManager.GetUserId(User)!;
        return View(await _prestamos.ListarPorUsuarioAsync(usuarioId));
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int recursoId)
    {
        var recurso = await _recursos.ObtenerAsync(recursoId);
        if (recurso == null) return NotFound();

        var inicio = DateTime.Now.AddMinutes(10);
        inicio = new DateTime(inicio.Year, inicio.Month, inicio.Day, inicio.Hour, inicio.Minute, 0);
        var duracionInicial = recurso is Sala ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);

        return View(new PrestamoCreateViewModel
        {
            RecursoId = recurso.Id,
            RecursoResumen = recurso.Resumen(),
            DuracionMaxima = recurso.DuracionMaximaPrestamo.ToTextoLegible(),
            FechaInicio = inicio,
            FechaFin = inicio.Add(duracionInicial)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PrestamoCreateViewModel modelo)
    {
        var recurso = await _recursos.ObtenerAsync(modelo.RecursoId);
        if (recurso == null) return NotFound();
        modelo.RecursoResumen = recurso.Resumen();
        modelo.DuracionMaxima = recurso.DuracionMaximaPrestamo.ToTextoLegible();

        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var usuarioId = _userManager.GetUserId(User)!;
            await _prestamos.RegistrarAsync(usuarioId, modelo.RecursoId, modelo.FechaInicio, modelo.FechaFin, modelo.Observaciones);
            TempData["Mensaje"] = "Préstamo registrado correctamente.";
            return RedirectToAction(nameof(MisPrestamos));
        }
        catch (RecursoOcupadoException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.PermiteCola = true; // la vista muestra el botón "Unirme a la lista de espera"
            return View(modelo);
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int id, string? volverA)
    {
        try
        {
            await _prestamos.DevolverAsync(id, _userManager.GetUserId(User)!, User.IsInRole(RolesApp.Administrador));
            TempData["Mensaje"] = "Devolución registrada.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return volverA == "admin" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(MisPrestamos));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? volverA)
    {
        try
        {
            await _prestamos.CancelarAsync(id, _userManager.GetUserId(User)!, User.IsInRole(RolesApp.Administrador));
            TempData["Mensaje"] = "Préstamo cancelado.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return volverA == "admin" ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(MisPrestamos));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UnirseCola(int recursoId)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var agregado = _cola.Encolar(new SolicitudEspera(recursoId, usuario.Id, usuario.NombreCompleto, DateTime.Now));
        TempData["Mensaje"] = agregado
            ? "Te agregamos a la lista de espera de este recurso."
            : "Ya estabas en la lista de espera de este recurso.";
        return RedirectToAction("Index", "Recursos");
    }
}
