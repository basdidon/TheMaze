using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;

[Serializable]
public struct TileConnection
{
    public bool isConnectN;
    public bool isConnectS;
    public bool isConnectW;
    public bool isConnectE;

    public TileConnection(bool n = false,bool w = false, bool e = false, bool s = false)
    {
        isConnectN = n;
        isConnectW = w;
        isConnectE = e;
        isConnectS = s;
    }

    public TileConnection(int code)
    {
        isConnectN = ((code >> 3) & 1) == 1;
        isConnectW = ((code >> 2) & 1) == 1;
        isConnectE = ((code >> 1) & 1) == 1;
        isConnectS = (code & 1) == 1;
    }

    public int GetConditionCode()
    {
        int code = 0;
        code |= (isConnectN ? 1 : 0) << 3;
        code |= (isConnectS ? 1 : 0) << 2;
        code |= (isConnectW ? 1 : 0) << 1;
        code |= (isConnectE ? 1 : 0);
        return code;
    }

    public override string ToString()
    {
        return $"[{isConnectN},{isConnectS},{isConnectW},{isConnectE}]";
    }
}

[CustomPropertyDrawer(typeof(TileConnection))]
public class TileConnectionPropotyDrawer : PropertyDrawer
{
    readonly Sprite crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/close.png");
    readonly Sprite rightArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor/UiDocs/Sprites/right-arrow.png");

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UiDocs/TileRulePickerEditor.uxml");
        var root = assetTree.Instantiate();
        CreateTileConnectionFields(root, property);

        return root;
    }

    readonly string[] rulePickerKeywords = new string[] { "n", "w", string.Empty, "e", "s" };

    void CreateTileConnectionFields(VisualElement root,SerializedProperty property)
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
                var propertyName = $"isConnect{keyword.ToUpper()}";
                rulePicker.sprite = GetRulePickerSprite(property.FindPropertyRelative(propertyName).boolValue);
                rulePicker.RegisterCallback<MouseDownEvent>(ev =>
                {
                    property.FindPropertyRelative(propertyName).boolValue ^= true;

                    rulePicker.sprite = GetRulePickerSprite(property.FindPropertyRelative(propertyName).boolValue);

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

        Sprite GetRulePickerSprite(bool isConnect)
        {
            return isConnect switch
            {
                false => crossSprite,
                true => rightArrowSprite,
            };
        }
    }

}