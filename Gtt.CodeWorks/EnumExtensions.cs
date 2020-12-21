using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Gtt.CodeWorks
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum enumValue)
        {
            if (enumValue == null) return null;

            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
            var description = (DescriptionAttribute)memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return description?.Description ?? enumValue.ToString();
        }

        public static EnumDetails ToEnumDetails(this Enum enumValue)
        {
            return new EnumDetails(enumValue);
        }
    }
}
