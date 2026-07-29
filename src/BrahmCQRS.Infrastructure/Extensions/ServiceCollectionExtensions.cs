using BrahmCQRS.Application.Contracts.Services;
using BrahmCQRS.Application.Services.Commands;
using BrahmCQRS.Application.Services.Queries;
using BrahmCQRS.Domain.Contracts.Repositories;
using BrahmCQRS.Domain.Contracts.UnitOfWork;
using BrahmCQRS.Infrastructure.Configuration;
using BrahmCQRS.Infrastructure.Persistence.Repositories;
using BrahmCQRS.Infrastructure.Persistence.UnitOfWork;
using BrahmCQRS.Infrastructure.Services;
using BrahmCQRS.Infrastructure.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrahmCQRS.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering the BrahmCQRS core building blocks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the BrahmCQRS core services: generic CQRS repositories and services,
    /// unit of work, current user service, time provider, email service and its settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration used to bind email settings.</param>
    /// <param name="serverTimeZone">
    /// The timezone used by <see cref="ITimeProvider"/> for audit timestamps.
    /// Defaults to Central Standard Time when null.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Every registration uses TryAdd, so registering your own implementation before
    /// calling this method takes precedence.
    /// <para>
    /// The consuming application only needs to add the DbContext bridge, because the
    /// generic repositories depend on the base <c>DbContext</c> type:
    /// </para>
    /// <code>
    /// builder.Services.AddDbContext&lt;YourDbContext&gt;(options => options.UseSqlServer(cs));
    /// builder.Services.AddScoped&lt;DbContext&gt;(sp => sp.GetRequiredService&lt;YourDbContext&gt;());
    /// builder.Services.AddBrahmCQRSCore(builder.Configuration);
    /// </code>
    /// <para>
    /// Email settings are bound from the "Mail" and "Rutas" configuration sections
    /// (see <see cref="SmtpSettings.SectionName"/> and <see cref="EmailResourceSettings.SectionName"/>).
    /// </para>
    /// </remarks>
    public static IServiceCollection AddBrahmCQRSCore(
        this IServiceCollection services,
        IConfiguration configuration,
        TimeZoneInfo? serverTimeZone = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Generic CQRS repositories (open generics).
        services.TryAddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
        services.TryAddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));

        // Generic CQRS services (open generics).
        services.TryAddScoped(typeof(ICommandService<>), typeof(CommandService<>));
        services.TryAddScoped(typeof(IQueryService<>), typeof(QueryService<>));

        // Persistence. Fully qualified because the type and its namespace share a name.
        services.TryAddScoped<IUnitOfWork, Persistence.UnitOfWork.UnitOfWork>();

        // Cross-cutting services.
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();
        services.TryAddSingleton<ITimeProvider>(_ => serverTimeZone is null
            ? new Services.TimeProvider()
            : new Services.TimeProvider(serverTimeZone));

        // Email.
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<EmailResourceSettings>(configuration.GetSection(EmailResourceSettings.SectionName));
        services.TryAddScoped<IEmailService, EmailService>();

        return services;
    }
}
