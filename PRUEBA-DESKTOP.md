# Guía de ejecución y prueba de Préstamos USC Desktop

Préstamos USC Desktop es una aplicación de Windows Forms para consultar el catálogo de equipos y salas, registrar préstamos, devolverlos y cancelarlos. Comparte la base de datos SQLite y las cuentas de usuario con la aplicación web.

## Requisitos

- Windows.
- SDK de .NET 8.
- Una copia local del repositorio.

## Ejecutar la aplicación

Abre PowerShell en la carpeta que contiene `PrestamosUSC.sln`. Ejecuta estos comandos para configurar la base de datos compartida e iniciar la aplicación web. La web prepara la base y carga los datos de prueba al iniciar.

```powershell
$rutaDb = Join-Path (Get-Location).Path 'PrestamosUSC.Web\prestamos.db'
$env:ConnectionStrings__DefaultConnection = "Data Source=$rutaDb"
dotnet run --project .\PrestamosUSC.Web
```

Si ya tienes una base de datos en otra ubicación, usa su ruta absoluta en `$rutaDb`.

Cuando la terminal muestre la dirección local de la web, presiona **Ctrl + C** para detenerla. En la misma terminal, ejecuta la aplicación de escritorio.

```powershell
dotnet run --project .\PrestamosUSC.Desktop
```

## Probar las funciones principales

Las credenciales de prueba están en el [README del repositorio](README.md).

1. Ingresa como **Estudiante**, selecciona un recurso disponible y registra un préstamo con fecha de inicio futura. Comprueba que aparezca en **Mis préstamos**.
2. Cancela ese préstamo antes de su hora de inicio y verifica que su estado cambie a **Cancelado**.
3. Registra otro préstamo, selecciona **Devolver** y verifica que su estado cambie a **Devuelto**.
4. Cierra sesión e ingresa como **Administrador**. Comprueba que los registros del estudiante aparezcan en **Todos los préstamos**.
5. Cierra la aplicación de escritorio y vuelve a iniciar la web desde la misma terminal. Comprueba que allí se vean los cambios realizados desde el escritorio.