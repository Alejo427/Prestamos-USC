# Préstamo de equipos y salas — Miniproyecto 2 (Lenguajes de Programación, USC)

Solución .NET con **biblioteca de clases** (dominio, negocio y EF Core) y **aplicación web MVC** (ASP.NET Core Identity, 2 roles, diseño responsive).
La aplicación de escritorio se agrega después como tercer proyecto reutilizando `PrestamosUSC.Core`.

## Cómo ejecutarlo

Requisitos: SDK de .NET 8 (o superior, ver nota) y, opcional, la herramienta de EF: `dotnet tool install --global dotnet-ef`.

```bash
dotnet restore
dotnet build
dotnet run --project PrestamosUSC.Web
```
Abre http://localhost:5080. La base `prestamos.db` (SQLite) se crea sola con roles, usuarios y datos de ejemplo.

**Usuarios de prueba**

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@usc.edu.co | Admin123* |
| Estudiante | estudiante@usc.edu.co | Estudiante123* |

> Nota de versión: los `.csproj` apuntan a `net8.0` y paquetes `8.0.*`. Si tu SDK es .NET 9 o 10, cambia `net8.0` por `net9.0`/`net10.0` y `8.0.*` por `9.0.*`/`10.0.*` en ambos `.csproj`.


## Estructura
```
PrestamosUSC.sln
├── PrestamosUSC.Core          (Class Library)
│   ├── Entities      ApplicationUser, Recurso (abstracta), Equipo, Sala, Prestamo
│   ├── Enums         EstadoRecurso, EstadoPrestamo
│   ├── Data          AppDbContext (IdentityDbContext), DbInicializador (seed)
│   ├── Repositories  IRepositorio<T> / Repositorio<T> (genérico)
│   ├── Services      RecursoService, PrestamoService (reglas de negocio), DI
│   ├── Structures    ColaEspera (Queue), HistorialActividad (Stack), ComparadorEstructuras
│   ├── Exceptions    ReglaNegocioException, RecursoOcupadoException
│   └── Extensions    métodos de extensión (estados, fechas)
└── PrestamosUSC.Web           (ASP.NET Core MVC)
    ├── Controllers   Home, Account, Recursos, Prestamos, Admin
    ├── Models        ViewModels
    ├── Views         Razor + Bootstrap 5 (responsive)
    └── Program.cs    Identity, EF Core, DI
```

## Reglas de negocio (PrestamoService)
1. Fin posterior al inicio. 2. No se prestan fechas pasadas. 3. El recurso debe estar Disponible.
4. Duración máxima: Equipo 7 días, Sala 4 horas. 5. Máximo 3 préstamos activos por usuario.
6. Un recurso no puede tener dos préstamos activos traslapados. Si está ocupado se ofrece lista de espera (Queue).

## Roles
- **Administrador**: CRUD de equipos/salas, ver todos los préstamos, devolver/cancelar cualquiera, cambiar roles, ver estructuras de datos.
- **Estudiante** (rol por defecto al registrarse): ver catálogo, solicitar, devolver/cancelar sus préstamos.

## Cumplimiento del enunciado
| Requisito | Estado |
|---|---|
| Biblioteca de clases con lógica de dominio y negocio | Hecho (`PrestamosUSC.Core`) |
| Patrón MVC en la web | Hecho (`PrestamosUSC.Web`) |
| Entity Framework Core | Hecho (SQLite, TPH, migraciones opcionales) |
| Estructuras de datos justificadas y comparadas | Hecho (Admin > Estructuras de datos) |
| ASP.NET Core Identity con ≥ 2 roles | Hecho (Administrador, Estudiante) |
| Diseño responsive (tablet y smartphone) | Hecho (Bootstrap 5); falta tomar las capturas |
| Aplicación de escritorio | Pendiente (siguiente fase) |
| Repositorio en GitHub | Pendiente: `git init`, commit y push |
| Reporte de páginas + diapositivas (15 min) | Pendiente (se arma con este proyecto) |


tipos anulables · inyección de dependencias · EF Core (TPH, relaciones, migraciones) · ASP.NET Core Identity · lambdas · tuplas/`out`.

## Esquema sugerido para los 15 minutos
1. Problemática (2 min) 2. Justificación (2 min) 3. Metodología (2 min: MVC, capas, EF Core, Identity)
4. Demostración (6 min: login por rol, catálogo, préstamo, traslape + lista de espera, devolución, vista de estructuras, vista en celular)
5. Conclusiones (3 min). Recordar: solo diapositivas, sin otro dispositivo de apoyo.
