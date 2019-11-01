using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ObjectLabel : MonoBehaviour
{
    public Transform obj;

    private Text label;

    private void Awake()
    {
        label = GetComponent<Text>();
    }

    private void Start()
    {
        label.text = obj.name;
    }

    private void LateUpdate()
    {
        transform.position = Camera.main.WorldToScreenPoint(obj.position);
    }
}
