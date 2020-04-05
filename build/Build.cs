using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlobExpressions;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.VSTest.VSTestTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => IsWin ? x.WindowsBuild : x.CrossPlatformBuild);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The directory where build artifacts (NuGet packages) will be placed.")]
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";

    [Parameter("The directory where the project source files are located.")]
    readonly AbsolutePath SourceDirectory = RootDirectory / "src";
    
    [Solution] readonly Solution Solution;

    IReadOnlyList<Project> FilesProjects =>
         Solution.GetProjects("Files*").Where(p => p.Is(ProjectType.CSharpProject)).ToList();

    IReadOnlyList<Project> CrossPlatformProjects =>
         FilesProjects
        .Where(p => !p.Name.Contains("Windows"))
        .ToList();

    IReadOnlyList<Project> UwpProjects =>
         FilesProjects
        .Except(CrossPlatformProjects)
        .ToList();

    protected override void OnBuildInitialized()
    {
        Logger.Info($"{FilesProjects.Count} projects are available in total.");

        Logger.Info($"{CrossPlatformProjects.Count} cross-platform projects are available:");
        foreach (var project in CrossPlatformProjects)
        {
            Logger.Info($"  > {project.Name} at {project.Path}");
        }

        Logger.Info($"{UwpProjects.Count} UWP projects are available:");
        foreach (var project in UwpProjects)
        {
            Logger.Info($"  > {project.Name} at {project.Path}");
        }

        if (!IsWin)
        {
            Logger.Warn("The build is not running on Windows. Some projects and configurations will be skipped.");
        }

        Logger.Info("");
        Logger.Info($"Starting build...");
        Logger.Info($"Configuration: {Configuration}");
        Logger.Info($"Source Directory: {SourceDirectory}");
        Logger.Info($"Artifacts Directory: {ArtifactsDirectory}");
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(dir => Directory.Delete(dir, recursive: true));
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CompileCrossPlatformProjects => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            foreach (var project in CrossPlatformProjects)
            {
                DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetNoRestore(true));
            }
        });

    Target TestCrossPlatformProjects => _ => _
        .DependsOn(CompileCrossPlatformProjects)
        .Executes(() =>
        {
            foreach (var project in CrossPlatformProjects)
            {
                DotNetTest(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetLogger("trx")
                    .SetNoBuild(true)
                    .SetNoRestore(true));
            }
        });

    Target CompileUwpProjects => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
            foreach (var project in UwpProjects)
            {
                MSBuild(s => s
                    .SetTargetPath(project)
                    .SetTargets("rebuild")
                    .SetConfiguration(Configuration)
                    .SetRestore(true)
                    .SetMaxCpuCount(Environment.ProcessorCount)
                    .SetNodeReuse(false));
            }
        });

    Target TestUwpProjects => _ => _
        .DependsOn(CompileUwpProjects)
        .Executes(() =>
        {
            foreach (var appxRecipePath in SourceDirectory.GlobFiles("**/*.appxrecipe"))
            {
                VSTest(s => s
                    .SetToolPath(FindVsTestExe())
                    .SetWorkingDirectory(appxRecipePath.Parent)
                    .AddTestAssemblies(Path.GetFileName(appxRecipePath))
                    .SetFramework((VsTestFramework)"FrameworkUap10")
                    .SetLogger("trx"));
            }

            static string FindVsTestExe()
            {
                // For UWP projects we must specifically use a vstest.console.exe from a VS installation
                // and NOT one from a NuGet package (Microsoft.TestPlatform). The tests will not be run
                // otherwise.
                // Simply try to find the latest VS version and glob for the element.
                var vsBasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft Visual Studio"
                );

                var exePath = Glob.Files(vsBasePath, "**/vstest.console.exe", GlobOptions.CaseInsensitive)
                    .OrderBy(path => path)
                    .Last();

                return Path.Combine(vsBasePath, exePath);
            }
        });

    Target Pack => _ => _
        .DependsOn(TestCrossPlatformProjects)
        .DependsOn(TestUwpProjects)
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .OnlyWhenDynamic(() => IsWin)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("pack")
                .SetConfiguration(Configuration)
                .SetPackageOutputPath(ArtifactsDirectory));
        });

    Target CrossPlatformBuild => _ => _
        .DependsOn(TestCrossPlatformProjects);

    Target WindowsBuild => _ => _
        .DependsOn(TestCrossPlatformProjects)
        .DependsOn(TestUwpProjects)
        .DependsOn(Pack);
}
