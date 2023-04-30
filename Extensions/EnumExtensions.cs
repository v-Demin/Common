using System;
using Random = System.Random;

public static class EnumExtensions
{
    public static T GetRandomValue<T>(this Type enumType)
        where T : struct, Enum
    {
        Array values = Enum.GetValues(typeof(T));
        Random random = new Random();
        return (T)values.GetValue(random.Next(values.Length));
    }
}
