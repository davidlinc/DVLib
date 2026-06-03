using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ScanInfo = DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>;
using ScriptF = DVLib.LabDataHelper.Factory<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>;
using InfoList = System.Collections.Generic.List<DVLib.LabDataHelper.DVScript.ScriptInfo>;
using ScanList = DVLib.LabDataHelper.SList<DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.DVScript.ScriptObject, DVLib.LabDataHelper.DVScript.ScriptInfo, DVLib.LabDataHelper.DVScript.ScriptManager>>;
using DVOSLib;
using static DVLib.LabDataHelper.DVScript.ScriptObject;
using E= System.Linq.Expressions.Expression;
using System.Reflection;
using DVLib.LabDataHelper.MathObjectSystem;
using System.Security.Cryptography;
using MathBase;
using static DVLib.LabDataHelper.DVScript.ScriptHelper;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Xml.Linq;
using System.ComponentModel;

namespace DVLib.LabDataHelper.DVScript
{	public class ParamsCountMismatchException:Exception
		{
		public ParamsCountMismatchException(string info=""):base(info) { }
		}
	public class LoopLabel

	{
		public LabelTarget Break { get; private set; }
		public LabelTarget Continue { get; private set; }

		public LoopLabel()
		{
			Break = Expression.Label();
			Continue = Expression.Label();
		}
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
			public string Name;

			public TypeInfo(string name, Type type)
			{
				this.Name = name;
				Type = type;
			}

			public Type Type { get; internal set; }
		}
		public class TypeDic: StringDictionary<TypeInfo>
		{

			public void registerType(string name, Type type)
			{
				base.Add(name, new TypeInfo(name,type) );
			}

		}
		public  static readonly int  max = 8;
	
		TypeDic typeDic = new();
	    
		Stack<ReturnLabel> labels = new Stack<ReturnLabel>();
		Stack<LoopLabel> loopLabels = new Stack<LoopLabel>();
		Dictionary<string, Dictionary<TypeGroup, MethodInfo>> FuncOverride = new();


		static TypeInfo db = new TypeInfo("double", typeof(double));
		Stack<HashSet<ParameterExpression>> parameters = new();
		//public bool RegisterFunctionWhenRead = false;
		
			public void registerType(string token, Type type, ExpMaker<E> expMaker = null)
		{
			if(type!=typeof(void))
			{
				register(new ScriptInfo(token, max, null).setNoInstance());
				register(new ScriptInfo(token + "[]", max, null).setNoInstance());
				register(new ScriptInfo(token + "[][]", max, null).setNoInstance());
				register(new ScriptInfo(token + "[][][]", max, null).setNoInstance());
				register(new ScriptInfo(token + "[][][][]", max, null).setNoInstance());
				register(new ScriptInfo(token + "[][][][][]", max, null).setNoInstance());
				register(new ScriptInfo(token +'(', max, ConvertFunc(type, expMaker)).setTokenEndCount(1));

				register(new ScriptInfo("new" + token, max, New(type)));
				//register(new ScriptInfo("newList<" + token+">", max, NewG(typeof(List<>),type)));
				register(new ScriptInfo("new" + token+"[", max, NewArray(type)).setTokenEndCount(1));
				register(new ScriptInfo("new" + token + "[][", max, NewArray(type.MakeArrayType())).setTokenEndCount(1));
				register(new ScriptInfo("new" + token + "[][][", max, NewArray(type.MakeArrayType().MakeArrayType())).setTokenEndCount(1));
				register(new ScriptInfo("new" + token + "[][][][", max, NewArray(type.MakeArrayType().MakeArrayType().MakeArrayType())).setTokenEndCount(1));
				register(new ScriptInfo("new" + token + "[][][][][", max, NewArray(type.MakeArrayType().MakeArrayType().MakeArrayType().MakeArrayType())).setTokenEndCount(1));
			}
			else
			{
				register(new ScriptInfo(token+'(', max, Void()).setTokenEndCount(1));
			}
			
			typeDic.registerType(token,type);
		}
		public void registerGeneric(string token, Type type, object default_ = null, ExpMaker<E> expMaker = null)
		{
			register(new ScriptInfo(token+"<", max, null).setTokenEndCount(1).setNoInstance());
			register(new ScriptInfo("new" + token + "<", max, NewGeneric(type,"new"))
				.setTokenEndCount(1+token.Length)
				);

			typeDic.registerType(token,type);
		}


		internal override ScriptObject GetObject(string text, ScanList infos, ScanResult result)
		{
			//DVOS.writeLine("go:" + text);
			//foreach (var vv in infos)
			//{
			//DVOS.writeLine(vv.Mark + ":" + vv.Position);
			//}
			//DVOS.writeLine("go/");
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
			register(new ScriptInfo("++",max,IncrementAssign()));
			register(new ScriptInfo("--", max,DecrementAssign()));
			register(new ScriptInfo("-", max - 3, Subtract()));
			register(new ScriptInfo("*", max - 2, NaiveMath("op_Multiply", Expression.Multiply)));

			register(new ScriptInfo(">", max - 4, NaiveMath("op_GreaterThan", Expression.GreaterThan,(a,b)=>typeof(bool)))
				.addOveriide((sp, i, l, s) =>
				{

					if(s.TryPeek(out var wn))
					{
						if (wn.Token == "<")
							return true;
					}
					return false;

				},new ScriptInfo(">",max,null).setNoInstance().setLevelChange(-1))
				);

			register(new ScriptInfo("<", max - 4, NaiveMath("op_LessThan", Expression.LessThan, (a, b) => typeof(bool)))
				.addOveriide((sp, i, l, s) => { if (l.Count > 0) {
						var v = l.Last();
						if(v.operatorInfo.TokenEndCount==1&&v.operatorInfo.mark.EndsWith('<'))
						{
							return true;
						}
					} return false; },new ScriptInfo("<", max, null).setLevelChange(1).setNoInstance().nextPart((a, b) =>
				{
					return b.Token[0] == '>' && b.Level == a.Level + 1;
				}))
				
				);
			register(new ScriptInfo(">[", max,null).setTokenEndCount(1).setLevelChange(-1)); 
			register(new ScriptInfo(">[][", max, null).setTokenEndCount(1).setLevelChange(-1));
			register(new ScriptInfo(">[][][", max, null).setTokenEndCount(1).setLevelChange(-1));
			register(new ScriptInfo(">[][][][", max, null).setTokenEndCount(1).setLevelChange(-1));
			register(new ScriptInfo(">[][][][][", max, null).setTokenEndCount(1).setLevelChange(-1));
			register(new ScriptInfo(">=", max - 4, NaiveMath("op_GreaterThanOrEqual", Expression.GreaterThanOrEqual, (a, b) => typeof(bool))));
            register(new ScriptInfo("<=", max - 4, NaiveMath("op_LessThanOrEqual", Expression.LessThanOrEqual, (a, b) => typeof(bool))));
			register(new ScriptInfo("==", max - 4, NaiveMath("op_Equality", Expression.Equal, (a, b) => typeof(bool))));

			
			register(new ScriptInfo("/", max - 2, NaiveMath("op_Division",Expression.Divide)));
			register(new ScriptInfo("%", max - 2, NaiveMath("op_Modulus",Expression.Modulo)));
			register(new ScriptInfo("^", max - 1, LR<double, double, double>(Math.Pow,true)));
			register(new ScriptInfo("*-", max - 2, NaiveMath("op_Multiply", Negate(Expression.Multiply))));
			register(new ScriptInfo("/-", max - 2, NaiveMath("op_Division",Negate(Expression.Divide))));
			register(new ScriptInfo("%-", max - 2, NaiveMath("op_Modulus",Negate(Expression.Modulo))));
			register(new ScriptInfo("^-", max - 1, LR<double, double, double>((a, b) => Math.Pow(a, -b), true)));
			register(new ScriptInfo("sin", max , Func<double,double>(Math.Sin, true)));
			register(new ScriptInfo("cos", max, Func<double, double>(Math.Cos, true)));
			register(new ScriptInfo("tan", max, Func<double, double>( Math.Tan, true)));
			register(new ScriptInfo("atan2", max, Func<double, double, double>(Math.Atan2, true)));
			register(new ScriptInfo("asin", max, Func<double, double>(Math.Asin, true)));
			register(new ScriptInfo("acos", max, Func<double, double>(Math.Acos, true)));
			register(new ScriptInfo("atan", max, Func<double, double>(Math.Atan, true)));
			register(new ScriptInfo("sqrt", max, Func<double, double>(Math.Sqrt, true)));
			register(new ScriptInfo("pow", max, Func<double,double, double>(Math.Pow, true)));
			register(new ScriptInfo("ln", max, Func<double, double>( Math.Log, true)));
			register(new ScriptInfo("exp", max, Func<double, double>( Math.Exp, true)));
			register(new ScriptInfo("printf", max, Func<object,string>(WriteLine,true)));
			register(new ScriptInfo("x", max, ParamX()));
			register(new ScriptInfo("y", max, ParamY()));
			register(new ScriptInfo("z", max , ParamZ()));
			register(new ScriptInfo("=", max-5, EQ()).setReverse());

			register(new ScriptInfo("{", max + 1, Block()).nextPart((t, n) =>
			{

				return n.Token == "}" && n.Level == t.Level + 1;

			}));
			//No instance token
			register(new ScriptInfo("(",0,null).nextPart((t, n) =>
			{
				return n.Token == ")" && n.Level == t.Level + 1;
			}).setNoInstance());

			register(new ScriptInfo("[", max-1, Index()).nextPart((t, n) =>
			{
				return n.Token == "]" && n.Level == t.Level + 1;
			})
			.addOveriide((s, i, l, st) =>{

				if(l.Count>0)
				{
					var v=l.Last();

					if(v.operatorInfo.TokenEndCount==1&&v.operatorInfo.mark.EndsWith('['))
					return true;
				}
				return false;

			},new ScriptInfo("[",max,null).setNoInstance().nextPart((t, n) =>
			{
				return n.Token == "]" && n.Level == t.Level + 1;
			}))
	
			);

			register(new ScriptInfo(")", 0, null).setNoInstance());
			register(new ScriptInfo("}", 0, null).setNoInstance());
			register(new ScriptInfo("]", 0, null).setNoInstance());
			register(new ScriptInfo(";", 0, null).setNoInstance());
			register(new ScriptInfo(",", 0, null).setNoInstance());

			register(new ScriptInfo("return",max-5, Return()));
			register(new ScriptInfo(".", max-1, Call()).setCondition((s, i,l, st) => i + 1 < s.Length && !s[i+1].isNumber()));

			register(new ScriptInfo("true", max, Const<bool>(true)));
			register(new ScriptInfo("false", max, Const<bool>(false)));

			register(new ScriptInfo("Math", max,ClasS( typeof(Math))));

			register(new ScriptInfo("if", max-6, If()).setReverse());
			register(new ScriptInfo("else",0, null).setNoInstance());
			int a;
			//while (true) if (true) a = 0; else a = 1;

			register(new ScriptInfo("break", max,Break()));
			register(new ScriptInfo("continue", max,Continue()));
			register(new ScriptInfo("while", max - 6, While()).setReverse());
			register(new ScriptInfo("for", max - 6, For()).setReverse());

			registerType("double",typeof(double));
			registerType("float",typeof(float));
			registerType("int",typeof(int));
			registerType("long",typeof(long));
			registerType("short",typeof(short));
			registerType("bool",typeof(bool));
			registerType("string", typeof(string));
			registerType("sbyte", typeof(sbyte));
			registerType("uint", typeof(uint));
			registerType("ushort", typeof(ushort));
			registerType("ulong", typeof(ulong));
			registerType("char", typeof(char));
			registerType("byte", typeof(byte));
			registerType("object", typeof(object));

			registerGeneric("List", typeof(List<>));
			registerGeneric("Dictionary", typeof(Dictionary<,>));
			registerGeneric("HashSet", typeof(HashSet<>));
			registerGeneric("Stack", typeof(Stack<>));
			registerGeneric("Queue", typeof(Queue<>));

			registerType("Vector3", typeof(Vector3));

			registerType("void", typeof(void));

			register(new ScriptInfo("string", max, Func<object, string>(o => o.ToString(),true)));

			registerFuncOverride("+", typeof(string), typeof(string).GetMethod("Concat", new[] {typeof(string),typeof(string) } ));

			addTokenIgnore((s, i) => s[i] == '"', (s, i) => s[i] == '"');
			addTokenIgnore((s, i) => s[i] == '.' && i + 1 < s.Length && !s[i + 1].isNumber(), (s, i) => s[i] == '@' || s[i] == '(');
		}

		static string WriteLine(object b)
		{
			string s=b.ToString();
			DVOS.writeLine(s);
			return s;
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

		int getArrayLevel(string s)
		{
			ReadOnlySpan<char> sSpan = s;
			int end = -1;
			for (int i = sSpan.Length-1;i>=0;i--)
			{
				if(sSpan[i] == ']')
				{
					end = i; break;
				}
			}
			if(end < 0)
			{
				return 0;
			}
			sSpan=sSpan.Slice(0,end+1);
			int l = 0;
			while (sSpan.Length > 2 && sSpan.EndsWith("[]")) {
				sSpan = sSpan.Slice(0, sSpan.Length - 2);
				l++;
			}
			return l;
		}
		public (Type type,string typeName) getType(string text,SList<ScanInfo> scans)
		{

			int arrayLevel=getArrayLevel(text);
			if (scans.Count>0)
			{
				var info=scans[0];
		if(typeDic.match(info.Token, out var typeInfo, out var key))
			{
				if(typeInfo.Type.IsGenericType)
				{
					var s0 = scans[info.Index + 1];
					var s1 = scans[s0.Index+s0.NextOffset];

					var context = scans.getContext(text,s0, s1);

					var v = context.list.Split(context.Item1, ",");
					(Type type, string typeName)[]types=new (Type type, string typeNmae)[v.Length];
					for(int i = 0;i<types.Length;i++)
					{
						types[i] = getType(v[i].Item1,  v[i].list);
					}

						Type t = typeInfo.Type.MakeGenericType((from (Type type, string typeName) a in types select a.type).ToArray());
						string name = text.Substring(info.Position, s1.Position + 1 - info.Position);
						for(int i = 0;i<arrayLevel;i++)
						{
							t=t.MakeArrayType();
							name += "[]";
						}
					return( t,name);

				}
				else
				{
						Type type=typeInfo.Type;
						string typeName = typeInfo.Name;
						for (int i = 0; i < arrayLevel; i++)
						{
							type = type.MakeArrayType();
							typeName += "[]";
						}
						return (type,typeName);
				}
			}
			}
	
			return (null,null);

		}
		public override void registerLG(LevelGetter getter)
		{

			getter.register('(', 1);
			getter.register(')', -1);
			getter.register('{', 1);
			getter.register('}', -1);
			getter.register('[', 1);
			getter.register(']', -1);

			addReplacePair(("}", (s, i) =>
			{
			

				if (s.Length > i + 1 && s[i+1]!=';'&&!(s.Length>i+4&&s.Slice(i+1,4).SequenceEqual("else")))
				{	
					return "};";
				}
				return "}";
			}
			));
			addReplacePair((";", (s, i) =>
			{


				if (s.Length>i+4&&s.Slice(i+1,4).SequenceEqual("else"))
				{
					return "";
				}
				return ";";
			}
			));
			/*
			addReplacePair(("}{", "};{"));
			addReplacePair((";}", ";}"));
			addReplacePair( ("}", ";}"));
			*/
		}
		public ScriptObject Read(string text)
		{
			resetDepth();
			pushStack();
			ScriptObject So=null;
			Exception e = null; 
			try
			{

	        Helper.clean(ref text);
			SList<ScanInfo> info = new();
			var r = ScanForOperators(ref text, info);
			
				So = GetObject(text, info, r);
			}
			catch(Exception e1)
			{
				e = e1;
			}

		
			popStack(out var v);
			resetDepth();

			if(e!=null)
			{
				throw (e);
			}

			return So;
		}
		public ScriptObject ReadTopLevel(string text)
		{
			return Read("{" + text + "void();}");
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
		public void pushLoopLabel()
		{
			loopLabels.Push(new LoopLabel());
		}
		public bool popLoopLabel(out LoopLabel label)
		{
			return loopLabels.TryPop(out label);
		}

		public bool peakLoopLabel(out LoopLabel label)
		{
			return loopLabels.TryPeek(out label);
		}

		internal (string name,TypeInfo t)getTypeAndName(string name) 
			{

			SList<ScanInfo> info = new();
			ScanForOperators(ref name, info);
			
			var v = getType(name, info);
			if (v.type != null) 
			{
				
					return (name.Substring(v.typeName.Length), new TypeInfo(v.typeName, v.type));
	
			}

			bool match = typeDic.match(name, out TypeInfo type,out string key);
		
            if (match)
            {	
				return (name.Substring(key.Length),type);
            }
			return (name,db);
        }
		internal (string name, TypeInfo t) getName_func(string name)
		{
			SList<ScanInfo> info = new();
			ScanForOperators(ref name, info);

			var v = getType(name, info);
			if (v.type != null)
			{
				
					return (name.Substring(v.typeName.Length), new TypeInfo(v.typeName, v.type));

				
			}

			bool match = typeDic.match(name, out TypeInfo type, out string key);

			if (match)
			{
				return (name.Substring(key.Length), type);
			}
			return (name, null);
		}





		public Expression getDefault(Type t)
		{
			return Expression.Default(t);
			
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
