/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

public class GravitySwitch : MonoBehaviour
{
    private Toggle toggle;
    private ParticleGravity[] gravityGenerators;

    private void Awake()
    {
        toggle = GetComponentInChildren<Toggle>();
        gravityGenerators = FindObjectsOfType<ParticleGravity>();
    }

    private void OnEnable() => toggle.onValueChanged.AddListener(SetGravity);
    private void OnDisable() => toggle.onValueChanged.RemoveListener(SetGravity);

    private void SetGravity(bool value)
    {
        foreach (ParticleGravity generator in gravityGenerators)
            generator.enabled = value;
    }
}
