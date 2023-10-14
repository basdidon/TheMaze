using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TimerUiController : MonoBehaviour
{
    UIDocument uiDoc;
    VisualElement root;

    float timeSinceLevelLoad;

    Label secondsLabel;
    Label minutesLabel;
    Label hoursLabel;

    private void Awake()
    {
        uiDoc = GetComponent<UIDocument>();

        root = uiDoc.rootVisualElement;

        secondsLabel = root.Q<Label>("seconds-label");
        minutesLabel = root.Q<Label>("minutes-label");
        hoursLabel = root.Q<Label>("hours-label");
    }

    private void Update()
    {
        timeSinceLevelLoad = Time.timeSinceLevelLoad;
        UpdateUi();
    }

    void UpdateUi()
    {
        var timeInt = Mathf.FloorToInt(timeSinceLevelLoad);
        secondsLabel.text = ( timeInt % 60).ToString("D2");
        minutesLabel.text = (Mathf.FloorToInt(timeInt / 60) % 60).ToString("D2");
        hoursLabel.text = (Mathf.FloorToInt(timeInt / 3600).ToString("D2"));

    }
}
