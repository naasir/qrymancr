namespace Qrymancr.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using NSpec;

    using Qrymancr;

    public class describe_qrymancr : nspec
    {
        void given_query_against_boolean_property()
        {
            context["that is not nullable"] = () =>
            {
                it["should build correct expression for equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?boolprop=true", "(boolprop == true)");

                it["should build correct expression for inequality comparison"] =
                    () => this.BuildAndVerifyNotNull("?boolprop!=true", "(boolprop != true)");

                it["should NOT build expression for null equality comparison"] =
                    () => this.BuildAndVerifyNull("?boolprop=null", "(boolprop == null)");
            };

            context["that is nullable"] = () =>
            {
                it["should build correct expression for equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableboolprop=true", "(nullableboolprop == true)");

                it["should build correct expression for inequality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableboolprop!=true", "(nullableboolprop != true)");

                it["should build correct expression for null equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableboolprop=null", "(nullableboolprop == null)");
            };
        }

        void give_query_against_integer_property()
        {
            context["that is not nullable"] = () =>
            {
                it["should build correct expression for equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?intprop=24", "(intprop == 24)");

                it["should build correct expression for inequality comparison"] =
                    () => this.BuildAndVerifyNotNull("?intprop!=24", "(intprop != 24)");

                it["should NOT build expression for null equality comparison"] =
                    () => this.BuildAndVerifyNull("?intprop=null", "(intprop == null)");
            };

            context["that is nullable"] = () =>
            {
                it["should build correct expression for equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableintprop=24", "(nullableintprop == 24)");

                it["should build correct expression for inequality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableintprop!=24", "(nullableintprop != 24)");

                it["should build correct expression for null equality comparison"] =
                    () => this.BuildAndVerifyNotNull("?nullableintprop=null", "(nullableintprop == null)");
            };
        }

        void BuildAndVerifyNotNull(string queryString, string expectedExpression)
        {
            var qrymancr = new Qrymancr<Mock>(queryString);
            var actualExpression = qrymancr.ExpressionString;
            actualExpression.should_be(expectedExpression);
            
            var predicate = qrymancr.Build();
            Console.WriteLine(predicate.ToString());
            predicate.should_not_be_null();

            var list = new List<Mock> { new Mock() };
            var results = list.Where(predicate.Compile());
        }

        void BuildAndVerifyNull(string queryString, string expectedExpression)
        {
            var qrymancr = new Qrymancr<Mock>(queryString);
            var actualExpression = qrymancr.ExpressionString;
            actualExpression.should_be(expectedExpression);
            var linq = qrymancr.Build();
            Console.WriteLine(linq.ToString());
            linq.should_be_null();
        }

        private static bool ExpressionEqual(Expression x, Expression y)
        {
            // deal with the simple cases first...
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.NodeType != y.NodeType
                || x.Type != y.Type) return false;

            switch (x.NodeType)
            {
                case ExpressionType.Lambda:
                    return ExpressionEqual(((LambdaExpression)x).Body, ((LambdaExpression)y).Body);
                case ExpressionType.MemberAccess:
                    MemberExpression mex = (MemberExpression)x, mey = (MemberExpression)y;
                    return mex.Member == mey.Member; // should really test down-stream expression
                default:
                    throw new NotImplementedException(x.NodeType.ToString());
            }
        }

        class Mock
        {
            public bool BoolProp { get; set; }

            public bool? NullableBoolProp { get; set; }

            public int IntProp { get; set; }

            public int? NullableIntProp { get; set; }

            public long LongProp { get; set; }

            public DayOfWeek EnumProp { get; set; }

            public string StringProp { get; set; }

            public DateTime DateTimeProp { get; set; }

            public DateTime? NullableDateTimeProp { get; set; }

            public Type NestedProp { get; set; }

            public IList<DateTime> ArrayProp { get; set; }
        }
    }
}