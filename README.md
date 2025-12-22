# BrahmCQRS

Una librería .NET 8.0 modular para implementar arquitectura limpia (Clean Architecture) con el patrón CQRS (Command Query Responsibility Segregation).

## 📋 Descripción

BrahmCQRS proporciona una implementación robusta y genérica del patrón CQRS siguiendo los principios de Clean Architecture. La librería está dividida en **proyectos independientes** que garantizan la separación física de responsabilidades y permiten un consumo flexible mediante NuGet packages.

## 🏗️ Arquitectura

El proyecto está organizado en **4 bibliotecas de clases separadas**:

```
BrahmCQRS/
├── src/
│   ├── BrahmCQRS.Domain/             # 📦 Proyecto independiente (sin dependencias)
│   │   ├── Entities/
│   │   │   └── BaseEntity.cs
│   │   ├── Contracts/
│   │   │   ├── Repositories/
│   │   │   │   ├── ICommandRepository.cs
│   │   │   │   └── IQueryRepository.cs
│   │   │   ├── Specifications/
│   │   │   │   └── ISpecification.cs
│   │   │   └── Common/
│   │   │       └── IPaginatedList.cs
│   │   ├── Specifications/
│   │   │   └── BaseSpecification.cs
│   │   └── Exceptions/
│   │       └── NotFoundException.cs
│   │
│   ├── BrahmCQRS.Application/        # 📦 Proyecto independiente → depende de Domain
│   │   ├── Contracts/Services/
│   │   │   ├── ICommandService.cs
│   │   │   └── IQueryService.cs
│   │   ├── Services/
│   │   │   ├── Commands/
│   │   │   │   └── CommandService.cs
│   │   │   └── Queries/
│   │   │       └── QueryService.cs
│   │   ├── DTOs/
│   │   │   ├── Queries/
│   │   │   │   └── PaginationParameters.cs
│   │   │   └── Common/
│   │   │       └── PaginationMetadata.cs
│   │   └── Common/
│   │       └── PaginatedList.cs
│   │
│   ├── BrahmCQRS.Infrastructure/     # 📦 Proyecto independiente → depende de Domain, Application, Shared
│   │   ├── Persistence/
│   │   │   ├── BaseDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── Base/
│   │   │       │   └── DisposeRepository.cs
│   │   │       ├── CommandRepository.cs
│   │   │       └── QueryRepository.cs
│   │   └── Security/
│   │       └── PasswordHasher.cs
│   │
│   └── BrahmCQRS.Shared/             # 📦 Proyecto independiente (sin dependencias)
│       └── TimeZone/
│           └── ServerTimeProvider.cs
│
├── BrahmCQRS.sln                     # Archivo de solución
└── README.md
```

### 📊 Diagrama de Dependencias

```
┌─────────────────────────────────────┐
│   BrahmCQRS.Infrastructure          │
│   (EF Core, Repositorios)           │
└────────────┬────────────────────────┘
             │ depende de
             ↓
┌─────────────────────────────────────┐
│   BrahmCQRS.Application             │
│   (Servicios CQRS, DTOs)            │
└────────────┬────────────────────────┘
             │ depende de
             ↓
┌─────────────────────────────────────┐
│   BrahmCQRS.Domain                  │
│   (Entidades, Contratos)            │
│   ✓ Sin dependencias externas       │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│   BrahmCQRS.Shared                  │
│   (Utilidades compartidas)          │
│   ✓ Sin dependencias externas       │
└─────────────────────────────────────┘
```

## 🚀 Características

### ✨ Patrón CQRS

- **Command Services**: Operaciones de escritura (Create, Update, Soft Delete, Reactivate)
- **Query Services**: Operaciones de lectura con especificaciones flexibles
- Separación clara de responsabilidades
- **CancellationToken**: Soporte completo para cancelación de operaciones

### 🔧 Repositorios Genéricos

- `ICommandRepository<TEntity>` / `CommandRepository<TEntity>`
- `IQueryRepository<TEntity>` / `QueryRepository<TEntity>`
- Soporte para operaciones CRUD asíncronas con CancellationToken
- JSON Patch con **validación de campos protegidos**
- **Bulk operations** optimizadas (BulkInsert, BulkUpdate)
- **Soft delete y reactivación** con ExecuteUpdateAsync (EF Core 7+)

### 📝 Patrón Specification

- Construcción flexible de consultas
- Soporte para filtros, ordenamiento y paginación
- Includes para carga eager de relaciones
- Filtros globales configurables
- **AsNoTracking configurable** para optimización de performance

### 🔐 Auditoría Automática

- Seguimiento automático de creación y actualización
- Campos: `CreatedBy`, `CreatedDate`, `UpdatedBy`, `UpdatedDate`
- Soft delete con campo `Activated`
- **ICurrentUserService**: Abstracción para obtener usuario autenticado (sin HttpContext en Domain)
- **ITimeProvider**: Abstracción para manejo de zonas horarias configurable

### ⚡ Performance y Thread-Safety

- **ExecuteUpdateAsync** para operaciones bulk sin materialización
- **SemaphoreSlim** en UnitOfWork para operaciones thread-safe
- Repositorios no hacen dispose del DbContext (manejado por DI)
- Optimización de queries con AsNoTracking por defecto

### 🔒 Seguridad

- Hash de contraseñas con BCrypt
- Generación de contraseñas seguras
- Validación de complejidad de contraseñas

### 📄 Paginación

- Soporte completo para paginación
- Metadatos de paginación
- Ordenamiento dinámico

## 📦 Instalación

Puedes instalar los paquetes individualmente según tus necesidades:

### Opción 1: Instalar solo lo que necesitas

```bash
# Solo contratos del dominio (sin dependencias)
dotnet add package BrahmCQRS.Domain

# Servicios CQRS genéricos
dotnet add package BrahmCQRS.Application

# Implementaciones con Entity Framework Core
dotnet add package BrahmCQRS.Infrastructure

# Utilidades (TimeZone, etc.)
dotnet add package BrahmCQRS.Shared
```

### Opción 2: Instalar todo

```bash
# Si quieres toda la funcionalidad
dotnet add package BrahmCQRS.Infrastructure
# (esto incluye las dependencias: Domain, Application y Shared)
```

## 📚 Dependencias por Proyecto

### BrahmCQRS.Domain
```xml
<PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="9.0.4" />
```

### BrahmCQRS.Application
```xml
<ProjectReference Include="..\BrahmCQRS.Domain\BrahmCQRS.Domain.csproj" />
<PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="9.0.4" />
```

### BrahmCQRS.Infrastructure
```xml
<ProjectReference Include="..\BrahmCQRS.Domain\BrahmCQRS.Domain.csproj" />
<ProjectReference Include="..\BrahmCQRS.Application\BrahmCQRS.Application.csproj" />
<ProjectReference Include="..\BrahmCQRS.Shared\BrahmCQRS.Shared.csproj" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.3.0" />
<PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.4" />
<PackageReference Include="System.Linq.Dynamic.Core" Version="1.6.0.2" />
```

### BrahmCQRS.Shared
```xml
<!-- Sin dependencias externas -->
```

## 💻 Uso

### 1. Crear una Entidad

```csharp
using BrahmCQRS.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### 2. Crear un DbContext

```csharp
using BrahmCQRS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<ApplicationDbContext> options)
        : base(httpContextAccessor, options)
    {
    }

    public DbSet<Product> Products { get; set; }
}
```

### 3. Registrar Servicios en el Contenedor de DI

```csharp
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.Services.Commands;
using BrahmCQRS.Application.Services.Queries;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Infrastructure.Persistence.Repositories;

// En Program.cs o Startup.cs
services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
services.AddScoped(typeof(ICommandService<>), typeof(CommandService<>));
services.AddScoped(typeof(IQueryService<>), typeof(QueryService<>));
```

### 4. Crear una Especificación

```csharp
using BrahmCQRS.Domain.Specifications;

public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification(string searchTerm = null)
        : base(p => p.Activated)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Criteria = p => p.Activated && p.Name.Contains(searchTerm);
        }

        AddOrderByDescending(p => p.CreatedDate);
        ApplyPaging(pageIndex: 1, pageSize: 10);
    }
}
```

### 5. Usar los Servicios

#### Comandos (Escritura)

```csharp
public class ProductController : ControllerBase
{
    private readonly ICommandService<Product> _commandService;
    private readonly IQueryService<Product> _queryService;

    public ProductController(
        ICommandService<Product> commandService,
        IQueryService<Product> queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        var created = await _commandService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        product.Id = id;
        var updated = await _commandService.UpdateAsync(product);
        return Ok(updated);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument patchDoc)
    {
        var updated = await _commandService.PatchAsync(id, patchDoc);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _commandService.DeleteAsync(id);
        if (deleted == null)
            return NotFound();

        return NoContent();
    }
}
```

#### Consultas (Lectura)

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var product = await _queryService.GetByIdAsync(id);
    if (product == null)
        return NotFound();

    return Ok(product);
}

[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] string search = null)
{
    var specification = new ActiveProductsSpecification(search);
    var products = await _queryService.GetPaginatedAsync(specification);

    return Ok(products);
}

[HttpGet("count")]
public async Task<IActionResult> Count()
{
    var specification = new ActiveProductsSpecification();
    var count = await _queryService.CountAsync(specification);

    return Ok(new { count });
}
```

### 6. Usar Utilidades

#### Hash de Contraseñas

```csharp
using BrahmCQRS.Infrastructure.Security;

// Encriptar contraseña
string password = "MiContraseña123!";
string hashedPassword = password.EncryptPassword();

// Validar contraseña
bool isValid = hashedPassword.ValidatePassword(password);

// Generar contraseña segura
string generatedPassword = PasswordHasher.GeneratePassword(
    includeLowercase: true,
    includeUppercase: true,
    includeNumeric: true,
    includeSpecial: true,
    includeSpaces: false,
    lengthOfPassword: 16
);
```

#### Zonas Horarias

```csharp
using BrahmCQRS.Shared.TimeZone;

DateTime cstTime = ServerTimeProvider.GetServerTimeCST();
DateTime estTime = ServerTimeProvider.GetServerTimeEST();
DateTime pstTime = ServerTimeProvider.GetServerTimePST();
```

## 🎯 Principios de Clean Architecture

Esta librería sigue los principios de Clean Architecture:

1. **Independencia de Frameworks**: El dominio no depende de frameworks externos
2. **Testeable**: La lógica de negocio puede probarse sin UI, DB, servidor web, etc.
3. **Independencia de UI**: La UI puede cambiar sin cambiar el resto del sistema
4. **Independencia de Base de Datos**: Puedes cambiar de SQL Server a Oracle, MongoDB, etc.
5. **Independencia de cualquier agente externo**: Las reglas de negocio no saben nada del mundo exterior

### Flujo de Dependencias

```
Presentation/API
       ↓
  Application (Services, DTOs)
       ↓
    Domain (Entities, Contracts)
       ↑
Infrastructure (Repositories, DbContext)
```

## 📚 Patrones Implementados

- **CQRS**: Separación de comandos y consultas
- **Repository Pattern**: Abstracción del acceso a datos
- **Specification Pattern**: Consultas flexibles y reutilizables
- **Unit of Work**: A través de DbContext de Entity Framework
- **Dependency Injection**: Todos los servicios se registran en el contenedor DI

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor, asegúrate de:

1. Seguir los principios de Clean Architecture
2. Mantener la separación de responsabilidades CQRS
3. Incluir documentación XML en el código
4. Escribir código limpio y mantenible

## 📄 Licencia

Este proyecto está disponible para uso personal y comercial.

## 🔗 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Specification Pattern](https://en.wikipedia.org/wiki/Specification_pattern)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Desarrollado con ❤️ para la comunidad .NET**
