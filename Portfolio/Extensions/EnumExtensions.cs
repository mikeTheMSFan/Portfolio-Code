#region Imports

using System.ComponentModel.DataAnnotations;
using System.Reflection;

#endregion

namespace Portfolio.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDisplayName(this Enum enumType)
    {
        var enumDisplayName = enumType.GetType().GetMember(enumType.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()!
            .Name;

        return enumDisplayName ??= string.Empty;
    }
}