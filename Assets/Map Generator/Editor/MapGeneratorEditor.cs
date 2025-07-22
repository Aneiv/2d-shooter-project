using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor
{
    ReorderableList regionList;

    void OnEnable()
    {
        regionList = new ReorderableList(
            serializedObject,
            serializedObject.FindProperty("regions"),
            true, true, true, true
        );

        regionList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Terrain Regions");
        };

        regionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = regionList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            float third = rect.width / 3;

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("name"), GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + third, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("height"), GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + 2 * third, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("colour"), GUIContent.none);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        DrawPropertiesExcluding(serializedObject, "regions");

        regionList.DoLayoutList();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            MapGenerator mapGen = (MapGenerator)target;
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }

        MapGenerator mapGenButton = (MapGenerator)target;
        if (GUILayout.Button("Generate"))
        {
            mapGenButton.DrawMapInEditor();
        }
    }

}
