/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class TimeScale : MonoBehaviour
{
    public static event System.Action<int> timeScaleChanged;

    private InputField input;
    private Dropdown dropdown;
    private Text dropdownText;

    private Dictionary<string, int> ToSeconds = new Dictionary<string, int>
    {
        {"second", 1}, {"minute", 60}, {"hour", 3600}, {"day", 86400},
        {"week", 604800}, {"month", 2628000}, {"year", 31535965}
    };

    private void Awake()
    {
        input = GetComponentInChildren<InputField>();
        dropdown = GetComponentInChildren<Dropdown>();
        dropdownText = dropdown.GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        input.onValueChanged.AddListener((s) => SetTimeScale());
        dropdown.onValueChanged.AddListener((i) => SetTimeScale());
    }

    private void OnDisable()
    {
        input.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.RemoveAllListeners();
    }

    private void Start()
    {
        SetTimeScale();
    }

    private void SetTimeScale()
    {
        int amount = int.Parse(input.text);
        string unit = dropdown.options[dropdown.value].text;

        dropdownText.text = (amount == 1) ? unit : $"{unit}s";
        timeScaleChanged?.Invoke(amount * ToSeconds[unit]);
    }
}
