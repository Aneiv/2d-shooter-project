using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class EndlessTerrainEditor : Editor
{
    ReorderableList regionList;

    void OnEnable()
    {
        regionList = new ReorderableList(
            serializedObject,
            serializedObject.FindProperty("objectsSpawner"),
            true, true, true, true
        );

        regionList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Terrain Objects Spawner");
        };

        regionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = regionList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            float third = rect.width / 3;

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("objectPrefab"), GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + third, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("objectSpawnChance"), GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + 2 * third, rect.y, third, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("colours"), GUIContent.none);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        DrawPropertiesExcluding(serializedObject, "Objects spawn settings");

        regionList.DoLayoutList();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
