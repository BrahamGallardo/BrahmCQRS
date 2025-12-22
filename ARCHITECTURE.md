# Arquitectura del Proyecto BrahmCQRS

## 🎯 Decisión Arquitectónica: Proyectos Separados

Este proyecto utiliza **bibliotecas de clases separadas** en lugar de carpetas dentro de un solo proyecto. Esta decisión arquitectónica proporciona múltiples beneficios:

## ✅ Ventajas de la Separación en Proyectos

### 1. **Separación Física Forzada**
```
❌ Carpetas: Cualquiera puede hacer `using Infrastructure` desde Domain
✅ Proyectos: El compilador IMPIDE referencias incorrectas
```

**Ejemplo:**
```csharp
// En Domain/Entities/Product.cs
// ❌ ESTO NO COMPILARÁ si Domain no referencia Infrastructure
using BrahmCQRS.Infrastructure.Persistence; // ERROR DE COMPILACIÓN

// ✅ Solo puedes usar lo que está en tu proyecto o referencias
using BrahmCQRS.Domain.Entities; // OK
```

### 2. **Consumo Flexible**

Los consumidores pueden elegir qué instalar:

```bash
# Escenario 1: Solo quiero los contratos para mi propio proyecto
dotnet add package BrahmCQRS.Domain

# Escenario 2: Quiero servicios pero usaré mi propia capa de datos
dotnet add package BrahmCQRS.Application

# Escenario 3: Quiero todo (implementación completa)
dotnet add package BrahmCQRS.Infrastructure
```

### 3. **Versionado Independiente**

Cada proyecto puede tener su propia versión:

```
BrahmCQRS.Domain            v1.0.0  (estable, raramente cambia)
BrahmCQRS.Application       v1.2.0  (nuevas features)
BrahmCQRS.Infrastructure    v1.3.1  (bugfixes específicos de EF)
BrahmCQRS.Shared            v1.0.0  (estable)
```

### 4. **Dependencias Aisladas**

Cada proyecto solo incluye lo que necesita:

```xml
<!-- Domain: CERO dependencias de infraestructura -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="9.0.4" />
</ItemGroup>

<!-- Infrastructure: Todas las dependencias de datos -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.4" />
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  ...
</ItemGroup>
```

### 5. **Testing Simplificado**

Puedes probar cada capa de forma aislada:

```
BrahmCQRS.Domain.Tests          → Solo lógica de dominio
BrahmCQRS.Application.Tests     → Servicios con mocks de repositorios
BrahmCQRS.Infrastructure.Tests  → Tests de integración con BD
```

### 6. **Build Incremental**

Solo se recompila lo que cambia:

```
Cambio en Domain     → Recompila: Domain, Application, Infrastructure
Cambio en Shared     → Recompila: Shared, Infrastructure
Cambio en Infrastructure → Recompila: Solo Infrastructure
```

## 📊 Comparación: Carpetas vs Proyectos

| Aspecto | Carpetas | Proyectos Separados |
|---------|----------|---------------------|
| **Simplicidad inicial** | ✅ Más simple | ❌ Más setup |
| **Separación forzada** | ❌ Solo convención | ✅ Compilador la fuerza |
| **Consumo modular** | ❌ Todo o nada | ✅ A la carta |
| **NuGet packages** | 1 paquete | 4 paquetes |
| **Dependencias claras** | ❌ No verificables | ✅ Verificables |
| **Escalabilidad** | ❌ Limitada | ✅ Excelente |
| **Testing** | ⚠️ Más complejo | ✅ Más simple |

## 🏗️ Grafo de Dependencias

```
                        ┌──────────────────────┐
                        │   Tu Aplicación      │
                        │   (API/Web/Console)  │
                        └──────────┬───────────┘
                                   │
                   ┌───────────────┼───────────────┐
                   │               │               │
                   ↓               ↓               ↓
        ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
        │ Application  │  │Infrastructure│  │    Shared    │
        │  (Servicios) │  │ (Repos, EF)  │  │ (Utilidades) │
        └──────┬───────┘  └──────┬───────┘  └──────────────┘
               │                 │
               │         ┌───────┘
               │         │
               ↓         ↓
        ┌──────────────────────┐
        │      Domain          │
        │  (Contratos, Modelos)│
        │  ✓ SIN DEPENDENCIAS  │
        └──────────────────────┘
```

## 🔒 Reglas de Dependencia Forzadas

El compilador garantiza estas reglas:

```csharp
✅ Application → Domain         // PERMITIDO
✅ Infrastructure → Domain      // PERMITIDO
✅ Infrastructure → Application // PERMITIDO
✅ Infrastructure → Shared      // PERMITIDO

❌ Domain → Application         // BLOQUEADO POR COMPILADOR
❌ Domain → Infrastructure      // BLOQUEADO POR COMPILADOR
❌ Application → Infrastructure // BLOQUEADO POR COMPILADOR
```

## 🎓 Cuándo Usar Cada Enfoque

### Usa Carpetas Si:
- ✅ Es un proyecto pequeño (<10 clases por capa)
- ✅ Solo tú o tu equipo pequeño lo mantendrá
- ✅ No planeas distribuirlo como librería
- ✅ Quieres simplicidad sobre rigidez

### Usa Proyectos Separados Si:
- ✅ Es una **librería compartida** (como BrahmCQRS)
- ✅ Múltiples equipos consumirán la librería
- ✅ Necesitas **distribución por NuGet**
- ✅ Quieres **garantías del compilador**
- ✅ El proyecto crecerá significativamente
- ✅ Necesitas **versionado independiente**

## 💡 Best Practices

### 1. Mantén Domain Puro
```csharp
// ❌ MAL: Domain depende de frameworks
public interface ICommandRepository<T> where T : DbContext { }

// ✅ BIEN: Domain solo usa abstracciones puras
public interface ICommandRepository<T> where T : class { }
```

### 2. Inyección de Dependencias
```csharp
// En tu Startup.cs o Program.cs
services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
services.AddScoped(typeof(ICommandService<>), typeof(CommandService<>));
services.AddScoped(typeof(IQueryService<>), typeof(QueryService<>));
```

### 3. Referencias Circulares
```
❌ NUNCA hagas esto:
   Application → Infrastructure → Application (CIRCULAR!)

✅ SIEMPRE flujo unidireccional:
   Infrastructure → Application → Domain
```

## 📝 Conclusión

Para **BrahmCQRS**, la separación en proyectos es la elección correcta porque:

1. Es una **librería reutilizable**
2. Será consumida por **múltiples proyectos**
3. Necesita **Clean Architecture estricta**
4. Los beneficios superan la complejidad adicional

Esta decisión arquitectónica garantiza que los principios de Clean Architecture no sean solo una convención, sino una **garantía del compilador**.

---

**Principio SOLID relevante:**
> **Dependency Inversion Principle (DIP)**: Las capas de alto nivel (Domain) no deben depender de las de bajo nivel (Infrastructure). Ambas deben depender de abstracciones.

La separación en proyectos **fuerza** este principio a nivel de compilador. 🚀
