// Copies Identity's JWT public key into the user-secrets of every other service API.
// Identity signs tokens with the RSA private key (Jwt:SigningKey); other services only get the
// public half (Jwt:PublicKey, base64 SubjectPublicKeyInfo). No key is stored in this file.
//
// Run from the repo root (needs the .NET 10 SDK):
//   dotnet run helpers/set-jwt-public-key.cs                                  all services, key from Identity's user-secrets
//   dotnet run helpers/set-jwt-public-key.cs -- "<private key>"               all services, the key you pass in
//   dotnet run helpers/set-jwt-public-key.cs -- "<private key>" Business      only the services you name
// Pass "" as the key to read it from Identity and still name services.
// The private key can be base64 PKCS#8 or PEM (-----BEGIN PRIVATE KEY-----).
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var services = FindServiceSecretsIds();
var identitySecretsId = services.GetValueOrDefault("Identity")
    ?? throw new InvalidOperationException("Run this from the repo root: Identity.Api's UserSecretsId was not found.");

var privateKey = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : ReadIdentitySigningKey(identitySecretsId);
var targets = args.Length > 1 ? args[1..] : services.Keys.Where(name => name != "Identity").ToArray();

using var rsa = RSA.Create();
rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(StripPem(privateKey)), out _);
var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());

foreach (var target in targets)
{
    if (!services.TryGetValue(target, out var secretsId))
    {
        Console.WriteLine($"Skipped {target}: no {target}.Api project with a UserSecretsId.");
        continue;
    }

    var secrets = ReadSecrets(secretsId);
    if (secrets["Jwt"] is JsonObject jwt)
    {
        jwt["PublicKey"] = publicKey;
    }
    else
    {
        secrets["Jwt:PublicKey"] = publicKey;
    }

    var path = SecretsPath(secretsId);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, secrets.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"{target}: Jwt:PublicKey set.");
}

Console.WriteLine("Public key (use it for Jwt__PublicKey in .env too):");
Console.WriteLine(publicKey);

// Service name (e.g. "Business") -> UserSecretsId, from src/Services/*/InvoiceTracker.*.Api/*.csproj
static Dictionary<string, string> FindServiceSecretsIds()
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var servicesDir = Path.Combine("src", "Services");
    if (!Directory.Exists(servicesDir))
    {
        return result;
    }

    foreach (var csproj in Directory.EnumerateFiles(servicesDir, "InvoiceTracker.*.Api.csproj", SearchOption.AllDirectories))
    {
        var match = Regex.Match(File.ReadAllText(csproj), "<UserSecretsId>(.+?)</UserSecretsId>");
        if (match.Success)
        {
            var name = Path.GetFileNameWithoutExtension(csproj).Split('.')[1];
            result[name] = match.Groups[1].Value.Trim();
        }
    }

    return result;
}

static string ReadIdentitySigningKey(string identitySecretsId)
{
    var identitySecrets = ReadSecrets(identitySecretsId);
    return identitySecrets["Jwt:SigningKey"]?.GetValue<string>()
        ?? identitySecrets["Jwt"]?["SigningKey"]?.GetValue<string>()
        ?? throw new InvalidOperationException("Jwt:SigningKey is not in Identity's user-secrets. Pass the key as an argument instead.");
}

static string StripPem(string key)
{
    return string.Concat(key
        .Split('\n')
        .Where(line => !line.StartsWith("-----", StringComparison.Ordinal))
        .Select(line => line.Trim()));
}

static string SecretsPath(string id)
{
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Microsoft", "UserSecrets", id, "secrets.json");
}

static JsonObject ReadSecrets(string id)
{
    var path = SecretsPath(id);
    return File.Exists(path)
        ? JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? []
        : [];
}
