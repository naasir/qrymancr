namespace Qrymancr
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using System.Web;

    using Qrymancr.Extensions;

    /// <summary>
    /// Dynamically builds LINQ expressions from a URL query string.
    /// </summary>
    /// <typeparam name="TFor">The type we want to build a query expression for.</typeparam>
    public class Qrymancr<TFor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Qrymancr{TFor}"/> class.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        public Qrymancr(string queryString)
            : this(queryString, new List<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Qrymancr{TFor}"/> class.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <param name="toIgnore">The list of parameters to ignore.</param>
        public Qrymancr(string queryString, IEnumerable<string> toIgnore)
        {
            var queryParameters = HttpUtility.ParseQueryString(queryString);
            this.ExpressionString = BuildQueryExpressionString<TFor>(queryParameters, toIgnore);
        }

        /// <summary>
        /// Gets the resulting LINQ expression string.
        /// </summary>
        public string ExpressionString { get; private set; }

        /// <summary>
        /// Builds a LINQ query expression from the specified query string.
        /// </summary>
        /// <typeparam name="TFor">The type we want to build a query expression for.</typeparam>
        /// <param name="queryString">The query string.</param>
        /// <returns>The built LINQ query expression.</returns>
        public static Expression<Func<TFor, bool>> Build(string queryString)
        {
            return Build(queryString, new List<string>());
        }

        /// <summary>
        /// Builds a LINQ query expression from the specified query string.
        /// </summary>
        /// <typeparam name="TFor">The type we want to build a query expression for.</typeparam>
        /// <param name="queryString">The query string.</param>
        /// <param name="toIgnore">The list of parameters to ignore.</param>
        /// <returns>The built LINQ query expression.</returns>
        public static Expression<Func<TFor, bool>> Build(string queryString, IEnumerable<string> toIgnore)
        {
            return new Qrymancr<TFor>(queryString, toIgnore).Build();
        }

        /// <summary>
        /// Builds a compiled LINQ query from the specified query string.
        /// </summary>
        /// <typeparam name="TFor">The type we want to build a query expression for.</typeparam>
        /// <param name="queryString">The query string.</param>
        /// <returns>The compiled LINQ query.</returns>
        public static Func<TFor, bool> Compile(string queryString)
        {
            return Compile(queryString, new List<string>());
        }

        /// <summary>
        /// Builds a compiled LINQ query from the specified query string.
        /// </summary>
        /// <typeparam name="TFor">The type we want to build a query expression for.</typeparam>
        /// <param name="queryString">The query string.</param>
        /// <param name="toIgnore">The list of parameters to ignore.</param>
        /// <returns>The compiled LINQ query.</returns>
        public static Func<TFor, bool> Compile(string queryString, IEnumerable<string> toIgnore)
        {
            return new Qrymancr<TFor>(queryString, toIgnore).Compile();
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>The built LINQ query expression.</returns>
        public Expression<Func<TFor, bool>> Build()
        {
            return string.IsNullOrEmpty(this.ExpressionString)
                ? null
                : System.Linq.Dynamic.DynamicExpression.ParseLambda<TFor, bool>(this.ExpressionString);
        }

        /// <summary>
        /// Compiles this instance.
        /// </summary>
        /// <returns>The compiled LINQ query.</returns>
        public Func<TFor, bool> Compile()
        {
            return this.Build().Compile();
        }

        /// <summary>
        /// Builds a dynamic LINQ query expression string from the specified query parameters.
        /// </summary>
        /// <typeparam name="T">The object type to query.</typeparam>
        /// <param name="queryParameters">The query parameters.</param>
        /// <param name="toIgnore">The list of parameters to ignore.</param>
        /// <returns>
        /// The dynamic LINQ query expression string.
        /// </returns>
        /// <example>
        /// <![CDATA[
        /// query: ?deleted=true                becomes => (deleted == true)
        /// query: ?status!=failed              becomes => (status != \"failed\")
        /// query: ?status=created,pending      becomes => (status == \"created\") OR (status == \"pending\")
        /// query: ?deleted=false&archived=true becomes => (deleted == false) AND (archived == true)
        /// query: ?project-name=test           becomes => (project.name == test)
        /// query: ?legs[0]-status=scheduled    becomes => (legs[0].status == scheduled)
        /// ]]>
        /// For more examples, see the unit tests for this class (DynamicLinqExpressionBuilderTest).
        /// </example>
        /// <remarks>
        /// - Currently, query parameters can be used to filter based on equality, inequality, greater-than or less-than.
        /// - Equality comparisons for string properties are case-insensitive.
        /// - Nested properties can be specified via hyphenated query parameters (e.g. template-name).
        /// - Generic collection properties are also supported, but Arrays are not (not yet).
        /// </remarks>
        private static string BuildQueryExpressionString<T>(NameValueCollection queryParameters, IEnumerable<string> toIgnore)
        {
            var items = new List<string>();
            foreach (string name in queryParameters)
            {
                if (toIgnore.Contains(name))
                {
                    continue;
                }

                // every query parameter can have multiple values delimited by a comma,
                // so build a LINQ expression string in the form of:
                // (key = value 1 OR key = value2 OR key = value 3)
                var key = name;
                var values = queryParameters[name].Split(',');
                var pairs = values.Select(value => FormatKeyValuePairForLinqExpression<T>(key, value));
                var subexpression = string.Join(" OR ", pairs);

                if (string.IsNullOrEmpty(subexpression))
                {
                    continue;
                }

                items.Add(string.Format("({0})", subexpression));
            }

            return string.Join(" AND ", items.ToArray());
        }

        /// <summary>
        /// Formats the key value pair for the dynamic LINQ expression.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The formatted key value pair.</returns>
        private static string FormatKeyValuePairForLinqExpression<T>(string key, string value)
        {
            var comparison = new KeyValueComparison(key, value);

            // build the property path from the key by removing any array brackets (e.g. [0])
            var path = Regex.Replace(comparison.Key, @"\[\d+\]", string.Empty);
            var property = typeof(T).GetNestedProperty(path);
            if (property == null)
            {
                return null;
            }

            var type = property.PropertyType;
            if (type.IsNullable())
            {
                type = type.GetGenericArguments()[0];
            }

            if (!type.IsEnum && (type.IsNumeric() || type == typeof(bool) || value.ToLower() == "null"))
            {
                return comparison.ToString("{0} {2}= {1}");
            }

            if (type == typeof(string))
            {
                return FormatStringComparison(comparison);
            }

            if (type == typeof(DateTime))
            {
                return comparison.ToString("{0} {2}= DateTime.Parse(\"{1}\")");
            }

            return comparison.ToString("{0} {2}= \"{1}\"");
        }

        /// <summary>
        /// Formats the specified string comparison as a Dynamic LINQ expression string.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        /// <returns>The formatted string.</returns>
        private static string FormatStringComparison(KeyValueComparison comparison)
        {
            // Simplest way to do a case-insensitive search is to convert everything to the same case.
            // Tried to do .Equals(value, StringComparison.IgnoreCase), but that didn't seem to play nice.
            var equality = string.Format("{0}.ToUpper() == \"{1}\".ToUpper()", comparison.Key, comparison.Value);

            // Using the same convention as CSS attribute selectors
            // see: https://developer.mozilla.org/en/CSS/Attribute_selectors
            var operatorMethodMap = new Dictionary<char, string>
                {
                    { '^', "StartsWith" },
                    { '$', "EndsWith" },
                    { '*', "Contains" }
                };

            if (operatorMethodMap.Keys.Contains(comparison.Operator))
            {
                var method = operatorMethodMap[comparison.Operator];
                return string.Format("{0}.{2}(\"{1}\")", comparison.Key, comparison.Value, method);
            }

            if (comparison.Operator == '!')
            {
                return string.Format("!({0})", equality);
            }

            return equality;
        }
    }
}