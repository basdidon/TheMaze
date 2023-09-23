using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using System;


[Serializable]
public class TileCondition
{
    public Sprite Sprite;
    public bool isConnect_N;
    public bool isConnect_S;
    public bool isConnect_W;
    public bool isConnect_E;

    public bool TryGetSprite(int code, out Sprite sprite)
    {
        sprite = null;

        if (GetConditionCode() == code)
        {
            sprite = Sprite;
            return true;
        }

        return false;
    }

    // Helper method to convert conditions to a binary code
    private int GetConditionCode()
    {
        int code = 0;
        code |= (isConnect_N ? 1 : 0) << 3;
        code |= (isConnect_S ? 1 : 0) << 2;
        code |= (isConnect_W ? 1 : 0) << 1;
        code |= (isConnect_E ? 1 : 0);
        return code;
    }
}

[CustomPropertyDrawer(typeof(TileCondition))]
public class TileConditionUIE : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        var spriteProperty = property.FindPropertyRelative("Sprite");

        EditorGUIUtility.wideMode = true;
        EditorGUIUtility.labelWidth = 70;
        rect.height /= 2;
        //spriteProperty = EditorGUI.ObjectField(rect, spriteProperty,typeof(Sprite), new GUIContent());
        rect.y += rect.height;
    }
}
