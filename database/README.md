# GymGo · Base de datos

Toda la gestión de schema se hace con **scripts T-SQL versionados**. No usamos `dotnet ef migrations`.

## Estructura

```
database/
└── sql/
    ├── 00_database/      Creación de la base de datos
    ├── 01_schema/        DDL: CREATE TABLE, indexes, constraints, FKs
    └── 02_seed/          Datos mínimos (tenants y usuarios iniciales)
```

## Convención de nombres

`NN_NombreDescriptivo.sql`

El prefijo numérico determina el orden de ejecución dentro de la carpeta. Cada script es **idempotente**: chequea si el objeto/dato ya existe antes de crearlo, así se puede correr múltiples veces sin romper.

## Orden de ejecución (primera vez)

```
00_database/01_CreateDatabase.sql
01_schema/01_Tenants.sql
01_schema/02_Users.sql
02_seed/01_DefaultData.sql
```

## Cómo ejecutarlos

**Opción A — SQL Server Management Studio (SSMS) / Azure Data Studio:**
1. Conectarse a `(localdb)\MSSQLLocalDB`
2. Abrir cada `.sql` en el orden de arriba
3. F5 (Execute)

**Opción B — `sqlcmd` desde PowerShell:**
```powershell
$server = "(localdb)\MSSQLLocalDB"
$dir = "C:\Adrovez\DevAzure\gymgo-saas\database\sql"

sqlcmd -S $server -i "$dir\00_database\01_CreateDatabase.sql"
sqlcmd -S $server -d GymGoDb_Dev -i "$dir\01_schema\01_Tenants.sql"
sqlcmd -S $server -d GymGoDb_Dev -i "$dir\01_schema\02_Users.sql"
sqlcmd -S $server -d GymGoDb_Dev -i "$dir\02_seed\01_DefaultData.sql"
```

## Credenciales del seed

| Email             | Contraseña en claro | Rol             | Tenant     |
|-------------------|---------------------|-----------------|------------|
| `admin@gymgo.io`  | `Admin#2026`        | PlatformAdmin   | (sin tenant) |
| `owner@demo.gym`  | `Owner#2026`        | GymOwner        | Demo Gym   |

> ⚠️ **Los hashes BCrypt del script `02_seed/01_DefaultData.sql` son PLACEHOLDERS.**
> Antes de poder loguearte, regenerá los hashes reales con BCrypt y reemplazá los del script. En LINQPad o un Console App con `BCrypt.Net-Next`:
>
> ```csharp
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin#2026", 12));
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Owner#2026", 12));
> ```

## Para agregar una nueva tabla / cambio de schema

1. Crear `01_schema/NN_NombreDeLaFeature.sql` (idempotente con `IF OBJECT_ID(...) IS NULL`)
2. Si la entidad mapea desde código C#, agregar/actualizar el `IEntityTypeConfiguration<T>` en `src/GymGo.Infrastructure/Persistence/Configurations/` para que coincida con el schema
3. Si necesita datos seed, agregar `02_seed/NN_...sql`
4. Documentar en este README cualquier paso especial
