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
    public string someText = "hehe";

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
    ObjectField spriteObjectField;
    VisualElement objectFieldDisplay;
    VisualElement objectFieldInput;
    Image spritePreview;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/CustomTileRuleEditor.uxml");
        var root = assetTree.Instantiate();

        spriteObjectField = root.Q<ObjectField>();
        spriteObjectField.RegisterValueChangedCallback(OnSpriteChanged);
        objectFieldDisplay = root.Q<VisualElement>(null,new string[] { ObjectField.objectUssClassName });
        objectFieldDisplay.style.display = DisplayStyle.None;
        objectFieldInput = root.Q<VisualElement>(null, new string[] { ObjectField.inputUssClassName });
        spritePreview = new Image();
        objectFieldInput.Insert(0,spritePreview);
        //spritePreview.image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/fridge.png");

        GameObject selectedObject = Selection.activeObject as GameObject;
        if (selectedObject != null)
        {
            SerializedObject so = new(selectedObject);
            // Bind it to the root of the hierarchy. It will find the right object to bind to.
            root.Bind(so);
        }
        else
        {
            root.Unbind();
        }

        return root;
        //return base.CreatePropertyGUI(property);
    }

    void OnSpriteChanged(ChangeEvent<UnityEngine.Object> callback)
    {
        if(callback.newValue is Sprite sprite)
        {
            spritePreview.sprite = sprite;
        }
    }
}
