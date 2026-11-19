namespace PrestamosUSC.Core.Structures;

/// <summary>Una persona esperando que se libere un recurso (record = tipo inmutable por valor).</summary>
public record SolicitudEspera(int RecursoId, string UsuarioId, string NombreUsuario, DateTime FechaSolicitud);

public interface IColaEspera
{
    bool Encolar(SolicitudEspera solicitud);
    SolicitudEspera? Siguiente(int recursoId);
    IReadOnlyList<SolicitudEspera> Listar();
}

/// <summary>
/// Lista de espera por recurso. Estructura elegida: Dictionary&lt;int, Queue&lt;T&gt;&gt;.
/// - Queue (FIFO): el primero que pidió es el primero en ser atendido, Enqueue/Dequeue en O(1).
/// - Dictionary: acceso directo a la cola de cada recurso por su Id en O(1).
/// Es singleton (vive mientras corre la aplicación), por eso se protege con lock.
/// </summary>
public class ColaEspera : IColaEspera
{
    private readonly Dictionary<int, Queue<SolicitudEspera>> _colas = new();
    private readonly object _candado = new();

    public bool Encolar(SolicitudEspera solicitud)
    {
        lock (_candado)
        {
            if (!_colas.TryGetValue(solicitud.RecursoId, out var cola))
            {
                cola = new Queue<SolicitudEspera>();
                _colas[solicitud.RecursoId] = cola;
            }

            if (cola.Any(s => s.UsuarioId == solicitud.UsuarioId))
                return false; // ya estaba en la cola

            cola.Enqueue(solicitud);
            return true;
        }
    }

    public SolicitudEspera? Siguiente(int recursoId)
    {
        lock (_candado)
        {
            return _colas.TryGetValue(recursoId, out var cola) && cola.Count > 0
                ? cola.Dequeue()
                : null;
        }
    }

    public IReadOnlyList<SolicitudEspera> Listar()
    {
        lock (_candado)
        {
            return _colas.Values
                .SelectMany(c => c)
                .OrderBy(s => s.FechaSolicitud)
                .ToList();
        }
    }
}
