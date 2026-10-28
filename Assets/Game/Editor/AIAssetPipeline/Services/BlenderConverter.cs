using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Game.Editor.AIAssetPipeline
{
    public static class BlenderConverter
    {
        private const string BlenderPathPreference = "Game.AIAssetPipeline.BlenderPath";

        public static string BlenderPath
        {
            get
            {
                string configured = EditorPrefs.GetString(BlenderPathPreference, string.Empty);
                if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
                    return configured;
                return DetectBlenderPath();
            }
            set { EditorPrefs.SetString(BlenderPathPreference, value ?? string.Empty); }
        }

        public static string DetectBlenderPath()
        {
            string[] knownPaths =
            {
                @"C:\Program Files\Blender Foundation\Blender 3.0\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.5\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.3\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.2\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.1\blender.exe",
                @"C:\Program Files\Blender Foundation\Blender 4.0\blender.exe"
            };

            foreach (string path in knownPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            string root = @"C:\Program Files\Blender Foundation";
            if (!Directory.Exists(root))
                return string.Empty;

            try
            {
                string[] candidates = Directory.GetFiles(root, "blender.exe", SearchOption.AllDirectories);
                return candidates.Length == 0 ? string.Empty : candidates[candidates.Length - 1];
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task ConvertAsync(
            string inputGlbAbsolutePath,
            string outputFbxAbsolutePath,
            CancellationToken cancellationToken)
        {
            string blender = BlenderPath;
            if (string.IsNullOrWhiteSpace(blender) || !File.Exists(blender))
                throw new FileNotFoundException("Blender executable was not found. Configure it in the AI Asset Pipeline window.");
            if (!File.Exists(inputGlbAbsolutePath))
                throw new FileNotFoundException("Input GLB does not exist.", inputGlbAbsolutePath);

            string script = Path.Combine(
                AIAssetPipelinePaths.ProjectRoot,
                "Assets",
                "Game",
                "Editor",
                "AIAssetPipeline",
                "Tools",
                "convert_glb_to_fbx.py");
            if (!File.Exists(script))
                throw new FileNotFoundException("Blender conversion script was not found.", script);

            Directory.CreateDirectory(Path.GetDirectoryName(outputFbxAbsolutePath));
            string arguments = string.Join(" ", new[]
            {
                "--background",
                "--factory-startup",
                "--python", Quote(script),
                "--", Quote(inputGlbAbsolutePath), Quote(outputFbxAbsolutePath)
            });

            ProcessResult result = await Task.Run(() => Run(blender, arguments, cancellationToken), cancellationToken);
            if (result.ExitCode != 0 || !File.Exists(outputFbxAbsolutePath))
            {
                throw new InvalidOperationException(
                    "Blender conversion failed (exit " + result.ExitCode + ").\n" + result.StandardError + "\n" + result.StandardOutput);
            }
        }

        private static ProcessResult Run(string executable, string arguments, CancellationToken cancellationToken)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = AIAssetPipelinePaths.ProjectRoot
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (_, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                        output.AppendLine(eventArgs.Data);
                };
                process.ErrorDataReceived += (_, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                        error.AppendLine(eventArgs.Data);
                };

                if (!process.Start())
                    throw new InvalidOperationException("Blender process could not be started.");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                using (cancellationToken.Register(() =>
                       {
                           try
                           {
                               if (!process.HasExited)
                                   process.Kill();
                           }
                           catch
                           {
                               // Process may exit between HasExited and Kill.
                           }
                       }))
                {
                    process.WaitForExit();
                }

                cancellationToken.ThrowIfCancellationRequested();
                return new ProcessResult(process.ExitCode, output.ToString(), error.ToString());
            }
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private readonly struct ProcessResult
        {
            public readonly int ExitCode;
            public readonly string StandardOutput;
            public readonly string StandardError;

            public ProcessResult(int exitCode, string standardOutput, string standardError)
            {
                ExitCode = exitCode;
                StandardOutput = standardOutput;
                StandardError = standardError;
            }
        }
    }
}
