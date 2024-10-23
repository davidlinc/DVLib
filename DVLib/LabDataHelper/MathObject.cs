using DVOSLib;
using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using  OperatorScanInfo=DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.MathObject,DVLib.LabDataHelper.OperatorInfo, DVLib.LabDataHelper.MathObjectManager>;
using ObjectFactory = DVLib.LabDataHelper.Factory<DVLib.LabDataHelper.MathObject, DVLib.LabDataHelper.OperatorInfo, DVLib.LabDataHelper.MathObjectManager>;
using ScanSet = System.ValueTuple<string,System.Collections.Generic.List<DVLib.LabDataHelper.ScanInfo<DVLib.LabDataHelper.MathObject, DVLib.LabDataHelper.OperatorInfo, DVLib.LabDataHelper.MathObjectManager>>>;
using DVLib.LabDataHelper.MathObjectSystem;
using System.Runtime.Intrinsics.X86;
using System.Xml;

using System.Diagnostics;
using NewPhysics;
using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp;

namespace DVLib.LabDataHelper
{

	public delegate double ThreeElementsOperator(double a, double b,double c);
	public delegate double FourElementsOperator(double a, double b, double c,double d);
	public delegate double TwoElementsOperator(double a,double b);
	public delegate double OneElementOperator(double a);
	public delegate double SourceOperator(params double[] doubles);
	public delegate MathObject MathMaster(RuntimeMathObject executor);
	public delegate (object,SourceOperator) MethodFactory(MathObjectManager manager, params (string, List<OperatorScanInfo>)[] vars);
	public delegate void OnReturn(object o);
	public delegate void AfterRun<T>(T o);
	public delegate MathObject OperatorReplace(MathObjectManager m, MathOperator o, MathObject[] objects);
	public delegate MathObject DerivativeGetter(int index,MathObject source,params MathObject[] objects);

   



    public class MathObjectManager:ObjectManager<MathObject,OperatorInfo,MathObjectManager>
	{
		 int max = 8;
		Random random = new Random();
		static NumberObject ONE=new NumberObject(1);

		static NumberObject HALF = new NumberObject(0.5);

		static NumberObject TWO = new NumberObject(2);
		StringDictionary<(string, string)> toReplace = new StringDictionary<(string, string)>();
		internal static MathObject error = new NumberObject(double.NaN);
		internal static MathObject trueO = new NumberObject(1);
		internal static MathObject falseO = new NumberObject(0);

		internal static MathObject nullO = new NumberObject(-1);
	    bool canOver = true;

		static StringBuilder SB = new StringBuilder();
		Stack<OnReturn> funcReturnStack = new Stack<OnReturn>();
		int RunTimeCount;

		public void pushRunTime()
		{
			RunTimeCount++;
		}
		public void popRunTime()
		{
			RunTimeCount--;
			if(RunTimeCount<0)
			{
				RunTimeCount = 0;
			}
		}

		public bool isRuntime { get { return RunTimeCount>0; } }

		public MathObjectManager()
		{

		}
		static MathObjectManager()
		{
		}

		public void addReturn(OnReturn @return)
		{
			funcReturnStack.Push(@return);

		}

		public void removeReturn()
		{
			funcReturnStack.TryPop(out OnReturn or);
		}

		internal override void onCreated(MathObject obj, OperatorInfo info)
		{
			//obj.setPmSize(info.pmSize);
		}

		public void returnObject(object o)
		{
	        if(funcReturnStack.TryPeek(out OnReturn r))
			{
				r(o);

			}
			else
			{
			}
		}

		public override	void  registerDefault()
		{
			
			register(new OperatorInfo("+", OperatorType.LeftRight, max - 4, so: Helper.toSourceOperator((a, b) => { return a + b; }), d: (i,o, m) =>
			{

				return OP("+", m[0].getDerivative(i), m[1].getDerivative(i));
			}
			));

			register(new OperatorInfo("sum", OperatorType.Func, max, so: (double[] m) => { double sum = 0; foreach (var i in m) sum += i; return sum; }, d: (i,o, m) =>
			{
				MathObject[] ms = new MathObject[m.Length];
				for (int j = 0; j < m.Length; j++)
				{
					ms[j] = m[j].getDerivative(i);
				}
				return OP("sum", ms);
			}

			));


			register(new OperatorInfo("==", OperatorType.LeftRight, max - 5, so: Helper.toSourceOperator((a, b) => { return a == b ? 1 : 0; })));

			register(new OperatorInfo(">=", OperatorType.LeftRight, max - 5, so: Helper.toSourceOperator((a, b) => { return a >= b ? 1 : 0; })));

			register(new OperatorInfo("<=", OperatorType.LeftRight, max - 5, so: Helper.toSourceOperator((a, b) => { return a <= b ? 1 : 0; })));

			register(new OperatorInfo(">", OperatorType.LeftRight, max - 5, so: Helper.toSourceOperator((a, b) => { return a > b ? 1 : 0; })));

			register(new OperatorInfo("<", OperatorType.LeftRight, max - 5, so: Helper.toSourceOperator((a, b) => { return a < b ? 1 : 0; })));

			register(new OperatorInfo("&&", OperatorType.LeftRight, max - 6, so: Helper.toSourceOperator((a, b) => { return (b > 0 && a > 0) ? 1 : 0; })));

			register(new OperatorInfo("||", OperatorType.LeftRight, max - 6, so: Helper.toSourceOperator((a, b) => { return (b > 0 || a > 0) ? 1 : 0; })));

			register(";", OperatorType.RUNCODE, 0, Helper.toSourceOperator((a, b) => { return 0; }),OperatorInfo.RuntimeLR);
			register(new OperatorInfo("=", OperatorType.LeftRight, 1, so: Helper.toSourceOperator((a, b) => { return 0; }), d: null, factory: EQ
				).setReverse());
			register(new OperatorInfo("-", OperatorType.LeftRightOrRight, max - 3,
				so: (double[] data) => { if (data.Length == 1) return -data[0]; return data[0] - data[1]; }
				, d: (i,o, m) =>
				{
					if (m.Length == 1)
					{
						return OP("-", m[0].getDerivative());
					}
					else { return OP("-", m[0].getDerivative(), m[1].getDerivative()); }
				})

				); 
			register(new OperatorInfo("--", OperatorType.LeftRightOrRight, max - 3,
				so: (double[] data) => { if (data.Length == 1) return data[0]; return data[0] + data[1]; }
				, d: (i,o, m) =>
				{
					if (m.Length == 1)
					{
						return OP("+", m[0].getDerivative());
					}
					else { return OP("+", m[0].getDerivative(), m[1].getDerivative()); }
				})
				);
			register(new OperatorInfo("if", OperatorType.RUNCODE, max, so: error.getValue, d: null, factory: IF));
			register(new OperatorInfo("else", OperatorType.RUNCODE, max - 1, so: error.getValue, d: null, factory: ELSE));
			register(new OperatorInfo("return", OperatorType.RUNCODE, 2, so: error.getValue, d: null, factory: RETURN));
			register(new OperatorInfo("{", OperatorType.RUNCODE, max + 1, so: error.getValue, d: null, factory: CODEBLOCK));

			register(new OperatorInfo("/", OperatorType.LeftRight, max - 2, so: (a, b) => { return a / b; }
			, derivative: (i,o, m) =>
			{
				var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
				return OP("/", OP("-", OP("*", x_, y), OP("*", y_, x)), OP("*", y, y));
			}
			));
			register(new OperatorInfo("*", OperatorType.LeftRight, max - 2, so: (a, b) => { return a * b; }
			, derivative: (i, o, m) =>
			{
				var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
				return OP("+", OP("*", x_, y), OP("*", y_, x));
			}));
			register(new OperatorInfo("%", OperatorType.LeftRight, max - 2, so: (a, b) => { return a % b; }));
			register(new OperatorInfo("^", OperatorType.LeftRight, max - 1, so: Math.Pow, derivative: (i, o, m) =>
			{
				var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
				return OP("*", OP("^", x, y), OP("+", OP("*", y_, OP("ln", x)), OP("*", y, OP("/", x_, x))));
			}));
			register(new OperatorInfo("/-", OperatorType.LeftRight, max - 2, so: (a, b) => { return a / -b; }
				, derivative: (i, o, m) =>
				{
					var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
					return OP("-", OP("/", OP("-", OP("*", x_, y), OP("*", y_, x)), OP("*", y, y)));
				}
			)

				);
			register(new OperatorInfo("*-", OperatorType.LeftRight, max - 2, so: (a, b) => { return a * -b; },
				derivative: (i, o, m) =>
				{
					var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
					return OP("-", OP("+", OP("*", x_, y), OP("*", y_, x)));
				}));
			register(new OperatorInfo("%-", OperatorType.LeftRight, max - 2, so: (a, b) => { return a % -b; }));
			register(new OperatorInfo("^-", OperatorType.LeftRight, max - 1, so: (a, b) => Math.Pow(a, -b), derivative: (i, o, m) =>
			{
				var x = m[0]; var y = m[1]; var x_ = x.getDerivative(i); var y_ = y.getDerivative(i);
				return OP("-", OP("*", OP("^", x, y), OP("+", OP("*", y_, OP("ln", x)), OP("*", y, OP("/", x_, x)))));
			}));
			register(new OperatorInfo("dirac", OperatorType.Func, max, so: (a) => { if (a==0) return double.PositiveInfinity; return 0; }, d: (i,o, m) =>
			{
				return mulID(OP("'", OP("dirac", m[0]),new NumberObject(1)), m[0], i) ;
			}));

			register(new OperatorInfo("'", OperatorType.LeftRight, max - 1, "raw", (a) => a[0] ==0?double.NaN:0, (i, o, m) =>
			{
				return mulID(OP("'", m[0],new NumberObject(m[1].getValue() + 1)), m[0].getElement());
			},DE));

			register(new OperatorInfo("IF", OperatorType.Func, max, so: (a, b, c) => { if (a > 0) return b; return c; }));
			register(new OperatorInfo("sin", OperatorType.Func, max, so: Math.Sin, d: (i,o, m) =>
			{

				return OP("*", OP("cos", m[0]), m[0].getDerivative(i));

			}));
			register(new OperatorInfo("cos", OperatorType.Func, max, so: Math.Cos, d: (i,o, m) =>
			{

				return OP("*-", OP("sin", m[0]), m[0].getDerivative(i));

			}));
			register(new OperatorInfo("tan", OperatorType.Func, max, so: Math.Tan,d:(i, o, m) =>
			{
				return mulID(OP("/", ONE, OP("sq", OP("cos", m[0]))), m[0], i);
			}));
			register(new OperatorInfo("arcsin", OperatorType.Func, max, so: Math.Asin, d: (i,o, m) =>
			{


				return mulID(OP("/", ONE, OP("sqrt", OP("-", ONE, OP("sq", m[0])))), m[0], i);

			}));
			register(new OperatorInfo("arccos", OperatorType.Func, max, so: Math.Acos, d: (i,o, m) =>
			{


				return mulID(OP("/-", ONE, OP("sqrt", OP("-", ONE, OP("sq", m[0])))), m[0], i);

			}));
			register(new OperatorInfo("arctan", OperatorType.Func, max, so: Math.Atan, d: (i,o, m) =>
			{


				return mulID(OP("/", ONE, OP("+", ONE, OP("sq", m[0]))), m[0], i);

			}));

			register(new OperatorInfo("arccosh", OperatorType.Func, max, so: Math.Acosh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE, OP("sqrt", OP("+", OP("sq", m[0]), NumberObject.ONE))), m[0], i);

			}));
			register(new OperatorInfo("arcsinh", OperatorType.Func, max, so: Math.Asinh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE, OP("sqrt", OP("-", OP("sq", m[0]), NumberObject.ONE))), m[0], i);

			}));
			register(new OperatorInfo("arctanh", OperatorType.Func, max, so: Math.Atanh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE,  OP("-", NumberObject.ONE, OP("sq", m[0]))), m[0], i);

			})); ;

			register(new OperatorInfo("arctan2", OperatorType.Func, max, so: Math.Atan2));
			
			register(new OperatorInfo("ln", OperatorType.Func, max, so: (OneElementOperator)Math.Log, d: (i,o, m) =>
			{
				return OP("*", OP("/", new NumberObject(1), m[0]), m[0].getDerivative(i));
			}));
			register(new OperatorInfo("asin", OperatorType.Func, max, so: Math.Asin, d: (i,o, m) =>
			{


				return mulID(OP("/", ONE, OP("sqrt", OP("-", ONE, OP("sq", m[0])))), m[0], i);

			}));
			register(new OperatorInfo("acos", OperatorType.Func, max, so: Math.Acos, d: (i,o, m) =>
			{


				return mulID(OP("/-", ONE, OP("sqrt", OP("-", ONE, OP("sq", m[0])))), m[0], i);

			}));
			register(new OperatorInfo("atan", OperatorType.Func, max, so: Math.Atan, d: (i,o, m) =>
			{


				return mulID(OP("/", ONE, OP("+", ONE, OP("sq", m[0]))), m[0], i);

			}));

			register(new OperatorInfo("abs", OperatorType.Func, max, so: Math.Abs, d: (i,o, m) => {
				return mulID(OP("/", m[0], OP("abs", m[0])), m[0], i);
			}));
			register(new OperatorInfo("acosh", OperatorType.Func, max, so: Math.Acosh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE, OP("sqrt", OP("+", OP("sq", m[0]), NumberObject.ONE))), m[0], i);

			}));
			register(new OperatorInfo("asinh", OperatorType.Func, max, so: Math.Asinh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE, OP("sqrt", OP("-", OP("sq", m[0]), NumberObject.ONE))), m[0], i);

			}));
			register(new OperatorInfo("atanh", OperatorType.Func, max, so: Math.Atanh, "raw", (i, o, m) =>
			{

				return mulID(OP("/", NumberObject.ONE, OP("-", NumberObject.ONE, OP("sq", m[0]))), m[0], i);

			})); ;

			register(new OperatorInfo("atan2", OperatorType.Func, max, so: Math.Atan2));

			register(new OperatorInfo("sinh", OperatorType.Func, max, so: Math.Sinh));
			register(new OperatorInfo("cosh", OperatorType.Func, max, so: Math.Cosh));
			register(new OperatorInfo("tanh", OperatorType.Func, max, so: Math.Tanh));
			register(new OperatorInfo("exp", OperatorType.Func, max, so: Math.Exp, "raw", (i, o, m) => { return mulID(OP("exp", m[0]), m[0], i); }));
			register(new OperatorInfo("sqrt", OperatorType.Func, max, so: Math.Sqrt, d: (i,o, m) => {
				
				return mulID(OP("*", HALF, OP("/", ONE, OP("sqrt", m[0]))), m[0], i); }));
			register(new OperatorInfo("sq", OperatorType.Func, max, so: a => a * a,d: (i, o, m) => {
				return mulID(OP("*",TWO,  m[0]), m[0], i);
		}));
			register(new OperatorInfo("round", OperatorType.Func, max, so: Math.Round));
			register(new OperatorInfo("max", OperatorType.Func, max, so: Math.Max));
			register(new OperatorInfo("min", OperatorType.Func, max, so: Math.Min));
			register(new OperatorInfo("random", OperatorType.Source, max, so: (double[] data) => random.NextDouble(), d: (i, o, m) =>
			{
				return NumberObject.ZERO;
			}));
			register(new OperatorInfo("randInt", OperatorType.Func, max, (a, b) => random.Next((int)Math.Min(a, b), (int)Math.Max(a, b)),"raw", (i, o, m) =>
			{
				return NumberObject.ZERO;
			}));
			register(new OperatorInfo("pi", OperatorType.Number, max).setValue(Math.PI));
			register(new OperatorInfo("rad", OperatorType.Func, max, so: (a) => { return a * Math.PI / 180; }));
			register(new OperatorInfo("degree", OperatorType.Func, max, so: (a) => { return a * 180 / Math.PI; }));
			register(new OperatorInfo("x", OperatorType.Source, max, tag: "varsindex0").setDot(0));
			register(new OperatorInfo("y", OperatorType.Source, max, tag: "varsindex1").setDot(1));
			register(new OperatorInfo("z", OperatorType.Source, max, tag: "varsindex2").setDot(2));
		}
		
		OperatorInfo register(string mark, OperatorType type, int priority, SourceOperator so,ObjectFactory factory=null , string tag = "raw")
		{
			return register(new OperatorInfo(mark, type, priority, tag, so, null, factory));
		}

		MathObject mulID(MathObject d,MathObject m,int i=0)
		{
			return OP("*",m.getDerivative(i),d);
		}
	
		public MathOperator OP(string name,params MathObject[] objects)
		{
			var v=match(name);

			if(v!=null)
			{
				return new MathOperator( v,objects);
			}

			return MathOperator.NAN;
		}
		OperatorInfo register(string mark, OperatorType type, int priority,OneElementOperator so, ObjectFactory factory = null, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so, tag, null, factory));
		}
		OperatorInfo register(string mark, OperatorType type, int priority, TwoElementsOperator so, ObjectFactory factory = null, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so, tag, null, factory));
		}


		public override void clear()
		{

			base.clear();
			registerDefault();
		}

		public override string formalizeCode(string s)
		{
			SB.Clear();
			SB.EnsureCapacity(s.Length);
			var ss = s.AsSpan();
			char v;
			for(int i = 0;i < ss.Length; i++)
			{
				v = ss[i];
				if(v!=' '&&v!='\n')
				{
					if(toReplace.match(s,out var pair,i))
					{
						SB.Append(pair.Item2);
						i += pair.Item1.Length - 1;
					}
					else
					{
						SB.Append(v);
					}
				}
			}
			var sb = SB.ToString();
			return sb;
		}
		public void Add(string s)
		{
			s=s.Trim();
			List<int> l = s.findString("=");
			List<int> l2 = s.findString("(");
			if (l.Count==1)
			{
				int pos = l.First();

				if (pos > 0 && s.AsSpan()[pos - 1] == ')' && l2.Count > 0 && l2.First() < pos - 1)
				{
					AddFunction(s);
				}
				else if (l2.Count == 0 || l2.First() > pos) 
				{
					AddVar(s);	
				}
			}
			else
				if(l.Count>1)
			{
				string[] strings=new string[l.Count];
				for(int i=0; i<strings.Length; i++)
				{
					if(i==l.Count-1)
					{
						strings[i] = s.Substring(0, l[1]);
					}
					else if(i==0)
					{
						strings[i] = s.Substring(l[l.Count - i - 2] + 1);
					}
					else
					{
						strings[i] = s.Substring(l[l.Count - i - 2] + 1, l[l.Count - i]);
					}
				
				}
				foreach(string ss in strings)
				{
					Add(ss);
				}
			}
		}
		public override void registerLG(LevelGetter getter)
		{

			getter.register('(', 1);
			getter.register(')',-1);
			getter.register('{', 1);
			getter.register('}', -1);
			getter.register('[', 1);
			getter.register(']', -1);

			toReplace.Add("}{", ("}{", "};{"));
			toReplace.Add(";}", (";}", ";}"));
			toReplace.Add("}", ("}", ";}"));
		}
		static MathObject CODEBLOCK(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult r)
		{
		int end = text.AsSpan().findEnd('{', '}');
			string block=text.Substring(0, end+1);
			List<int> pos = new List<int>();
			string[] ss= block.Substring(1,end).cutZeroLevel(';', manager.levelGetter,pos);
			for(int i=0; i<ss.Length;i++)
			{
				pos[i]++;
			}

			/*动态代码重新扫描
			List<OperatorScanInfo>[] scanInfos = new List<OperatorScanInfo>[ss.Length];
			for(int i=0;i<scanInfos.Length;i++)
			{
				scanInfos[i] = new List<OperatorScanInfo>();
			}
	
			foreach(var v in infos)
			{
				for(int i=pos.Count-1;i>=0;i--)
				{		
					
					 if (i == 0 )
					{
						if (v.position < pos[i])
						{
							v.position--;
							v.level--;
							scanInfos[i].Add(v);
						}
					}
					else if (v.position < pos[i] && v.position > pos[i-1])
					{
							v.position -= pos[i - 1] + 1;
							v.level--;
							scanInfos[i].Add(v);
						     break;		
					}
				}
			}
			*/

		

			return new RuntimeMathObject((e) =>
			{
				ScanSet[] ss_ = new ScanSet[ss.Length];
				MathObject[] mathObjects = new MathObject[ss.Length];

				List<OperatorScanInfo> sl;
				for (int i = 0; i < ss.Length; i++)
				{

					sl = new List<OperatorScanInfo>();
					var v= manager.ScanForOperators(ref ss[i], sl);
					ss_[i] = (ss[i], sl);
					mathObjects[i] = manager.GetObject(ss_[i].Item1, ss_[i].Item2,v);
				}
                for (int i = 0; i < ss.Length; i++)
                {
                }return new MathOperator((d) => { if(d.Length>0) return d[d.Length-1]; return -1; },mathObjects);
			});

		}

		static MathObject ELSE(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult r)
		{
			var v=OperatorInfo.solveLR(text, ois, infos, manager);

			MathObject M1 = manager.Run(v[0].name);
			MathObject M2 = manager.Run(v[1].name);
			return new RuntimeMathObject((b) => {

				return new Else(M1, M2,b);

			});
		}
		internal static MathObject DE(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			var v =OperatorInfo.solveL(text, ois, infos, manager);
			var mo = manager.GetObject(v.name, v.infos, result).getDerivative(0);
			return mo;
		}
		static MathObject IF(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
		

			var v = OperatorInfo.solveCodeBlock(text, ois, infos, manager);


			if (v.vars.Length > 0)
			{
			
			

				return new RuntimeMathObject((e) =>
				{
					MathObject[] objects = new MathObject[v.vars.Length];
					for (int i = 0; i < v.vars.Length; i++)
					{
						foreach (var vm in v.vars[i].infos)
							objects[i] = manager.Run(v.vars[i].name);
					}
					string nameCode = "";
					List<OperatorScanInfo> scanCode = new List<OperatorScanInfo>();
					if (v.code.HasValue)
					{
						nameCode = v.code.Value.name;

						result = manager.ScanForOperators(ref nameCode, scanCode);
						//vr = manager.Run(v.code.Value.name);
					}
					return new If(objects, manager.GetObject(nameCode,scanCode,result),e);
				});
			}

			return error;

		}



		static MathObject RETURN(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			
			var v=OperatorInfo.solveR(text, ois, infos, manager);

			return new RuntimeMathObject(
				(b) =>
				{

					var mo = manager.Run(v.name);
					if (mo is RuntimeMathObject)
					{
						mo.AsRuntime().setAfter(e => manager.returnObject(e.valueOut));
						return mo.AsRuntime();
					}



					return new MathOperator(mo.getValue);
				});

			return error;

		}
		static MathObject EQ(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			var v=OperatorInfo.solveLR(text, ois, infos, manager);

			
			if (v[0].name.isFuncName())
            {
		
				ReadOnlySpan<char> funcname = v[0].name.AsSpan();
				ReadOnlySpan<char> func = v[1].name.AsSpan();
		
				if (funcname != null && func != null)
				{
					List<string> names = new List<string>();
					int plpos = -1;
					int lastPos = -1;
					for (int i = 0; i < funcname.Length; i++)
					{
						if (plpos < 0)
						{
							if (funcname[i] == '(')
							{
								plpos = i;
								lastPos = i + 1;
								names.Add(funcname.Slice(0, i).ToString());
							}
						}
						else
						{
							if (funcname[i] == ','&& i - lastPos>0)
							{
								names.Add(funcname.Slice(lastPos, i - lastPos).ToString());
								lastPos = i + 1;
							}
							else if (funcname[i] == ')'&&i-lastPos>0)
							{
								names.Add(funcname.Slice(lastPos, i - lastPos).ToString());
								break;
							}
						}

					}
					int funSize = names.Count - 1;
					List<OperatorInfo> old = new List<OperatorInfo>();
					if (funSize >= 0)
					{
						for (int i = 0; i < funSize; i++)
						{
							var olds = manager.registerSource(names[i + 1], i, "tempindex"+i);
							if (olds != null)
							{
								old.Add(olds);
							}
						}
						//MathObject m = manager.Run(v[1].name);
						List<OperatorScanInfo> scanInfos = new List<OperatorScanInfo>();
					var r=	manager.ScanForOperators(ref v[1].name, scanInfos);
						if(r.containsType(OperatorType.RUNCODE))
						{

                      manager.registerFunc(names[0], (v[1].name,scanInfos),result, "runtime");

						}
						else
						{
							manager.registerMathFunc(names[0], manager.GetObject(v[1].name, scanInfos, r),funSize,"CMath");
						}
						
						manager.removeWithCondition((v) => { return v.tag.StartsWith("tempindex"); });
						foreach (OperatorInfo o in old)
						{
							manager.register(o);
						}
						return trueO;
					}


				}
				return falseO;
			}
			else if (v[0].name.isVarName())
			{
				double r = double.NaN;
				ReadOnlySpan<char> funcname = v[0].name.AsSpan();
				ReadOnlySpan<char> func = v[1].name.AsSpan();

				if (funcname != null && func != null)
				{
					MathObject m = manager.GetObject(v[1].name, v[1].infos,result);
					r = m.getValue();
					manager.registerNumber(v[0].name, r,"runtime");
				}
				return new NumberObject(r);
			}
			return trueO;
		}

		public MathObject AddFunction(string s, string tag = "customize")
		{
			s=s.Trim();
			ReadOnlySpan<char> l=s.AsSpan();
			ReadOnlySpan<char> funcname=null;
			ReadOnlySpan<char> func=null;
			for(int i=0;i<l.Length; i++)
			{
				if (l[i] =='=')
				{
					funcname=l.Slice(0,i);
					func=l.Slice(i+1);
					break;
				}
			}
			if(funcname!=null&&func!=null)
			{
	        List<string> names=new List<string>();
				int plpos = -1;
				int lastPos=-1;
			for (int i = 0; i < funcname.Length; i++)
			{
					if(plpos<0)
					{
	                 if (l[i] == '(')
				     {
					        plpos= i;
							lastPos = i +1;
						names.Add(funcname.Slice(0,i).ToString());
				     }
					}
					else
					{
						if (l[i]==',')
						{
							names.Add(funcname.Slice(lastPos, i - lastPos).ToString());
							lastPos = i + 1;
						}
						else if (l[i]==')')
						{
							names.Add(funcname.Slice(lastPos,i-lastPos).ToString());
							break;
						}
					}
			
			}
				int funSize = names.Count - 1;
				List<OperatorInfo> old = new List<OperatorInfo>();
				if(funSize>=0)
				{
					for (int i = 0;i<funSize;i++)
					{
						var olds= registerSource(names[i +1], i, "temp");
						if(olds!=null)
						{
							old.Add(olds);
						}
					}
					string ss=func.ToString();
					List<OperatorScanInfo> scan = new List<OperatorScanInfo>();
					var sr= ScanForOperators(ref ss, scan);
					registerFunc(names[0],(ss,scan),sr , tag);
					removeWithTag("temp");
					foreach(OperatorInfo o in old)
					{
						register(o);
					}
					return GetObject(ss,scan,sr);
				}


			}
			return error;

		}
		public double AddVar(string s, string tag = "customize")
		{
			s=s.Trim();
			double r = double.NaN;
			ReadOnlySpan<char> l=s.AsSpan();
			ReadOnlySpan<char> funcname=null;
			ReadOnlySpan<char> func=null;
			for(int i=0;i<l.Length; i++)
			{
				if (l[i] =='=')
				{
					funcname=l.Slice(0,i);
					func=l.Slice(i+1);
					break;
				}
			}
			if(funcname!=null&&func!=null)
			{
					MathObject m = Run(func.ToString());
				r = m.getValue();
					registerNumber(funcname.ToString(),r, tag);
			}
		
			return r;
		}

		OperatorInfo registerSource(string name,int index,string tag="temp")
		{

			return register( new OperatorInfo( name, OperatorType.Source,max,tag).setDot(index));
		}
		OperatorInfo registerNumber(string name,double value, string tag = "temp")
		{

			return register( new OperatorInfo(name, OperatorType.Number, max, tag).setValue(value));
		}



		public  (string s, List<OperatorScanInfo> r) fixForFunc((string s, List<OperatorScanInfo> r) r, (string s, List<OperatorScanInfo> r)[] v)
		{

			ReadOnlySpan<char> chars = r.s.AsSpan();
			string[] strings = new string[chars.Length];
			for (int i = 0; i < chars.Length; i++)
			{
				strings[i] = chars[i].ToString();
			}
			for (int i = 0; i < r.r.Count; i++)
			{
				if (r.r[i].tag.StartsWith("tempindex"))
				{
					int index = int.Parse(r.r[i].tag.Substring(9));
					if (index > -1 &&v!=null&& index < v.Length)
					{
						int pos = r.r[i].position;
						if (pos >= 0 && pos < strings.Length)
						{
						strings[pos] = v[index].s;
						}
					}
				}
			}
			SB.Clear();
			foreach (var s in strings)
			{
				SB.Append(s);
			}
			List<OperatorScanInfo> infos = new List<OperatorScanInfo>();
			string t = SB.ToString();
			ScanForOperators(ref t, infos);
			return (t, infos);
		}
		public void registerMathFunc(string name, MathObject mathObject,int pm, string tag = "customize")
		{ 
		register(new OperatorInfo(name, OperatorType.Func, max, tag, mathObject.getValue, (i, o, m) =>
		{
			MathObject[] ms = new MathObject[pm];
			for (int j = 0; j < pm; j++)
			{
				ms[j] = OP("*", mathObject.getDerivative(j).clone().replaceVarible(j, m[j]), m[j].getDerivative(i));
			}
			return OP("sum", ms);
		}, null));
		}

			public void registerFunc(string name,ScanSet mathObject,ScanResult sr,string tag="customize")
		{

			MathObject m = GetObject(mathObject.Item1, mathObject.Item2, sr);

			if (match(name) == null || canOver)
				register(new OperatorInfo(name, OperatorType.Func, max, tag, (double[] data) => (m.getValue(data)), null, (string code, OperatorScanInfo selected, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult sr) =>
				{
					var r = ObjectInfo<MathObject, OperatorInfo, MathObjectManager>.solveFunc(code, selected, infos, manager);
					int funcSize = r.Length;
					MathObject[] mathObjects = new MathObject[funcSize];
					for (int i = 0; i < funcSize; i++)
					{
						mathObjects[i] = manager.GetObject(r[i].name, r[i].infos, sr);
					}
					var rmo = new RuntimeMathObject((e) =>
					{

						addReturn(o => { e.setValueOut(o); });
						e.setAfter(manager => removeReturn());
						if (m is RuntimeMathObject)
						{

							for (int i = 0; i < mathObject.Item2.Count; i++)
							{
								var cc = mathObject.Item2[i];
							}
							var v = updatePms(mathObject, r, this);
							return Run(v.Item1);
						}
						return new MathOperator(selected.operatorInfo, mathObjects);
					}
							);

					return rmo;
				}));
		}

		static (string, List<OperatorScanInfo> infos)[] updatePms( (string, List<OperatorScanInfo>)[] r, (string, List<OperatorScanInfo>)[] vs,int funcSize,MathObjectManager m)
		{
			(string, List<OperatorScanInfo> infos)[] nr = new (string, List<OperatorScanInfo> infos)[funcSize];
			for (int i = 0; i < funcSize; i++)
			{
				nr[i] = m.fixForFunc( r[i],vs);
			}
			return nr;
		}
        static (string, List<OperatorScanInfo> infos) updatePms((string, List<OperatorScanInfo>) r, (string, List<OperatorScanInfo>)[] vs, MathObjectManager m)
        {
         
            return m.fixForFunc(r, vs);
        }
        public void regiseterMethod(string name ,MethodFactory method,SourceOperator op=null)
		{
			register(name, OperatorType.Func, max, op, (string code, OperatorScanInfo selected, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result) => {
				var r = OperatorInfo.solveFunc(code, selected, infos, manager);
				

				int funcSize = r.Length;
				var rtm= new RuntimeMathObject((e) => {

			
					addReturn(o => { e.setValueOut(o); });
				    var rr = method(this,r);
					returnObject(rr.Item1);
					removeReturn();
					return new MethodObject(rr.Item2, rr.Item1); });
				return rtm;
			
			},"Method");
		}

		public MathObject Run(string text,bool simplify=false,bool runtime=false)
		{

			
			if(runtime)
			{
				return new RuntimeMathObject((e) => Run(text, false));
			}
		   Helper.clean(ref text);
			
			
			List<OperatorScanInfo> info = new List<OperatorScanInfo>();
			var r = ScanForOperators(ref text, info);
	
			if(r.containsType(OperatorType.RUNCODE))
			{
				pushRunTime();
				var go = GetObject(text, info, r) ;
				if(simplify)
				{
					go = go.simplify(this);
				}
				popRunTime();
				return go;
			}
			var v = GetObject(text, info, r);
			if(simplify)
				v=v.simplify(this);
			return v;
		}

		public MathObject RunAndSimplify(string name,bool rt=false)
		{
			return Run(name, true, rt);
		}
	
		public override MathObject getBaseType(string s)
		{
			return new NumberObject(double.Parse(s));
		}
		public override MathObject getErrorType()
		{
			return error;
		}

		public override MathObject GetObject(string text, List<OperatorScanInfo> infos,ScanResult r)
		{
			

			return base.GetObject(text,infos,r);
		}

	}


	public static class Helper
	{
		static internal SourceOperator toSourceOperator(this OneElementOperator OperatorInfo)
		{

			return data => OperatorInfo(data[0]);
		}

		static internal Factory<MathObject,OperatorInfo,MathObjectManager> asFactory(this DerivativeGetter getter)
		{
			return OperatorInfo.getDerivableFunc(getter);
		}
		static internal SourceOperator toSourceOperator(this TwoElementsOperator OperatorInfo)
		{
			return data => OperatorInfo(data[0], data[1]);
		}
		static internal SourceOperator toSourceOperator(this ThreeElementsOperator OperatorInfo)
		{
			return data => OperatorInfo(data[0], data[1], data[2]);
		}
		static internal SourceOperator toSourceOperator(this FourElementsOperator OperatorInfo)
		{
			return data => OperatorInfo((double)data[0], (double)data[1], data[2], data[3]);
		}
		public static List<int>  findDot(string s,char dot=',')
		{

			List<int> ints = new List<int>();

			ReadOnlySpan<char> text_ = s.AsSpan();
			int leftParentheses = 0;
			int rightParentheses = 0;
			int level;
			for (int i = 0; i < text_.Length; i++)
			{
				if (text_[i] == '(')
				{
					leftParentheses++;
				}

				else if (text_[i] == ')')
				{
					rightParentheses++;
				}
				else if (text_[i] == dot)
				{
					level = leftParentheses - rightParentheses;

					if (level == 0)
					{
						ints.Add(i);
					}

				}
			}
			return ints;
		}

		public static int clean(ref string text)
		{
			text = text.Replace(" ", "").Replace("\n", "");
			text = text.Trim();
			int i = 0;//|| (text.StartsWith('{') && text.EndsWith("}")
			while ((text.StartsWith('(') && text.EndsWith(")")))
			{
				text = text.Substring(1, text.Length - 2);
				i++;
			}
			text = text.Trim();
			return i;
		}
		public static string[] cutZeroLevel(this string text, char knife, LevelGetter getter)
		{ 
		return cutZeroLevel(text,knife,getter,new List<int>());
		}
			public static string[] cutZeroLevel(this string text,char knife,LevelGetter getter,List<int> pos,int fp=-1)
		{
			List<string> list = new List<string>();
			int l = 0;
			char c;
			ReadOnlySpan<char> chars = text.AsSpan();
			int s = fp;
			for (int i = 0;i<text.Length;i++)
			{
				c = chars[i];
				if(c==knife&&l==0&&i-s>1)
				{
					list.Add(chars.Slice(s+1, i - s-1).ToString());
					pos.Add(i);
					s = i;
				}
				l += getter.getLevel(c);
			}
			return list.ToArray();
		}
		public static int findEnd(this ReadOnlySpan<char> chars,char start,char end, int startPos=0,List<(int start,int end)> list=null)
		{
			if(list == null)
			{
				list=new List<(int start, int end)> ();
			}
			for (int i = startPos+1;i<chars.Length;i++)
			{
				if (chars[i] == start)
				{
				
				  i=findEnd(chars,start,end,i,list);
				}
				else if (chars[i] == end)
				{
					list.Add((startPos, i));
					return i;
				}
			}
			return chars.Length;
		}


		public static int findFirstChar(this ReadOnlySpan<char> text, char c)
		{ 
		 for(int i = 0;i<text.Length;i++)
			{
				if (text[i] == c)
				{
					return i;
				}
			}
		 return -1;
		}
		public static List<int> findChar(this ReadOnlySpan<char> text, char c)
		{
			List<int> result = new List<int>();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == c)
				{
					result.Add(i);
				}
			}
			return result;
		}
		public static List<int> findString(this string text_, string toFind)
		{
			return findString(text_.AsSpan(), toFind);
		}
		public static bool isFuncName(this string fn)
		{
			var v1 = fn.findString("(");
			var v2= fn.findString(")");
			return v1.Count==1&& v2.Count==1&&v1.First()>0&&v2.First()>v1.First();
		}
		public static bool isVarName(this string fn)
		{
			var v1 = fn.findString("(");
			var v2 = fn.findString(")");
			return v1.Count == 0 && v2.Count == 0;
		}
		public static List<int> findString(this ReadOnlySpan<char> text_, string toFind)
		{
			List<int> ints = new List<int>();
			ReadOnlySpan<char> mark = toFind.AsSpan();
			int l = toFind.Length;
			int times = text_.Length - toFind.Length + 1;
			for (int i = 0; i < times; i++)
			{
				if (text_[i] == toFind[0] && mark.SequenceEqual(text_.Slice(i, l)))
				{
					ints.Add(i);
				}
			}
			return ints;
		}
		public static int findFirstString(this string text_, string toFind)
		{ 
		return findFirstString(text_.AsSpan(), toFind);
		}
			public static int findFirstString(this ReadOnlySpan<char> text_, string toFind)
		{
			ReadOnlySpan<char> mark = toFind.AsSpan();
			int l = toFind.Length;
			int times = text_.Length - toFind.Length + 1;
			for (int i = 0; i < times; i++)
			{
				if (text_[i] == toFind[0] && mark.SequenceEqual(text_.Slice(i, l)))
				{
					return i;
				}
			}
			return -1 ;
		}
	}

	public class OperatorInfo:ObjectInfo<MathObject,OperatorInfo,MathObjectManager>
	{

		internal SourceOperator Operator0 { get; private set; }
		internal bool Random { get; private set; } = false;
		internal DerivativeGetter DerivativeGetter { get; private set; }
		internal static OperatorInfo info=new OperatorInfo("Empty",OperatorType.Func,0);
		internal double value ;
		//internal int pmSize { get; private set; }
		public OperatorInfo()
		{

		}
		/*
		public OperatorInfo setPmSize(int s)
		{
			pmSize = s;
			return this;
		}*/
		public OperatorInfo(OperatorInfo other)
		{
			copyForm(other);
		}

		public OperatorInfo setRandom()
		{
			Random = true;
			return this;
		}
		public OperatorInfo(string mark, OperatorType type, int priority, string tag = "raw", SourceOperator so = null, DerivativeGetter d = null, ObjectFactory factory = null) : base()
	
		{
			this.mark = mark;
			this.type = type;
			Operator0=so;
			this.priority=priority;
			this.tag = tag;
			this.factory = factory;
			DerivativeGetter = d;
			if(this.factory==null)
			{
				this.factory = getDefault(this);
			}
		}
	
		public OperatorInfo(string mark, OperatorType type, int priority, OneElementOperator so , string tag = "raw", DerivativeGetter d = null, ObjectFactory factory = null) : this(mark, type, priority, tag, Helper.toSourceOperator(so), d, factory)
		{
			
		}
		public OperatorInfo setDot(char size)
		{
			dot = size;
			return this;
		}
		public OperatorInfo setDot(short size)
		{
			dot = (char)size;
			return this;
		}
		public OperatorInfo setDot(int size)
		{
			dot = (char)size;
			return this;
		}
		public OperatorInfo setValue(double size)
		{
			value = size;
			return this;
		}
		public OperatorInfo setReverse()
		{
			reverse = true;
			return this;
		}

		public override OperatorInfo copyForm(OperatorInfo operatorInfo)
		{
			this.mark = operatorInfo.mark;
			this.type = operatorInfo.type;
			this.priority = operatorInfo.priority;
			this.Operator0 = operatorInfo.Operator0;
			this.factory = operatorInfo.factory;
			this.tag = operatorInfo.tag;
			this.dot = operatorInfo.dot;
			this.reverse = operatorInfo.reverse;
			this.DerivativeGetter = operatorInfo.DerivativeGetter;
			return this;
		}

		public OperatorInfo(string mark, OperatorType type, int priority, TwoElementsOperator so , string tag = "raw", DerivativeGetter derivative = null, ObjectFactory factory = null) : this(mark, type, priority, tag, Helper.toSourceOperator(so), derivative, factory)

		{

		}
		public OperatorInfo(string mark, OperatorType type, int priority, ThreeElementsOperator so , string tag = "raw", DerivativeGetter derivative = null, ObjectFactory factory = null) : this(mark, type, priority, tag, Helper.toSourceOperator(so), derivative, factory)
		{

		}
		static MathObject LR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{

		

			var v=solveLR(text, ois, infos, manager);
			return new MathOperator( ois.operatorInfo,manager.GetObject(v[0].name, v[0].infos,r),
				manager.GetObject(v[1].name, v[1].infos, r)).setDerivativeGetter(ois.operatorInfo.DerivativeGetter);
		}

		static MathObject RunTimeR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{
			return new RuntimeMathObject((e) =>
			{
				var v = solveR(text, ois, infos, manager);

				var mo = manager.GetObject(v.name, v.infos,r);
			
				return new MathOperator(ois.operatorInfo,mo );

			});
		
		}

		internal	static MathObject RuntimeLR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult result)
		{
			var r = solveLR(text, ois, infos, manager);
			var a = manager.Run(r[0].name);
			var b = manager.Run(r[1].name);
		return	new RuntimeMathObject((e) =>
			{

				return  new MathOperator(ois.operatorInfo,a ,
				b);

			});

		}
static	MathObject R(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{
			var v=solveR(text, ois, infos, manager);
			return new MathOperator(ois.operatorInfo,manager. GetObject(v.name,v.infos,r)).setDerivativeGetter(ois.operatorInfo.DerivativeGetter);
		}
	static	MathObject L(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			var v=solveL(text, ois, infos, manager);
			return new MathOperator(ois.operatorInfo,manager. GetObject(v.name,v.infos,result)).setDerivativeGetter(ois.operatorInfo.DerivativeGetter);
		}


		static MathObject VAR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			return new VaribleObject(ois.operatorInfo);
		}
		static MathObject NUM(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			return new NumberObject(ois.operatorInfo.value);
		}
		static	MathObject RLOR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			if(ois.position==0)
			{
			return	R(text,ois,infos,manager,result);
			}
			else
			{
			return	LR(text,ois,infos,manager, result);
			}
		}
	static	MathObject Error(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			return MathObjectManager.error;
		}

	static	MathOperator Func(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult result)
		{
			var r=solveFunc(text, ois, infos, manager);
			int funcSize = r.Length;
			MathObject[] mathObjects = new MathObject[funcSize];			
			
			for (int i = 0; i < funcSize; i++)
			{
					mathObjects[i] = manager.GetObject(r[i].name, r[i].infos,result);
			}
			return new MathOperator(ois.operatorInfo, mathObjects

				).setDerivativeGetter(ois.operatorInfo.DerivativeGetter);
		}
			ObjectFactory getDefault(OperatorInfo operatorInfo)
		{
			if (operatorInfo.type == OperatorType.LeftRight )
			{
				return LR;
			}
			else if (operatorInfo.type == OperatorType.Right )
			{
				return R;
			}
			else if (operatorInfo.type == OperatorType.Left)
			{
				return L;
			}
			else if (operatorInfo.type == OperatorType.Func)
			{
				return Func;
			}
			else if (operatorInfo.type == OperatorType.LeftRightOrRight)
			{
				return RLOR;
			}
			else if (operatorInfo.type == OperatorType.Source)
			{
				return VAR;
			}
			else if (operatorInfo.type == OperatorType.Number)
			{
				return NUM;
			}

			return Error;


		}

		internal static ObjectFactory getDerivableFunc(DerivativeGetter getter)
		{
		return(a,b,c,d,e)=>(Func(a,b,c,d,e).setDerivativeGetter(getter));
		}
	}

	public abstract class MathObject
	{
		[NotNull]
		internal  OperatorInfo operatorInfo{get; private set;}
		public string mark { get { return operatorInfo.mark; } }
		public bool isRandom { get { return operatorInfo.Random; } }
		internal MathObject(OperatorInfo operatorInfo)
		{
			this.operatorInfo = operatorInfo;
		}
		class ERRORObject : MathObject
		{
			internal ERRORObject():base(null)
			{

			}
			public override double getValue(params double[] ms)
			{
				return double.NaN;
			}
		}
		internal static MathObject ERROR=new ERRORObject() ;
		internal bool isRuntime
		{
			get { return this is RuntimeMathObject; }
		}
		internal virtual MathObject clone()
		{
			return ERROR;
		}
		public virtual bool isSame(MathObject mathObject)
		{
			return mathObject == this;
		}

		public virtual MathObject getElement(int index = 0)
		{
			return this;
		}
		public virtual bool isNumber()
		{
			return false;
		}
		public virtual bool isNumber(double value)
		{
			return false;
		}

		public virtual string ToStringWithAddtional(string addtional="")
		{
			return ToString();
		}
		internal virtual MathObject replaceVarible(int index,MathObject m)
		{
			return this;
		}
		//internal int pmSize = 0;
		/*public  int getPmSize()
		{
			return pmSize;
		}
		*/

		/*
		internal MathObject setPmSize(int s)
		{
			pmSize = s;
			return this;
		}*/
		public virtual MathObject getDerivative(int index=0)
		{
			return ERROR;
		}

		public virtual MathObject getSimplify(MathObjectManager m)
		{
			return this;
		}
		public virtual MathObject simplify(MathObjectManager m)
		{
			return this;
		}
		public RuntimeMathObject AsRuntime()
		{
			if(this is RuntimeMathObject)
			{
				return (RuntimeMathObject)this;
			}
			return null;
		}
		public virtual RuntimeMathObject AsRuntimeOrWraped()
		{
			if (this is RuntimeMathObject)
			{
				return (RuntimeMathObject)this;
			}
			return new RuntimeMathObject((e) => { return this; });
		}
		public abstract double getValue(params double[] ms);
	}

	public class LinearMap : MathObject
	{
		List<Vector2> RawData = new List<Vector2>();
		int[] vector2sMap;
		double refData;
		int dData;
		bool created = false;
		int Size;
		public LinearMap( int size):base(null)
		{
			this.Size = size;
		}

		public override double getValue(params double[] ms)
		{
			if(RawData.Count<2)
			{
				return double.NaN;
			}
			if(!created)
			{
				CreateMap(Size);
			}
			double v = ms[0];
			int i = findIndexStart(v);
			int max = RawData.Count - 1;
			if(i<0)
			{
				return ip(v, RawData[0], RawData[1]);
			}
			else if(i<max)
			{
				Vector2 p1;
				Vector2 p2;
				for(int j=i;j<max;j++)
				{
					p1= RawData[j];
					p2= RawData[j+1];
					if (p1.X <= v && p2.X >v)
					{
						return ip(v,p1, p2);
					}
				}
			}

			return ip(v, RawData[RawData.Count - 1], RawData[RawData.Count - 2]);

		}

		public LinearMap Clear()
		{
			RawData.Clear();
			created = false;
			return this;
		}
		public LinearMap Add(params Vector2[] data )
		{
			foreach(Vector2 p in data)
			{
				RawData.Add(p);
			}
			created = false;
			return this;
		}

		double ip(double x,Vector2 p1,Vector2 p2)
		{
			return p1.Y + (x -p1.X) / (p2.X -p1.X) * (p2.Y -p1.Y);

		}
		public void CreateMap(int AreaSize)
		{
			if(RawData.Count<2)
				return;

			var data = (from Vector2 d in RawData orderby d.X ascending select d).ToArray();
			RawData.Clear();
			foreach(var d in data)
			{
				RawData.Add(d);
			}
			int count = RawData.Count / AreaSize;refData = RawData[0].X;
			dData =(int) ((RawData.Last().X -refData)/count);
			double min =refData+dData;
			
			List<int> list = new List<int>
			{
				0
			};
			for(int i=1;i<RawData.Count;i++)
			{
				if (RawData[i].X  >= min && RawData[i-1].X<=min)
				{
					list.Add(i-1);
					min += dData;
				}
			}
			vector2sMap = list.ToArray();
			created= true;
		}

		int findIndexStart(double x)
		{
			double i = ((x - refData) / dData);
			if(i>=0&&i<vector2sMap.Length)
			{
				return vector2sMap[(int)i];
			}
			else if(i<0)
			{
				return -1;
			}
			else
			{
				return RawData.Count;
			}
		}
	}

	public class RuntimeMathObject:MathObject
	{
		MathMaster MathMaster;
		public object valueOut { get;internal set; }

	
		AfterRun<RuntimeMathObject> afterRun;

		public void setAfter(AfterRun<RuntimeMathObject> afterRun)
		{
			this.afterRun = afterRun;
		}
		public RuntimeMathObject(MathMaster mathMaster):base(null)
		{
			this.MathMaster = mathMaster;
		}
		public void setValueOut(object value)
		{
			valueOut = value;
		}
    
		public override double getValue(params double[] ms)
		{
			var v =	MathMaster(this);
		
			var  vs=v.getValue(ms);	
			if(afterRun!=null)
			{
				afterRun(this);
			}
			return vs;
		}
	}
	public class If : MathObject
	{
		MathObject[] IF;
		MathObject Code;
		RuntimeMathObject shell;
		public If(MathObject[] IF, MathObject Code, RuntimeMathObject shell):base(null)
		{
			this.IF = IF;
			this.Code = Code;
			this.shell = shell;
		}
		public override double getValue(params double[] ms)
		{

			double d = -1;
			for(int i = 0;IF.Length > i;i++)
			{
			d = IF[i].getValue(ms);
			}
			if(d>0)
			{
				double d2=Code.getValue(ms);
				shell.setValueOut(true);
				return d2;
			}
			shell.setValueOut(false);
			return double.NaN;

		}
	}
	public class Else : MathObject
	{
		MathObject If;
		MathObject ELse;
		RuntimeMathObject shell;
		public Else(MathObject If, MathObject ELse,RuntimeMathObject shell):base(null)
		{
			this .If = If;
			this .ELse = ELse;
			this.shell = shell;
		}
		public override double getValue(params double[] ms)
		{

			double d= If.getValue(ms);
			bool c = true;
			shell.setValueOut(false);
			if(If.isRuntime&& If.AsRuntime().valueOut is bool)
			{
				bool b=(bool)If.AsRuntime().valueOut;
				if(b)
				{
					c = false;
					shell.setValueOut(true);
				}
				else
				{
					shell.setValueOut(false);
				}
			}
			if(c)
			{

			double d2= ELse.getValue(ms);
				shell.setValueOut(true);
				if (ELse.isRuntime&&ELse.AsRuntime().valueOut is bool)
				{
					bool b = (bool)ELse.AsRuntime().valueOut;
					if (b)
					{
						shell.setValueOut(true);
					}
					else
					{
						shell.setValueOut(false);
					}
				}
				return d2;
			}
			return d;

		}
	}

	public class MathOperator : MathObject
	{
		MathObject[] A;
		SourceOperator Operator;
		internal static DerivativeGetter defaultDerivative = delegate { return NumberObject.ZERO; };
		internal static MathOperator NAN;

		DerivativeGetter getter;
	
		static Dictionary<string,OperatorReplace> keyValuePairs=new Dictionary<string, OperatorReplace> ();

		static MathOperator()
		{
			NAN = new MathOperator(a => double.NaN);
			registerReplace("sum", (m, o, ms) =>
			{
				if(ms.Length==1)
				{
					return ms[0];
				}

				return o;

			});
			registerReplace("*", (m, o, ms) =>
			{
				var v1 = ms[0];
				var v2 = ms[1];


				bool i01 = v1.isNumber(0);
				bool i02 = v2.isNumber(0);
				if (i01 || i02)
				{
					return NumberObject.ZERO;
				}
				
				if(v1.isNumber(1))
				{
					return v2;
				}
				if(v2.isNumber(1))
				{
					return v1;
				}


				return o;

			});
			registerReplace("+", (m,o, ms) =>
			{
				var v1 = ms[0];
				var v2 = ms[1];

				if(v1.isNumber()&&v2.isNumber())
				{
					
						return new NumberObject(v1.getValue() + v2.getValue());
					
				}
				bool i01 = v1.isNumber(0);
				bool i02 = v2.isNumber(0);
				if (i01 && i02)
				{
					return NumberObject.ZERO;
				}
				else if (i01)
				{
					return v2;
				}
				else if (i02)
				{
					return v1;
				}



				return o;

			});
			registerReplace("-", (m,o, ms) =>
			{
				if(ms.Length==2)
				{
                var v1 = ms[0];
				var v2 = ms[1];
                 if(v1.isSame(v2))
				{
					return NumberObject.ZERO;
				}
				if (v1.isNumber() && v2.isNumber())
				{

					return new NumberObject(v1.getValue() - v2.getValue());

				}
				bool i01 = v1.isNumber(0);
				bool i02 = v2.isNumber(0);
				if (i01 && i02)
				{
					return NumberObject.ZERO;
				}
				else if (i01)
				{
					return m.OP("-",v2);
				}
				else if (i02)
				{
					return v1;
				}
				}
				else
				{
					var v=ms[0];
					if(v.isNumber(0))
					{
						return NumberObject.ZERO;
					}
				}
				return o;

			});
		}
		static void registerReplace(string name,OperatorReplace replace)
		{
			keyValuePairs.Add(name, replace);
		}
		public MathOperator(OperatorInfo info, params MathObject[] a) : base(info)
		{
			A = a;
			Operator = info.Operator0;
			getter = info.DerivativeGetter;
		}
		
	
		private MathOperator(SourceOperator s, OperatorInfo i, MathObject[] m):base(i)
			{
			A = m;
			Operator = s;
			
			}
		public MathOperator(SourceOperator op, params MathObject[] a) : base(OperatorInfo.info)
		{
			A = a;
			Operator = op;
		}

		public override MathObject getElement(int index = 0)
		{
			return A[index];
		}
		internal override MathObject clone()
		{
			MathOperator op = new MathOperator(Operator,operatorInfo, new MathObject[A.Length]);
			op.setDerivativeGetter(getter);

			for (int i = 0; i < A.Length; i++)
			{
				op.A[i] = A[i].clone();	
			}

			return op;
		}

		public override bool isSame(MathObject mathObject)
		{
			if(mathObject is MathOperator)
			{
				if (mathObject is MathOperator && mark == mathObject.mark)
				{
					bool flag = true;
					var mo = (MathOperator)mathObject;
					if (mo.A.Length == A.Length)
					{
						for (int i = 0; i < A.Length; i++)
						{
							if (!A[i].isSame(mo.A[i]))
							{
								flag = false;
								break;
							}
						}
						return flag;
					}
				}
			}

			return false;
		}
		public override MathObject simplify(MathObjectManager m)
		{
			MathOperator v = new MathOperator(operatorInfo,new MathObject[A.Length] );	
			for(int i = 0;i<A.Length;i++)
			{
				v.A[i]=A[i].simplify(m);
			}
			var v2 = v.getSimplify(m);
			return v2;
		}
		public override MathObject getSimplify(MathObjectManager m)
		{

			double[] doubles = new double[A.Length];
			bool allIsNumber = !isRandom;
			for(int i = 0;i<A.Length;i++)
			{
				if (A[i].isNumber())
				{
					doubles[i] = A[i].getValue();
				}
				else
				{
					allIsNumber=false;
					break;
				}
			}
			if(allIsNumber&&A.Length>0)
			{
				return new NumberObject(Operator(doubles));
			}

			if(keyValuePairs.TryGetValue(operatorInfo.mark,out var v))
			{
				return v(m,this,A);
			}
			return base.getSimplify(m);
		}
		public override string ToString()
		{
			return ToStringWithAddtional();
		}
		public override string ToStringWithAddtional(string addtional="")
		{
		
			var t = operatorInfo.type;
			var mark=operatorInfo.mark+addtional;
			if(operatorInfo.mark=="'")
			{
				char[] s = new char[(int)A[1].getValue()];
				s.AsSpan().Fill('\'');

				return A[0].ToStringWithAddtional(new string(s));
			}

			StringBuilder stringBuilder = new StringBuilder();
			if(t==OperatorType.Func)
			{
				stringBuilder.Append(mark);
				stringBuilder.Append("(");
				for(int i = 0;i < A.Length;i++)
				{
					stringBuilder.Append(A[i].ToString());
					if (i<A.Length-1)
					{
						stringBuilder.Append(",");
					}
				}
				stringBuilder.Append(")");
			}else
			if(t==OperatorType.Left&&A.Length>0)
			{
				stringBuilder.Append("(");
				stringBuilder.Append(A[0].ToString());
				stringBuilder.Append(mark);
				stringBuilder.Append(")");
			}
			else if (t == OperatorType.Right && A.Length > 0)
			{
				stringBuilder.Append("(");
				stringBuilder.Append(mark);	
				stringBuilder.Append(A[0].ToString());
				stringBuilder.Append(")");
			}
			else if(t==OperatorType.LeftRight)
			{
				if(A.Length>1) {
					stringBuilder.Append("(");
					stringBuilder.Append(A[0].ToString());
					stringBuilder.Append(mark);
					stringBuilder.Append(A[1].ToString());
					stringBuilder.Append(")");
				}
			}
			else if (t == OperatorType.LeftRightOrRight)
			{
				if (A.Length > 1)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(A[0].ToString());
					stringBuilder.Append(mark);
					stringBuilder.Append(A[1].ToString());
					stringBuilder.Append(")");
				}
				else if(A.Length==1)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(mark);
					stringBuilder.Append(A[0].ToString());
					stringBuilder.Append(")");
				}
			}
			else
			{
			}

			return stringBuilder.ToString();
		}
		internal override MathObject replaceVarible(int index, MathObject m)
		{

			for(int i=0;i<A.Length;i++)
			{
                if (A[i] is VaribleObject)
				{
					bool r=((VaribleObject)A[i]).index==index;
					if(r)
					{
						A[i] = m;
					}
				}
				else 
                {
                   A[i].replaceVarible(index, m); 
                }
            }

			return this;
		}
		public MathOperator setDerivativeGetter(DerivativeGetter getter)
		{
			if(getter==null)
			{
				return this;
			}
			this.getter = getter;
			return this;
		}
		public override MathObject getDerivative(int index = 0)
		{
			if(getter!=null)
			return getter(index,this, A);
			return base.getDerivative(index);
		}

		double[] getValues(double[] input)
		{
			double[] doubles= new double[A.Length];
			for (int i = 0; i < A.Length; i++)
			{
				doubles[i] = A[i].getValue(input);
			}
			return doubles;
		}
		public override double getValue(params double[] inputValues)
		{
			if(Operator is null)
			{
				return double.NaN;
			}
			if (A is null )
			{
				return  Operator(inputValues) ;
			}

			return  Operator(getValues(inputValues)) ;
		}
	}
	public class VaribleObject : MathObject
	{



		internal int index;



		public VaribleObject(OperatorInfo index):base(index)
		{
			this.index = index.dot;
		}

		public override MathObject getDerivative(int index = 0)
		{
			if (index == this.index)
			return new NumberObject(1);
			return new NumberObject(0);
		}
		internal override MathObject clone()
		{
			return this;
		}
		public VaribleObject setIndex(int i)
		{
			index = i;
			return this ;
		}
		public override bool isSame(MathObject mathObject)
		{
			if(mathObject is VaribleObject)
			{
				return index == ((VaribleObject)mathObject).index;
			}
			return false;
		}
		public override string ToString()
		{
			return operatorInfo.mark;
		}

		public override double getValue(params double[] inputValues)
		{
			return inputValues[index] ;
		}
	}
	public class MethodObject : MathObject
	{
		SourceOperator operator1;
		public object value{get;internal set;}
		public MethodObject(SourceOperator one,object v) : base(OperatorInfo.info)
		{
			value = v;
			operator1 = one; ;
		}

		public override double getValue(params double[] inputValues)
		{
			return operator1(inputValues);
		}
	}

	
	public class NumberObject:MathObject
	{
		double value;
		public static readonly NumberObject ZERO = new NumberObject(0);
		public static readonly NumberObject ONE = new NumberObject(1);
		public static readonly NumberObject HALF = new NumberObject(0.5);
		public static readonly NumberObject TWO= new NumberObject(2);
		public static readonly NumberObject E = new NumberObject(Math.E);
		public static readonly NumberObject PI = new NumberObject(Math.PI);

		public NumberObject(OperatorInfo info):base(info)
		{
			this.value = info.value;
		}
		public NumberObject(double v) : base(OperatorInfo.info)
		{
			this.value = v;
		}
		public override MathObject getDerivative(int index = 0)
		{
			return new NumberObject(0);
		}
		public override bool isSame(MathObject mathObject)
		{
			if(mathObject is NumberObject)
			{
				return ((NumberObject)mathObject).value == value;
			}
			return false;
		}
		public override bool isNumber()
		{
			return true;
		}

		public override bool isNumber(double value)
		{
			return value==this.value;
		}
		internal override MathObject clone()
		{
			return this;
		}
		public override double getValue(params double[] inputValues)
		{
			return value ;
		}
		
		public override string ToString()
		{
			return value.ToString();
		}
	}


}
