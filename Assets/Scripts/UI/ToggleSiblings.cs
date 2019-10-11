/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections.Generic;

public class ToggleSiblings : MonoBehaviour
{
    public bool SiblingsVisible
    {
        get => siblingsVisible;
        private set => siblingsVisible = value;
    }
    [SerializeField]
    private bool siblingsVisible = true;

    public bool flipImages;

    private Image[] images;
    private Dictionary<GameObject, bool> siblings = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        images = GetComponentsInChildren<Image>();

        Transform parent = transform.parent;

        for (int i = transform.GetSiblingIndex() + 1; i < transform.parent.childCount; i++)
            siblings.Add(
                parent.GetChild(i).gameObject,
                parent.GetChild(i).gameObject.activeSelf);
    }

    public void Toggle()
    {
        foreach (GameObject sibling in siblings.Keys.ToList())
        {
            if (SiblingsVisible)
            {
                siblings[sibling] = sibling.activeSelf;
                sibling.SetActive(false);
            }
            else
            {
                sibling.SetActive(siblings[sibling]);
            }
        }

        if (flipImages)
        {
            foreach (Image image in images)
                image.transform.localEulerAngles = new Vector3(
                    0, 0, SiblingsVisible ? 0f : 180f);
        }

        SiblingsVisible = !SiblingsVisible;
    }
}
