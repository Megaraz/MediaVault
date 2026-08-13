var builder = DistributedApplication.CreateBuilder(args);

const string ApiProjectPath = "../MediaVault.Api/media-vault-app.API/media-vault-app.API.csproj";
const string ClientsPath = "../MediaVault.Clients";

var preset = (Environment.GetEnvironmentVariable("MEDIAVAULT_PRESET") ?? "web")
    .Trim()
    .ToLowerInvariant();

var runWeb = preset is "web" or "all";
var runAndroid = preset is "android" or "all";

if (preset is not ("api" or "web" or "android" or "all"))
{
    throw new InvalidOperationException(
        $"Unknown MediaVault preset '{preset}'. Expected api, web, android, or all.");
}

var api = builder.AddProject("api", ApiProjectPath, options =>
{
    options.LaunchProfileName = "http-otel";
});

var npm = OperatingSystem.IsWindows() ? "npm.cmd" : "npm";

if (runWeb)
{
    builder.AddExecutable("web", npm, ClientsPath, "run", "dev:web")
        .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5210")
        .WithHttpsEndpoint(
            port: 61366,
            targetPort: 61366,
            name: "https",
            isProxied: false)
        .WaitFor(api);
}

if (runAndroid)
{
    builder.AddExecutable("android", npm, ClientsPath, "run", "android")
        .WithEnvironment(
            "EXPO_PUBLIC_MEDIA_VAULT_API_URL",
            "http://10.0.2.2:5210")
        .WaitFor(api);
}

builder.Build().Run();
