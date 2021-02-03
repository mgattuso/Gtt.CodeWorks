using System;

namespace Gtt.CodeWorks.AzureStorage
{
    public static class DataTableExtensions
    {
        public static double ToDouble(this decimal val)
        {
            return Convert.ToDouble(val);
        }

        public static decimal ToDecimal(this double val)
        {
            return Convert.ToDecimal(val);
        }

        public static Guid ToGuid(this string val)
        {
            return new Guid(val);
        }

        public static long ToLong(this string val)
        {
            return Convert.ToInt64(val);
        }

        public static T UpdateAuditable<T>(this T table, BaseDto dto) where T : BaseTable
        {
            table.Created = dto.Created;
            table.Modified = dto.Modified;
            table.CreatedBy = dto.CreatedBy;
            table.ModifiedBy = dto.ModifiedBy;
            return table;
        }

        public static T UpdateAuditable<T>(this T table, BaseTable dto) where T : BaseDto
        {
            table.Created = dto.Created;
            table.Modified = dto.Modified;
            table.CreatedBy = dto.CreatedBy;
            table.ModifiedBy = dto.ModifiedBy;
            return table;
        }
    }
}