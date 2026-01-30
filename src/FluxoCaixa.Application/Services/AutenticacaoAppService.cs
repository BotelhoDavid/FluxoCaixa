using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace FluxoCaixa.Application.Services
{
    public class AutenticacaoAppService : IAutenticacaoAppService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AutenticacaoAppService> _logger;

        public AutenticacaoAppService(
            IUsuarioRepository usuarioRepository,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            IUnitOfWork uow,
            ILogger<AutenticacaoAppService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _uow = uow;
            _logger = logger;
        }

        public async Task<LoginResponse> AutenticarAsync(LoginRequest loginRequest)
        {
            _logger.LogInformation("Tentativa de login para o usuário: {Email}", loginRequest.Email);
            var usuario = await _usuarioRepository.GetByEmailAsync(loginRequest.Email);

            if (usuario == null)
            {
                _logger.LogWarning("Usuário não encontrado: {Email}", loginRequest.Email);
                throw new ApiException(message: "Usuário ou senha inválida",
                                       statusCode: HttpStatusCode.Unauthorized);
            }

            try 
            {
                usuario.Autenticar(loginRequest.Password, _passwordHasher);
            }
            catch(ApiException)
            {
                _logger.LogWarning("Falha na autenticação (senha incorreta ou inativo): {Email}", loginRequest.Email);
                throw;
            }

            await _uow.CommitAsync();

            _logger.LogInformation("Usuário autenticado com sucesso: {Email}", loginRequest.Email);
            var token = GerarTokenJwt(usuario);

            return new LoginResponse
            {
                AccessToken = token,
                ExpiresIn = 3600,
                TokenType = "Bearer"
            };
        }

        public async Task RegistrarAsync(UsuarioRegistroRequest registroRequest)
        {
            _logger.LogInformation("Iniciando registro de novo usuário: {Email}", registroRequest.Email);
            bool emailExiste = await _usuarioRepository.ExisteEmailAsync(registroRequest.Email);

            if (emailExiste)
            {
                _logger.LogWarning("Tentativa de registro com e-mail já cadastrado: {Email}", registroRequest.Email);
                throw new ApiException(message: "E-mail já cadastrado",
                                       statusCode: HttpStatusCode.BadRequest);
            }

            var usuario = Usuario.Criar(
                    nome: registroRequest.Nome,
                    email: registroRequest.Email,
                    senha: registroRequest.Password,
                    passwordHasher: _passwordHasher
                );

            await _usuarioRepository.CreateAsync(usuario);

            if (!await _uow.CommitAsync())
            {
                _logger.LogError("Falha ao persistir novo usuário: {Email}", registroRequest.Email);
                throw new ApiException(message: "Erro ao registrar usuário",
                                       statusCode: HttpStatusCode.InternalServerError);
            }
            
            _logger.LogInformation("Usuário registrado com sucesso: {Email}", registroRequest.Email);
        }

        private string GerarTokenJwt(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "ChaveMestraSuperSecretaUtilizadaParaTeste123!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
