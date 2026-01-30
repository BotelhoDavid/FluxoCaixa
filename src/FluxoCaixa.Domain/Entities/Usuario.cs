using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Domain.ValueObjects;
using System.Net;
using System.Text.RegularExpressions;

namespace FluxoCaixa.Domain.Entities
{
    /// <summary>
    /// Entidade de domínio que representa um usuário do sistema
    /// </summary>
    public class Usuario : Entity
    {
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // Construtor protegido para EF Core
        protected Usuario() { }

        /// <summary>
        /// Cria um novo usuário com senha
        /// </summary>
        private Usuario(string nome, string email, SenhaHash senhaHash)
        {
            ValidarNome(nome);


            Nome = nome;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            SenhaHash = senhaHash ?? throw new ArgumentNullException(nameof(senhaHash));
            Ativo = true;
            DataUltimoAcesso = null;
        }

        public string Nome { get; private set; }
        public string Email { get; private set; }
        public SenhaHash SenhaHash { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime? DataUltimoAcesso { get; private set; }

        #region Factory Methods

        /// <summary>
        /// Cria um novo usuário (Factory Method)
        /// </summary>
        public static Usuario Criar(string nome, string email, string senha, IPasswordHasher passwordHasher)
        {
            if (passwordHasher == null)
                throw new ArgumentNullException(nameof(passwordHasher));

            ValidarEmail(email);
            ValidarSenha(senha);

            var emailVO = email;
            var senhaHash = passwordHasher.HashPassword(senha);

            return new Usuario(nome, emailVO, senhaHash);
        }

        #endregion

        #region Comportamentos de Domínio

        /// <summary>
        /// Autentica o usuário verificando a senha
        /// </summary>
        public void Autenticar(string senha, IPasswordHasher passwordHasher)
        {
            try
            {
                if (!Ativo)
                    throw new InvalidOperationException("Usuário inativo não pode se autenticar");

                if (passwordHasher == null)
                    throw new ArgumentNullException(nameof(passwordHasher));

                var senhaValida = passwordHasher.VerifyPassword(senha, SenhaHash);

                if (!senhaValida)
                    throw new Exception();

                RegistrarAcesso();
            }
            catch (Exception)
            {
                throw new ApiException(message: "Usuário ou senha inválida",
                                       statusCode: HttpStatusCode.Unauthorized);
            }
        }

        #endregion

        #region Métodos Privados

        private void RegistrarAcesso()
        {
            DataUltimoAcesso = DateTime.Now;
        }

        private void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome não pode ser vazio", nameof(nome));

            if (nome.Length < 3)
                throw new ArgumentException("O nome deve ter no mínimo 3 caracteres", nameof(nome));

            if (nome.Length > 200)
                throw new ArgumentException("O nome deve ter no máximo 200 caracteres", nameof(nome));
        }

        private static void ValidarSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("A senha não pode ser vazia", nameof(senha));

            if (senha.Length < 6)
                throw new ArgumentException("A senha deve ter no mínimo 6 caracteres", nameof(senha));
        }

        private static void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O e-mail não pode ser vazio", nameof(email));

            if (!Regex.IsMatch(email, EmailPattern))
                throw new ArgumentException("Formato de e-mail inválido", nameof(email));
        }

        #endregion
    }
}
