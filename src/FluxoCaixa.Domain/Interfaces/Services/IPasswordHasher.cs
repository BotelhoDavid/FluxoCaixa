using FluxoCaixa.Domain.ValueObjects;

namespace FluxoCaixa.Domain.Interfaces.Services
{
    /// <summary>
    /// Serviço de domínio responsável por operações de hash e verificação de senha
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Cria um hash a partir de uma senha em texto plano
        /// </summary>
        SenhaHash HashPassword(string senha);

        /// <summary>
        /// Verifica se uma senha em texto plano corresponde ao hash armazenado
        /// </summary>
        bool VerifyPassword(string senha, SenhaHash senhaHash);
    }
}
