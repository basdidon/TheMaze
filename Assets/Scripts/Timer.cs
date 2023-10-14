using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }

    public bool IsCounting { get; private set; } = false;

    double value;
    public double Value {
        get => value;
        private set
        {
            this.value = value;
            TimeChanged?.Invoke(Value);
        }
    }

    Time v;

    // Events
    public UnityAction<double> TimeChanged;

    public void StartTimer() => IsCounting = true;
    public void StopTimer() => IsCounting = false;
    public void ResetTimer()
    {
        StopTimer();
        Value = 0;
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        v = new Time();
    }

    private void Update()
    {
        if (IsCounting)
        {
            Debug.Log($"Timer.Time {value}");
            Value += Value + UnityEngine.Time.deltaTime;
        }
    }
}
