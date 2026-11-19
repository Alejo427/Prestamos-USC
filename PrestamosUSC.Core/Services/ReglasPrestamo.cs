namespace PrestamosUSC.Core.Services;

/// <summary>Constantes de las reglas de negocio (un solo lugar para cambiarlas).</summary>
public static class ReglasPrestamo
{
    public const int MaxPrestamosActivosPorUsuario = 3;
    public const int ToleranciaMinutosFechaPasada = 5;
}
