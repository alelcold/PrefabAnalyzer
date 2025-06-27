using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class PrefabToCSClassGenerator : EditorWindow
{
    private GameObject selectedPrefab;
    private Vector2 scrollPos;
    private StringBuilder scriptContent = new StringBuilder();
    private StringBuilder prefabHierarchy = new StringBuilder();
    private string namespaceName = "Cody"; // 預設 namespace
    private string className = "GeneratedPrefab";

    [MenuItem("Tools/Prefab to Class Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabToCSClassGenerator>("Prefab to C# Class");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab to C# Class Generator", EditorStyles.boldLabel);

        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Select Prefab", selectedPrefab, typeof(GameObject), false);
        namespaceName = EditorGUILayout.TextField("Namespace:", namespaceName);
        className = EditorGUILayout.TextField("Main Class Name:", className);

        if (GUILayout.Button("Analyze Prefab") && selectedPrefab != null)
        {
            AnalyzePrefab(selectedPrefab);
        }

        if (prefabHierarchy.Length > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Prefab Hierarchy:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            EditorGUILayout.TextArea(prefabHierarchy.ToString(), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("Generate C# Classes") && selectedPrefab != null)
        {
            GenerateCSharpScript(selectedPrefab);
        }

        if (scriptContent.Length > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Generated C# Script:", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));
            EditorGUILayout.TextArea(scriptContent.ToString(), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Copy to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = scriptContent.ToString();
                Debug.Log("C# Script copied to clipboard.");
            }

            if (GUILayout.Button("Save as C# File"))
            {
                string path = EditorUtility.SaveFilePanel("Save Script", "Assets", className + ".cs", "cs");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, scriptContent.ToString());
                    AssetDatabase.Refresh();
                    Debug.Log("C# Script saved: " + path);
                }
            }
        }
    }

    private void AnalyzePrefab(GameObject prefab)
    {
        prefabHierarchy.Clear();
        TraverseHierarchy(prefab.transform, 0);
        Debug.Log("Prefab hierarchy analyzed successfully.");
    }

    private void GenerateCSharpScript(GameObject prefab)
    {
        scriptContent.Clear();
        scriptContent.AppendLine("using UnityEngine;");
        scriptContent.AppendLine();
        scriptContent.AppendLine($"namespace {namespaceName}");
        scriptContent.AppendLine("{");

        scriptContent.AppendLine($"    public class {className}");
        scriptContent.AppendLine("    {");

        foreach (Transform child in prefab.transform)
        {
            string childClassName = child.name.Replace(" ", "_");
            scriptContent.AppendLine($"        public {childClassName} {childClassName}Component;");
        }

        scriptContent.AppendLine();
        scriptContent.AppendLine($"        public {className}(GameObject root)");
        scriptContent.AppendLine("        {");

        foreach (Transform child in prefab.transform)
        {
            string childClassName = child.name.Replace(" ", "_");
            scriptContent.AppendLine($"            var {childClassName}Obj = root.transform.Find(\"{child.name}\")?.gameObject;");
            scriptContent.AppendLine($"            if ({childClassName}Obj == null) Debug.LogWarning($\"[AutoGen Warning] Could not find child '{child.name}' under '{{root.name}}'\");");
            scriptContent.AppendLine($"            {childClassName}Component = new {childClassName}({childClassName}Obj);");
        }

        scriptContent.AppendLine("        }");
        scriptContent.AppendLine("    }");

        foreach (Transform child in prefab.transform)
        {
            GenerateClass(child, 1);
        }

        scriptContent.AppendLine("}");
        Debug.Log("C# Classes generated successfully.");
    }

    private void TraverseHierarchy(Transform obj, int depth)
    {
        prefabHierarchy.AppendLine(new string('-', depth * 2) + " " + obj.name);
        for (int i = 0; i < obj.childCount; i++)
        {
            TraverseHierarchy(obj.GetChild(i), depth + 1);
        }
    }

    private void GenerateClass(Transform obj, int depth)
    {
        string indent = new string(' ', depth * 4);
        string className = obj.name.Replace(" ", "_");
        scriptContent.AppendLine($"{indent}public class {className}");
        scriptContent.AppendLine($"{indent}{{");

        scriptContent.AppendLine($"{indent}    private GameObject parentObject;");
        scriptContent.AppendLine($"{indent}    public Transform Transform => parentObject?.transform;");

        for (int i = 0; i < obj.childCount; i++)
        {
            string childName = obj.GetChild(i).name.Replace(" ", "_");
            scriptContent.AppendLine($"{indent}    public GameObject {childName};");
        }

        scriptContent.AppendLine();
        scriptContent.AppendLine($"{indent}    public {className}(GameObject obj)");
        scriptContent.AppendLine($"{indent}    {{");
        scriptContent.AppendLine($"{indent}        parentObject = obj;");

        for (int i = 0; i < obj.childCount; i++)
        {
            Transform child = obj.GetChild(i);
            string childName = child.name.Replace(" ", "_");
            scriptContent.AppendLine($"{indent}        {childName} = parentObject?.transform.Find(\"{child.name}\")?.gameObject;");
            scriptContent.AppendLine($"{indent}        if ({childName} == null) Debug.LogWarning($\"[AutoGen Warning] Could not find child '{child.name}' under '{{parentObject?.name}}'\");");
        }

        scriptContent.AppendLine($"{indent}    }}");
        scriptContent.AppendLine($"{indent}}}");
    }
}
