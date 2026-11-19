namespace PrestamosUSC.Core.Structures;

public interface IHistorialActividad
{
    void Registrar(string mensaje);
    IReadOnlyList<string> Recientes(int cantidad = 10);
}

/// <summary>
/// Bitácora de actividad reciente. Estructura elegida: Stack&lt;string&gt; (LIFO).
/// Lo último que ocurre queda en la cima, así mostrar "lo más reciente primero"
/// no requiere ordenar ni insertar al inicio de una lista (Push es O(1)).
/// </summary>
public class HistorialActividad : IHistorialActividad
{
    private const int Maximo = 200;
    private readonly Stack<string> _pila = new();
    private readonly object _candado = new();

    public void Registrar(string mensaje)
    {
        lock (_candado)
        {
            _pila.Push($"{DateTime.Now:dd/MM HH:mm} - {mensaje}");

            if (_pila.Count > Maximo)
            {
                // Conserva solo los 100 más recientes (la enumeración de Stack va de la cima hacia abajo)
                var conservar = _pila.Take(100).Reverse().ToArray();
                _pila.Clear();
                foreach (var linea in conservar) _pila.Push(linea);
            }
        }
    }

    public IReadOnlyList<string> Recientes(int cantidad = 10)
    {
        lock (_candado)
        {
            return _pila.Take(cantidad).ToList();
        }
    }
}
