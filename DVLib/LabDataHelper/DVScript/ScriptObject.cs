using System.Linq.Expressions;

using E = System.Linq.Expressions.Expression;
namespace DVLib.LabDataHelper.DVScript
{

	internal interface ISetter
	{
		AssignScript setValue(ScriptObject value);
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
		public abstract (Expression,HashSet<ParameterExpression>) getExpression(bool force=false);

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
			return new RootElementScriptObject(typeof(double),Expression.Parameter(typeof(double)));
		}

		public Func<double,double> AsFx()
		{
			return (Func<double,double>)Compile();
		}
		public static ScriptObject ConstDouble(double d)
		{
			return new RootElementScriptObject(typeof(double),Expression.Constant(d));
		}
		public static ScriptObject ConstObject(object? o)
		{
			return new RootElementScriptObject(typeof(object),Expression.Constant(o));
		}
		public static ScriptObject ConstString(string s)
		{
			return new RootElementScriptObject(typeof(string),Expression.Constant(s));
		}
	}



	
	internal class RootElementScriptObject : ScriptObject
	{
		internal Expression expression;
		bool checkParameter = true;
		public RootElementScriptObject(Type type, Expression expression):base(type) 
		{ this.expression = expression; }

		public virtual RootElementScriptObject setCheck(bool check)
		{
			this.checkParameter = check;	
			return this;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			HashSet <ParameterExpression> tmp = new HashSet <ParameterExpression> ();
			if ((checkParameter||force)&& expression is ParameterExpression)
			{
				tmp.Add((ParameterExpression)expression);
			}
			return (expression, tmp);
		}
	}

	internal class AssignScript : ScriptObject<E, E>
	{
		internal AssignScript(Type type, VarScriptObject a, ScriptObject b) : base(type, (e1, e2) => Expression.Assign(e1,e2), a, b)
		{

		}

		public VarScriptObject GetVarScript()
		{
			return (VarScriptObject)base.a;
		}
	}
	internal class ReturnScript : ScriptObject<E>
	{
		LabelTarget label;
		internal ReturnScript(Type type, LabelTarget a,ScriptObject b) : base(type,(lb)=> { if (lb == null) return Expression.Return(a); return Expression.Return(a, lb); }, b)
		{

		}
		public LabelTarget GetLabel()
		{
			return label;
		}
	}
	internal class VarScriptObject : RootElementScriptObject,ISetter
	{
		public VarScriptObject(Type type, ParameterExpression expression) : base(type, expression)
		{
		}

		public virtual AssignScript  setValue(ScriptObject value)
		{
			return  new AssignScript(typeof(void),this,value);
			//return new SourceScriptObject( typeof(void), Expression.Assign(expression, value.getExpression().Item1)); 
		}

		public virtual ParameterExpression getValue() {
		return (ParameterExpression)expression;
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

		(Expression[],HashSet<ParameterExpression>) GetExpressions(bool force = false)
		{
			var es = new Expression[expressions.Length];
			HashSet<ParameterExpression> list = new HashSet<ParameterExpression>();
			(Expression, HashSet<ParameterExpression>) p;
			for (int i = 0; i < es.Length; i++) {
				p=expressions[i].getExpression(force);
				es[i] = p.Item1;
				foreach(var pp in p.Item2)
				{
					list.Add(pp);
				}
				
			}
			return (es,list);
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var v = GetExpressions(force);
			return (expMaker(v.Item1),v.Item2);
		}

	
	}
	





}
