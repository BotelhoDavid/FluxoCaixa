using Swashbuckle.AspNetCore.Annotations;

namespace FluxoCaixa.Application.ViewModels
{
    public class LoginRequest
    {
        /// <summary>
        /// E-mail do usuário
        /// </summary>
        /// <example>usuario@exemplo.com</example>
        [SwaggerSchema(Description = "E-mail do usuário", Format = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário
        /// </summary>
        /// <example>senhaSegura123</example>
        [SwaggerSchema(Description = "Senha do usuário", Format = "password")]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        /// <summary>
        /// Token JWT para autenticação
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6...</example>
        [SwaggerSchema(Description = "Token JWT para autenticação")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Tempo de expiração em segundos
        /// </summary>
        /// <example>3600</example>
        [SwaggerSchema(Description = "Tempo de expiração em segundos")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Tipo do token
        /// </summary>
        /// <example>Bearer</example>
        [SwaggerSchema(Description = "Tipo do token")]
        public string TokenType { get; set; }
    }

    public class UsuarioRegistroRequest
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        /// <example>João Silva</example>
        [SwaggerSchema(Description = "Nome completo do usuário")]
        public string Nome { get; set; }

        /// <summary>
        /// E-mail válido
        /// </summary>
        /// <example>joao.silva@example.com</example>
        [SwaggerSchema(Description = "E-mail válido", Format = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Senha forte (mínimo 6 caracteres)
        /// </summary>
        /// <example>Senha@123</example>
        [SwaggerSchema(Description = "Senha forte (mínimo 6 caracteres)", Format = "password")]
        public string Password { get; set; }
    }
}

