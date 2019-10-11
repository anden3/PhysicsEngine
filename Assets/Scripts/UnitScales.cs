/*
 * Written by André Vennberg, Sebastian Karlsson & Sara Uvalic.
 */

public static class UnitScales
{
    public const double Mass = 1e10;
    public const double Distance = 1e8;
    public const double Time = 1e0;

    public const double Velocity = Distance / Time;
    public const double Force = Mass * (Distance / Time);
    public const double G = Force * (Distance * Distance) / (Mass * Mass);
}
