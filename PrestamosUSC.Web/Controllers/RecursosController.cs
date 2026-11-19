using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrestamosUSC.Core.Constants;
using PrestamosUSC.Core.Entities;
using PrestamosUSC.Core.Exceptions;
using PrestamosUSC.Core.Services;
using PrestamosUSC.Web.Models;

namespace PrestamosUSC.Web.Controllers;

[Authorize]
public class RecursosController : Controller
{
    private readonly IRecursoService _recursos;

    public RecursosController(IRecursoService recursos) => _recursos = recursos;

    // Catálogo: lo ven todos los usuarios autenticados
    public async Task<IActionResult> Index(string? tipo, string? q, DateTime? desde, DateTime? hasta)
    {
        ViewBag.Tipo = tipo;
        ViewBag.Q = q;
        ViewBag.Desde = desde?.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-ddTHH:mm");
        return View(await _recursos.ListarAsync(tipo, q, desde, hasta));
    }

    // ---------- Solo Administrador ----------
    [Authorize(Roles = RolesApp.Administrador)]
    public IActionResult Create(string? tipo) =>
        View(new RecursoFormViewModel { Tipo = tipo == "Sala" ? "Sala" : "Equipo" });

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> Create(RecursoFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        Recurso recurso = modelo.Tipo == "Sala"
            ? new Sala { Capacidad = modelo.Capacidad, Piso = modelo.Piso }
            : new Equipo { Marca = modelo.Marca, Serial = modelo.Serial };

        recurso.Codigo = modelo.Codigo;
        recurso.Nombre = modelo.Nombre;
        recurso.Descripcion = modelo.Descripcion;
        recurso.Ubicacion = modelo.Ubicacion;
        recurso.Estado = modelo.Estado;

        try
        {
            await _recursos.CrearAsync(recurso);
            TempData["Mensaje"] = $"{recurso.Tipo} \"{recurso.Nombre}\" creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var recurso = await _recursos.ObtenerAsync(id);
        if (recurso == null) return NotFound();

        var modelo = new RecursoFormViewModel
        {
            Id = recurso.Id,
            Tipo = recurso.Tipo,
            Codigo = recurso.Codigo,
            Nombre = recurso.Nombre,
            Descripcion = recurso.Descripcion,
            Ubicacion = recurso.Ubicacion,
            Estado = recurso.Estado
        };

        // Pattern matching para copiar los datos propios de cada tipo
        switch (recurso)
        {
            case Equipo e:
                modelo.Marca = e.Marca;
                modelo.Serial = e.Serial;
                break;
            case Sala s:
                modelo.Capacidad = s.Capacidad;
                modelo.Piso = s.Piso;
                break;
        }

        return View(modelo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> Edit(RecursoFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var recurso = await _recursos.ObtenerAsync(modelo.Id);
        if (recurso == null) return NotFound();

        recurso.Codigo = modelo.Codigo;
        recurso.Nombre = modelo.Nombre;
        recurso.Descripcion = modelo.Descripcion;
        recurso.Ubicacion = modelo.Ubicacion;
        recurso.Estado = modelo.Estado;

        if (recurso is Equipo equipo)
        {
            equipo.Marca = modelo.Marca;
            equipo.Serial = modelo.Serial;
        }
        else if (recurso is Sala sala)
        {
            sala.Capacidad = modelo.Capacidad;
            sala.Piso = modelo.Piso;
        }

        try
        {
            await _recursos.ActualizarAsync(recurso);
            TempData["Mensaje"] = "Recurso actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> Delete(int id)
    {
        var recurso = await _recursos.ObtenerAsync(id);
        return recurso == null ? NotFound() : View(recurso);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = RolesApp.Administrador)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _recursos.EliminarAsync(id);
            TempData["Mensaje"] = "Recurso eliminado.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
