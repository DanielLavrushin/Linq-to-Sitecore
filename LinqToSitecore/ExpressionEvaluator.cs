using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LinqToSitecore.Opcodes;
using Sitecore.Data.Query;
using Sitecore.Data.Serialization.Templates;

namespace LinqToSitecore
{
    public class OpcodeHelper
    {
        public Opcode Code { get; set; }
    }

    public class LinqToSitecoreVisitor: ExpressionVisitor
    {
        private readonly Type _type;
        public Opcode CodeTree;

        public LinqToSitecoreVisitor(Opcode code, Type type)
        {
            CodeTree = code;
            _type = type;
        }


        public override Expression Visit(Expression node)
        {
            base.Visit(node);
            return node;
        }

        private ConstantExpression EvalMember(Expression node)
        {
            if (node is ConstantExpression)
            {
                return node.Cast<ConstantExpression>();
            }

            if (node is MemberExpression)
            {
                return EvalMember(node.Cast<MemberExpression>().Expression);
            }
            return null;
        }
        private object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
        public Opcode EvalOpcodeExpression(Expression node)
        {

            if (node is MemberExpression)
            {
                var mNode = node.Cast<MemberExpression>();
                var expression = EvalMember(mNode.Expression);
                if (expression is ConstantExpression)
                {
                    var value = GetValue(mNode);

                    if (value is bool)
                    {
                        return new BooleanValue((bool)value);

                    }
                    return new Literal(value?.ToString() ?? string.Empty);
                }

                return new FieldElement(mNode.Member.Name);
            }
            if (node is MethodCallExpression)
            {
                var mNode = node.Cast<MethodCallExpression>();
                var field = ((MemberExpression)mNode.Object).Member.Name;

                object obj = null;


                var value = GetValue(mNode.Arguments[0]);
                var func = new EqualsOperator(EvalOpcodeExpression(mNode.Object), new LinqToSitecoreFunction("contains", field, value));
                return func;
            }
            if (node is ConstantExpression)
            {
                var value = GetValue(node);
                if (value is bool)
                {
                    return new BooleanValue((bool)value);

                }

            }
            if (node is BinaryExpression)
            {
                var bnode = node.Cast<BinaryExpression>();

                var left = EvalOpcodeExpression(bnode.Left);
                var right = EvalOpcodeExpression(bnode.Right);
                switch (node.NodeType)
                {
                    case ExpressionType.And:
                        return new AndOperator(left, right);
                    case ExpressionType.AndAlso:
                        return new AndOperator(left, right);
                    case ExpressionType.Or:
                        return new OrOperator(left, right);
                    case ExpressionType.OrElse:
                        return new OrOperator(left, right);
                    case ExpressionType.NotEqual:
                        return new UnequalsOperator(left, right);
                    case ExpressionType.Equal:
                        return new EqualsOperator(left, right);
                    default:
                        throw new NotImplementedException($"{node.NodeType} is not supported.");
                }
            }
            if (node is LambdaExpression)
            {
                var templateCode = new AndOperator(
               new EqualsOperator(new FieldElement("@templatename"), new Literal(_type.Name)), EvalOpcodeExpression(node.Cast<LambdaExpression>().Body));
                var root = new Root { NextStep = new DescendantOrSelf(new ItemElement("*", new Predicate(templateCode))) };
                CodeTree = root;
            }

            return CodeTree;
        }



        public static Opcode GetCode(Expression expression, Opcode code, Type type)
        {
            var visitor = new LinqToSitecoreVisitor(code, type);
            visitor.EvalOpcodeExpression(expression);

            if (visitor.CodeTree == null)
            {
                var templateCode = new EqualsOperator(new FieldElement("@templatename"), new Literal(type.Name));
                var root = new Root { NextStep = new Descendant(new ItemElement("*", new Predicate(templateCode))) };
                return root;
            }
            return visitor.CodeTree;
        }
    }















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
        public static Opcode EvalToSitecore(Expression expression, Type itemToType)
        {
            var evalexpression = PartialEval(expression, ExpressionEvaluator.CanBeEvaluatedLocally);

            var something = PathVisitor.GetSitecoreQuery(evalexpression, itemToType);

            return something;
        }

        class PathVisitor: ExpressionVisitor
        {
            private string _path;
            private Dictionary<string, string> _values;
            List<Opcode> _codes;
            private Opcode _code;
            private Opcode _codeBinaryLeft;
            private Opcode _codeBinaryRight;
            private Opcode _left;
            private Opcode _right;
            private Type _type;
            private HashSet<Expression> candidates;
            protected override Expression VisitLambda<T>(Expression<T> node)
            {

                Visit(node.Body);


                var templateCode =
                    new AndOperator(new EqualsOperator(new FieldElement("@templatename"), new Literal(_type.Name)),
                        _code ?? _left);

                var root = new Root();
                root.NextStep = new DescendantOrSelf(new ItemElement("*", new Predicate(templateCode)));
                _code = root;

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

                _right = new Literal(value);

                return node;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                Visit(node.Left);
                if (node.Left is BinaryExpression)
                {
                    _codeBinaryLeft = _code;
                }
                Visit(node.Right);
                if (node.Right is BinaryExpression)
                {
                    _codeBinaryRight = _code;
                }

                switch (node.NodeType)
                {
                    case ExpressionType.And:
                        _code = new AndOperator(_left, _right);
                        break;
                    case ExpressionType.Or:
                        _code = new OrOperator(_left, _right);
                        break;
                    case ExpressionType.OrElse:
                        _code = new OrOperator(_codeBinaryLeft, _codeBinaryRight);
                        break;
                    case ExpressionType.AndAlso:
                        _code = new AndOperator(_codeBinaryLeft, _codeBinaryRight);
                        break;
                    case ExpressionType.Equal:
                        _code = new EqualsOperator(_left, _right);
                        break;
                    default:
                        throw new NotImplementedException($"{node.NodeType} is not supported.");
                }

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                base.VisitMethodCall(node);
                _code = new EqualsOperator(_left, _right);

                return node;

            }

            protected override Expression VisitMember(MemberExpression node)
            {
                base.VisitMember(node);
                _left = new FieldElement(node.Member.Name);
                return node;
            }

            public override Expression Visit(Expression exp)
            {
                base.Visit(exp);
                _path = exp.ToString();
                return exp;
            }

            public static Opcode GetSitecoreQuery(Expression expression, Type itemToType)
            {
                var visitor = new PathVisitor
                {
                    _type = itemToType,
                    _values = new Dictionary<string, string>(),
                    _codes = new List<Opcode>()
                };
                visitor.Visit(expression);
                return visitor._code;
            }



            public static string GetPath(Expression expression)
            {
                var visitor = new PathVisitor();
                visitor._values = new Dictionary<string, string>();
                visitor._codes = new List<Opcode>();
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
