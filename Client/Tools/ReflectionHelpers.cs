using System.ComponentModel;
using System;

namespace Client.Tools;

public static class ReflectionHelpers
{
    public static string GetCustomDescription(object objEnum)
    {
        var fi = objEnum.GetType().GetField(objEnum.ToString() ?? string.Empty);
        var attributes = (DescriptionAttribute[]?)fi?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return (attributes?.Length > 0) ? attributes[0].Description : objEnum.ToString() ?? string.Empty;
    }
    public static string Description(this Enum value)
    {
        return GetCustomDescription(value);
    }
}