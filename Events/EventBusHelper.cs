using System;
using System.Collections.Generic;
using System.Linq;

internal static class EventBusHelper
{
    private static Dictionary<Type, List<Type>> s_CashedSubscriberTypes =
        new Dictionary<Type, List<Type>>();

    public static List<Type> GetSubscriberTypes(ISubscriber subscriber)
    {
        var type = subscriber.GetType();
        if (s_CashedSubscriberTypes.TryGetValue(type, out var types))
        {
            return types;
        }

        var subscriberTypes = type
            .GetInterfaces()
            .Where(t => typeof(ISubscriber).IsAssignableFrom(t))
            .ToList();

        s_CashedSubscriberTypes[type] = subscriberTypes;
        return subscriberTypes;
    }
}
