using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[Serializable]
public class TileCondition
{
    public Tile Tile;
    public TileRulePicker rulePicker;

    public bool TryGetTile(TileConnection connection, out Tile tile)
    {
        tile = null;

        if(rulePicker.IsMacth(connection))
        {
            tile = Tile;
            return true;
        }

        return false;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TileCondition))]
public class TileConditionPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Load Uxml
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/CustomTileRuleEditor.uxml");
        var root = assetTree.Instantiate();

        // rule picker
        var tileRulePickerContainer = root.Q("tile-rule-picker-container");
        var TileRulePickerField = new PropertyField(property.FindPropertyRelative("rulePicker"));
        tileRulePickerContainer.Add(TileRulePickerField);

        // Sprite Object Field
        VisualElement objectFieldDisplay = root.Q<VisualElement>(null, new string[] { ObjectField.objectUssClassName });
        Image objectFieldDisplayIcon = objectFieldDisplay.Q<Image>();
        VisualElement objectFieldInput = root.Q<VisualElement>(null, new string[] { ObjectField.inputUssClassName });

        Image spritePreview = new();
        objectFieldDisplay.Insert(1, spritePreview);
        spritePreview.pickingMode = PickingMode.Ignore;

        ObjectField spriteObjectField = root.Q<ObjectField>();
        spriteObjectField.RegisterValueChangedCallback((ev) =>
        {
            if (ev.newValue is Sprite sprite)
            {
                objectFieldDisplayIcon.AddToClassList("hidden");
                spritePreview.sprite = sprite;
            }
            if (ev.newValue is Tile tile)
            {
                objectFieldDisplayIcon.AddToClassList("hidden");
                spritePreview.sprite = tile.sprite;
            }
            else if (ev.newValue == null)
            {
                spritePreview.sprite = null;
                objectFieldDisplayIcon.RemoveFromClassList("hidden");
            }
        });

        // Binding data
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
#endif