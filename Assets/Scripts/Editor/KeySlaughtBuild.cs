using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace KeySlaught.Editor
{
    public static class KeySlaughtBuild
    {
        private const string BuildOutputArgument = "-buildOutput";

        [MenuItem("KeySlaught/Build/WebGL")]
        public static void BuildWebGl()
        {
            BuildPlayer(BuildTarget.WebGL, GetOutputPath("Builds/WebGL"));
        }

        [MenuItem("KeySlaught/Build/Windows 64-bit")]
        public static void BuildWindows()
        {
            BuildPlayer(BuildTarget.StandaloneWindows64, GetOutputPath("Builds/Windows/KeySlaught.exe"));
        }

        private static string GetOutputPath(string fallback)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], BuildOutputArgument, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(arguments[index + 1]);
                }
            }

            return Path.GetFullPath(fallback);
        }

        private static void BuildPlayer(BuildTarget target, string outputPath)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are present in Build Settings.");
            }

            string outputDirectory = target == BuildTarget.WebGL
                ? outputPath
                : Path.GetDirectoryName(outputPath) ?? outputPath;

            Directory.CreateDirectory(outputDirectory);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.CleanBuildCache
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{target} build failed with {summary.totalErrors} error(s) and {summary.totalWarnings} warning(s)."
                );
            }
        }
    }
}
