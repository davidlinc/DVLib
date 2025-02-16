using DVOSLib;
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

		Delegate cache;
	
		public abstract (Expression,HashSet<ParameterExpression>) getExpression(bool force=false);

		internal static ScriptObject Convert(ScriptObject script,Type returnType)
		{
			return new ScriptObject<Expression>(returnType,a => ScriptHelper.Convert(a, returnType), script);
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

		public Delegate Compile(bool force=false)
		{
			if(cache==null)
			{
				cache=lambda().Compile();
			}
			return cache;
		}
		public unsafe T getDelegate<T>(bool force=false) where T : Delegate
		{

			return  (T)lambda().Compile(force);
		}

		public static ScriptObject DoubleParam()
		{
			return new RootElementScript(typeof(double),Expression.Parameter(typeof(double)));
		}

		public Func<double,double> AsFx()
		{
			return (Func<double,double>)Compile();
		}
		public static ScriptObject ConstDouble(double d)
		{
			return new RootElementScript(typeof(double),Expression.Constant(d));
		}
		public static ScriptObject ConstObject(object? o)
		{
			return new RootElementScript(typeof(object),Expression.Constant(o));
		}
		public static ScriptObject ConstString(string s)
		{
			return new RootElementScript(typeof(string),Expression.Constant(s));
		}
	}



	
	internal class RootElementScript : ScriptObject
	{
		internal Expression expression;
		bool checkParameter = true;
		public RootElementScript(Type type, Expression expression):base(type) 
		{ this.expression = expression; }

		public virtual RootElementScript setCheck(bool check)
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
		internal AssignScript(Type type, VarScript a, ScriptObject b) : base(type, (e1, e2) => Expression.Assign(e1,e2), a, b)
		{

		}

		public VarScript GetVarScript()
		{
			return (VarScript)base.a;
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
	internal class VarScript : RootElementScript,ISetter
	{
		public VarScript(Type type, ParameterExpression expression) : base(type, expression)
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

	internal class ClassScript:ScriptObject
	{
		public Type ClassType {  get;private set; }

		public ClassScript(Type type) : base(typeof(void))
		{
			this.ClassType = type;
		}

		public override (E, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			return (Expression.Empty(), new());
		}

		public ScriptObject Call(string name,ScriptObject instance, ScriptObject[] params_, Type[] types=null)
		{
			var m=ClassType.GetMethod( name,types!=null?types:params_.GetTypes());
			ScriptObject[] s = new ScriptObject[params_.Length + 1];
			Array.Copy(params_, s, params_.Length);
			s[params_.Length]= instance;
			return new MultiScript(m.ReturnType, es =>
			{
				return Expression.Call(es[es.Length-1],m,es.Take(es.Length-1));
			}
			, s);
		}

		public ScriptObject Field(string name,ScriptObject instance)
		{
			var f = ClassType.GetField(name);
			if(f == null)
			{
				var p=ClassType.GetProperty(name);
				return new MultiScript(p.PropertyType, e => Expression.Property(e[0],p),instance);
			}
			return new MultiScript(f.FieldType, e => Expression.Field(e[0], f), instance);

		}
	}

	internal class MultiScript : ScriptObject
	{

		ScriptObject[] expressions;
		MutiExpMaker expMaker;

		internal MultiScript(Type type, MutiExpMaker expMaker,params ScriptObject[] expressions):base(type)
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

				if (expressions[i]==null)
				{
					es[i] = null;
					continue;
				}

				p=expressions[i].
				getExpression(force);
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
