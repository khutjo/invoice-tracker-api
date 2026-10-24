using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceTracker.Identity.Infrastructure.Tokens;

public sealed class SigningKeyProvider : IDisposable
{
    private readonly RSA _rsa;

    public SigningKeyProvider(IOptions<JwtOptions> options)
    {
        var jwt = options.Value;
        _rsa = RSA.Create();
        _rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(jwt.SigningKey), out _);

        SigningCredentials = new SigningCredentials(
            new RsaSecurityKey(_rsa) { KeyId = jwt.KeyId },
            SecurityAlgorithms.RsaSha256);

        PublicKey = new RsaSecurityKey(_rsa.ExportParameters(includePrivateParameters: false))
        {
            KeyId = jwt.KeyId
        };
    }

    public SigningCredentials SigningCredentials { get; }

    public RsaSecurityKey PublicKey { get; }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}
