internal static class LaunchSelection
{
    private const string PresetEnvironmentVariable = "MEDIAVAULT_PRESET";
    private const string LaunchModeEnvironmentVariable = "MEDIAVAULT_LAUNCH_MODE";
    private const string DefaultPreset = "web";
    private static readonly string[] ValidPresets = ["api", "web", "android", "all"];

    public static string ResolvePreset()
    {
        var explicitPreset = Normalize(Environment.GetEnvironmentVariable(PresetEnvironmentVariable));
        if (explicitPreset is not null)
        {
            return explicitPreset;
        }

        var workspaceRoot = FindWorkspaceRoot();
        var presetFile = Path.Combine(workspaceRoot, ".mediavault-preset");
        var lastPreset = File.Exists(presetFile)
            ? Normalize(File.ReadAllText(presetFile))
            : null;
        var launchMode = Environment.GetEnvironmentVariable(LaunchModeEnvironmentVariable)
            ?.Trim()
            .ToLowerInvariant();

        var preset = launchMode switch
        {
            "interactive" => Prompt(lastPreset ?? DefaultPreset),
            "last" => lastPreset ?? DefaultPreset,
            _ => DefaultPreset,
        };

        File.WriteAllText(presetFile, preset);
        return preset;
    }

    private static string Prompt(string initialPreset)
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            Console.WriteLine(
                $"Interactive Aspire selection is unavailable in redirected output; using '{initialPreset}'.");
            return initialPreset;
        }

        var selected = PresetToSelection(initialPreset);
        var focusedIndex = 0;

        while (true)
        {
            Render(selected, focusedIndex);
            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    focusedIndex = (focusedIndex + 2) % 3;
                    break;
                case ConsoleKey.DownArrow:
                    focusedIndex = (focusedIndex + 1) % 3;
                    break;
                case ConsoleKey.Spacebar:
                    selected[focusedIndex] = !selected[focusedIndex];
                    break;
                case ConsoleKey.Enter:
                    var preset = SelectionToPreset(selected);
                    if (preset is not null)
                    {
                        Console.WriteLine($"Starting '{preset}'...");
                        return preset;
                    }

                    Console.WriteLine("Select at least one resource before pressing Enter.");
                    break;
                case ConsoleKey.Escape:
                    throw new OperationCanceledException("Aspire launch cancelled.");
            }
        }
    }

    private static void Render(bool[] selected, int focusedIndex)
    {
        Console.Clear();
        Console.WriteLine("MediaVault Aspire launch");
        Console.WriteLine("Use Up/Down to focus, Space to toggle, Enter to launch, Esc to cancel.");
        Console.WriteLine();

        var labels = new[] { "API", "Web", "Android" };
        for (var index = 0; index < labels.Length; index++)
        {
            var focus = index == focusedIndex ? ">" : " ";
            var check = selected[index] ? "[x]" : "[ ]";
            Console.WriteLine($"{focus} {check} {labels[index]}");
        }
    }

    private static bool[] PresetToSelection(string preset) => preset switch
    {
        "api" => [true, false, false],
        "android" => [true, false, true],
        "all" => [true, true, true],
        _ => [true, true, false],
    };

    private static string? SelectionToPreset(bool[] selected)
    {
        if (!selected.Any(static value => value))
        {
            return null;
        }

        var web = selected[1];
        var android = selected[2];
        return (web, android) switch
        {
            (true, true) => "all",
            (true, false) => "web",
            (false, true) => "android",
            _ => "api",
        };
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized is not null && ValidPresets.Contains(normalized)
            ? normalized
            : null;
    }

    internal static string FindWorkspaceRoot()
    {
        var current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MediaVault.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Environment.CurrentDirectory;
    }
}
