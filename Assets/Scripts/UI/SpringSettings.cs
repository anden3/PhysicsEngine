using UnityEngine;
using UnityEngine.UI;

public class SpringSettings : MonoBehaviour
{
    public ParticleStandaloneSpring spring;

    private Text springName;
    private Text length;

    private Slider stiffnessSlider;

    private InputField sliderValue;
    private InputField restLength;

    private void Awake()
    {
        springName = transform.Find("Name").GetComponent<Text>();
        length = transform.Find("Length").GetComponent<Text>();

        stiffnessSlider = GetComponentInChildren<Slider>();

        sliderValue = stiffnessSlider.GetComponentInChildren<InputField>();
        restLength = transform.Find("Rest Length").GetComponent<InputField>();
    }

    private void Start()
    {
        // Show default values.
        stiffnessSlider.value = spring.springConstant;

        springName.text = spring.name;
        sliderValue.text = stiffnessSlider.value.ToString("F2");
        restLength.text = spring.restLength.ToString("F2");
    }

    private void OnEnable()
    {
        // Set up stiffness events.
        sliderValue.onEndEdit.AddListener(val => stiffnessSlider.value = float.Parse(val));
        stiffnessSlider.onValueChanged.AddListener(val =>
        {
            sliderValue.text = val.ToString("F2");
            spring.springConstant = val;
        });

        // Set up rest length event.
        restLength.onEndEdit.AddListener(val => spring.restLength = float.Parse(val));
    }

    private void Update()
    {
        // Update length value.
        length.text = (spring.transform.localScale.y * 2).ToString("F2");
    }
}
