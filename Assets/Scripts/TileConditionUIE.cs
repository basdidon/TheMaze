using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using System;

public enum TileRules { ANY = 0, CONNECT = 1, NOT_CONNECT = 2 }

[Serializable]
public struct TileRule4Dir
{
    public TileRules rule_n;
    public TileRules rule_s;
    public TileRules rule_w;
    public TileRules rule_e;
}

[Serializable]
public class TileCondition
{
    public Sprite Sprite;
    public TileRule4Dir rule4Dir;
    public int count;

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
        /*
        int code = 0;
        code |= (isConnect_N ? 1 : 0) << 3;
        code |= (isConnect_S ? 1 : 0) << 2;
        code |= (isConnect_W ? 1 : 0) << 1;
        code |= (isConnect_E ? 1 : 0);
        return code;*/
        return 0;
    }
}

[CustomPropertyDrawer(typeof(TileCondition))]
public class TileConditionUIE : PropertyDrawer
{
    Sprite crossSprite;
    Sprite rightArrowSprite;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/close.png");
        rightArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/right-arrow.png");
        // Load Uxml
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/CustomTileRuleEditor.uxml");
        var root = assetTree.Instantiate();

        // Rule Picker
        CreateRulePickers(root,property);

        // Sprite Object Field
        VisualElement objectFieldDisplay = root.Q<VisualElement>(null, new string[] { ObjectField.objectUssClassName });
        Image objectFieldDisplayIcon = objectFieldDisplay.Q<Image>();
        VisualElement objectFieldInput = root.Q<VisualElement>(null, new string[] { ObjectField.inputUssClassName });
        
        Image spritePreview = new ();
        objectFieldDisplay.Insert(1, spritePreview);
        spritePreview.pickingMode = PickingMode.Ignore;
        
        ObjectField spriteObjectField = root.Q<ObjectField>();
        spriteObjectField.RegisterValueChangedCallback((ev)=> {
            if (ev.newValue is Sprite sprite)
            {
                objectFieldDisplayIcon.AddToClassList("hidden");
                spritePreview.sprite = sprite;
            }
            else if(ev.newValue == null)
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

    readonly string[] rulePickerKeywords = new string[] {"n","w",string.Empty,"e","s"};

    void CreateRulePickers(VisualElement root,SerializedProperty property)
    {
        var topRow = root.Q<VisualElement>("rule-picker-top-row");
        var middleRow = root.Q<VisualElement>("rule-picker-middle-row");
        var bottomRow = root.Q<VisualElement>("rule-picker-bottom-row");

        foreach(var keyword in rulePickerKeywords)
        {
            var rulePicker = new Image();
            rulePicker.AddToClassList("condition-box");
            if (keyword != string.Empty)
            {
                var propertyName = $"rule4Dir.rule_{keyword}";
                rulePicker.sprite = GetRulePickerSprite(property.FindPropertyRelative(propertyName).enumValueIndex);
                rulePicker.RegisterCallback<MouseDownEvent>(ev =>
                {
                    int enumValue = property.FindPropertyRelative(propertyName).enumValueIndex;
                    enumValue = (enumValue + 1) % Enum.GetValues(typeof(TileRules)).Length;
                    property.FindPropertyRelative(propertyName).enumValueIndex = enumValue;

                    rulePicker.sprite = GetRulePickerSprite(enumValue);

                    // Update Changed value
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
            }

            // assign to thier row
            switch (keyword)
            {
                case "n":
                    topRow.Add(rulePicker);
                    break;
                case "w":case "":case "e":
                    middleRow.Add(rulePicker);
                    break;
                case "s":
                    bottomRow.Add(rulePicker);
                    break;
            }

            if (keyword == string.Empty)
                continue;

            rulePicker.style.borderLeftWidth = 0;

            if (keyword == "e")
                continue;

            rulePicker.style.rotate = keyword switch
            {
                "n" => new StyleRotate(new Rotate(new Angle(270f, AngleUnit.Degree))),
                "w" => new StyleRotate(new Rotate(new Angle(180f, AngleUnit.Degree))),
                "s" => new StyleRotate(new Rotate(new Angle(90f, AngleUnit.Degree))),
                _ => throw new Exception("unexpected keyword"),
            };
            
        }
    }

    Sprite GetRulePickerSprite(int tileRuleIndex)
    {
        return tileRuleIndex switch
        {
            (int)TileRules.ANY => null,
            (int)TileRules.CONNECT => rightArrowSprite,
            (int)TileRules.NOT_CONNECT => crossSprite,
            _ => throw new Exception("unexpected enum index value."),
        };
    }

    
}