using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sisman.Mantenimiento.Web.Api;

var builder = WebApplication.CreateBuilder(args);

var urlApi = builder.Configuration["Api:UrlBase"]
    ?? throw new InvalidOperationException("Falta Api:UrlBase (por ejemplo http://localhost:5300/). Variable de entorno: Api__UrlBase.");

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.AccessDeniedPath = "/login";
        o.Cookie.Name = "sisman.web";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Strict;
        // La cookie no dura más que el token de la API (30 minutos, RN-02).
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        o.SlidingExpiration = false;
    });

// El texto en español (tildes, «») se emite tal cual en vez de como entidades HTML.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(o =>
    o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

builder.Services.AddAuthorization();
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Login");
    o.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient<ApiCliente>(c =>
{
    c.BaseAddress = new Uri(urlApi.EndsWith('/') ? urlApi : urlApi + "/");
    c.Timeout = TimeSpan.FromSeconds(15);
}).AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Si la API rechaza el token (venció), se cierra la sesión de la web y se vuelve al login con un aviso.
app.Use(async (ctx, siguiente) =>
{
    try
    {
        await siguiente(ctx);
    }
    catch (SesionVencidaException)
    {
        await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ctx.Response.Redirect("/login?vencida=1");
    }
});

app.MapRazorPages();
app.Run();

public partial class Program;
