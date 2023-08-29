namespace Application.Common.Configurations;

public class IdentityServerConfiguration
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public int AccessTokenLifetime { get; set; }

    public string Audience { get; set; }

    public IdentityServerResourceConfiguration[] Resources { get; set; }
}

public class IdentityServerResourceConfiguration
{
    public string Name { get; set; }

    public string[] Scopes { get; set; }
}