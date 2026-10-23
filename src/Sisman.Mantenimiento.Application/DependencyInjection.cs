using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sisman.Mantenimiento.Application.Autenticacion;
using Sisman.Mantenimiento.Application.Consultas;
using Sisman.Mantenimiento.Application.MantenimientosPreventivos;
using Sisman.Mantenimiento.Application.OrdenesTrabajo;

namespace Sisman.Mantenimiento.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddScoped<AutenticacionServicio>();
        services.AddScoped<MpServicio>();
        services.AddScoped<OtServicio>();
        services.AddScoped<ConsultasServicio>();
        return services;
    }
}
