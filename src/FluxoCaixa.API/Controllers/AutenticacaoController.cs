using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FluxoCaixa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints para autenticação e registro de usuários")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacaoAppService _autenticacaoAppService;

        public AutenticacaoController(IAutenticacaoAppService autenticacaoAppService)
        {
            _autenticacaoAppService = autenticacaoAppService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Realizar login", 
            Description = "Autentica um usuário existente utilizando e-mail e senha. Em caso de sucesso, retorna um token JWT Bearer necessário para acessar os endpoints protegidos."
        )]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody, SwaggerParameter("Dados de acesso do usuário (E-mail e Senha)", Required = true)] 
            LoginRequest loginRequest)
        {
            var response = await _autenticacaoAppService.AutenticarAsync(loginRequest);
            return Ok(response);
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Registrar novo usuário", 
            Description = "Cria uma nova conta de usuário para acesso ao sistema. Após o registro bem-sucedido, o usuário poderá realizar o login."
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Registrar(
            [FromBody, SwaggerParameter("Dados cadastrais do novo usuário", Required = true)] 
            UsuarioRegistroRequest registroRequest)
        {
            await _autenticacaoAppService.RegistrarAsync(registroRequest);
            return Created("", null);
        }

    }
}
