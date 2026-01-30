using FluxoCaixa.Application.ViewModels;
using System.Threading.Tasks;

namespace FluxoCaixa.Application.Interfaces
{
    public interface IAutenticacaoAppService
    {
        Task<LoginResponse> AutenticarAsync(LoginRequest loginRequest);
        Task RegistrarAsync(UsuarioRegistroRequest registroRequest);
    }
}

