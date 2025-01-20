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

namespace DVLib.LabDataHelper.DVScript
{	public class ParamsCountMismatchException:Exception
		{

		}
	public class ScriptManager:ObjectManager<ScriptObject,ScriptInfo,ScriptManager>
	{

		internal static int  max = 8;
	
		StringDictionary<Type> typeDic = new();
	

		public void registerType(string token, Type type, ExpMaker<E> expMaker = null)
		{
			register(new ScriptInfo(token, max, ConvertFunc(type,expMaker)));
			typeDic.Add(token, type);
		}
		public override void registerDefault()
		{
			register(new ScriptInfo("+",max-4,NaiveMath(Expression.Add)));
			register(new ScriptInfo("-", max - 3, Subtract()));
			register(new ScriptInfo("*", max - 2, NaiveMath(Expression.Multiply)));
			register(new ScriptInfo("/", max - 2, NaiveMath(Expression.Divide)));
			register(new ScriptInfo("%", max - 2, NaiveMath(Expression.Modulo)));
			register(new ScriptInfo("^", max - 1, LR<double, double, double>((a, b) => Math.Pow(a,b),true)));
			register(new ScriptInfo("*-", max - 2, NaiveMath(Negate(Expression.Multiply))));
			register(new ScriptInfo("/-", max - 2, NaiveMath(Negate(Expression.Divide))));
			register(new ScriptInfo("%-", max - 2, NaiveMath(Negate(Expression.Modulo))));
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
			register(new ScriptInfo("=", 1, EQ()));

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
			registerType("object", typeof(object));

			register(new ScriptInfo("string", max, Func<object, string>(o => o.ToString(),true)));

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

	



		 internal (string name,Type t)getName(string name) 
			{
			bool match = typeDic.match(name, out Type type,out string key);
            if (match)
            {
				return (name.Replace(key, ""),type);
            }

			return (name, typeof(double));
        }


	


	
		public override ScriptObject getBaseType(string s)
		{

			try
			{
				double d=double.Parse(s);
				Expression c = Expression.Constant(d);
			
				return new SourceScriptObject(typeof(double),c);
			}
			catch
			{
			}

			if(s.StartsWith('"')&&s.EndsWith('"'))
			{
				s = s.TrimStart('"');
				s = s.TrimEnd('"');
			}

			return ScriptObject.ConstString(s).setReturnType(typeof(string));

		}


		internal override void onCreated(ScriptObject obj, ScriptInfo info)
		{
			obj.setScriptInfo(info);
		}


	}
}
