using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace FluxoCaixa.Infra.Data.Services
{
    /// <summary>
    /// Implementação do serviço de domínio para hash de senhas usando ASP.NET Identity
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private readonly IPasswordHasher<object> _passwordHasher;

        public PasswordHasher()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        public SenhaHash HashPassword(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("A senha não pode ser vazia", nameof(senha));

            var hash = _passwordHasher.HashPassword(null, senha);
            return new SenhaHash(hash);
        }

        public bool VerifyPassword(string senha, SenhaHash senhaHash)
        {
            if (string.IsNullOrWhiteSpace(senha))
                return false;

            if (senhaHash == null)
                return false;

            var result = _passwordHasher.VerifyHashedPassword(null, senhaHash.Hash, senha);
            return result == PasswordVerificationResult.Success || 
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
