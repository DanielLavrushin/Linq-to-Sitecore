using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSitecore
{


    public static class ExpressionEvaluator
    {

        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        public static string EvalToString(Expression expression)
        {
            var evalexpression = PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);


            return PathVisitor.GetPath(evalexpression);
        }

        class PathVisitor: ExpressionVisitor
        {
            private string _path;
            private Dictionary<string, string> _values;

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Visit(node.Body);
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                var value = node.Value.ToString();
                var oldvalue = node.Value.ToString();
                if (node.Value is DateTime)
                {
                    //20120514T000000
                    value = $"'{Sitecore.DateUtil.ToIsoDate((DateTime)node.Value)}'";
                }
                if (!_values.ContainsKey(oldvalue))
                {
                    _values.Add(oldvalue, value);
                }
                return node;
            }

            public override Expression Visit(Expression exp)
            {
                base.Visit(exp);
                _path = exp.ToString();
                return exp;
            }



            public static string GetPath(Expression expression)
            {
                var visitor = new PathVisitor();
                visitor._values = new Dictionary<string, string>();
                visitor.Visit(expression);

                foreach (var v in visitor._values)
                {
                    visitor._path = visitor._path.Replace(v.Key, v.Value);
                }
                return visitor._path;
            }
        }

        class SubtreeEvaluator: ExpressionVisitor
        {

            HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {

                this.candidates = candidates;
            }


            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }


            public override Expression Visit(Expression exp)
            {

                if (exp == null) return null;


                if (this.candidates.Contains(exp))
                {

                    return this.Evaluate(exp);

                }

                return base.Visit(exp);

            }



            private Expression Evaluate(Expression e)
            {

                if (e.NodeType == ExpressionType.Constant)
                {

                    return e;

                }

                LambdaExpression lambda = Expression.Lambda(e);

                Delegate fn = lambda.Compile();

                return Expression.Constant(fn.DynamicInvoke(null), e.Type);

            }

        }



        class Nominator: ExpressionVisitor
        {

            Func<Expression, bool> fnCanBeEvaluated;

            HashSet<Expression> candidates;

            bool cannotBeEvaluated;



            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {

                this.fnCanBeEvaluated = fnCanBeEvaluated;

            }



            internal HashSet<Expression> Nominate(Expression expression)
            {

                this.candidates = new HashSet<Expression>();

                this.Visit(expression);

                return this.candidates;

            }


            public override Expression Visit(Expression expression)
            {

                if (expression != null)
                {

                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;

                    this.cannotBeEvaluated = false;

                    base.Visit(expression);

                    if (!this.cannotBeEvaluated)
                    {

                        if (this.fnCanBeEvaluated(expression))
                        {

                            this.candidates.Add(expression);

                        }

                        else
                        {

                            this.cannotBeEvaluated = true;

                        }

                    }

                    this.cannotBeEvaluated |= saveCannotBeEvaluated;

                }
                return expression;

            }

        }

    }
}
