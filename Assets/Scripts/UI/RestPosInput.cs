using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestPosInput : MonoBehaviour
{
    public ParticleStandaloneSpring spring;

    private InputField input;


    private void Awake()
    {
        input = GetComponent<InputField>();
    }

    void Start()
    {
        input.text = spring.restLength.ToString();
    }

    void Update()
    {
        
    }
    public void OnEnable()
    {
        input.onEndEdit.AddListener(SetRestPos);
    }

    public void OnDisable()
    {
        input.onEndEdit.RemoveListener(SetRestPos);
    }

    private void SetRestPos(string value)
    {
        spring.restLength = float.Parse(value);
    }
}
