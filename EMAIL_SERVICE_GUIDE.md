# 📧 Email Service - Guía de Uso

## 🎯 Implementación Completa del Servicio de Email

Esta guía explica cómo usar el servicio de email genérico implementado en **BrahmCQRS**.

---

## 📁 Estructura del Proyecto

```
BrahmCQRS/
├── src/
│   ├── BrahmCQRS.Application/
│   │   ├── DTOs/Email/
│   │   │   └── EmailDto.cs                    # DTO genérico de email
│   │   └── Contracts/Services/
│   │       └── IEmailService.cs               # Interfaz del servicio
│   │
│   └── BrahmCQRS.Infrastructure/
│       ├── Configuration/
│       │   ├── SmtpSettings.cs                # Configuración SMTP
│       │   └── EmailResourceSettings.cs       # Rutas de recursos (logos)
│       └── Services/Email/
│           └── EmailService.cs                # Implementación con MailKit
│
└── examples/BrahmCQRS.Example/
    ├── Controllers/
    │   └── EmailController.cs                 # Ejemplo de uso
    ├── appsettings.json                       # Configuración de email
    └── Program.cs                             # Registro de servicios
```

---

## ⚙️ Configuración

### 1. Configurar `appsettings.json`

```json
{
  "Mail": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@example.com",
    "SenderPassword": "your-app-password",
    "SenderName": "Your App Name",
    "EnableSsl": true,
    "EnableTestMode": false
  },
  "Rutas": {
    "Logo": "C:/path/to/your/assets"
  }
}
```

#### Configuración para Gmail

Si usas Gmail, necesitas crear una **App Password**:

1. Ve a tu cuenta de Google → Seguridad
2. Habilita la autenticación de 2 factores
3. Ve a "Contraseñas de aplicación"
4. Genera una nueva contraseña para "Correo"
5. Usa esa contraseña en `SenderPassword`

#### Modo de Pruebas

Para entornos de **testing/staging**, establece `EnableTestMode: true`. Esto previene el envío real de emails.

---

### 2. Registrar el Servicio en `Program.cs`

La forma recomendada es `AddBrahmCQRSCore`, que ya registra `IEmailService` y hace el binding de ambas secciones de configuración (`Mail` y `Rutas`):

```csharp
using BrahmCQRS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registra IEmailService + IOptions<SmtpSettings> + IOptions<EmailResourceSettings>
builder.Services.AddBrahmCQRSCore(builder.Configuration);
```

<details>
<summary>Registro manual (solo si no usas <code>AddBrahmCQRSCore</code>)</summary>

```csharp
using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Infrastructure.Configuration;
using BrahmCQRS.Infrastructure.Services.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection(SmtpSettings.SectionName)
);
builder.Services.Configure<EmailResourceSettings>(
    builder.Configuration.GetSection(EmailResourceSettings.SectionName)
);
builder.Services.AddScoped<IEmailService, EmailService>();
```

</details>

> `AddBrahmCQRSCore` usa `TryAdd`, así que si registras tu propia implementación de `IEmailService` **antes** de llamarlo, la tuya se respeta.

---

## 🚀 Uso del Servicio

### Ejemplo 1: Email Simple

```csharp
public class UserService
{
    private readonly IEmailService _emailService;

    public UserService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendSimpleEmail(string userEmail)
    {
        var emailDto = new EmailDto
        {
            To = userEmail,
            Subject = "Hello World",
            Body = "This is a simple email message.",
            IsHtml = false
        };

        await _emailService.SendEmailAsync(emailDto);
    }
}
```

---

### Ejemplo 2: Email HTML con Logo Embebido

```csharp
public async Task SendWelcomeEmail(string userEmail, string userName)
{
    var emailBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .header {{ text-align: center; border-bottom: 2px solid #007bff; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <img src='cid:logoId' alt='Logo' style='width:200px;' />
            </div>
            <h1>Welcome {userName}!</h1>
            <p>Thank you for joining us.</p>
        </body>
        </html>";

    var emailDto = new EmailDto
    {
        To = userEmail,
        Subject = "Welcome!",
        Body = emailBody,
        IsHtml = true
    };

    await _emailService.SendEmailAsync(emailDto);
}
```

**Nota:** El servicio detecta automáticamente `cid:logoId` en el HTML y adjunta la imagen desde la ruta configurada en `Rutas:Logo`.

---

### Ejemplo 3: Email con Cc y Bcc

```csharp
var emailDto = new EmailDto
{
    To = "user@example.com",
    Cc = "manager@example.com",
    Bcc = "admin@example.com;supervisor@example.com",
    Subject = "Important Notification",
    Body = "<h1>This is an important message</h1>",
    IsHtml = true
};

await _emailService.SendEmailAsync(emailDto);
```

**Nota:** Puedes usar `;` para separar múltiples destinatarios en Cc y Bcc.

---

## 🖼️ Recursos Embebidos (Imágenes)

### Cómo funciona

El servicio detecta automáticamente referencias `cid:logoId` en el cuerpo del email y adjunta la imagen correspondiente.

### Pasos:

1. **Coloca tu imagen** en la ruta configurada en `appsettings.json`:
   ```json
   "Rutas": {
     "Logo": "C:/MyApp/Assets"
   }
   ```

2. **Nombra la imagen** como `image.png` (por defecto)

3. **Usa `cid:logoId`** en tu HTML:
   ```html
   <img src='cid:logoId' alt='Logo' />
   ```

4. El servicio automáticamente:
   - Busca la imagen en `C:/MyApp/Assets/image.png`
   - La embebe como recurso inline
   - La vincula con el `cid:logoId`

---

## 🔧 Extensión para Más Recursos

Si quieres agregar más recursos embebidos (banners, footers, etc.), edita el método `ProcessEmbeddedResources` en `EmailService.cs`:

```csharp
private void ProcessEmbeddedResources(string body, BodyBuilder bodyBuilder)
{
    // Logo existente
    if (body.Contains("cid:logoId") && !string.IsNullOrWhiteSpace(_resourceSettings.Logo))
    {
        var logoPath = Path.Combine(_resourceSettings.Logo, "image.png");
        if (File.Exists(logoPath))
        {
            var linkedResource = new MimePart("image", "png")
            {
                Content = new MimeContent(File.OpenRead(logoPath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                ContentTransferEncoding = ContentEncoding.Base64,
                ContentId = "logoId"
            };
            bodyBuilder.LinkedResources.Add(linkedResource);
        }
    }

    // Nuevo: Banner
    if (body.Contains("cid:bannerId") && !string.IsNullOrWhiteSpace(_resourceSettings.Assets))
    {
        var bannerPath = Path.Combine(_resourceSettings.Assets, "banner.png");
        if (File.Exists(bannerPath))
        {
            var linkedResource = new MimePart("image", "png")
            {
                Content = new MimeContent(File.OpenRead(bannerPath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                ContentTransferEncoding = ContentEncoding.Base64,
                ContentId = "bannerId"
            };
            bodyBuilder.LinkedResources.Add(linkedResource);
        }
    }
}
```

---

## 🧪 Testing

### Endpoint de Ejemplo

El proyecto incluye un `EmailController` con endpoints de prueba:

#### 1. Enviar Email de Prueba
```http
POST /api/email/send-test
Content-Type: application/json

{
  "to": "recipient@example.com",
  "subject": "Test Email",
  "body": "<h1>Hello World</h1>",
  "isHtml": true
}
```

#### 2. Enviar Email de Bienvenida
```http
POST /api/email/send-welcome
Content-Type: application/json

{
  "email": "user@example.com",
  "userName": "John Doe"
}
```

---

## ✅ Ventajas de esta Implementación

1. **Genérico y Reutilizable** - Un solo método para todos los emails
2. **Procesamiento Automático** - Detecta y adjunta recursos embebidos
3. **Configuración Fuerte** - Usa `IOptions<T>` para type safety
4. **Clean Architecture** - Contratos en Application, implementación en Infrastructure
5. **Testeable** - Modo de pruebas para evitar envíos reales
6. **Inyección de Dependencias** - Fácil de usar en cualquier servicio
7. **Extensible** - Fácil agregar más recursos o funcionalidades

---

## 🔐 Seguridad

- **Nunca** subas las credenciales de email al control de versiones
- Usa **variables de entorno** o **Azure Key Vault** para producción
- Considera usar **User Secrets** para desarrollo local:
  ```bash
  dotnet user-secrets init
  dotnet user-secrets set "Mail:SenderPassword" "your-password"
  ```

---

## 📚 Referencias

- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [MimeKit Documentation](https://github.com/jstedfast/MimeKit)
- [Options Pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)

---

## 🆘 Troubleshooting

### Error: "Authentication failed"
- Verifica que `SenderEmail` y `SenderPassword` sean correctos
- Si usas Gmail, asegúrate de usar una App Password
- Verifica que la autenticación de 2 factores esté habilitada

### Error: "Unable to read data from the transport connection"
- Verifica que `SmtpServer` y `SmtpPort` sean correctos
- Prueba con `EnableSsl: false` si estás en red interna

### El logo no aparece
- Verifica que la ruta en `Rutas:Logo` sea correcta
- Asegúrate de que el archivo se llame `image.png`
- Usa rutas absolutas (ej: `C:/MyApp/Assets`)

---

¡Disfruta usando el servicio de email en BrahmCQRS! 🚀
