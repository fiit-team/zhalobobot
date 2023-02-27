namespace Zhalobobot.Bot.Api.Models;

public class Credentials
{
    public string type { get; init; }
    public string project_id { get; init; }
    public string private_key_id { get; init; }
    public string private_key { get; init; }
    public string client_email { get; init; }
    public string client_id { get; init; }
    public string auth_uri { get; init; }
    public string token_uri { get; init; }
    public string auth_provider_x509_cert_url { get; init; }
    public string client_x509_cert_url { get; init; }
}