using UnityEngine;

using System;
using System.Collections.Concurrent;

public class GizmoHandler : MonoBehaviour
{
    public static event Action DrawGizmos;
    public static event Action DrawGizmosSelected;

    public static ConcurrentQueue<Action> OnGizmo = new ConcurrentQueue<Action>();
    public static ConcurrentQueue<Action> OnGizmoSelected = new ConcurrentQueue<Action>();

    public static void OnDrawGizmos(Action action)
        => DrawGizmos += action;

    public static void OnDrawGizmosSelected(Action action)
        => DrawGizmosSelected += action;

    public static void DrawGizmosOneShot(Action action)
        => OnGizmo.Enqueue(action);

    public static void DrawGizmosSelectedOneShot(Action action)
        => OnGizmoSelected.Enqueue(action);

    public static void DrawGizmoLine(Vector3 start, Vector3 end, Color color)
    {
        DrawGizmosOneShot(() =>
        {
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
        });
    }

    private void OnDrawGizmos()
    {
        DrawGizmos?.Invoke();

        while (OnGizmo.Count > 0)
        {
            if (OnGizmo.TryDequeue(out Action action))
                action.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmosSelected?.Invoke();

        while (OnGizmoSelected.Count > 0)
        {
            if (OnGizmoSelected.TryDequeue(out Action action))
                action.Invoke();
        }
    }

    private void OnDestroy()
    {
        try
        {
            foreach (Action a in DrawGizmos?.GetInvocationList())
                DrawGizmos -= a;

            foreach (Action a in DrawGizmosSelected?.GetInvocationList())
                DrawGizmosSelected -= a;
        }
        catch (NullReferenceException) { }
        

        OnGizmo = new ConcurrentQueue<Action>();
        OnGizmoSelected = new ConcurrentQueue<Action>();
    }
}

