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
    int count = 0;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        count = 0;

        // Load Uxml
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/CustomTileRuleEditor.uxml");
        var root = assetTree.Instantiate();

        VisualElement objectFieldDisplay = root.Q<VisualElement>(null, new string[] { ObjectField.objectUssClassName });
        VisualElement objectFieldDisplayIcon = objectFieldDisplay.Q<Image>();
        //objectFieldDisplayIcon.style.display = DisplayStyle.None;
        objectFieldDisplayIcon.RemoveFromClassList("unity-object-field-display__icon");
        objectFieldDisplayIcon.RemoveFromClassList("unity-image");
        VisualElement objectFieldInput = root.Q<VisualElement>(null, new string[] { ObjectField.inputUssClassName });

        Label name = new()
        {
            text = count.ToString()
        };
        
        Image spritePreview = new ();
        objectFieldInput.Insert(0, spritePreview);

        ObjectField spriteObjectField = root.Q<ObjectField>();
        spriteObjectField.RegisterValueChangedCallback((ev)=> {
            if (ev.newValue is Sprite sprite)
            {
                spritePreview.sprite = sprite;
                count++;
                name.text = count.ToString();
            }
        });
        
        root.Add(name);

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
    }
}