/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

public static class UnitScales
{
    public static readonly float Mass = 1e10f;
    public static readonly float Distance = 1e8f;
    public static readonly float Time = 1e0f;

    public static readonly float Velocity = Distance / Time;
    public static readonly float Force = Mass * (Distance / Time);
    public static readonly float G = Force * (Distance * Distance) / (Mass * Mass);
}
