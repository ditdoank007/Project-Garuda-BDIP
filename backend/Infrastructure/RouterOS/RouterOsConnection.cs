using tik4net;

namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsConnection
{
    public static ITikConnection Create(
        RouterOsOptions options)
    {
        return ConnectionFactory.OpenConnection(
            TikConnectionType.Api,
            options.Host,
            options.Port,
            options.Username,
            options.Password);
    }
}