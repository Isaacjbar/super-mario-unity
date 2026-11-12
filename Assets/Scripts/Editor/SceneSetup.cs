using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates Assets/Scenes/Main.unity (the level itself is built at runtime by GameManager)
/// and registers it in the Build Settings. Also usable from the menu: Tools > Super Mario > Create Scene.
/// </summary>
public static class SceneSetup
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Tools/Super Mario/Create Scene")]
    public static void CreateScene()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Main scene created: " + ScenePath);
    }
}
