namespace Qrymancr.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions to the base <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the specified nested property of this type or null if it does not exist.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The specified nested property.</returns>
        /// <remarks>
        /// Inspired by: http://stackoverflow.com/questions/3111725/how-to-get-nested-properties
        /// </remarks>
        public static PropertyInfo GetNestedProperty(this Type type, string propertyName)
        {
            var parts = propertyName.Split('.');
            const BindingFlags Flags = BindingFlags.IgnoreCase
                                       | BindingFlags.FlattenHierarchy
                                       | BindingFlags.Public
                                       | BindingFlags.Instance;

            if (parts.Length == 1)
            {
                return type.GetProperty(propertyName, Flags);
            }

            var nestedProperty = type.GetProperty(parts[0], Flags);

            if(nestedProperty == null) return null;

            type = nestedProperty.PropertyType.IsGenericCollectionType()
                       ? nestedProperty.PropertyType.GetGenericArguments()[0]
                       : nestedProperty.PropertyType;

            return GetNestedProperty(type, parts.Skip(1).Aggregate((a, i) => a + "." + i));
        }

        /// <summary>
        /// Gets the default value for the given type.
        /// </summary>
        /// <param name="type">The type to get the default value of.</param>
        /// <returns>The default value.</returns>
        /// <remarks>
        /// Borrowed from: http://stackoverflow.com/questions/325426/c-programmatic-equivalent-of-defaulttype
        /// </remarks>
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified type is a generic collection type.
        /// </summary>
        /// <remarks>
        /// A generic collection is either of type IEnumerable(T) or inherits from ICollection(T)
        /// </remarks>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <c>true</c> if it is a generic collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericCollectionType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return true;
            }

            return type.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        /// <summary>
        /// Determines whether the specified type is a generic dictionary type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        ///   <c>true</c> if it is a generic dictionary; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericDictionaryType(this Type type)
        {
            return type.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        /// <summary>
        /// Determines if this type is numeric.
        /// </summary>
        /// <param name="type">The type in question.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is numeric; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// - Nullable numeric types are considered numeric.
        /// - Boolean is not considered numeric.
        /// - Borrowed from: http://stackoverflow.com/questions/124411/using-net-how-can-i-determine-if-a-type-is-a-numeric-valuetype
        /// </remarks>
        public static bool IsNumeric(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumeric(Nullable.GetUnderlyingType(type));
                    }

                    return false;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is Nullable.
        /// </summary>
        /// <param name="type">The type in question.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is Nullable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType 
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}