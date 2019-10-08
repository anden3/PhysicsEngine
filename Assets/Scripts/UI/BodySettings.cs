﻿/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

public class BodySettings : MonoBehaviour
{
    public static event System.Action<Transform> targetChanged;

    // Body picker.
    private Text bodyName;

    private Button prevBody;
    private Button nextBody;

    private Particle[] bodies;
    private Particle selectedBody;
    private int selectedIndex = 0;

    private Particle orbitCenter;

    // Body settings.
    private InputField mass;

    private InputField velocityX;
    private InputField velocityY;
    private InputField velocityZ;

    private InputField orbitalHeight;
    private GameObject orbitalHeightSetting;

    private void Awake()
    {
        bodies = FindObjectsOfType<Particle>();

        float largestMass = 0;

        // Select body with largest mass.
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].mass > largestMass)
            {
                selectedIndex = i;
            }
        }

        orbitCenter = bodies[selectedIndex];

        bodyName = transform.Find("Body Picker/Name").GetComponent<Text>();
        prevBody = transform.Find("Body Picker/Prev").GetComponent<Button>();
        nextBody = transform.Find("Body Picker/Next").GetComponent<Button>();

        mass = transform.Find("Mass/Input").GetComponent<InputField>();

        velocityX = transform.Find("Velocity/X").GetComponent<InputField>();
        velocityY = transform.Find("Velocity/Y").GetComponent<InputField>();
        velocityZ = transform.Find("Velocity/Z").GetComponent<InputField>();

        orbitalHeightSetting = transform.Find("Orbit Height").gameObject;
        orbitalHeight = orbitalHeightSetting.GetComponentInChildren<InputField>();
    }

    private void OnEnable()
    {
        prevBody.onClick.AddListener(() => SwitchBody(selectedIndex + 1));
        nextBody.onClick.AddListener(() => SwitchBody(selectedIndex - 1));

        mass.onEndEdit.AddListener(SetMass);

        velocityX.onEndEdit.AddListener(SetVelocity);
        velocityY.onEndEdit.AddListener(SetVelocity);
        velocityZ.onEndEdit.AddListener(SetVelocity);

        orbitalHeight.onEndEdit.AddListener(SetOrbitalHeight);
    }

    private void OnDisable()
    {
        prevBody.onClick.RemoveAllListeners();
        nextBody.onClick.RemoveAllListeners();

        mass.onEndEdit.RemoveAllListeners();

        velocityX.onEndEdit.RemoveListener(SetVelocity);
        velocityY.onEndEdit.RemoveListener(SetVelocity);
        velocityZ.onEndEdit.RemoveListener(SetVelocity);

        orbitalHeight.onEndEdit.RemoveListener(SetOrbitalHeight);
    }

    private void Start()
    {
        SwitchBody(selectedIndex);
    }

    private void Update()
    {
        if (selectedBody == null)
            return;

        Vector3 trueVelocity = selectedBody.velocity * UnitScales.Velocity;

        // Only update velocity values if they aren't being edited.
        if (!velocityX.isFocused && !velocityY.isFocused && !velocityZ.isFocused)
        {
            velocityX.SetTextWithoutNotify(trueVelocity.x.ToString("F2"));
            velocityY.SetTextWithoutNotify(trueVelocity.y.ToString("F2"));
            velocityZ.SetTextWithoutNotify(trueVelocity.z.ToString("F2"));
        }
    }

    private void SwitchBody(int newIndex)
    {
        selectedIndex = Mod(newIndex, bodies.Length);
        selectedBody = bodies[selectedIndex];

        bodyName.text = selectedBody.name;
        mass.SetTextWithoutNotify((selectedBody.mass * UnitScales.Mass).ToString("0.##E+00"));

        if (selectedBody != orbitCenter)
        {
            orbitalHeightSetting.SetActive(true);

            orbitalHeight.SetTextWithoutNotify((
                Vector3.Distance(selectedBody.transform.position, orbitCenter.transform.position) * UnitScales.Distance
            ).ToString("0.##E+00"));
        }
        else
        {
            // Don't show orbital height if new body is the orbital center.
            orbitalHeightSetting.SetActive(false);
        }        

        targetChanged?.Invoke(selectedBody.transform);
    }

    private void SetMass(string newMass)
        => selectedBody.mass = float.Parse(newMass) / UnitScales.Mass;

    private void SetVelocity(string _)
    {
        selectedBody.velocity = new Vector3(
            float.Parse(velocityX.text),
            float.Parse(velocityY.text),
            float.Parse(velocityZ.text)
        ) / UnitScales.Velocity;
    }

    private void SetOrbitalHeight(string newHeight)
    {
        if (selectedBody == orbitCenter)
            return;

        Vector3 center = orbitCenter.transform.position;

        selectedBody.transform.position = Vector3.LerpUnclamped(
            center, selectedBody.transform.position,
            (float.Parse(newHeight) / UnitScales.Distance) / Vector3.Distance(center, selectedBody.transform.position)
        );
    }


    /// <summary>
    /// <para>C#'s modulo operator isn't actually modulo, but remainder.</para>
    /// <para>Thus it doesn't work as expected for negative values of <paramref name="a"/>.</para>
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="n">Divisor.</param>
    /// <source>https://stackoverflow.com/a/1082938</source>
    /// <returns>a mod n</returns>
    private static int Mod(int a, int n) => ((a % n) + n) % n;
}
