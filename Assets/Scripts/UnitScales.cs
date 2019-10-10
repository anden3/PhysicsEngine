/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

public static class UnitScales
{
    public static readonly double Mass = 1e10;
    public static readonly double Distance = 1e8;
    public static readonly double Time = 1e0;

    public static readonly double Velocity = Distance / Time;
    public static readonly double Force = Mass * (Distance / Time);
    public static readonly double G = Force * (Distance * Distance) / (Mass * Mass);
}
