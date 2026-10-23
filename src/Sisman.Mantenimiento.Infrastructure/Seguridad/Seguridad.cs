using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sisman.Mantenimiento.Application.Abstracciones;
using Sisman.Mantenimiento.Domain.Entidades;
using Sisman.Mantenimiento.Infrastructure.Persistencia;

namespace Sisman.Mantenimiento.Infrastructure.Seguridad;

public class JwtOpciones
{
    public const string Seccion = "Jwt";
    public string Clave { get; set; } = "";
    public string Emisor { get; set; } = "sisman-mantenimiento";
    public string Audiencia { get; set; } = "sisman-mantenimiento";
    public int Minutos { get; set; } = 30;
}

public class HasheadorClave : IHasheadorClave
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hashear(Usuario usuario, string clave) => _hasher.HashPassword(usuario, clave);

    public bool Verificar(Usuario usuario, string clave) =>
        _hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, clave) != PasswordVerificationResult.Failed;
}

public class GeneradorTokenJwt(IOptions<JwtOpciones> opciones, IReloj reloj) : IGeneradorToken
{
    public TokenEmitido Generar(Usuario usuario)
    {
        var o = opciones.Value;
        var vence = reloj.UtcAhora.AddMinutes(o.Minutos);
        var claves = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Clave)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
        };
        var jwt = new JwtSecurityToken(o.Emisor, o.Audiencia, claims, reloj.UtcAhora, vence, claves);
        return new TokenEmitido(new JwtSecurityTokenHandler().WriteToken(jwt), vence);
    }
}

public class UsuarioRepositorio(SismanDbContext db) : IUsuarioRepositorio
{
    public Task<Usuario?> BuscarPorNombreAsync(string nombreUsuario, CancellationToken ct) =>
        db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);
}

public class RelojSistema : IReloj
{
    public DateTime UtcAhora => DateTime.UtcNow;
    public DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);
}
