using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Application.Autenticacion;

namespace Sisman.Mantenimiento.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddScoped<AutenticacionServicio>();
        return services;
    }
}
