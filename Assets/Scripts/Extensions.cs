using UnityEngine;

using System.Collections.Generic;

namespace AndreExtensions
{
    public enum ContainmentType
    {
        Contains,   // Fully contains.
        Intersects, // Partially contains.
        Disjoint    // Completely separate.
    }

    public static class BoundsExtensions
    {
        public static ContainmentType Contains(this Bounds bounds, BoundingVolume volume)
        {
            switch (volume.type)
            {
                case BoundingVolume.Type.Cube:
                    BoundingCube cube = (BoundingCube)volume;

                    if (bounds.min.LessThanOrEqual(cube.bounds.min) && bounds.max.GreaterThanOrEqual(cube.bounds.max))
                        return ContainmentType.Contains;

                    if (bounds.Intersects(cube.bounds))
                        return ContainmentType.Intersects;

                    return ContainmentType.Disjoint;

                case BoundingVolume.Type.Sphere:
                    BoundingSphere sphere = (BoundingSphere)volume;

                    // Treat spheres as AABBs.
                    float radiusSqr = sphere.radius * sphere.radius;
                    Bounds aabb = new Bounds(sphere.center, new Vector3(radiusSqr, radiusSqr, radiusSqr));

                    if (bounds.min.LessThanOrEqual(aabb.min) && bounds.max.GreaterThanOrEqual(aabb.max))
                        return ContainmentType.Contains;

                    if (bounds.Intersects(aabb))
                        return ContainmentType.Intersects;

                    return ContainmentType.Disjoint;
            }

            return ContainmentType.Disjoint;
        }

        public static Bounds GetMinikowskiDifference(this Bounds bounds, Bounds other)
        {
            Vector3 topLeft = bounds.min - other.max;
            Vector3 fullSize = bounds.size + other.size;

            return new Bounds(topLeft + fullSize / 2, fullSize);
        }
    }

    public static class Vector3Extensions
    {
        public static bool GreaterThan(this Vector3 target, Vector3 other)
            => target.x > other.x && target.y > other.y && target.z > other.z;

        public static bool GreaterThanOrEqual(this Vector3 target, Vector3 other)
            => target.x >= other.x && target.y >= other.y && target.z >= other.z;

        public static bool LessThan(this Vector3 target, Vector3 other)
            => target.x < other.x && target.y < other.y && target.z < other.z;

        public static bool LessThanOrEqual(this Vector3 target, Vector3 other)
            => target.x <= other.x && target.y <= other.y && target.z <= other.z;

        public static float GetMax(this Vector3 target)
            => Mathf.Max(target.x, target.y, target.z);

        public static Vector3 Abs(this Vector3 target)
            => new Vector3(Mathf.Abs(target.x), Mathf.Abs(target.y), Mathf.Abs(target.z));
    }

    public static class FloatExtensions
    {
        public static float Squared(this float f) => f * f;
    }

    public static class QueueExtensions
    {
        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> list)
        {
            foreach (T item in list)
                queue.Enqueue(item);
        }
    }

    public static class ListExtensions
    {
        public static T Back<T>(this List<T> list)
        {
            if (list.Count > 0)
                return list[list.Count - 1];

            throw new System.IndexOutOfRangeException("Cannot get the last element of an empty list.");
        }
    }
}
