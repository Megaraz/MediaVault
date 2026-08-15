var builder = DistributedApplication.CreateBuilder(args);

const string DefaultAndroidApiUrl = "http://192.168.0.12:5210";
var workspaceRoot = LaunchSelection.FindWorkspaceRoot();
var apiProjectPath = Path.Combine(
    workspaceRoot,
    "MediaVault.Api",
    "media-vault-app.API",
    "media-vault-app.API.csproj");
var clientsPath = Path.Combine(workspaceRoot, "MediaVault.Clients");
var mobilePath = Path.Combine(clientsPath, "apps", "mobile");

var preset = LaunchSelection.ResolvePreset();

var runWeb = preset is "web" or "all";
var runAndroid = preset is "android" or "all";

if (preset is not ("api" or "web" or "android" or "all"))
{
    throw new InvalidOperationException(
        $"Unknown MediaVault preset '{preset}'. Expected api, web, android, or all.");
}

var dotnet = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
var npm = OperatingSystem.IsWindows() ? "npm.cmd" : "npm";
var npx = OperatingSystem.IsWindows() ? "npx.cmd" : "npx";
var androidApiUrl = Environment.GetEnvironmentVariable("MEDIAVAULT_ANDROID_API_URL")
    ?? DefaultAndroidApiUrl;

var api = builder.AddExecutable(
        "api",
        dotnet,
        workspaceRoot,
        "run",
        "--project",
        apiProjectPath,
        "--launch-profile",
        "http-otel")
    .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:5210");

if (runWeb)
{
    builder.AddExecutable("web", npm, clientsPath, "run", "dev:web")
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
    builder.AddExecutable("android", npx, mobilePath, "expo", "start", "--lan")
        .WithEnvironment(
            "EXPO_PUBLIC_MEDIA_VAULT_API_URL",
            androidApiUrl)
        .WaitFor(api);
}

builder.Build().Run();
