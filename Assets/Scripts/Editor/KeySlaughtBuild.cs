using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KeySlaught.Editor
{
    public static class KeySlaughtBuild
    {
        private const string BuildOutputArgument = "-buildOutput";
        private const string WebGlBuildRoot = "Builds/WebGL";
        private const string ItchWebGlFolderName = "KeySlaught-itch";
        private const int ItchViewportWidth = 576;
        private const int ItchViewportHeight = 1024;
        private const string PortraitCssMarker = "/* KeySlaught itch portrait shell */";

        [MenuItem("KeySlaught/Build/WebGL for itch.io")]
        public static void BuildWebGl()
        {
            string outputPath = GetOutputPath(Path.Combine(WebGlBuildRoot, ItchWebGlFolderName));
            ConfigureWebGlPortraitPlayerSettings();
            PrepareCleanWebGlOutput(outputPath);
            BuildPlayer(BuildTarget.WebGL, outputPath);
            MakeWebGlHostingPortable(outputPath);

            string archivePath = GetItchArchivePath(outputPath);
            CreatePortableZip(outputPath, archivePath);
            Debug.Log($"KeySlaught itch.io WebGL package created: {archivePath}");
        }

        [MenuItem("KeySlaught/Build/Repackage existing WebGL for itch.io")]
        public static void RepackageExistingWebGlForItch()
        {
            string outputPath = Path.GetFullPath(Path.Combine(WebGlBuildRoot, ItchWebGlFolderName));
            EnsureInsideWebGlBuildRoot(outputPath);
            MakeWebGlHostingPortable(outputPath);

            string archivePath = GetItchArchivePath(outputPath);
            CreatePortableZip(outputPath, archivePath);
            Debug.Log($"KeySlaught itch.io WebGL package refreshed: {archivePath}");
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

        private static void MakeWebGlHostingPortable(string outputPath)
        {
            string indexPath = Path.Combine(outputPath, "index.html");
            string buildDirectory = Path.Combine(outputPath, "Build");

            if (!File.Exists(indexPath) || !Directory.Exists(buildDirectory))
            {
                throw new BuildFailedException(
                    $"The WebGL output at '{outputPath}' is incomplete; expected index.html and a Build folder."
                );
            }

            var renamedFiles = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] compressedFiles = Directory
                .EnumerateFiles(buildDirectory, "*", SearchOption.TopDirectoryOnly)
                .Where(path => path.EndsWith(".br", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string compressedPath in compressedFiles)
            {
                string extension = Path.GetExtension(compressedPath);
                string portablePath = compressedPath.Substring(0, compressedPath.Length - extension.Length);

                using (Stream input = File.OpenRead(compressedPath))
                using (Stream decompressor = CreateDecompressionStream(input, extension))
                using (Stream output = new FileStream(portablePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    decompressor.CopyTo(output);
                }

                renamedFiles[Path.GetFileName(compressedPath)] = Path.GetFileName(portablePath);
                File.Delete(compressedPath);
            }

            string indexHtml = File.ReadAllText(indexPath);
            foreach (KeyValuePair<string, string> renamedFile in renamedFiles)
            {
                indexHtml = indexHtml.Replace(renamedFile.Key, renamedFile.Value);
            }

            indexHtml = ApplyPortraitCanvasMarkup(indexHtml);
            File.WriteAllText(indexPath, indexHtml, new UTF8Encoding(false));
            ApplyPortraitStyles(Path.Combine(outputPath, "TemplateData", "style.css"));
        }

        private static void ConfigureWebGlPortraitPlayerSettings()
        {
            PlayerSettings.defaultWebScreenWidth = ItchViewportWidth;
            PlayerSettings.defaultWebScreenHeight = ItchViewportHeight;
        }

        private static string ApplyPortraitCanvasMarkup(string indexHtml)
        {
            const string viewportMeta =
                "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, viewport-fit=cover\">";

            if (!indexHtml.Contains("<meta name=\"viewport\"", StringComparison.Ordinal))
            {
                indexHtml = indexHtml.Replace(
                    "    <meta charset=\"utf-8\">",
                    $"    <meta charset=\"utf-8\">{Environment.NewLine}{viewportMeta}"
                );
            }

            indexHtml = Regex.Replace(
                indexHtml,
                "(<canvas\\s+id=\"unity-canvas\"\\s+width=)\\d+(\\s+height=)\\d+",
                match =>
                    $"{match.Groups[1].Value}{ItchViewportWidth}{match.Groups[2].Value}{ItchViewportHeight}"
            );
            indexHtml = Regex.Replace(
                indexHtml,
                "canvas\\.style\\.width\\s*=\\s*\"\\d+px\";",
                $"canvas.style.width = \"{ItchViewportWidth}px\";"
            );
            indexHtml = Regex.Replace(
                indexHtml,
                "canvas\\.style\\.height\\s*=\\s*\"\\d+px\";",
                $"canvas.style.height = \"{ItchViewportHeight}px\";"
            );

            return indexHtml;
        }

        private static void ApplyPortraitStyles(string stylePath)
        {
            if (!File.Exists(stylePath))
            {
                throw new BuildFailedException($"WebGL build is missing its stylesheet: {stylePath}");
            }

            string styleSheet = File.ReadAllText(stylePath);
            int existingPortraitStyles = styleSheet.IndexOf(PortraitCssMarker, StringComparison.Ordinal);
            if (existingPortraitStyles >= 0)
            {
                styleSheet = styleSheet.Substring(0, existingPortraitStyles).TrimEnd();
            }

            string portraitStyles = $@"

{PortraitCssMarker}
html, body {{ width: 100%; height: 100%; overflow: hidden; background: #1f7098; }}
#unity-container.unity-desktop, #unity-container.unity-mobile {{
  position: fixed;
  inset: 0;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  transform: none;
}}
#unity-canvas {{
  width: min(100vw, 56.25vh) !important;
  height: min(100vh, 177.7778vw) !important;
  max-width: {ItchViewportWidth}px;
  max-height: {ItchViewportHeight}px;
  aspect-ratio: {ItchViewportWidth} / {ItchViewportHeight};
}}
#unity-footer {{ display: none; }}
";

            File.WriteAllText(stylePath, styleSheet + portraitStyles, new UTF8Encoding(false));
        }

        private static Stream CreateDecompressionStream(Stream input, string extension)
        {
            if (string.Equals(extension, ".br", StringComparison.OrdinalIgnoreCase))
            {
                return new BrotliStream(input, System.IO.Compression.CompressionMode.Decompress, false);
            }

            if (string.Equals(extension, ".gz", StringComparison.OrdinalIgnoreCase))
            {
                return new GZipStream(input, System.IO.Compression.CompressionMode.Decompress, false);
            }

            throw new BuildFailedException($"Unsupported WebGL compression extension: {extension}");
        }

        private static void PrepareCleanWebGlOutput(string outputPath)
        {
            EnsureInsideWebGlBuildRoot(outputPath);

            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            Directory.CreateDirectory(outputPath);
        }

        private static string GetItchArchivePath(string outputPath)
        {
            string parent = Path.GetDirectoryName(outputPath)
                ?? throw new BuildFailedException("The WebGL output folder has no parent directory.");
            string folderName = Path.GetFileName(outputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string archivePath = Path.Combine(parent, $"{folderName}-upload.zip");
            EnsureInsideWebGlBuildRoot(archivePath);
            return archivePath;
        }

        private static void EnsureInsideWebGlBuildRoot(string path)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, WebGlBuildRoot));
            string candidate = Path.GetFullPath(path);
            string allowedPrefix = allowedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            if (!candidate.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException(
                    $"The itch.io WebGL output must be inside '{allowedRoot}', but received '{candidate}'."
                );
            }
        }

        private static void CreatePortableZip(string sourceDirectory, string archivePath)
        {
            string indexPath = Path.Combine(sourceDirectory, "index.html");
            if (!File.Exists(indexPath))
            {
                throw new BuildFailedException($"WebGL build is missing its root index.html: {indexPath}");
            }

            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            using (FileStream archiveStream = new FileStream(archivePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (ZipArchive archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            {
                foreach (string filePath in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                {
                    string entryName = filePath
                        .Substring(sourceDirectory.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    ZipArchiveEntry entry = archive.CreateEntry(
                        entryName,
                        System.IO.Compression.CompressionLevel.Optimal
                    );
                    using (Stream input = File.OpenRead(filePath))
                    using (Stream output = entry.Open())
                    {
                        input.CopyTo(output);
                    }
                }
            }
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
