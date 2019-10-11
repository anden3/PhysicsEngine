/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

using System;

public class TimeScale : MonoBehaviour
{
    // Not exact for months and years, but eh :P
    private enum TimeUnit
    {
        Second  =           1,
        Minute  = Second * 60,
        Hour    = Minute * 60,
        Day     = Hour   * 24,
        Week    = Day    *  7,
        Month   = Week   *  4,
        Year    = Month  * 12
    }

    public static int CurrentTimeScale;

    private InputField input;
    private Dropdown dropdown;
    private Text dropdownText;

    private string[] timeUnitNames;

    private void Awake()
    {
        input = GetComponentInChildren<InputField>();
        dropdown = GetComponentInChildren<Dropdown>();
        dropdownText = dropdown.GetComponentInChildren<Text>();

        timeUnitNames = Array.ConvertAll(
            Enum.GetNames(typeof(TimeUnit)), u => u.ToLower());

        for (int i = 0; i < timeUnitNames.Length; i++)
            dropdown.options[i].text = timeUnitNames[i];
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

        if (Enum.TryParse(unit, true, out TimeUnit value))
        {
            CurrentTimeScale = amount * (int)value;
        }
    }
}
