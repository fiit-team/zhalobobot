using System;
using System.ComponentModel;
using System.Linq;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum FromDescriptionTo<TEnum>(this string description)
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .First(c => string.Equals(c.GetDescription(), description, StringComparison.InvariantCultureIgnoreCase));
        }
        
        public static string? GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);

            if (name == null) 
                return null;
            
            var field = type.GetField(name);
            
            if (field == null) 
                return null;
            
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr?.Description;
        }
    }
}