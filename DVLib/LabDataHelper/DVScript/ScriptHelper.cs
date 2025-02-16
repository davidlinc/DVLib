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
using static DVLib.LabDataHelper.DVScript.ScriptManager;
using E = System.Linq.Expressions.Expression;
using System.Reflection;
using DVLib.LabDataHelper.MathObjectSystem;
using System.Security.Cryptography;
using MathBase;
using System.Data;
using System.Xml.Linq;
namespace DVLib.LabDataHelper.DVScript
{
	internal static class ScriptHelper
	{
		internal static ScriptObject x = ScriptObject.DoubleParam();
		internal static ScriptF ParamX()
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{
					return x;
				};
		}
		internal static ScriptObject y = ScriptObject.DoubleParam();
		internal static ScriptF ParamY()
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{
					return y;
				};
		}
		internal static ScriptObject z = ScriptObject.DoubleParam();
		internal static ScriptF ParamZ()
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{
					return z;
				};
		}
		internal static ScriptF Const<T>( T t)
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{
					return new RootElementScript(typeof(T),Expression.Constant(t));
				};
		}
		internal static ExpMaker<E, E> Negate(ExpMaker<E, E> maker)
		{
			return (A, B) => maker(A, Expression.Negate(B));
		}

		internal static ScriptInfo param(string name, Type type, out ParameterExpression e,string tag= "tempParam",bool check=true)
		{
			e = Expression.Parameter(type,name);
			ParameterExpression ee = e;
			return new ScriptInfo(name, max,

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					return new VarScript(type, ee).setCheck(check);
				}

				, tag);
		}
		internal static Dictionary<Type, int> typePriority = new Dictionary<Type, int>() { { typeof(byte), 0 }, { typeof(sbyte), 1 }, { typeof(short), 2 }, { typeof(ushort), 3 }, { typeof(int), 4 }, { typeof(uint), 5 }, { typeof(long), 6 }, { typeof(ulong), 7 }, { typeof(float), 8 }, { typeof(double), 9 }, { typeof(string), 10 } };

		internal static Type getNewType(Type a, Type b)
		{
			int t = -1, t2 = -1;
			bool t_=typePriority.TryGetValue(a, out t);
			bool t2_=typePriority.TryGetValue(b, out t2);
			if(!(t_&&t2_))
			{
				return null;
			}
			if (t >= t2 && t >= 0)
			{
				return a;
			}
			else
			{
				return b;
			}


		}

		static MethodInfo StringConcat = typeof(string).GetMethod("Concat",new[] { typeof(string), typeof(string) });

		static Expression StringAdd(Expression a, Expression b)
		{
			return Expression.Add(a, b,StringConcat);
		}
		internal static ScriptF NaiveMath(string name,ExpMaker<E, E> expMaker,Func<Type,Type,Type> dfType=null)
		{
			if(dfType==null)
			{
				dfType = (a, b) => a;
			}
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{
					var s = ScriptInfo.solveLR(text, ois, infos, manager);
					var A = manager.GetObject(s[0].name, s[0].infos, r);
					var B = manager.GetObject(s[1].name, s[1].infos, r);
					var Atype = A.returnType;
					var Btype = B.returnType;
					bool cast = false;
					Type rt = null;
					if (Atype != Btype)
					{
						var nt=getNewType(Atype, Btype);
						if (nt != null)
						{
							Atype = Btype = nt;
							cast = true;
						}
				
					}

					var m = Atype.GetMethod(name, new[] { Atype, Btype });
					if (m == null)
					{
						m = Btype.GetMethod(name, new[] { Atype, Btype });
					}
					if(m!=null)
					{
					rt = m.ReturnType;		
					}
					else
					{
						rt = dfType(Atype, Btype);
					}

					var v = manager.GetMethodOverride(ois.operatorInfo.Token, Atype);

					return new ScriptObject<E, E>( rt, (a, b) => { if (cast) { a = ScriptHelper.Convert(a, Atype); b = ScriptHelper.Convert(b, Btype); } 
						if(v!=null)
						{
							return Expression.Call(null,v, a, b);
						}
						return expMaker(a, b); }, A, B);
				};
		}


		public static Expression Convert(E e,Type t)
		{
			if(t==typeof(string))
			{
				return Expression.Call(e, e.Type.GetMethod("ToString",new Type[0]));
			}

			return Expression.Convert(e, t);
		}
		internal static ScriptF Subtract()
		{
			return
(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
{
if (ois.position == 0)
{
	var s = ScriptInfo.solveR(text, ois, infos, manager);
	var A = manager.GetObject(s.name, s.infos, r);
	return new ScriptObject<E>(A.returnType, (a) => { return Expression.Negate(a); }, A);
}
else
{

	var s = ScriptInfo.solveLR(text, ois, infos, manager);
	var A = manager.GetObject(s[0].name, s[0].infos, r);
	var B = manager.GetObject(s[1].name, s[1].infos, r);
	var re = A.returnType;
		var Atype=A.returnType;
		var Btype=B.returnType;
	bool cast = false;
	if (A.returnType != B.returnType)
	{
		var nt=getNewType(A.returnType, B.returnType);
			if(nt!=null)
			{
				re = nt;
				cast = true;
			}
		}

		var m = Atype.GetMethod("op_Subtraction", new[] { Atype, Btype });
		if (m == null)
		{
			m = Btype.GetMethod("op_Subtraction", new[] { Atype, Btype });
		}
		if(m!=null)
		{

		re = m.ReturnType;
		}
		else
		{
			re = Atype;
		}

		return new ScriptObject<E, E>(re, (a, b) => { if (cast) { a = ScriptHelper.Convert(a, re); b = ScriptHelper.Convert(b, re); } return Expression.Subtract(a, b); }, A, B);

}
};


		}

	
		internal static ScriptF Void()
		{
			return

			(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
			{

				return new RootElementScript(typeof(void), Expression.Empty());

				throw new ParamsCountMismatchException();
			};
		}

	
		internal static ScriptF ConvertFunc( Type type, ExpMaker<E> exp = null)
		{
			return

			(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
			{

				var s = ScriptInfo.solveFunc(text, ois, infos, manager);
				if (s.Length > 0)
				{
					var v = manager.GetObject(s[0].name, s[0].infos, r);
					return new ScriptObject<E>(type, (a) => { if (exp == null) return ScriptHelper.Convert(a, type); return exp(a); }, v);

				}

				throw new ParamsCountMismatchException();
			};
		}
		internal static ScriptObject Cast(this ScriptObject script, Type type)
		{
			return new ScriptObject<E>(type, e => ScriptHelper.Convert(e, type), script);
		}
		internal static ScriptF LR<L, R, O>(Func<L, R, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveLR(text, ois, infos, manager);
					Expression<Func<L, R, O>> f = (a, b) => func(a, b);
					var A = manager.GetObject(s[0].name, s[0].infos, r);
					var B = manager.GetObject(s[1].name, s[1].infos, r);

					if(cast&&A.returnType!=typeof(L))
					{
						A=Cast(A,typeof(L));
					}
					if (cast && B.returnType != typeof(R))
					{
						B = Cast(B, typeof(R));
					}
					return new ScriptObject<E, E>(typeof(O), (a, b) => {   return Expression.Invoke(f, a, b); }, A, B);
				};
		}

		internal static (string name, List<string> params_) getParams(ReadOnlySpan<char> funcname)
		{
			List<string> Params = new List<string>();
			int plpos = -1;
			int lastPos = -1;
			string name = "";
			for (int i = 0; i < funcname.Length; i++)
			{
				if (plpos < 0)
				{
					if (funcname[i] == '(')
					{
						plpos = i;
						lastPos = i + 1;
						name = funcname.Slice(0, i).ToString();
					}
				}
				else
				{
					if (funcname[i] == ',' && i - lastPos > 0)
					{
						Params.Add(funcname.Slice(lastPos, i - lastPos).ToString());
						lastPos = i + 1;
					}
					else if (funcname[i] == ')' && i - lastPos > 0)
					{
						Params.Add(funcname.Slice(lastPos, i - lastPos).ToString());
						break;
					}
				}

			}
			return (name, Params);
		}

		public static Type[] GetTypes(this ScriptObject[] scripts  )
		{
			Type[] types = new Type[scripts.Length];
			for(int i = 0;i < scripts.Length;i++)
			{
				types[i] = scripts[i].returnType;
			}
			return types;
		}
		internal static ScriptF Return()
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{

						var s = ScriptInfo.solveR(text, ois, infos, manager);

						bool hasLabel=manager.peakLabel(out var label);
						if(s.name.Length==0)
						{
							return new ReturnScript(typeof(void), label.Target, null); 
						}
						//Expression<Func<I, O>> f = a => func(a);
						var A = manager.GetObject(s.name, s.infos, r);

					

						return new ReturnScript(A.returnType, label.Target, A);

						throw new ParamsCountMismatchException();
					};
		}
		internal static ScriptF ClasS(Type t)
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{

						return new ClassScript(t);
					};
		}

		internal static ScriptF New(Type t)
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{
						var v=ScriptInfo.solveFunc(text, ois, infos, manager);
						ScriptObject[] scriptObjects=new ScriptObject[v.Length];

						for(int i=0; i<v.Length;i++)
						{
							scriptObjects[i] = manager.GetObject(v[i].name, v[i].infos, r);
						}

						var c = t.GetConstructor(scriptObjects.GetTypes());

						var p = c.GetParameters();
						for(int i=0; i<scriptObjects.Length;i++)
						{
							if (scriptObjects[i].returnType!=p[i].ParameterType)
							{
								Type t = p[i].ParameterType;
								scriptObjects[i] = new ScriptObject<E>(t, e => Convert(e, t), scriptObjects[i]);
							}
						}
						return new MultiScript(t, es =>
						Expression.New( c, es)
						, scriptObjects);
					};
		}

		public static List<ScanInfo<T, I, M>> Slice<T,I,M>(this List<ScanInfo<T, I, M>> list,int index,int length=-1,int levelChange=0,bool inplace=true) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			List<ScanInfo<T, I, M>> result = new();
			int start = index;
			int end=index+length;
			bool toEnd = length < 0;
			ScanInfo<T, I, M> info;
			foreach (var s in list) {

				if(s.position>=start&&(toEnd||( s.position<end)))
				{
					info = inplace ? s : s.getCopy();
					result.Add( info);
					info.position -= index;
					info.level += levelChange;
				}
			}
			return result;
		}
		public static List<ScanInfo<T, I, M>> Slice<T, I, M>(this  List<ScanInfo<T, I, M>> list, IList< (int index, int length)> pos , int levelChange = 0, bool inplace = true) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			List<ScanInfo<T, I, M>> result = new();
			int order = 0;
			var pair = pos[0];

			int start = pair.index;
			int end = pair.index + pair.length;
			bool toEnd = pair.length < 0;

			ScanInfo<T, I, M> info;
			foreach (var s in list)
			{
				if(s.position>=end)
				{	order++;
					pair = pos[order];
					start = pair.index;
				    end = pair.index + pair.length;
					toEnd = pair.length < 0;
				}
				if (s.position >= start && (toEnd || (s.position < end)))
				{
					info = inplace ? s : s.getCopy();
					result.Add(info);
					info.position -= start;
					info.level += levelChange;
				}
			}
			return result;
		}
		static (string instance,List<ScanInfo> IInfo,string name, (string name,List<ScanInfo> infos)[]Params)?slove_call_(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager)
		{
			int dotPos = ois.position;
			int endPos=-1;
			char endToken='0';
			char c;
			for (int i = dotPos+1; i < text.Length; i++) {
			
				c= text[i];
				if(endPos==-1 )
				{
					if (c == '@' || c == '(')
					{
						endPos= i;
						endToken = c;

						if(c=='@')
						{
							return (text.Substring(0, dotPos),infos.Slice(0,dotPos),text.Substring(dotPos+1,endPos-dotPos-1),null);
						}
					}
				}
			}

			if (endToken == '(')
			{
				var v=ScriptInfo.solveFunc(text.Substring(endPos),null,infos.Slice(endPos),manager);
				return (text.Substring(0, dotPos),infos.Slice(0,dotPos), text.Substring(dotPos + 1, endPos-dotPos-1), v);
			}

			return null;
		}


		internal static ScriptF Call()
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{
						var v=slove_call_(text,ois,infos,manager);
						if (v.HasValue)
						{

							var ins = manager.GetObject(v.Value.instance, v.Value.IInfo, r);
							Type t = ins.returnType;

							if(ins is ClassScript)
							{
								var classz=ins as ClassScript;
								if(v.Value.Params==null)
								{
									var re= classz.Field(v.Value.name, null);
									return re;
								}
								else
								{


								
									var vv = v.Value;

									

									ScriptObject[] scriptObjects = new ScriptObject[vv.Params.Length];

									Type[] types = new Type[vv.Params.Length];
									for (int i = 0; i < vv.Params.Length; i++)
									{
										scriptObjects[i] = manager.GetObject(vv.Params[i].name, vv.Params[i].infos, r);
									
										types[i] = scriptObjects[i].returnType;
									}
									return classz.Call(v.Value.name, null, scriptObjects, types);
								}
							}

							if (v.Value.Params == null)
							{


								var f = t.GetField(v.Value.name);
								if (f == null)
								{
									var p = t.GetProperty(v.Value.name);
									return new ScriptObject<E>(p.PropertyType, e => Expression.Property(e, p), ins);
								}
								return new ScriptObject<E>(f.FieldType, e => Expression.Field(e, f), ins);
							}
							else
							{
								var vv = v.Value;
								Type[] types = new Type[vv.Params.Length];
								ScriptObject[] scriptObjects = new ScriptObject[vv.Params.Length + 1];
								scriptObjects[vv.Params.Length] = ins;
								for (int i = 0; i < vv.Params.Length; i++)
								{
									scriptObjects[i] = manager.GetObject(vv.Params[i].name, vv.Params[i].infos, r);
									types[i] = scriptObjects[i].returnType;
								}
								var m = t.GetMethod(vv.name, types);
						
								return new MultiScript(m.ReturnType, e => { return Expression.Call(e[e.Length - 1], m, e.Take(e.Length - 1)); }, scriptObjects);
							}
						}
					
						throw new ParamsCountMismatchException(text+"?");
					};
		}
		internal static ScriptF Block()
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{

						int end = text.AsSpan().findEnd('{', '}');
						string block = text.Substring(0, end + 1);
						List<int> pos = new List<int>();
						string[] ss = block.Substring(1, end).cutZeroLevel(';', manager.levelGetter, pos);

						manager.pushStack();
						bool shouldReturn=manager.peakLabel(out var label)&&!label.Used;

						if(shouldReturn)
						{
							label.Use();
						}
						//HashSet<ParameterExpression> parameters = new HashSet<ParameterExpression>();
						int l = shouldReturn ? ss.Length + 1:ss.Length ;
						ScriptObject[] scriptObjects = new ScriptObject[l];
						for (int i = 0; i < ss.Length; i++)
						{
							//pos[i]++;
							var script = manager.GetObject(ss[i]);
							scriptObjects[i] = script;
						}
						if(shouldReturn)
						{
							scriptObjects[l - 1] = new RootElementScript(label.Target.Type, Expression.Label(label.Target, manager.getDefault(label.Target.Type)));
						}

						var parameters = manager.peakParam();
						manager.popStack(out var remove);


						return new MultiScript(scriptObjects[scriptObjects.Length-1].returnType, 

						(ss) =>

						Expression.Block(parameters, ss),

						scriptObjects
						
						);


						throw new ParamsCountMismatchException();
					};










		}
		internal static ScriptF EQ()
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{

						var v = ScriptInfo.solveLR(text, ois, infos, manager);

						var n = manager.getName_func(v[0].name);
						if (n.name.isFuncName())
						{

							manager.pushStack();
							ReadOnlySpan<char> funcname = n.name.AsSpan();
							ReadOnlySpan<char> func = v[1].name.AsSpan();
							bool cast = true;
							if (funcname.EndsWith("#"))
							{
								cast = false;
							}
							if(n.t!=null)
							{
								manager.pushLabel(n.t.Type);
							}
							if (funcname != null && func != null)
							{

								(string name, List<string> args) = getParams(funcname);
								int paramCount = args.Count;
								ScriptInfo[] scriptInfos = new ScriptInfo[paramCount];
								ParameterExpression[] es = new ParameterExpression[paramCount];
								if (paramCount > 0)
								{
									for (int i = 0; i < paramCount; i++)
									{
										var v1 = manager.getName(args[i]);
										scriptInfos[i] = manager.registerInStack(param(v1.name, v1.t.Type, out es[i]));
									}
								}

								List<ScanInfo> scanInfos = new();
								var re = manager.ScanForOperators(ref v[1].name, scanInfos);
								var obj = manager.GetObject(v[1].name, scanInfos, re);
								var ge = obj.getExpression();
								var vr = Expression.Lambda(ge.Item1,es);

								var vr1 = () => { manager.register(new ScriptInfo(name, max, Func(vr, cast))); };

								Expression t = vr1.Target != null ? Expression.Constant(vr1.Target) : null;

								Expression ea = Expression.Call(t, vr1.Method);
								/*
								for (int i = 0; i < paramCount; i++)
								{
									var vv= scriptInfos[i];
									if (vv != null)
									manager.register(vv);
								}*/
								if(n.t!=null)
								{
									manager.popLabel(out var label);
								}
								manager.popStack(out var vv);
								//DVOS.writeLine(vv.Count);
								return new RootElementScript(typeof(void), ea);

							}
						}
						else if (n.name.isVarName())
						{
							//n = manager.getName(v[0].name);


							var o = manager.GetObject(n.name, v[0].infos, r);

							var o2 = manager.GetObject(v[1].name, v[1].infos, r);
							if (o is VarScript)
							{

								var vr = (o as VarScript).setValue(o2);
								//manager.peakParam().Add(vr.GetVarScript().getValue());
								//return new ScriptObject<E, E>(o.returnType, (a, b) => Expression.Assign(a, b), o, o2);
								return vr;
							}
							else
							{
								Type t = n.t == null ? o2.returnType : n.t.Type;
								manager.registerInStack(param(n.name, t, out var e,"tempParam",false));
								//var newO=
                            var vso=new VarScript(t, e);
								vso.setCheck(false);
								manager.peakParam().Add(vso.getValue());
								return vso.setValue(o2);

							}



						}

						throw new ParamsCountMismatchException();
					};











			;
		}
		public static bool withPoint(ReadOnlySpan<char> span)
		{
			for (int i = 0; i < span.Length - 1; i++)
			{
				if (span[i] == '.' && span[i + 1] >= '0' && span[i + 1] <= '9')
				{
					return true;
				}

			}
			return false;
		}
		internal static ScriptF Action<I>(Action<I> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Action<I>> f = (a) => func(a);
					int t = s.Length;
					if (t > 0)
					{
						var A = manager.GetObject(s[0].name, s[0].infos, r);

						if(cast&& A.returnType != typeof(I))
						{
							A=Cast(A,typeof(I));
						}

						return new ScriptObject<Expression>(typeof(void), (a) => Expression.Invoke(f,a), A);
					}
					else
					{
						throw new ParamsCountMismatchException();

					}

				};
		}
		internal static ScriptF Func<I1, I2, I3, O>(Func<I1, I2, I3, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Func<I1, I2, I3, O>> f = (a, b, c) => func(a, b, c);
					int t = s.Length;
					if (t >= 3)
					{

						(var a, var b, var c) = (manager.GetObject(s[0].name, s[0].infos, r), manager.GetObject(s[1].name, s[1].infos, r), manager.GetObject(s[2].name, s[2].infos, r));

						if (a.returnType != typeof(I1) && cast)
						{
							a = Cast(a, typeof(I1));
						}
						if (b.returnType != typeof(I2) && cast)
						{
							b = Cast(b, typeof(I2));
						}
						if (b.returnType != typeof(I3) && cast)
						{
							b = Cast(b, typeof(I3));
						}

						return new ScriptObject<E, E, E>(typeof(O), (a, b, c) => { return Expression.Invoke(f, a, b, c); }, a, b, c);
					}
					else
					{

						throw new ParamsCountMismatchException();
					}

				};
		}


		internal static ScriptF Func(LambdaExpression delegate_, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{
					var param = delegate_.Parameters;
					int l = param.Count;
					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					int t = s.Length;
					if (t >= l)
					{
						ScriptObject[] scriptObjects = new ScriptObject[l];
						for (int i = 0; i < l; i++)
						{
							scriptObjects[i] = manager.GetObject(s[i].name, s[i].infos, r);
							var type = param[i].Type;
							if (type != scriptObjects[i].returnType && cast)
							{
								scriptObjects[i] = new ScriptObject<E>(type, (a) => { return ScriptHelper.Convert(a, type); }, scriptObjects[i]);
							}
						}
						return new MultiScript(delegate_.ReturnType, (m) => {
							return Expression.Invoke(delegate_, m);
						}, scriptObjects);
					}
					else
					{
						throw new ParamsCountMismatchException();

					}

				};
		}
		internal static ScriptF Func(Delegate delegate_, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{
					var info = delegate_.GetMethodInfo();

					var param = info.GetParameters();
					int l = param.Length;

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					int t = s.Length;
					if (t >= l)
					{
						ScriptObject[] scriptObjects = new ScriptObject[l];
						for (int i = 0; i < l; i++)
						{
							var v=scriptObjects[i] = manager.GetObject(s[i].name, s[i].infos, r);
							if (cast && v.returnType != param[i].ParameterType)
							{
								scriptObjects[i] = v.Cast(param[i].ParameterType);
							}
						}
						return new MultiScript(info.ReturnType, (m) => {
							bool isNull = delegate_.Target == null;
							Expression e = isNull ? null : Expression.Constant(delegate_.Target);
						
							return Expression.Call(e, info, m);
						}, scriptObjects);
					}
					else
					{
						throw new ParamsCountMismatchException();

					}

				};
		}

		internal static ScriptF R<I, O>(Func<I, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveR(text, ois, infos, manager);
					Expression<Func<I, O>> f = a => func(a);
					var A = manager.GetObject(s.name, s.infos, r);
					if(A.returnType!= typeof(I)&&cast)
					{
						A = A.Cast(typeof(I));
					}	

					return new ScriptObject<E>(typeof(O), (a) => Expression.Invoke(f,  a), A);
				};
		}
		internal static ScriptF FuncS<I, O>(Function<I, O> func,bool cast=false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Func<I[], O>> f = (a) => func(a);
					int t = s.Length;
					ScriptObject[] scripts = new ScriptObject[t];
					for (int i = 0; i < t; i++)
					{
						scripts[i] = manager.GetObject(s[i].name, s[i].infos, r);
						if(cast&&scripts[i].returnType!=typeof(I))
						{
							scripts[i]=scripts[i].Cast(typeof(I));
						}
					}

					return new MultiScript(typeof(O), (a) => Expression.Invoke(f, a), scripts);


				};
		}

		internal static ScriptF RLOR<I, I1, O>(Func<I1, O> func1, Func<I, I1, O> func2, bool cast = false)
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{
					if (ois.position == 0)
					{
						var s = ScriptInfo.solveR(text, ois, infos, manager);
						Expression<Func<I1, O>> f = a => func1(a);
						var A = manager.GetObject(s.name, s.infos, r);

						if( A.returnType != typeof(I1)&&cast)
						{
							A=A.Cast(typeof(I1));
						}

						return new ScriptObject<E>(typeof(O), (a) => Expression.Invoke(f, a), A);
					}
					else
					{
						var s = ScriptInfo.solveLR(text, ois, infos, manager);
						Expression<Func<I, I1, O>> f = (a, b) => func2(a, b);
						var A = manager.GetObject(s[0].name, s[0].infos, r);
						var B = manager.GetObject(s[1].name, s[1].infos, r);
						if( A.returnType != typeof(I)&&cast)
							{
							A = A.Cast(typeof(I));
						}
						if(B.returnType!=typeof(I1)&&cast)
						{
							B=B.Cast(typeof(I1));
						}

						return new ScriptObject<E, E>(typeof(O), (a, b) => { return Expression.Invoke(f, a, b); }, A, B);
					}
				};
		}

		internal static ScriptF L<I, O>(Func<I, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, ScanList infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveL(text, ois, infos, manager);
					Expression<Func<I, O>> f = a => func(a);
					var A = manager.GetObject(s.name, s.infos, r);

					if(cast&& A.returnType != typeof(I))
					{
						A=A.Cast(typeof(I));
					}
					return new ScriptObject<E>(typeof(O), (a) => Expression.Invoke(f, a), A);
				};
		}

		internal static ScriptF Func<O>(Func<O> func)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Func<O>> f = () => func();
					int t = s.Length;

					return new RootElementScript(typeof(O), f);


				};
		}


		internal static ScriptF Func<I, O>(Func<I, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Func<I, O>> f = (a) => func(a);
					int t = s.Length;
					
					if (t > 0)
					{
						var A = manager.GetObject(s[0].name, s[0].infos, r);
						if(A.returnType!=typeof(I))
						{
							A=A.Cast(typeof(I));
						}
						return new ScriptObject<E>(typeof(O), (a) =>
						

						{
							return Expression.Invoke(f, a);
						}


						,A
						);
					}
					else
					{

						throw new ParamsCountMismatchException();
					}

				};
		}



		internal static ScriptF Func<I1, I2, O>(Func<I1, I2, O> func, bool cast = false)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					var s = ScriptInfo.solveFunc(text, ois, infos, manager);
					Expression<Func<I1, I2, O>> f = (a, b) => func(a, b);
					int t = s.Length;
					(var A,var B)=( manager.GetObject(s[0].name, s[0].infos, r), manager.GetObject(s[1].name, s[1].infos, r));

					if(A.returnType!=typeof(I1)&&cast)
					{
						A= A.Cast(typeof(I1));
					}

					if(B.returnType!=typeof(I2)&&cast)
					{
						B= B.Cast(typeof(I2));
					}

					if (t > 1)
					{
						return new ScriptObject<E, E>(typeof(O), (a, b) => {return Expression.Invoke(f, a, b); },A,B );
					}
					else
					{

						throw new ParamsCountMismatchException();
					}

				};
		}

	}
}