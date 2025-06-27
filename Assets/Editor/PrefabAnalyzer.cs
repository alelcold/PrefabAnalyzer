using UnityEngine;
using UnityEditor;
using System.Text;

public class PrefabNameAnalyzer : EditorWindow
{
    private GameObject selectedPrefab;
    private Vector2 scrollPos;
    private StringBuilder hierarchyString = new StringBuilder();

    //[MenuItem("Tools/Prefab/ Name Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabNameAnalyzer>("Prefab Name Analyzer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Name Analyzer", EditorStyles.boldLabel);
        
        // 拖曳 Prefab
        selectedPrefab = (GameObject)EditorGUILayout.ObjectField("Select Prefab", selectedPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Analyze Prefab") && selectedPrefab != null)
        {
            AnalyzePrefab(selectedPrefab);
        }

        GUILayout.Space(10);
        GUILayout.Label("Prefab Hierarchy:", EditorStyles.boldLabel);
        
        // 滾動區域
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));
        EditorGUILayout.TextArea(hierarchyString.ToString(), GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void AnalyzePrefab(GameObject prefab)
    {
        hierarchyString.Clear();
        TraverseHierarchy(prefab.transform, 0);
    }

    private void TraverseHierarchy(Transform obj, int depth)
    {
        // 縮排 + 只顯示 GameObject 名稱
        hierarchyString.AppendLine(new string('-', depth * 2) + " " + obj.name);

        // 遞歸處理子物件
        for (int i = 0; i < obj.childCount; i++)
        {
            TraverseHierarchy(obj.GetChild(i), depth + 1);
        }
    }
}
