# Préstamo de equipos y salas — Miniproyecto 2 (Lenguajes de Programación, USC)

Sistema de préstamo de equipos y salas de la Universidad Santiago de Cali. La aplicación web MVC y la aplicación de escritorio Windows Forms reutilizan los servicios de `PrestamosUSC.Core` y pueden trabajar con la misma base de datos SQLite.

## Cómo ejecutarlo

Instala el **SDK de .NET 8**. Para ejecutar la aplicación de escritorio y compilar la solución completa, usa **Windows**. Abre PowerShell en la carpeta que contiene `PrestamosUSC.sln`.

### 1. Iniciar la web y preparar la base de datos

```powershell
$rutaDb = Join-Path (Get-Location).Path 'PrestamosUSC.Web/prestamos.db'
$env:ConnectionStrings__DefaultConnection = "Data Source=$rutaDb"
dotnet run --project PrestamosUSC.Web
```

Abre [http://localhost:5080](http://localhost:5080). Al iniciar, la web aplica las migraciones de EF Core incluidas en el proyecto y carga los roles, los usuarios y los recursos de ejemplo. Si ya tienes una base para este proyecto, usa su ruta absoluta en ambas aplicaciones.

### 2. Iniciar la aplicación de escritorio

Abre otra ventana de PowerShell en la misma carpeta y usa la misma ruta de SQLite:

```powershell
$rutaDb = Join-Path (Get-Location).Path 'PrestamosUSC.Web/prestamos.db'
$env:ConnectionStrings__DefaultConnection = "Data Source=$rutaDb"
dotnet run --project PrestamosUSC.Desktop
```

Inicia sesión con una de las cuentas de prueba. Puedes consultar el catálogo, solicitar un préstamo y ver tus préstamos. Para comprobar que ambas aplicaciones comparten la base, registra un préstamo en Desktop y verifica que aparezca en **Mis préstamos** en la web con la misma cuenta.

Para compilar la solución completa en Windows:

```powershell
dotnet build PrestamosUSC.sln
```

### Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
<<<<<<< HEAD
| Administrador | admin@usc.edu.co | Admin123* |
| Estudiante | estudiante@usc.edu.co | Estudiante123* |

> Nota de versión: los `.csproj` apuntan a `net8.0` y paquetes `8.0.*`. Si tu SDK es .NET 9 o 10, cambia `net8.0` por `net9.0`/`net10.0` y `8.0.*` por `9.0.*`/`10.0.*` en ambos `.csproj`.

=======
| Administrador | admin@usc.edu.co | `Admin123*` |
| Estudiante | estudiante@usc.edu.co | `Estudiante123*` |
>>>>>>> 2bdfdc0 (docs(readme): documenta la aplicacion de escritorio")

## Estructura
```
PrestamosUSC.sln
├── PrestamosUSC.Core          (Class Library)
│   ├── Entities      ApplicationUser, Recurso (abstracta), Equipo, Sala, Prestamo
│   ├── Enums         EstadoRecurso, EstadoPrestamo
│   ├── Migrations    migraciones de EF Core
│   ├── Data          AppDbContext (IdentityDbContext), DbInicializador (seed)
│   ├── Repositories  IRepositorio<T> / Repositorio<T> (genérico)
│   ├── Services      RecursoService, PrestamoService (reglas de negocio), DI
│   ├── Structures    ColaEspera (Queue), HistorialActividad (Stack), ComparadorEstructuras
│   ├── Exceptions    ReglaNegocioException, RecursoOcupadoException
│   └── Extensions    métodos de extensión (estados, fechas)
├── PrestamosUSC.Web           (ASP.NET Core MVC)
│   ├── Controllers   Home, Account, Recursos, Prestamos, Admin
│   ├── Models        ViewModels
│   ├── Views         Razor + Bootstrap 5 (responsive)
│   └── Program.cs    Identity, EF Core, DI
└── PrestamosUSC.Desktop       (Windows Forms)
    ├── Form1.cs                inicio de sesión, catálogo y préstamos
    ├── SolicitudPrestamoForm.cs formulario de solicitud
    └── Program.cs              configuración de Identity, SQLite y Core
```

## Reglas de negocio (PrestamoService)
1. Fin posterior al inicio. 2. No se prestan fechas pasadas. 3. El recurso debe estar Disponible.
4. Duración máxima: Equipo 7 días, Sala 4 horas. 5. Máximo 3 préstamos activos por usuario.
6. Un recurso no puede tener dos préstamos activos traslapados. Si está ocupado se ofrece lista de espera (Queue).

## Roles en la aplicación web
- **Administrador**: CRUD de equipos/salas, ver todos los préstamos, devolver/cancelar cualquiera, cambiar roles, ver estructuras de datos.
- **Estudiante** (rol por defecto al registrarse): ver catálogo, solicitar, devolver/cancelar sus préstamos.

## Cumplimiento del enunciado
| Requisito | Estado |
|---|---|
| Biblioteca de clases con lógica de dominio y negocio | Hecho (`PrestamosUSC.Core`) |
| Patrón MVC en la web | Hecho (`PrestamosUSC.Web`) |
| Entity Framework Core | Hecho (SQLite, TPH, migraciones incluidas) |
| Estructuras de datos justificadas y comparadas | Hecho (Admin > Estructuras de datos) |
| ASP.NET Core Identity con ≥ 2 roles | Hecho (Administrador, Estudiante) |
<<<<<<< HEAD
| Diseño responsive (tablet y smartphone) | Hecho (Bootstrap 5) carpeta /docs |
| Aplicación de escritorio | Pendiente (siguiente fase) |
| Repositorio en GitHub | Hecho: `git init`, commit y push |
| Reporte de páginas + diapositivas (15 min) | Hecho |


tipos anulables · inyección de dependencias · EF Core (TPH, relaciones, migraciones) · ASP.NET Core Identity · lambdas · tuplas/`out`.
=======
| Diseño responsive (tablet y smartphone) | Hecho con Bootstrap 5. [Capturas de celular y tablet](docs/capturas) |
| Aplicación de escritorio | Hecho con Windows Forms (`PrestamosUSC.Desktop`) |
| Repositorio en GitHub | Hecho |
| Informe técnico | Hecho. [Ver informe](docs/informe-tecnico.md) |


## Características de C# y .NET

Clases abstractas, herencia, polimorfismo, interfaces, genéricos, delegados y eventos, LINQ, `async`/`await`, colecciones, records, métodos de extensión, pattern matching, enums, excepciones personalizadas, tipos anulables, inyección de dependencias, EF Core, ASP.NET Core Identity y Windows Forms.

## Esquema sugerido para la sustentación (15 minutos)

1. Problemática (2 min).
2. Justificación (2 min).
3. Metodología (2 min). Explicar la organización en capas, MVC, WinForms, EF Core e Identity.
4. Demostración (6 min). Mostrar el catálogo y el flujo de un préstamo, comprobar el cambio entre escritorio y web, y presentar las capturas responsive.
5. Conclusiones (3 min).
>>>>>>> 2bdfdc0 (docs(readme): documenta la aplicacion de escritorio")
