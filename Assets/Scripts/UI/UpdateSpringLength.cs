using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSpringLength : MonoBehaviour
{
    public ParticleStandaloneSpring spring;
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = spring.transform.localScale.y.ToString();
    }
}
