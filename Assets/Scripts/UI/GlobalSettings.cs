using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{
    private ParticlePhysicsEngine engine;

    private Slider dampingSlider;
    private Text dampingValue;

    private void Awake()
    {
        engine = FindObjectOfType<ParticlePhysicsEngine>();

        dampingSlider = transform.Find("Damping/Slider").GetComponent<Slider>();
        dampingValue = transform.Find("Damping/Value").GetComponent<Text>();
    }

    private void OnEnable()
    {
        dampingSlider.onValueChanged.AddListener(val =>
        {
            dampingValue.text = val.ToString("F2");

        });
    }

    private void Start()
    {
        dampingValue.text = dampingSlider.value.ToString("F2");
    }
}
