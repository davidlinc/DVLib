using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ScanInfo = DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>;
using ScriptF = DVLib.LabDataHelper.Factory<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>;
using InfoList = System.Collections.Generic.List<DVLib.LabDataHelper.DVScript.ScriptInfo>;
using ScanList = System.Collections.Generic.List<DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>>;
using DVOSLib;
using static DVLib.LabDataHelper.DVScript.ScriptObject;
using E= System.Linq.Expressions.Expression;
using System.Reflection;
using DVLib.LabDataHelper.MathObjectSystem;
using System.Security.Cryptography;
using MathBase;
using static DVLib.LabDataHelper.DVScript.ScriptHelper;
using System.Runtime.CompilerServices;

namespace DVLib.LabDataHelper.DVScript
{	public class ParamsCountMismatchException:Exception
		{
		public ParamsCountMismatchException(string info=""):base(info) { }
		}

	public class ReturnLabel
	{
		public ReturnLabel(Type type)
		{
			Target=Expression.Label(type);
		}
		public LabelTarget Target {  get;private set; }

		public bool Used { get; private set; } = false;

		public void Use()
		{
			Used = true;
		}
	}

	public class TypeGroup
	{
		Type[] types;

		public TypeGroup(ICollection<Type> types)
		{
			this.types=types.ToArray();
		}
		public TypeGroup(params Type[] types)
		{
			this.types = types.ToArray();
		}
		public Type[] GetTypes()
		{
			return this.types.ToArray();
		}
		public override int GetHashCode()
		{
			HashCode hashCode = new HashCode();
			foreach(var type in types)
			{
				hashCode.Add(type);
			}
			return hashCode.ToHashCode();
		}

		public static implicit operator TypeGroup(Type type)
		{
			return new TypeGroup(type);
		}
		public override bool Equals(object? obj)
		{
			if (obj is TypeGroup)
			{
				var v = obj as TypeGroup;
				bool flag = true; 
				if(v.types.Length==types.Length)
				{
					for(int i = 0; i < v.types.Length; i++)
					{
						if (types[i]!=v.types[i])
						{
							flag = false;
						}
					}
				}
				return flag;
			}

			return base.Equals(obj);
		}
	}
	
	public class ScriptManager:ObjectManager<ScriptObject,ScriptInfo,ScriptManager>
	{
		public class TypeInfo
		{
			public string name;

			public TypeInfo(string name, Type type, E expression)
			{
				this.name = name;
				Type = type;
				Expression = expression;
			}

			public Type Type { get; internal set; }
			public Expression Expression { get; internal set; }
		}
		public class TypeDic: StringDictionary<TypeInfo>
		{
			Dictionary<Type,Expression> exps=new();

			public void registerType(string name, Type type,Expression expression)
			{
				base.Add(name, new TypeInfo(name,type,expression) );
				exps.Add(type, expression);
			}

			public Expression getDefaultExpression(Type t)
			{
				return  exps[t];
			}
		}
		internal static int  max = 8;
	
		TypeDic typeDic = new();
	    
		Stack<ReturnLabel> labels = new Stack<ReturnLabel>();
		Dictionary<string, Dictionary<TypeGroup, MethodInfo>> FuncOverride = new();


		static TypeInfo db = new TypeInfo("double", typeof(double), Expression.Constant(0.0));
		Stack<HashSet<ParameterExpression>> parameters = new();
		//public bool RegisterFunctionWhenRead = false;

		public void registerType(string token, Type type,Expression expression, ExpMaker<E> expMaker = null)
		{
			if(type!=typeof(void))
			{
				register(new ScriptInfo(token +'(', max, ConvertFunc(type, expMaker)).setTokenEndCount(1));

				register(new ScriptInfo("new" + token, max, New(type)));
			}
			else
			{
				register(new ScriptInfo(token+'(', max, Void()).setTokenEndCount(1));
			}
			
			typeDic.registerType(token,type,expression);
		}

		internal override ScriptObject GetObject(string text, ScanList infos, ScanResult result)
		{
			
			var v=base.GetObject(text, infos, result);
			return v;
		}

		public void registerFuncOverride(string Token,TypeGroup type,MethodInfo expMaker)
		{
			Dictionary<TypeGroup, MethodInfo> dic;
			if(!FuncOverride.TryGetValue(Token,out dic))
			{
             dic = new Dictionary<TypeGroup, MethodInfo>();
				FuncOverride.Add(Token, dic);
			}
			
			dic.TryAdd(type, expMaker);

		}

		public MethodInfo GetMethodOverride(string Token,TypeGroup type)
		{

			if(FuncOverride.TryGetValue(Token,out var dic))
			{
				if(dic.TryGetValue(type, out var method))
				{
					return method;
				}
			}
			return null;
		}
		public override void registerDefault()
		{
			register(new ScriptInfo("+",max-4,NaiveMath("op_Addition",Expression.Add)));
			register(new ScriptInfo("-", max - 3, Subtract()));
			register(new ScriptInfo("*", max - 2, NaiveMath("op_Multiply", Expression.Multiply)));

			register(new ScriptInfo(">", max - 4, NaiveMath("op_GreaterThan", Expression.GreaterThan,(a,b)=>typeof(bool))));
			register(new ScriptInfo("<", max - 4, NaiveMath("op_LessThan", Expression.LessThan, (a, b) => typeof(bool))));
			register(new ScriptInfo(">=", max - 4, NaiveMath("op_GreaterThanOrEqual", Expression.GreaterThanOrEqual, (a, b) => typeof(bool))));
            register(new ScriptInfo("<=", max - 4, NaiveMath("op_LessThanOrEqual", Expression.LessThanOrEqual, (a, b) => typeof(bool))));
			register(new ScriptInfo("==", max - 4, NaiveMath("op_Equality", Expression.Equal, (a, b) => typeof(bool))));


			register(new ScriptInfo("/", max - 2, NaiveMath("op_Division",Expression.Divide)));
			register(new ScriptInfo("%", max - 2, NaiveMath("op_Modulus",Expression.Modulo)));
			register(new ScriptInfo("^", max - 1, LR<double, double, double>((a, b) => Math.Pow(a,b),true)));
			register(new ScriptInfo("*-", max - 2, NaiveMath("op_Multiply", Negate(Expression.Multiply))));
			register(new ScriptInfo("/-", max - 2, NaiveMath("op_Division",Negate(Expression.Divide))));
			register(new ScriptInfo("%-", max - 2, NaiveMath("op_Modulus",Negate(Expression.Modulo))));
			register(new ScriptInfo("^-", max - 1, LR<double, double, double>((a, b) => Math.Pow(a, -b), true)));
			register(new ScriptInfo("sin", max , Func<double,double>((a) => Math.Sin(a), true)));
			register(new ScriptInfo("cos", max, Func<double, double>((a) => Math.Cos(a), true)));
			register(new ScriptInfo("tan", max, Func<double, double>((a) => Math.Tan(a), true)));
			register(new ScriptInfo("atan2", max, Func<double, double, double>((a,b) => Math.Atan2(a,b), true)));
			register(new ScriptInfo("asin", max, Func<double, double>((a) => Math.Asin(a), true)));
			register(new ScriptInfo("acos", max, Func<double, double>((a) => Math.Acos(a), true)));
			register(new ScriptInfo("atan", max, Func<double, double>((a) => Math.Atan(a), true)));
			register(new ScriptInfo("sqrt", max, Func<double, double>((a) => Math.Sqrt(a), true)));
			register(new ScriptInfo("pow", max, Func<double,double, double>((a,b) => Math.Pow(a,b), true)));
			register(new ScriptInfo("ln", max, Func<double, double>((a) => Math.Log(a), true)));
			register(new ScriptInfo("exp", max, Func<double, double>((a) => Math.Exp(a), true)));
			register(new ScriptInfo("printf", max, Func<object,string >((a) => {string s= a.ToString();DVOS.writeLine(s) ; return s; },true)));
			register(new ScriptInfo("x", max, ParamX()));
			register(new ScriptInfo("y", max, ParamY()));
			register(new ScriptInfo("z", max , ParamZ()));
			register(new ScriptInfo("=", 1, EQ()).setReverse());
			register(new ScriptInfo("{",max+1,Block()));
			register(new ScriptInfo("return",2, Return()));
			register(new ScriptInfo(".", max, Call()).setContition((s, i) => i + 1 < s.Length && !s[i+1].isNumber()));

			register(new ScriptInfo("true", max, Const<bool>(true)));
			register(new ScriptInfo("false", max, Const<bool>(false)));

			register(new ScriptInfo("Math", max,ClasS( typeof(Math))));

			registerType("double",typeof(double),Expression.Constant(0.0));
			registerType("float",typeof(float), Expression.Constant(0f));
			registerType("int",typeof(int), Expression.Constant(0));
			registerType("long",typeof(long), Expression.Constant((long)0));
			registerType("short",typeof(short), Expression.Constant((short)0));
			registerType("bool",typeof(bool), Expression.Constant(false));
			registerType("string", typeof(string), Expression.Constant(""));
			registerType("sbyte", typeof(sbyte), Expression.Constant((sbyte)0));
			registerType("uint", typeof(uint), Expression.Constant((uint)0));
			registerType("ushort", typeof(ushort), Expression.Constant((ushort)0));
			registerType("ulong", typeof(ulong), Expression.Constant((ulong)0));
			registerType("char", typeof(char), Expression.Constant((char)0));
			registerType("byte", typeof(byte), Expression.Constant((byte)0));
			registerType("object", typeof(object), Expression.Constant(null));

			registerType("Vector3", typeof(Vector3), Expression.Constant(null));

			registerType("void", typeof(void),null);
			register(new ScriptInfo("string", max, Func<object, string>(o => o.ToString(),true)));
			registerFuncOverride("+", typeof(string), typeof(string).GetMethod("Concat", new[] {typeof(string),typeof(string) } ));

			addTokenIgnore((s, i) => s[i] == '"', (s, i) => s[i] == '"');
			addTokenIgnore((s, i) => s[i] == '.' && i + 1 < s.Length && !s[i + 1].isNumber(), (s, i) => s[i] == '@' || s[i] == '(');
		}


		internal override void pushStack()
		{
			base.pushStack();
			parameters.Push(new HashSet<ParameterExpression>());
		}
		public HashSet<ParameterExpression> peakParam()
		{
			return parameters.Peek();
		}
		internal override bool popStack(out Dictionary<char, HeadCharSet<ScriptObject, ScriptInfo, ScriptManager>> result)
		{
			parameters.TryPop(out var p);
			return base.popStack(out result);
		}
		public override void registerLG(LevelGetter getter)
		{

			getter.register('(', 1);
			getter.register(')', -1);
			getter.register('{', 1);
			getter.register('}', -1);
			getter.register('[', 1);
			getter.register(']', -1);
			/*
			toReplace.Add("}{", ("}{", "};{"));
			toReplace.Add(";}", (";}", ";}"));
			toReplace.Add("}", ("}", ";}"));*/
		}
		public ScriptObject Read(string text)
		{
			resetDepth();
			pushStack();
			Helper.clean(ref text);
			List<ScanInfo> info = new();
			var r = ScanForOperators(ref text, info);

			var go = GetObject(text, info, r);
			popStack(out var v);
			resetDepth();
			return go;
		}

	
		public void pushLabel(Type t)
		{
			labels.Push(new ReturnLabel(t));
		}
		public bool popLabel(out ReturnLabel label)
		{
		return	labels.TryPop(out label);
		}

		public bool peakLabel(out ReturnLabel label)
		{
			return labels.TryPeek(out label);
		}

		 internal (string name,TypeInfo t)getName(string name) 
			{
			bool match = typeDic.match(name, out TypeInfo type,out string key);
		
            if (match)
            {	
				return (name.Replace(key, ""),type);
            }
			return (name,db);
        }
		internal (string name, TypeInfo t) getName_func(string name)
		{
			bool match = typeDic.match(name, out TypeInfo type, out string key);
			if (match)
			{
				return (name.Substring(type.name.Length), type);
			}

			return (name, null);
		}





		public Expression getDefault(Type t)
		{

			return typeDic.getDefaultExpression(t);
			
			return Expression.Constant(0.0);
		}


		public override ScriptObject getBaseType(string s)
		{
			if (s.Length==0)
			{
				return new RootElementScript(typeof(void), Expression.Empty());
			}
			try
			{
				Expression c=null;
				if(withPoint(s) )
				{
                double d=double.Parse(s);
			    c = Expression.Constant(d);
				}
				else if(s.EndsWith('d') || s.EndsWith("D"))
				{
					double f = float.Parse(s.Substring(0, s.Length - 1));
					c = Expression.Constant(f);
				}
				else if(s.EndsWith('f')||s.EndsWith("F"))
				{
				float f=float.Parse(s.Substring(0,s.Length-1));
			    c = Expression.Constant(f);
				}
				else
				{
				int i=int.Parse(s);
				c = Expression.Constant(i);
				}

				if(c!=null)
				return new RootElementScript(c.Type,c);
			}
			catch
			{
			}

			if(s.StartsWith('"')&&s.EndsWith('"'))
			{
				s = s.TrimStart('"');
				s = s.TrimEnd('"');
			}

			return ScriptObject.ConstString(s);

		}


		internal override void onCreated(ScriptObject obj, ScriptInfo info)
		{
			obj.setScriptInfo(info);
		}


	}
}
