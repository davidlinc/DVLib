using System.Linq.Expressions;

namespace DVLib.LabDataHelper.DVScript
{

	internal interface ISetter
	{
		ScriptObject setValue(ScriptObject value);
	}
	public delegate Expression SingleExpMaker(Expression a);
	public delegate Expression BinaryExpMaker(Expression a,Expression b);
	public delegate Expression TriExpMaker(Expression a, Expression b, Expression c);
	public delegate Expression MutiExpMaker(params Expression[] a);
	public delegate T Function<I, T>(params I[] Params); 
	public abstract class ScriptObject
	{


		public ScriptObject(Type type) {
		this.returnType = type;
		}

		ScriptInfo ScriptInfo;
		internal Type returnType = typeof(double);
		public ScriptObject setReturnType(Type type)
		{
			this.returnType = type;
			return this;
		}
		public abstract (Expression,HashSet<ParameterExpression>) getExpression();

		internal static ScriptObject Convert(ScriptObject script,Type returnType)
		{
			return new ScriptObject<Expression>(returnType,a => Expression.Convert(a, returnType), script);
		}
	
		internal ScriptObject setScriptInfo(ScriptInfo info)
		{
			this.ScriptInfo = info;
			return this;
		}

		public LambdaExpression lambda()
		{
			var v=getExpression();                         
			return Expression.Lambda(v.Item1,v.Item2);
		}

		public Delegate Compile()
		{
			return lambda().Compile();
		}

		public static ScriptObject DoubleParam()
		{
			return new SourceScriptObject(typeof(double),Expression.Parameter(typeof(double)));
		}

		public Func<double,double> AsFx()
		{
			return (Func<double,double>)Compile();
		}
		public static ScriptObject ConstDouble(double d)
		{
			return new SourceScriptObject(typeof(double),Expression.Constant(d));
		}
		public static ScriptObject ConstObject(object? o)
		{
			return new SourceScriptObject(typeof(object),Expression.Constant(o));
		}
		public static ScriptObject ConstString(string s)
		{
			return new SourceScriptObject(typeof(string),Expression.Constant(s));
		}
	}



	
	internal class SourceScriptObject : ScriptObject
	{
		Expression expression;

		public SourceScriptObject(Type type, Expression expression):base(type) 
		{ this.expression = expression; }


		public override (Expression, HashSet<ParameterExpression>) getExpression()
		{
			HashSet <ParameterExpression> tmp = new HashSet <ParameterExpression> ();
			if (expression is ParameterExpression)
			{
				tmp.Add((ParameterExpression)expression);
			}
			return (expression, tmp);
		}
	}

	internal class VarScriptObject : SourceScriptObject,ISetter
	{
		public VarScriptObject(Type type, Expression expression) : base(type, expression)
		{
		}

		public ScriptObject setValue(ScriptObject value)
		{
			throw new NotImplementedException();
		}
	}

	internal class MutiScriptObject : ScriptObject
	{

		ScriptObject[] expressions;
		MutiExpMaker expMaker;

		internal MutiScriptObject(Type type, MutiExpMaker expMaker,params ScriptObject[] expressions):base(type)
		{
			this.expMaker = expMaker;
			this.expressions = expressions;
		}

		(Expression[],HashSet<ParameterExpression>) GetExpressions()
		{
			var es = new Expression[expressions.Length];
			HashSet<ParameterExpression> list = new HashSet<ParameterExpression>();
			(Expression, HashSet<ParameterExpression>) p;
			for (int i = 0; i < es.Length; i++) {
				p=expressions[i].getExpression();
				es[i] = p.Item1;
				foreach(var pp in p.Item2)
				{
					list.Add(pp);
				}
				
			}
			return (es,list);
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression()
		{
			var v = GetExpressions();
			return (expMaker(v.Item1),v.Item2);
		}

	
	}
	





}
