using System;
using System.Collections.Generic;
using CommandLine;
using Scripts;
using ScriptsBase.Models;
using ScriptsBase.Utilities;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        RunFolderChecker.EnsureRightRunningFolder("ThriveMods.sln");

        var result = CommandLineHelpers.CreateParser()
            .ParseArguments<ChangesOptions, PackageOptions>(args)
            .MapResult(
                (ChangesOptions options) => RunChangesFinding(options),
                (PackageOptions options) => RunPackage(options),
                CommandLineHelpers.PrintCommandLineErrors);

        ConsoleHelpers.CleanConsoleStateForExit();

        return result;
    }

    private static int RunChangesFinding(ChangesOptions options)
    {
        CommandLineHelpers.HandleDefaultOptions(options);

        ColourConsole.WriteDebugLine("Running changes finding tool");

        return OnlyChangedFileDetector.BuildListOfChangedFiles(options).Result ? 0 : 1;
    }

    private static int RunPackage(PackageOptions options)
    {
        CommandLineHelpers.HandleDefaultOptions(options);

        ColourConsole.WriteDebugLine("Running packaging tool");

        var tokenSource = ConsoleHelpers.CreateSimpleConsoleCancellationSource();

        var packager = new PackageTool(options);

        return packager.Run(tokenSource.Token).Result ? 0 : 1;
    }

    public class ChangesOptions : ChangesOptionsBase
    {
        [Option('b', "branch", Required = false, Default = "master", HelpText = "The git remote branch name")]
        public override string RemoteBranch { get; set; } = "master";
    }

    public class PackageOptions : PackageOptionsBase
    {
        [Option('m', "mod", Required = false, Default = null, MetaValue = "MODS",
            HelpText = "Specify mods to package, default is all.")]
        public IList<OfficialMod>? Mods { get; set; } = new List<OfficialMod>();

        [Option('z', "compress", Default = true,
            HelpText = "Control whether the packages are compressed or left as folders")]
        public bool? CompressRaw { get; set; }

        public override bool Compress => CompressRaw == true;
    }
}
