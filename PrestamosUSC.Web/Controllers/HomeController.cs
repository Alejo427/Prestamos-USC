using Microsoft.AspNetCore.Mvc;
using PrestamosUSC.Core.Services;

namespace PrestamosUSC.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPrestamoService _prestamos;

    public HomeController(IPrestamoService prestamos) => _prestamos = prestamos;

    public async Task<IActionResult> Index() => View(await _prestamos.ObtenerResumenAsync());

    public IActionResult Error() => View();
}
