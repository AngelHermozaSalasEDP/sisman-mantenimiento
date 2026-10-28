# ADR-006: Autenticación con JWT propio y dos perfiles

- **Estado:** Aceptada (2026-10-23)
- **Decide:** responsable del proyecto, con propuesta del asistente de IA

## Contexto

En el sistema original, cualquier usuario con sesión ve todo y las pantallas de SISMAN se abrían sin iniciar sesión (de las probadas, solo 2 pedían login). El curso pide seguridad básica con JWT. La seguridad real del sistema original depende de una base y de servicios de la empresa que no forman parte de este repositorio.

## Decisión

1. `POST /api/v1/auth/login` recibe usuario y clave y devuelve un **JWT de acceso** que vence a los **30 minutos** (RN-02). No hay *refresh token* (queda como mejora pendiente).
2. La clave se guarda con **PBKDF2** (`PasswordHasher` de ASP.NET Core Identity, sin las tablas de Identity), nunca en texto ni con un hash simple.
3. Dos perfiles como *claim* `role`: **Planificador** (todo) y **Especialista** (solo consultas) (RN-03). Se aplican como **políticas de autorización** en los controladores; el comportamiento por defecto es **exigir autenticación** (`FallbackPolicy`), de modo que un endpoint nuevo no queda abierto por olvido (RN-01).
4. La clave de firma sale de configuración (variable de entorno o *user-secrets*), de al menos 256 bits; en el repositorio solo hay `appsettings.example.json` con un marcador (RNF-03).
5. Tras tres intentos fallidos seguidos de un usuario, se responde 401 con el mismo mensaje (no se revela si el usuario existe). El bloqueo temporal queda como mejora pendiente.
6. Las credenciales de prueba (una por perfil) se crean con el seed y se documentan en el README; no son claves reales.

## Alternativas descartadas

| Alternativa | Por qué no |
|---|---|
| Reutilizar la seguridad del sistema original (BaanWeb) | No se puede ejecutar fuera de la empresa; contradice la reproducibilidad (RNF-05) |
| ASP.NET Core Identity completo | Trae tablas y pantallas que el módulo no necesita |
| Autenticación con cookies | La API se consume desde clientes que no son un navegador |
| IdentityServer / Keycloak | Excesivo para el alcance y el plazo |

## Consecuencias

- (+) Reproducible con `docker compose up` (sustituido por `dotnet run` con SQL Server local o LocalDB, ADR-007); el 401/403 se prueba sin servicios externos.
- (–) No hay gestión de usuarios (alta, cambio de clave) en la API; el administrador técnico queda fuera del alcance.
- (–) Un token robado vale 30 minutos; no hay revocación.
