using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace OrbitalDefense.EditorTools
{
    public static class WebPlaytestBuilder
    {
        private const string DefaultOutputPath = "Builds/WebPlaytest";
        private const string MainScenePath = "Assets/_Project/Scenes/MainGameplay.unity";

        [CliCommand("orbital_defense_build_web_playtest", "Build the Orbital Defense WebGL playtest", MainThreadRequired = true)]
        public static string BuildWebPlaytest()
        {
            string outputPath = Build(DefaultOutputPath);
            return $"Created WebGL playtest build at {outputPath}.";
        }

        public static void Build()
        {
            Build(GetBuildOutputPath());
        }

        private static string Build(string outputPath)
        {
            Directory.CreateDirectory(outputPath);

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            BuildPlayerOptions options = new()
            {
                scenes = GetEnabledScenes(),
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"WebGL build failed: {report.summary.result}");
            }

            return outputPath;
        }

        private static string[] GetEnabledScenes()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            return scenes.Length > 0 ? scenes : new[] { MainScenePath };
        }

        private static string GetBuildOutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput")
                {
                    return args[i + 1];
                }
            }

            return DefaultOutputPath;
        }
    }
}
