/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

using UnityEngine;
using UnityEngine.EventSystems;

public class DragArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	private GameObject child;
	private Vector3 mousePos;

	private void Awake()
	{
		child = transform.GetChild(0).gameObject;
	}

	private void Update()
	{
		if (Input.mousePosition != mousePos)
		{
			mousePos = Input.mousePosition;
			ExecuteEvents.Execute<IMouseMessageTarget>(child, null, (x, y) => x.MouseMove());
		}
	}

    public void OnPointerDown(PointerEventData evt)
        => ExecuteEvents.Execute<IMouseMessageTarget>(child, null, (x, y) => x.MouseDown());

    public void OnPointerUp(PointerEventData evt)
        => ExecuteEvents.Execute<IMouseMessageTarget>(child, null, (x, y) => x.MouseUp());
}
