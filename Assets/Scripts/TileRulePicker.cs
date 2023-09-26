using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public enum TileRules { ANY = 0, CONNECT = 1, NOT_CONNECT = 2 }

public struct TileConnection
{
    public bool isConnectN;
    public bool isConnectW;
    public bool isConnectE;
    public bool isConnectS;
}

[Serializable]
public struct TileRulePicker
{
    public TileRules rule_n;
    public TileRules rule_s;
    public TileRules rule_w;
    public TileRules rule_e;

    public bool IsMacth(TileConnection connection)
    {
        if(!IsRuleMacth(rule_n, connection.isConnectN))
        {
            return false;
        }else if(!IsRuleMacth(rule_w, connection.isConnectW))
        {
            return false;
        }else if(!IsRuleMacth(rule_e, connection.isConnectE))
        {
            return false;
        }else if(!IsRuleMacth(rule_s, connection.isConnectS))
        {
            return false;
        }
        return true;
    }

    bool IsRuleMacth(TileRules tileRules,bool isConnect)
    {
        if(tileRules == TileRules.ANY)
        {
            return true;
        }    
        else if(tileRules == TileRules.CONNECT && isConnect)
        {
            return true;
        }
        else if(tileRules == TileRules.NOT_CONNECT && !isConnect)
        {
            return true;
        }

        return false;
    }
}

[CustomPropertyDrawer(typeof(TileRulePicker))]
public class TileRulePickerPropotyDrawer : PropertyDrawer
{
    readonly Sprite crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/close.png");
    readonly Sprite rightArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/right-arrow.png");

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/TileRulePickerEditor.uxml");
        var root = assetTree.Instantiate();
        CreateRulePickers(root,property);

        return root;
    }

    readonly string[] rulePickerKeywords = new string[] { "n", "w", string.Empty, "e", "s" };

    void CreateRulePickers(VisualElement root, SerializedProperty property)
    {
        var topRow = root.Q<VisualElement>("rule-picker-top-row");
        var middleRow = root.Q<VisualElement>("rule-picker-middle-row");
        var bottomRow = root.Q<VisualElement>("rule-picker-bottom-row");

        foreach (var keyword in rulePickerKeywords)
        {
            var rulePicker = new Image();
            rulePicker.AddToClassList("condition-box");
            if (keyword != string.Empty)
            {
                Debug.Log(keyword);
                var propertyName = $"rule_{keyword}";
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
                case "w":
                case "":
                case "e":
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