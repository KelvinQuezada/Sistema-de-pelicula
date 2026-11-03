# Mini sistema de renta de películas con Sakila

Aplicación ASP.NET Core MVC (.NET 10) con ADO.NET (`Microsoft.Data.SqlClient`) sobre la base de datos Sakila en SQL Server.

## Requisitos
- Visual Studio 2022/2026 con la carga de trabajo ASP.NET y SDK de .NET 10
- SQL Server (Express) con la base `sakila` ya cargada

## Ejecución
1. En SSMS, ejecute `Database/01_modificaciones_sakila.sql` (columna `eliminado`, tablas `usuarios` y `login_log`, y dirección genérica para clientes nuevos).
2. Edite la cadena de conexión `Sakila` en `appsettings.Development.json` (no se sube al repositorio) con su servidor y credenciales.
3. Abra `SistemaDePeliculaSakila.slnx` y ejecute con F5. Al iniciar se crean los usuarios de demostración.

| Usuario | Contraseña | Rol |
|---|---|---|
| admin | Admin123* | Administrador |
| cliente1 | Cliente123* | Cliente (customer 1) |
| cliente2 | Cliente123* | Cliente (customer 2) |

## Estructura
- `Controllers/`, `Models/`, `Views/` (con Partial Views en `Views/Shared`)
- `Repositories/`: acceso a datos con ADO.NET y consultas parametrizadas
- `Filters/AutorizarAttribute.cs`: autorización por sesión y rol
- `Services/ProtectorContrasena.cs`: hash PBKDF2 con sal
- `Database/`: scripts SQL (modificaciones y consultas del informe)

## Cuentas
- Los clientes nuevos se crean en `/Cuenta/Registro` (se inserta en `customer` y `usuarios`). El rol Administrador solo existe para cuentas creadas por script o por la aplicación al iniciar.
- Cada intento de login (exitoso o fallido) queda en `login_log`; el administrador lo ve en *Accesos*.
