namespace Sisman.Mantenimiento.Domain.Entidades;

public class Catalogo
{
    public int Id { get; set; }
    public CategoriaCatalogo Categoria { get; set; }
    public string Nombre { get; set; } = "";
}

public class Planta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Area
{
    public int Id { get; set; }
    public int PlantaId { get; set; }
    public string Nombre { get; set; } = "";
}

public class Maquina
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}

public class Sistema
{
    public int Id { get; set; }
    public int MaquinaId { get; set; }
    public string Nombre { get; set; } = "";
}

public class Equipo
{
    public int Id { get; set; }
    public int SistemaId { get; set; }
    public string Nombre { get; set; } = "";
}

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = "";
    public string ClaveHash { get; set; } = "";
    public Perfil Perfil { get; set; }
    public bool Activo { get; set; } = true;
}

public class Correlativo
{
    public string Nombre { get; set; } = "";
    public int UltimoValor { get; set; }
}
