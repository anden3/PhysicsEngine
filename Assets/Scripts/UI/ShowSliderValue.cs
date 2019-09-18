using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShowSliderValue : MonoBehaviour
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    public void UpdateText(float value)
        => text.text = value.ToString("0%");
}
