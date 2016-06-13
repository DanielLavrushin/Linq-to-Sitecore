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
using Sitecore.Publishing.Explanations;
using Sitecore.Xml.Patch;

namespace LinqToSitecore
{

    public class LinqToSitecoreVisitor: ExpressionVisitor
    {
        private string GetPropertyName(MemberInfo member, string propName)
        {
            if (member == null) return propName.ToLower();


            var sfn = (SitecoreFieldAttribute)member.GetCustomAttributes(typeof(SitecoreFieldAttribute)).FirstOrDefault();
            return sfn?.Name.ToLower() ?? propName.ToLower();
        }

        private readonly Type _type;
        public Opcode CodeTree;
        private bool _isBinaryComparor = false;
        private bool _isFieldUnaryEqual = false;
        private bool _isFuncUnariNot = false;
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

        private Expression EvalMember(Expression node)
        {
            if (node == null || node.NodeType == ExpressionType.Parameter) return null;
            if (node is ConstantExpression)
            {
                return node.Cast<ConstantExpression>();
            }

            if (node is MemberExpression)
            {
                return EvalMember(node.Cast<MemberExpression>().Expression) ?? node;
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


            if (node is UnaryExpression)
            {
                var uNode = node.Cast<UnaryExpression>();
                if (uNode.NodeType == ExpressionType.Not)
                {
                    _isFuncUnariNot = true;
                }
                var opcode = EvalOpcodeExpression(uNode.Operand);
                if (opcode is FieldElement)
                {
                    if (uNode.NodeType == ExpressionType.Not)
                    {
                        return new EqualsOperator(opcode, new BooleanValue(false));
                    }
                    return new EqualsOperator(opcode, new BooleanValue(true));
                }
            }
            if (node is MemberExpression)
            {
                var mNode = node.Cast<MemberExpression>();
                var expression = EvalMember(mNode.Expression ?? mNode);
                if (expression != null)
                {
                    var value = GetValue(mNode);

                    if (value is bool)
                    {
                        return new BooleanValue((bool)value);

                    }
                    if (value is DateTime)
                    {
                        return new Literal(((DateTime)value).ToString("yyyyMMddThhmmss"));

                    }
                    if (value is int)
                    {
                        return new Number((int)value);

                    }
                    return new Literal(value?.ToString() ?? string.Empty);
                }

                return new FieldElement(GetPropertyName(mNode.Member, mNode.Member.Name));

            }
            if (node is MethodCallExpression)
            {
                var mNode = node.Cast<MethodCallExpression>();
                var field = ((MemberExpression)mNode.Object).Member.Name;


                var value = GetValue(mNode.Arguments[0]);
                if (_isFuncUnariNot)
                {
                    var func = new UnequalsOperator(EvalOpcodeExpression(mNode.Object),
                        new LinqToSitecoreFunction(mNode.Method.Name.ToLower(), field, value));
                    _isFuncUnariNot = false;
                    return func;
                }

                var func2 = new EqualsOperator(EvalOpcodeExpression(mNode.Object),
                 new LinqToSitecoreFunction(mNode.Method.Name.ToLower(), field, value));
                _isFuncUnariNot = false;
                return func2;

            }
            if (node is ConstantExpression)
            {
                var value = GetValue(node);
                if (value is bool)
                {
                    return new BooleanValue((bool)value);
                }

                return new Literal(value?.ToString() ?? string.Empty);
            }
            if (node is BinaryExpression)
            {

                _isFieldUnaryEqual = false;
                var bnode = node.Cast<BinaryExpression>();



                var left = EvalOpcodeExpression(bnode.Left);
                var right = EvalOpcodeExpression(bnode.Right);

                if (node.NodeType != ExpressionType.Equal &&
                    node.NodeType != ExpressionType.NotEqual &&
                    bnode.Left is MemberExpression &&
                   (bnode.Right is BinaryExpression || bnode.Right.NodeType == ExpressionType.Call))
                {
                    if (bnode.Left.Cast<MemberExpression>().Type == typeof(bool))
                    {
                        left = new EqualsOperator(left, new BooleanValue(true));
                    }
                }
                if (node.NodeType != ExpressionType.Equal &&
                   node.NodeType != ExpressionType.NotEqual &&
                   bnode.Right is MemberExpression &&
                   (bnode.Left is BinaryExpression || bnode.Left.NodeType == ExpressionType.Call))
                {
                    if (bnode.Right.Cast<MemberExpression>().Type == typeof(bool))
                    {
                        right = new EqualsOperator(right, new BooleanValue(true));
                    }
                }

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
                    case ExpressionType.GreaterThanOrEqual:
                        return new GreaterOrEqualsOperator(left, right);
                    case ExpressionType.GreaterThan:
                        return new GreaterOperator(left, right);
                    case ExpressionType.LessThanOrEqual:
                        return new SmallerOrEqualsOperator(left, right);
                    case ExpressionType.LessThan:
                        return new SmallerOperator(left, right);
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


}
