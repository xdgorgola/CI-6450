using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;


public class WorldGridInspector : Editor
{
    public VisualTreeAsset m_InspectorXML;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();

        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Pathfinding/Editor/WorldGridInspector.uxml");
        inspector = m_InspectorXML.Instantiate();

        inspector.Q<Button>("generate-graph-button").clicked += GenerateGraph;

        return inspector;
    }

    public void GenerateGraph()
    {
        WorldGrid grid = serializedObject.targetObject as WorldGrid;
        SerializedProperty originP = serializedObject.FindProperty("_origin");
        SerializedProperty columnsP = serializedObject.FindProperty("_columnCount");
        SerializedProperty rowsP = serializedObject.FindProperty("_rowCount");
        SerializedProperty colWP = serializedObject.FindProperty("_columnWidth");
        SerializedProperty rowHP = serializedObject.FindProperty("_rowHeight");

        SerializedProperty graphP = serializedObject.FindProperty("_graph");
        graphP.managedReferenceValue = new TileGraph(originP.vector2Value, columnsP.intValue, rowsP.intValue, colWP.floatValue, rowHP.floatValue);
        graphP.FindPropertyRelative("_origin").vector2Value = originP.vector2Value;

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }
}
