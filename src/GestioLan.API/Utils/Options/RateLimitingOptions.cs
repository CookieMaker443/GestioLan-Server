namespace GestioLan.API.Utils.Options;


public class RateLimitingOptions
{
    public int Default { get; set; } = 100;
    public int AI { get; set; } = 1;
    public int WindowMinutes { get; set; } = 1;
}