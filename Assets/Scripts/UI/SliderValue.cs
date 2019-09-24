using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SliderValue : MonoBehaviour
{
    public Slider slider;

    private InputField input;

    private void Awake()
    {
        input = GetComponent<InputField>();
    }

    private void OnEnable()
    {
        input.onEndEdit.AddListener(SetSlider);
        slider.onValueChanged.AddListener(SetText);
    }

    private void OnDisable()
    {
        input.onEndEdit.AddListener(SetSlider);
        slider.onValueChanged.RemoveListener(SetText);
    }

    private void Start()
    {
        SetText(slider.value);
    }

    private void SetText(float value)
        => input.text = value.ToString();

    private void SetSlider(string value)
        => slider.value = float.Parse(value);
}
