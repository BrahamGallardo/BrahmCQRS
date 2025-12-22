# 🔐 BrahmCQRS - Módulo de Autenticación

## Descripción

El módulo de autenticación de BrahmCQRS proporciona una solución completa y genérica para autenticación JWT en aplicaciones .NET 8.0, integrada perfectamente con la arquitectura CQRS existente.

## ✨ Características

- **Autenticación JWT** con tokens seguros (HmacSha256)
- **Gestión completa de usuarios** (registro, login, confirmación de email)
- **Cambio y reseteo de contraseña** con tokens temporales
- **Tokens revocados** para invalidar sesiones
- **Roles configurables** con tiempos de expiración personalizados
- **Hash de contraseñas** con BCrypt
- **Verificación de email** obligatoria
- **100% integrado** con la arquitectura CQRS de BrahmCQRS

## 📦 Componentes Implementados

### BrahmCQRS.Domain
- `AuthUser` - Entidad de usuario
- `AuthRole` - Entidad de rol
- `AuthSession` - Sesiones activas
- `RevokedToken` - Tokens invalidados
- `InvalidCredentialsException` - Credenciales inválidas
- `EmailNotVerifiedException` - Email no verificado

### BrahmCQRS.Shared
- `PasswordHasher` - Utilidades para hash de contraseñas con BCrypt

### BrahmCQRS.Application
- **DTOs:** `AuthUserDto`, `SessionDto`, `LoginRequestDto`, `RegisterRequestDto`, etc.
- **Servicios:** `ISessionService`, `IAuthUserService`, `ITokenService`

### BrahmCQRS.Infrastructure
- `JwtSettings` - Configuración JWT
- `TokenService` - Generación y validación de tokens
- `AuthServiceExtensions` - Registro de servicios
- **Specifications:** `GetUserByEmailSpec`, `GetRevokedTokenSpec`

## 🚀 Instalación y Uso

### 1. Configurar appsettings.json

```json
{
  "BrahmCQRS": {
    "Auth": {
      "JWT": {
        "SecretKey": "tu-clave-super-secreta-minimo-32-caracteres-aqui",
        "Issuer": "https://tu-app.com",
        "Audience": "https://tu-app.com",
        "AccessTokenExpirationMinutes": 480,
        "ConfirmationTokenExpirationHours": 24,
        "PasswordResetTokenExpirationHours": 2,
        "RoleTimeouts": {
          "Admin": 480,
          "Customer": 120
        }
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  }
}
```

### 2. Configurar DbContext

```csharp
using BrahmCQRS.Domain.Entities;
using BrahmCQRS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class MiAppDbContext : BaseDbContext
{
    public MiAppDbContext(
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<MiAppDbContext> options)
        : base(httpContextAccessor, options)
    {
    }

    // Entidades de Auth
    public DbSet<AuthUser> AuthUsers { get; set; }
    public DbSet<AuthRole> AuthRoles { get; set; }
    public DbSet<AuthSession> AuthSessions { get; set; }
    public DbSet<RevokedToken> RevokedTokens { get; set; }

    // Tus entidades personalizadas
    public DbSet<MiEntidad> MisEntidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidades de Auth
        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.ToTable("auth_users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<AuthRole>(entity =>
        {
            entity.ToTable("auth_roles");

            // Seed de roles predeterminados
            entity.HasData(
                new AuthRole
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrador del sistema",
                    CreatedDate = DateTime.UtcNow,
                    Activated = true
                },
                new AuthRole
                {
                    Id = 2,
                    Name = "User",
                    Description = "Usuario regular",
                    CreatedDate = DateTime.UtcNow,
                    Activated = true
                }
            );
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("auth_sessions");
            entity.HasOne<AuthUser>()
                  .WithMany()
                  .HasForeignKey(s => s.UserId);
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.ToTable("auth_revoked_tokens");
            entity.HasIndex(e => e.Token);
        });
    }
}
```

### 3. Registrar servicios en Program.cs

```csharp
using BrahmCQRS.Infrastructure.Extensions;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Registrar módulo de autenticación BrahmCQRS
builder.Services.AddBrahmAuth(builder.Configuration);

// Registrar repositorios CQRS
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));

// Registrar DbContext
builder.Services.AddDbContext<MiAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

var app = builder.Build();

// IMPORTANTE: Usar en este orden
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### 4. Crear migraciones

```bash
dotnet ef migrations add InitialAuthModule
dotnet ef database update
```

### 5. Crear controlador de autenticación

```csharp
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAuthUserService _authUserService;

    public AuthController(
        ISessionService sessionService,
        IAuthUserService authUserService)
    {
        _sessionService = sessionService;
        _authUserService = authUserService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var session = await _sessionService.LoginAsync(request);
        return Ok(session);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var user = await _authUserService.RegisterAsync(request);
        return Ok(new { message = "Usuario creado. Revisa tu email para confirmar." });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
    {
        var user = await _authUserService.ConfirmEmailAsync(request.Token);
        return Ok(new { message = "Email confirmado. Revisa tu email para configurar contraseña." });
    }

    [HttpPost("setup-password")]
    public async Task<IActionResult> SetupPassword([FromBody] ResetPasswordDto request)
    {
        var user = await _authUserService.SetupPasswordAsync(request.Token, request.NewPassword);
        return Ok(new { message = "Contraseña configurada correctamente." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        await _authUserService.RequestPasswordResetAsync(email);
        return Ok(new { message = "Si el email existe, recibirás un link de reseteo." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var user = await _authUserService.ResetPasswordAsync(request);
        return Ok(new { message = "Contraseña reseteada correctamente." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _authUserService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Contraseña cambiada correctamente." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _sessionService.LogoutAsync(token, userId);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _authUserService.GetByIdAsync(userId);
        return Ok(user);
    }
}
```

## 🔄 Flujos de Autenticación

### Flujo de Registro

```
1. POST /api/auth/register
   └─> Usuario creado (EmailVerified=false, HasPassword=false)
   └─> Email enviado con ConfirmationToken

2. Usuario hace click en email
   └─> POST /api/auth/confirm-email
   └─> EmailVerified=true
   └─> Email enviado con SetupToken

3. Usuario configura contraseña
   └─> POST /api/auth/setup-password
   └─> HasPassword=true
   └─> Listo para login
```

### Flujo de Login

```
1. POST /api/auth/login
   └─> Valida email + password
   └─> Genera JWT token
   └─> Retorna SessionDto con token y datos de usuario

2. Cliente guarda token
3. Cliente envía token en header: Authorization: Bearer {token}
```

### Flujo de Reset de Contraseña

```
1. POST /api/auth/forgot-password
   └─> Email enviado con ResetToken

2. Usuario hace click en email
   └─> POST /api/auth/reset-password
   └─> Contraseña actualizada
   └─> Token revocado
```

## 🔒 Seguridad

### Password Hashing
- Algoritmo: **BCrypt** con salt automático
- Complejidad: Configurable mediante `PasswordHasher.GeneratePassword()`

### JWT Tokens
- Algoritmo de firma: **HmacSha256**
- Validación completa: Issuer, Audience, Lifetime, Signature
- ClockSkew: 0 (sin tolerancia de tiempo)
- Tokens revocados almacenados en BD

### Claims incluidos en JWT
```csharp
{
    "jti": "guid-unico",
    "sub": "userId",
    "email": "user@example.com",
    "name": "Nombre Usuario",
    "role": "roleId",
    "roleName": "Admin"
}
```

## 🎨 Extensibilidad

### Extender AuthUser con campos personalizados

```csharp
public class MiUsuario : AuthUser
{
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string? Departamento { get; set; }
}

// Usar servicios genéricos
services.AddScoped<ICommandService<MiUsuario>, CommandService<MiUsuario>>();
services.AddScoped<IQueryService<MiUsuario>, QueryService<MiUsuario>>();
```

## 📊 Estructura de Base de Datos

### Tabla: auth_users
```sql
- Id (int, PK)
- Name (string)
- LastName (string, nullable)
- Email (string, unique)
- PasswordHash (string)
- RoleId (int, FK)
- EmailVerified (bool)
- HasPassword (bool)
- Activated (bool)
- CreatedDate (datetime)
- UpdatedDate (datetime)
- CreatedBy (string)
- UpdatedBy (string)
```

### Tabla: auth_roles
```sql
- Id (int, PK)
- Name (string)
- Description (string, nullable)
- Activated (bool)
- CreatedDate (datetime)
```

### Tabla: auth_sessions
```sql
- Id (int, PK)
- UserId (int, FK)
- IsActive (bool)
- ExpiresAt (datetime, nullable)
- CreatedDate (datetime)
```

### Tabla: auth_revoked_tokens
```sql
- Id (int, PK)
- Token (string, indexed)
- RevokedAt (datetime)
- UserId (int, nullable)
- Reason (string, nullable)
- CreatedDate (datetime)
```

## 🛠️ Dependencias

- **System.IdentityModel.Tokens.Jwt** 8.2.1
- **Microsoft.AspNetCore.Authentication.JwtBearer** 8.0.11
- **BCrypt.Net-Next** 4.0.3
- **Microsoft.EntityFrameworkCore** 9.0.4

## 📝 Notas Importantes

1. **SecretKey** debe tener **mínimo 32 caracteres**
2. Los emails se envían mediante `IEmailService` que debe ser implementado por el proyecto consumidor
3. Todos los tokens (confirmación, reset, setup) son **temporales** y se almacenan revocados después de usarse
4. Las sesiones se pueden extender con `RefreshTokenAsync()`
5. El módulo usa las mismas interfaces CQRS del resto de BrahmCQRS

## 🎯 Próximos Pasos

1. Implementar servicio de email en tu proyecto
2. Personalizar templates de email
3. Configurar frontend para consumir los endpoints
4. Implementar refresh token automático
5. Agregar logging de intentos de login

---

**Desarrollado para BrahmCQRS - Arquitectura CQRS para .NET 8.0**
