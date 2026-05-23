using UnityEditor;
using UnityEngine;

public class ServerBuildTools : EditorWindow
{
    [MenuItem("Multiplayer/Build Dedicated Server (Linux)")]
    public static void BuildServerLinux()
    {
        string buildPath = "Builds/Server/Linux/server.x86_64";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { EditorBuildSettings.scenes[0].path },
            locationPathName = buildPath,
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None
        };

        Debug.Log("[Build] Начинается сборка Dedicated Server под Linux...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log($"[Build] Сборка успешно завершена! Файлы билда находятся в: Builds/Server/Linux/");
    }
}
