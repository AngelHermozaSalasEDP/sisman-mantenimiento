namespace Sisman.Mantenimiento.Domain.Excepciones;

public enum TipoError { Validacion, NoEncontrado, Conflicto }

/// <summary>Violación de una regla de negocio; la API la traduce a 400, 404 o 409 (ADR-005).</summary>
public class DominioException(TipoError tipo, string codigo, string mensaje) : Exception(mensaje)
{
    public TipoError Tipo { get; } = tipo;
    public string Codigo { get; } = codigo;

    public static DominioException Validacion(string codigo, string mensaje) => new(TipoError.Validacion, codigo, mensaje);
    public static DominioException NoEncontrado(string codigo, string mensaje) => new(TipoError.NoEncontrado, codigo, mensaje);
    public static DominioException Conflicto(string codigo, string mensaje) => new(TipoError.Conflicto, codigo, mensaje);
}
