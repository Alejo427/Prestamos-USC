using System.Diagnostics;

namespace PrestamosUSC.Core.Structures;

public record ResultadoComparacion(string Escenario, string Estructura, double Milisegundos, string Complejidad);

/// <summary>
/// Mide el tiempo real de varias estructuras de datos para sustentar la decisión final
/// ("ejemplos comparativos" que pide el enunciado). Se muestra en Admin > Estructuras de datos.
/// </summary>
public static class ComparadorEstructuras
{
    public static List<ResultadoComparacion> Ejecutar(int n = 20_000, int busquedas = 1_000)
    {
        var resultados = new List<ResultadoComparacion>();

        // ---- Escenario 1: buscar un código de recurso (peor caso: el último) ----
        var codigos = Enumerable.Range(1, n).Select(i => $"EQ-{i:00000}").ToList();
        var lista = new List<string>(codigos);
        var conjunto = new HashSet<string>(codigos);
        var diccionario = codigos.ToDictionary(c => c, c => c.Length);
        var objetivo = codigos[^1];

        resultados.Add(Medir("Buscar un código existente", "List<string>.Contains", "O(n)", () =>
        {
            for (var i = 0; i < busquedas; i++) lista.Contains(objetivo);
        }));
        resultados.Add(Medir("Buscar un código existente", "HashSet<string>.Contains", "O(1)", () =>
        {
            for (var i = 0; i < busquedas; i++) conjunto.Contains(objetivo);
        }));
        resultados.Add(Medir("Buscar un código existente", "Dictionary<string,int>.TryGetValue", "O(1)", () =>
        {
            for (var i = 0; i < busquedas; i++) diccionario.TryGetValue(objetivo, out _);
        }));

        // ---- Escenario 2: atender la lista de espera (sacar siempre el primero) ----
        var listaEspera = Enumerable.Range(0, n).ToList();
        var colaEspera = new Queue<int>(Enumerable.Range(0, n));

        resultados.Add(Medir("Atender la lista de espera", "List<int>.RemoveAt(0)", "O(n) por extracción", () =>
        {
            while (listaEspera.Count > 0) listaEspera.RemoveAt(0);
        }));
        resultados.Add(Medir("Atender la lista de espera", "Queue<int>.Dequeue", "O(1) por extracción", () =>
        {
            while (colaEspera.Count > 0) colaEspera.Dequeue();
        }));

        // ---- Escenario 3: bitácora con lo más reciente primero ----
        var listaBitacora = new List<string>();
        var pilaBitacora = new Stack<string>();

        resultados.Add(Medir("Registrar actividad (más reciente primero)", "List<string>.Insert(0, x)", "O(n) por inserción", () =>
        {
            for (var i = 0; i < n; i++) listaBitacora.Insert(0, "evento");
        }));
        resultados.Add(Medir("Registrar actividad (más reciente primero)", "Stack<string>.Push", "O(1) por inserción", () =>
        {
            for (var i = 0; i < n; i++) pilaBitacora.Push("evento");
        }));

        return resultados;
    }

    private static ResultadoComparacion Medir(string escenario, string estructura, string complejidad, Action accion)
    {
        var cronometro = Stopwatch.StartNew();
        accion();
        cronometro.Stop();
        return new ResultadoComparacion(escenario, estructura, Math.Round(cronometro.Elapsed.TotalMilliseconds, 3), complejidad);
    }
}
