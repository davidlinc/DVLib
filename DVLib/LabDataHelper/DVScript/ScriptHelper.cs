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
		internal static ExpMaker<E, E> Negate(ExpMaker<E, E> maker)
		{
			return (A, B) => maker(A, Expression.Negate(B));
		}

		internal static ScriptInfo param(string name, Type type, out ParameterExpression e)
		{
			e = Expression.Parameter(type);
			Expression ee = e;
			return new ScriptInfo(name, max,

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{

					return new SourceScriptObject(type, ee);
				}

				, "tempParam");
		}

		internal static Dictionary<Type, int> typePriority = new Dictionary<Type, int>() { { typeof(byte), 0 }, { typeof(sbyte), 1 }, { typeof(short), 2 }, { typeof(ushort), 3 }, { typeof(int), 4 }, { typeof(uint), 5 }, { typeof(long), 6 }, { typeof(ulong), 7 }, { typeof(float), 8 }, { typeof(double), 9 }, };

		internal static Type getNewType(Type a, Type b)
		{
			int t = -1, t2 = -1;
			typePriority.TryGetValue(a, out t);
			typePriority.TryGetValue(b, out t2);

			if (t >= t2 && t >= 0)
			{
				return a;
			}
			else
			{
				return b;
			}


		}

		internal static ScriptF NaiveMath(ExpMaker<E, E> expMaker)
		{
			return

				(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
				{
					var s = ScriptInfo.solveLR(text, ois, infos, manager);
					var A = manager.GetObject(s[0].name, s[0].infos, r);
					var B = manager.GetObject(s[1].name, s[1].infos, r);
					var Atype = A.returnType;
					var Btype = B.returnType;
					bool cast = false;
					if (Atype != Btype)
					{
						Atype = Btype = getNewType(Atype, Btype);
						cast = true;
					}


					return new ScriptObject<E, E>(Atype, (a, b) => { if (cast) { a = Expression.Convert(a, Atype); b = Expression.Convert(b, Btype); } return expMaker(a, b); }, A, B).setReturnType(Atype);
				};
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
	return new ScriptObject<E>(A.returnType, (a) => { return Expression.Negate(a); }, A).setReturnType(A.returnType);
}
else
{

	var s = ScriptInfo.solveLR(text, ois, infos, manager);
	var A = manager.GetObject(s[0].name, s[0].infos, r);
	var B = manager.GetObject(s[1].name, s[1].infos, r);
	var re = A.returnType;
	bool cast = false;
	if (A.returnType != B.returnType)
	{
		re = getNewType(A.returnType, B.returnType);
		cast = true;
	}
	return new ScriptObject<E, E>(re, (a, b) => { if (cast) { a = Expression.Convert(a, re); b = Expression.Convert(b, re); } return Expression.Subtract(a, b); }, A, B).setReturnType(re);

}
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
					return new ScriptObject<E>(type, (a) => { if (exp == null) return Expression.Convert(a, type); return exp(a); }, v).setReturnType(type);

				}

				throw new ParamsCountMismatchException();
			};
		}
		internal static ScriptObject Cast(this ScriptObject script, Type type)
		{
			return new ScriptObject<E>(type, e => Expression.Convert(e, type), script);
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
					return new ScriptObject<E, E>(typeof(O), (a, b) => {   return Expression.Invoke(f, a, b); }, A, B).setReturnType(typeof(O));
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
		internal static ScriptF EQ()
		{
			return


					(string text, ScanInfo ois, List<ScanInfo> infos, ScriptManager manager, ScanResult r) =>
					{

						var v = ScriptInfo.solveLR(text, ois, infos, manager);

						if (v[0].name.isFuncName())
						{
							ReadOnlySpan<char> funcname = v[0].name.AsSpan();
							ReadOnlySpan<char> func = v[1].name.AsSpan();
							bool cast = true;
							if (funcname.EndsWith("#"))
							{
								cast = false;
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
										scriptInfos[i] = manager.register(param(v1.name, v1.t, out es[i]));
									}
								}

								List<ScanInfo> scanInfos = new();
								var re = manager.ScanForOperators(ref v[1].name, scanInfos);
								var obj = manager.GetObject(v[1].name, scanInfos, re);
								var ge = obj.getExpression();
								var vr = Expression.Lambda(ge.Item1, ge.Item2);

								var vr1 = () => { manager.register(new ScriptInfo(name, max, Func(vr, cast))); };

								Expression t = vr1.Target != null ? Expression.Constant(vr1.Target) : null;

								Expression ea = Expression.Call(t, vr1.Method);

								for (int i = 0; i < paramCount; i++)
								{
									manager.register(scriptInfos[i]);
								}
								return new SourceScriptObject(typeof(void), ea);

							}
						}
						else if (v[0].name.isVarName())
						{
							var n = manager.getName(v[0].name);



						
							var o = manager.GetObject(n.name, v[0].infos, r);

							var o2 = manager.GetObject(n.name, v[0].infos, r);
							if (o is VarScriptObject)
							{
								return new ScriptObject<E, E>(o.returnType, (a, b) => Expression.Assign(a, b), o, o2);
							}
							else
							{
								var newO=

								return new ScriptObject<E, E>(o.returnType, (a, b) => Expression.Assign(a, b), o, o2);

							}



						}

						throw new ParamsCountMismatchException();
					};











			;
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

						return new ScriptObject<Expression>(null, (a) => Expression.Invoke(f,a), A).setReturnType(null);
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
								scriptObjects[i] = new ScriptObject<E>(type, (a) => { return Expression.Convert(a, type); }, scriptObjects[i]).setReturnType(type);
							}
						}
						return new MutiScriptObject(delegate_.ReturnType, (m) => {
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
						return new MutiScriptObject(info.ReturnType, (m) => {
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

					return new ScriptObject<E>(typeof(O), (a) => Expression.Invoke(f,  a), A).setReturnType(typeof(O));
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

					return new MutiScriptObject(typeof(O), (a) => Expression.Invoke(f, a), scripts);


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

					var s = ScriptInfo.solveR(text, ois, infos, manager);
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

					return new SourceScriptObject(typeof(O), f);


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