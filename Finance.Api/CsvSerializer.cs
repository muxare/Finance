﻿using System.Reflection;

namespace Finance.Api;


public static class CsvSerializer {
    /// <summary>
    /// <para>Serialize objects to Comma Separated Value (CSV) format [1].</para>
    /// <para>
    /// Rather than try to serialize arbitrarily complex types with this
    /// function, it is better, given type A, to specify a new type, A'.
    /// Have the constructor of A' accept an object of type A, then assign
    /// the relevant values to appropriately named fields or properties on
    /// the A' object.
    /// </para>
    /// <para>[1] http://tools.ietf.org/html/rfc4180</para>
    /// </summary>
    public static void Serialize<T>(TextWriter output, IEnumerable<T> objects) {
        var fields =
            from mi in typeof (T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            where new [] { MemberTypes.Field, MemberTypes.Property }.Contains(mi.MemberType)
            let orderAttr = (ColumnOrderAttribute) Attribute.GetCustomAttribute(mi, typeof (ColumnOrderAttribute))
            orderby orderAttr == null ? int.MaxValue : orderAttr.Order, mi.Name
            select mi;
        output.WriteLine(QuoteRecord(fields.Select(f => f.Name)));
        foreach (var record in objects) {
            output.WriteLine(QuoteRecord(FormatObject(fields, record)));
        }
    }

    static IEnumerable<string> FormatObject<T>(IEnumerable<MemberInfo> fields, T record) {
        foreach (var field in fields) {
            if (field is FieldInfo fieldInfo) {
                var fi =  fieldInfo;
                yield return Convert.ToString(fi.GetValue(record));
            } else if (field is PropertyInfo propertyInfo) {
                var pi =  propertyInfo;
                yield return Convert.ToString(pi.GetValue(record, null));
            } else {
                throw new Exception("Unhandled case.");
            }
        }
    }

    const string CsvSeparator = ",";

    static string QuoteRecord(IEnumerable<string> record) {
        return String.Join(CsvSeparator, record.Select(field => QuoteField(field)).ToArray());
    }

    static string QuoteField(string field) {
        if (string.IsNullOrEmpty(field)) {
            return "\"\"";
        } else if (field.Contains(CsvSeparator) || field.Contains('"') || field.Contains('\r') || field.Contains('\n')) {
            return string.Format("\"{0}\"", field.Replace("\"", "\"\""));
        } else {
            return field;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ColumnOrderAttribute : Attribute {
        public int Order { get; }
        public ColumnOrderAttribute(int order) { Order = order; }
    }
}
