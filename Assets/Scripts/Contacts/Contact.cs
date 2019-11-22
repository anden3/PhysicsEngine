/*
 * Based on code by Ian Millington in Game Physics Engine Development.
 */

using UnityEngine;

using System.Collections.Generic;

using AndreExtensions;

public class Contact
{
    public readonly RigidBody[] bodies = new RigidBody[2];

    public static float friction = 0.5f;
    public static float restitution = 0.2f;

    public static float angularLimit = 0.2f;
    public static float velocityLimit = 0.25f;

    public readonly Vector3 position;
    public readonly Vector3 normal;
    public float penetration;

    public Vector3[] relativePositions = new Vector3[2];

    public Vector3 contactVelocity;
    public float desiredDeltaVelocity;

    public Matrix3x3 contactToWorld;
    public Matrix3x3 worldToContact;

    public Contact(RigidBody first, RigidBody second, Vector3 position, Vector3 normal, float penetration)
    {
        bodies[0] = first;
        bodies[1] = second;

        this.position = position;
        this.normal = normal;
        this.penetration = penetration;
    }

    public void MatchAwakeState()
    {
        if (bodies[1] == null)
            return;

        if (bodies[0].isAwake != bodies[1].isAwake)
        {
            if (!bodies[0].isAwake) bodies[0].SetAwake(true);
            else                    bodies[1].SetAwake(true);
        }
    }

    public void CalculateInternals(float deltaTime)
    {
        CalculateContactBasis();

        relativePositions[0] = position - bodies[0].transform.position;
        contactVelocity = CalculateLocalVelocity(0, deltaTime);

        if (bodies[1])
        {
            relativePositions[1] = position - bodies[1].transform.position;
            contactVelocity -= CalculateLocalVelocity(1, deltaTime);
        }   

        CalculateDesiredDeltaVelocity(deltaTime);
    }

    private Vector3 CalculateLocalVelocity(int index, float deltaTime)
    {
        RigidBody body = bodies[index];
        Vector3 velocity = body.GetVelocityAtBodyPoint(relativePositions[index]);

        Vector3 contactVelocity = worldToContact.Transform(velocity);

        Vector3 accVelocity = worldToContact.Transform(body.acceleration * deltaTime);
        accVelocity.x = 0;

        return contactVelocity + accVelocity;
    }

    public void CalculateDesiredDeltaVelocity(float deltaTime)
    {
        float velocityFromAcc = Vector3.Dot(bodies[0].lastFrameAcceleration * deltaTime, normal);

        if (bodies[1])
            velocityFromAcc -= Vector3.Dot(bodies[1].lastFrameAcceleration * deltaTime, normal);

        float thisRestitution = restitution;

        // Stop bouncing due to restitution at low speeds.
        if (Mathf.Abs(contactVelocity.x) < velocityLimit)
            thisRestitution = 0.0f;

        desiredDeltaVelocity = -contactVelocity.x - thisRestitution * (contactVelocity.x - velocityFromAcc);
    }

    private void CalculateContactBasis()
    {
        Vector3[] tangent = new Vector3[2];

        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            float normalizingScale = 1.0f / Mathf.Sqrt(normal.z.Squared() + normal.x.Squared());

            tangent[0] = new Vector3(normal.z, 0, -normal.x) * normalizingScale;
            tangent[1] = new Vector3(normal.y, normal.z, -normal.y) * tangent[0].x;
            tangent[1].y -= normal.x * tangent[0].z;
        }
        else
        {
            float normalizingScale = 1.0f / Mathf.Sqrt(normal.z.Squared() + normal.y.Squared());

            tangent[0] = new Vector3(0, -normal.z, normal.y) * normalizingScale;
            tangent[1] = new Vector3(
                 normal.y * tangent[0].z - normal.z * tangent[0].y,
                -normal.x * tangent[0].z,
                 normal.x * tangent[0].y
            );
        }

        contactToWorld = new Matrix3x3(normal, tangent[0], tangent[1]);
        worldToContact = contactToWorld.GetTranspose();
    }

    private Vector3 CalculateFrictionlessImpulse()
    {
        float deltaVelocity = 0.0f;

        foreach ((int i, RigidBody body) in GetActiveBodies())
        {
            Vector3 deltaVelocityWorld = Vector3.Cross(relativePositions[i], normal);
            deltaVelocityWorld = body.inverseInertiaTensor.Transform(deltaVelocityWorld);
            deltaVelocityWorld = Vector3.Cross(deltaVelocityWorld, relativePositions[i]);

            deltaVelocity += body.inverseMass + Vector3.Dot(deltaVelocityWorld, normal);
        }

        return new Vector3(desiredDeltaVelocity / deltaVelocity, 0, 0);
    }

    private Vector3 CalculateFrictionImpulse()
    {
        float inverseMass = 0.0f;
        Matrix3x3 deltaVelocityWorld = Matrix3x3.Identity;

        foreach ((int i, RigidBody body) in GetActiveBodies())
        {
            Matrix3x3 impulseToTorque = Matrix3x3.Identity;
            impulseToTorque.SetSkewSymmetric(relativePositions[i]);

            Matrix3x3 bodyDeltaVelocityWorld = impulseToTorque;
            bodyDeltaVelocityWorld *= body.inverseInertiaTensor;
            bodyDeltaVelocityWorld *= impulseToTorque;
            bodyDeltaVelocityWorld *= -1;

            deltaVelocityWorld += bodyDeltaVelocityWorld;

            inverseMass += bodies[i].inverseMass;
        }

        Matrix3x3 deltaVelocity = worldToContact;
        deltaVelocity *= deltaVelocityWorld;
        deltaVelocity *= contactToWorld;

        deltaVelocity[0] += inverseMass;
        deltaVelocity[4] += inverseMass;
        deltaVelocity[8] += inverseMass;

        Matrix3x3 impulseMatrix = deltaVelocity.GetInverse();
        Vector3 velocityKill = new Vector3(desiredDeltaVelocity, -contactVelocity.y, -contactVelocity.z);
        Vector3 impulseContact = impulseMatrix.Transform(velocityKill);

        float planarImpulse = Mathf.Sqrt(impulseContact.y.Squared() + impulseContact.z.Squared());

        if (planarImpulse > impulseContact.x * friction)
        {
            impulseContact.y /= planarImpulse;
            impulseContact.z /= planarImpulse;

            impulseContact.x = deltaVelocity[0]
                + deltaVelocity[1] * friction * impulseContact.y
                + deltaVelocity[2] * friction * impulseContact.z;

            impulseContact.x = desiredDeltaVelocity / impulseContact.x;
            impulseContact.y *= friction * impulseContact.x;
            impulseContact.z *= friction * impulseContact.x;
        }

        return impulseContact;
    }

    public void ApplyPositionChange(float penetration, out Vector3[] linearChange, out Vector3[] angularChange)
    {
        float totalInertia = 0.0f;

        float[] linearMove = new float[2];
        float[] angularMove = new float[2];

        float[] linearInertia = new float[2];
        float[] angularInertia = new float[2];

        linearChange = new Vector3[2];
        angularChange = new Vector3[2];

        foreach ((int i, RigidBody body) in GetActiveBodies())
        {
            Vector3 angularInertiaWorld = Vector3.Cross(relativePositions[i], normal);
            angularInertiaWorld = body.inverseInertiaTensor.Transform(angularInertiaWorld);
            angularInertiaWorld = Vector3.Cross(angularInertiaWorld, relativePositions[i]);

            angularInertia[i] = Vector3.Dot(angularInertiaWorld, normal);
            linearInertia[i] = body.inverseMass;

            totalInertia += linearInertia[i] + angularInertia[i];
        }

        if (totalInertia == 0)
            return;

        foreach ((int i, RigidBody body) in GetActiveBodies())
        {
            float sign = (i == 0) ? 1 : -1;

            linearMove[i] = sign * penetration * (linearInertia[i] / totalInertia);
            angularMove[i] = sign * penetration * (angularInertia[i] / totalInertia);

            Vector3 projection = relativePositions[i] + normal * Vector3.Dot(-relativePositions[i], normal);
            float maxMagnitude = angularLimit * projection.magnitude;

            if (Mathf.Abs(angularMove[i]) > maxMagnitude)
            {
                float totalMove = angularMove[i] + linearMove[i];
                angularMove[i] = Mathf.Clamp(angularMove[i], -maxMagnitude, maxMagnitude);
                linearMove[i] = totalMove - angularMove[i];
            }

            if (angularMove[i] == 0)
            {
                angularChange[i] = Vector3.zero;
            }
            else
            {
                Vector3 targetAngularDirection = Vector3.Cross(relativePositions[i], normal);

                angularChange[i] =
                    body.inverseInertiaTensor.Transform(targetAngularDirection)
                    * (angularMove[i] / angularInertia[i]);
            }

            linearChange[i] = normal * linearMove[i];
            body.Move(linearChange[i], true);

            body.transform.rotation = Quaternion.AngleAxis(
                angularChange[i].magnitude, angularChange[i]
            ) * body.transform.rotation;

            body.CalculateDerivedData();
        }
    }

    public void ApplyVelocityChange(out Vector3[] velocityChange, out Vector3[] rotationChange)
    {
        Vector3 impulseContact = (friction == 0.0f)
            ? CalculateFrictionlessImpulse()
            : CalculateFrictionImpulse();

        Vector3 impulse = contactToWorld.Transform(impulseContact);

        velocityChange = new Vector3[2];
        rotationChange = new Vector3[2];

        foreach ((int i, RigidBody body) in GetActiveBodies())
        {
            rotationChange[i] = body.inverseInertiaTensor.Transform(Vector3.Cross(relativePositions[i], impulse));
            velocityChange[i] = impulse * body.inverseMass * (i == 0 ? 1 : -1);

            body.ChangeVelocity(velocityChange[i]);

            body.transform.rotation = Quaternion.AngleAxis(
                rotationChange[i].magnitude, rotationChange[i]
            ) * body.transform.rotation;
        }
    }

    public IEnumerable<(int index, RigidBody body)> GetActiveBodies()
    {
        Debug.Assert(bodies[0] != null);
        yield return (0, bodies[0]);

        if (bodies[1])
            yield return (1, bodies[1]);
    }

    public override string ToString() => $"{bodies[0]} <-> {bodies[1].name ?? "Plane"}";
}
