using Mapster;

namespace FluxoCaixa.Application.Mapster
{
    public class MapsterConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.Default.PreserveReference(true);
            config.Default.IgnoreNullValues(true);
        }
    }
}
