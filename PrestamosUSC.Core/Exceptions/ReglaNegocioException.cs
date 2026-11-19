namespace PrestamosUSC.Core.Exceptions;

/// <summary>Se lanza cuando se viola una regla de negocio (mensaje apto para mostrar al usuario).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

/// <summary>Caso particular: el recurso ya está prestado en ese horario (permite ofrecer lista de espera).</summary>
public class RecursoOcupadoException : ReglaNegocioException
{
    public int RecursoId { get; }

    public RecursoOcupadoException(int recursoId)
        : base("El recurso ya está prestado en ese horario. Puedes elegir otro horario o unirte a la lista de espera.")
    {
        RecursoId = recursoId;
    }
}
