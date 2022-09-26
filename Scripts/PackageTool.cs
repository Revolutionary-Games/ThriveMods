namespace Scripts;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScriptsBase.Models;
using ScriptsBase.ToolBases;
using ScriptsBase.Utilities;
using SharedBase.Utilities;

public class PackageTool : PackageToolBase<Program.PackageOptions>
{
    private static readonly IReadOnlyList<PackagePlatform> ModPlatforms = new List<PackagePlatform>
    {
        PackagePlatform.Linux,
    };

    private static readonly IReadOnlyCollection<FileToPackage> LicenseFiles = new List<FileToPackage>
    {
        new("assets_license.txt"),
        new("LICENSE"),
    };

    private OfficialMod currentlyProcessedMod;
    private bool dynamicGenerated;

    public PackageTool(Program.PackageOptions options) : base(options)
    {
        if (options.SourceCode == true)
        {
            ColourConsole.WriteErrorLine("Source packaging is not implemented for mods");
            options.SourceCode = false;
        }
    }

    protected override IReadOnlyCollection<PackagePlatform> ValidPlatforms => ModPlatforms;

    /// <summary>
    ///   Mods are cross-platform for now, luckily
    /// </summary>
    protected override IEnumerable<PackagePlatform> DefaultPlatforms => ModPlatforms;

    protected override IEnumerable<string> SourceFilesToPackage => throw new NotImplementedException();

    private string ReadmeFile => Path.Join(options.OutputFolder, "README.txt");
    private string RevisionFile => Path.Join(options.OutputFolder, "revision.txt");

    private string ResultZip => Path.Join(options.OutputFolder, "mods.zip");

    public override async Task<bool> Run(CancellationToken cancellationToken)
    {
        if (options.Mods is not { Count: >= 1 })
            options.Mods = Enum.GetValues<OfficialMod>();

        if (options.Mods.Count < 1)
        {
            ColourConsole.WriteErrorLine("No mods to package selected");
            return false;
        }

        if (options.CleanZips && File.Exists(ResultZip))
        {
            ColourConsole.WriteNormalLine($"Deleting {ResultZip} before creating it again");
            File.Delete(ResultZip);
        }

        foreach (var mod in options.Mods)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            ColourConsole.WriteInfoLine($"Starting packaging mod {mod}");
            currentlyProcessedMod = mod;

            if (!await base.Run(cancellationToken))
            {
                ColourConsole.WriteErrorLine("Packaging mod failed");
                return false;
            }

            ColourConsole.WriteSuccessLine($"Successfully packaged {mod}");
        }

        ColourConsole.WriteSuccessLine($"Mods have been packaged into: {ResultZip}");
        return true;
    }

    protected override async Task<bool> OnBeforeStartExport(CancellationToken cancellationToken)
    {
        if (dynamicGenerated)
            return true;

        await CreateDynamicallyGeneratedFiles(cancellationToken);

        dynamicGenerated = true;
        return true;
    }

    protected override string GetFolderNameForExport(PackagePlatform platform)
    {
        return currentlyProcessedMod.ToString();
    }

    protected override string GetCompressedExtensionForPlatform(PackagePlatform platform)
    {
        return ".zip";
    }

    protected override async Task<bool> Export(PackagePlatform platform, string folder,
        CancellationToken cancellationToken)
    {
        var target = GodotTargetFromPlatform(platform);

        ColourConsole.WriteDebugLine($"Using Godot target {target}");

        Directory.CreateDirectory(folder);

        ColourConsole.WriteNormalLine($"Exporting a mod to folder: {folder}");

        var targetFile = Path.Join(folder, currentlyProcessedMod + GodotTargetExtension(platform));

        var startInfo = new ProcessStartInfo("godot");
        startInfo.ArgumentList.Add("--no-window");
        startInfo.ArgumentList.Add("--export");
        startInfo.ArgumentList.Add(target);
        startInfo.ArgumentList.Add(targetFile);

        var result = await ProcessRunHelpers.RunProcessAsync(startInfo, cancellationToken, false);

        if (result.ExitCode != 0)
        {
            ColourConsole.WriteWarningLine("Exporting with Godot failed");
            return false;
        }

        ColourConsole.WriteSuccessLine("Godot export succeeded");
        return true;
    }

    protected override async Task<bool> Compress(PackagePlatform platform, string folder, string archiveFile,
        CancellationToken cancellationToken)
    {
        ColourConsole.WriteNormalLine($"Adding {folder} to {ResultZip}");

        // TODO: this could easily use the C# standard zip library for less dependencies
        var startInfo = new ProcessStartInfo("zip")
        {
            CreateNoWindow = true,
            WorkingDirectory = options.OutputFolder,
        };
        startInfo.ArgumentList.Add("-9");
        startInfo.ArgumentList.Add("-u");
        startInfo.ArgumentList.Add(Path.GetFullPath(ResultZip));
        startInfo.ArgumentList.Add(Path.GetFileName(folder));

        var result = await ProcessRunHelpers.RunProcessAsync(startInfo, cancellationToken, false);

        if (result.ExitCode != 0)
        {
            ColourConsole.WriteErrorLine("Running zip command failed");
            return false;
        }

        return true;
    }

    protected override IEnumerable<FileToPackage> GetFilesToPackage()
    {
        return LicenseFiles;
    }

    private string GodotTargetFromPlatform(PackagePlatform platform)
    {
        switch (platform)
        {
            // Mods are cross-platform so just this is needed
            case PackagePlatform.Linux:
                return "Linux/X11";
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
    }

    private string GodotTargetExtension(PackagePlatform platform)
    {
        switch (platform)
        {
            case PackagePlatform.Linux:
                return string.Empty;
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
    }

    private async Task CreateDynamicallyGeneratedFiles(CancellationToken cancellationToken)
    {
        await using var readme = File.CreateText(ReadmeFile);

        await readme.WriteLineAsync("Official and Example Thrive Mods");
        await readme.WriteLineAsync(string.Empty);
        await readme.WriteLineAsync(
            "This is a packaged release of all official Thrive mods. To install extract the mod's folders you want " +
            "(or all) to the Thrive mods folder (there's a button in the mod manager in Thrive to open that folder)");
        await readme.WriteLineAsync(string.Empty);
        await readme.WriteLineAsync(
            "Source code is available online: https://github.com/Revolutionary-Games/ThriveMods");
        await readme.WriteLineAsync(string.Empty);
        await readme.WriteLineAsync("Exact commit this build is made from is in revision.txt");

        cancellationToken.ThrowIfCancellationRequested();

        await using var revision = File.CreateText(RevisionFile);

        await revision.WriteLineAsync(await GitRunHelpers.Log("./", 1, cancellationToken));
        await revision.WriteLineAsync(string.Empty);

        var diff = (await GitRunHelpers.Diff("./", cancellationToken, false, false)).Trim();

        if (!string.IsNullOrEmpty(diff))
        {
            await readme.WriteLineAsync("dirty, diff:");
            await readme.WriteLineAsync(diff);
        }
    }
}
