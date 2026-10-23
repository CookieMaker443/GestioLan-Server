namespace GestioLan.API.Utils.Options;

public class ReverseProxyOptions
{
    public bool Enabled { get; set; } = false;
    public string[] KnownProxies { get; set; } = [];   // es. "172.18.0.2"
    public string[] KnownNetworks { get; set; } = [];  // es. "172.18.0.0/16"
}