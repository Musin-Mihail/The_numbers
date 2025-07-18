using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ScriptUsageReporter : EditorWindow
{
    private string _scriptsFolderPath = "Assets/Scripts";
    private Dictionary<string, List<(GameObject go, string path)>> scriptUsages = new();
    private Vector2 _scrollPosition;

    public ScriptUsageReporter(Vector2 scrollPosition)
    {
        _scrollPosition = scrollPosition;
    }

    [MenuItem("Tools/Script Usage Reporter")]
    public static void ShowWindow()
    {
        GetWindow<ScriptUsageReporter>("Script Usage Reporter");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Path to search scripts in:", EditorStyles.boldLabel);
        _scriptsFolderPath = EditorGUILayout.TextField(_scriptsFolderPath);

        if (GUILayout.Button("Find Script Usages In Scene"))
        {
            FindScriptsInScene();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        if (scriptUsages.Count == 0)
        {
            EditorGUILayout.LabelField("No scripts from the specified folder found in this scene.");
        }
        else
        {
            foreach (var entry in scriptUsages)
            {
                EditorGUILayout.LabelField(entry.Key, EditorStyles.boldLabel);
                foreach (var usage in entry.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("    " + usage.path);
                    EditorGUILayout.ObjectField(usage.go, typeof(GameObject), true, GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindScriptsInScene()
    {
        scriptUsages.Clear();
        var allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var script in allScripts)
        {
            var monoScript = MonoScript.FromMonoBehaviour(script);
            if (!monoScript) continue;

            var assetPath = AssetDatabase.GetAssetPath(monoScript);

            if (!assetPath.StartsWith(_scriptsFolderPath)) continue;
            var scriptName = script.GetType().Name;
            if (!scriptUsages.ContainsKey(scriptName))
            {
                scriptUsages[scriptName] = new List<(GameObject, string)>();
            }

            var hierarchyPath = GetHierarchyPath(script.gameObject);
            scriptUsages[scriptName].Add((script.gameObject, hierarchyPath));
        }

        scriptUsages = scriptUsages.OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Repaint();
    }

    private string GetHierarchyPath(GameObject go)
    {
        var pathBuilder = new StringBuilder(go.name);
        var current = go.transform.parent;

        while (current)
        {
            pathBuilder.Insert(0, current.name + "/");
            current = current.parent;
        }

        return pathBuilder.ToString();
    }
}