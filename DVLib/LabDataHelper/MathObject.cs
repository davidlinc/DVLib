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
	public delegate MathObject DerivativeGetter(int index,params MathObject[] objects);

   



    public class MathObjectManager:ObjectManager<MathObject,OperatorInfo,MathObjectManager>
	{
		 int max = 8;
		Random random = new Random();
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
			
			register(new OperatorInfo("+", OperatorType.LeftRight, max-4, Helper.toSourceOperator((a, b) => { return a + b; }), Helper.asFactory((i,m) => { return new Operator(Helper.toSourceOperator((x, y) => x + y), m[0].getDYDX(i), m[1].getDYDX(i)); }) ));

			register(new OperatorInfo("==", OperatorType.LeftRight, max - 5, Helper.toSourceOperator((a, b) => { return a==b?1:0; })));

			register(new OperatorInfo(">=", OperatorType.LeftRight, max - 5, Helper.toSourceOperator((a, b) => { return a >= b ? 1 : 0; })));

			register(new OperatorInfo("<=", OperatorType.LeftRight, max - 5, Helper.toSourceOperator((a, b) => { return a <= b ? 1 : 0; })));

			register(new OperatorInfo(">", OperatorType.LeftRight, max - 5, Helper.toSourceOperator((a, b) => { return a > b ? 1 : 0; })));

			register(new OperatorInfo("<", OperatorType.LeftRight, max - 5, Helper.toSourceOperator((a, b) => { return a < b ? 1 : 0; })));

			register(new OperatorInfo("&&", OperatorType.LeftRight, max - 6, Helper.toSourceOperator((a, b) => { return (b>0&&a>0) ? 1 : 0; })));

			register(new OperatorInfo("||", OperatorType.LeftRight, max - 6, Helper.toSourceOperator((a, b) => { return (b > 0 || a > 0) ? 1 : 0; })));

			register(";", OperatorType.RUNCODE, 0, Helper.toSourceOperator((a, b) => { return 0; }),OperatorInfo.RuntimeLR);
			register(new OperatorInfo("=", OperatorType.LeftRight, 1, Helper.toSourceOperator((a, b) => { return 0; }),EQ	
				).setReverse());
			register(new OperatorInfo("-", OperatorType.LeftRightOrRight, max-3,
				(double[] data) => { if (data.Length == 1) return -data[0]; return data[0] - data[1]; }
				)); 
			register(new OperatorInfo("--", OperatorType.LeftRightOrRight, max - 3,
				(double[] data) => { if (data.Length == 1) return data[0]; return data[0] +data[1]; }
				));
			register(new OperatorInfo("if", OperatorType.RUNCODE, max ,error.getValue,IF));
			register(new OperatorInfo("else", OperatorType.RUNCODE, max-1, error.getValue, ELSE));
			register(new OperatorInfo("return", OperatorType.RUNCODE, 2, error.getValue, RETURN));
			register(new OperatorInfo("{", OperatorType.RUNCODE, max+1, error.getValue, CODEBLOCK));

			register(new OperatorInfo("/", OperatorType.LeftRight, max-2, (a, b) => { return a / b; }));
			register(new OperatorInfo("*", OperatorType.LeftRight, max-2, (a, b) => { return a * b; }));
			register(new OperatorInfo("%", OperatorType.LeftRight, max-2, (a, b) => { return a % b; }));
			register(new OperatorInfo("^", OperatorType.LeftRight, max-1, Math.Pow,  Helper.asFactory((i, m) => { return new Operator(Helper.toSourceOperator((x, y,x_,y_) => x + y), m[0], m[1], m[0].getDYDX(i), m[1].getDYDX(i)); })));
			register(new OperatorInfo("/-", OperatorType.LeftRight, max-2, (a, b) => { return a / -b; }));
			register(new OperatorInfo("*-", OperatorType.LeftRight, max-2, (a, b) => { return a * -b; }));
			register(new OperatorInfo("%-", OperatorType.LeftRight, max-2, (a, b) => { return a % -b; }));
			register(new OperatorInfo("^-", OperatorType.LeftRight, max-1, (a, b) => Math.Pow(a, -b)));
			register(new OperatorInfo("IF", OperatorType.Func, max, (a, b,c) => { if (a > 0) return b; return c; }));
			register(new OperatorInfo("sin", OperatorType.Func, max, Math.Sin));
			register(new OperatorInfo("cos", OperatorType.Func, max, Math.Cos));
			register(new OperatorInfo("tan", OperatorType.Func, max, Math.Tan));
			register(new OperatorInfo("arcsin", OperatorType.Func, max, Math.Asin));
			register(new OperatorInfo("arccos", OperatorType.Func, max, Math.Acos));
			register(new OperatorInfo("arctan", OperatorType.Func, max, Math.Atan));
			register(new OperatorInfo("arccosh", OperatorType.Func, max, Math.Acosh));
			register(new OperatorInfo("arcsinh", OperatorType.Func, max, Math.Asinh));
			register(new OperatorInfo("arctanh", OperatorType.Func, max, Math.Atanh));
			register(new OperatorInfo("arctan2", OperatorType.Func, max, Math.Atan2));
			register(new OperatorInfo("asin", OperatorType.Func, max, Math.Asin));
			register(new OperatorInfo("acos", OperatorType.Func, max, Math.Acos));
			register(new OperatorInfo("atan", OperatorType.Func, max, Math.Atan));
			register(new OperatorInfo("abs", OperatorType.Func, max, Math.Abs));
			register(new OperatorInfo("acosh", OperatorType.Func, max, Math.Acosh));
			register(new OperatorInfo("asinh", OperatorType.Func, max, Math.Asinh));
			register(new OperatorInfo("atanh", OperatorType.Func, max, Math.Atanh));
			register(new OperatorInfo("atan2", OperatorType.Func, max, Math.Atan2));
			register(new OperatorInfo("sinh", OperatorType.Func, max, Math.Sinh));
			register(new OperatorInfo("cosh", OperatorType.Func, max, Math.Cosh));
			register(new OperatorInfo("tanh", OperatorType.Func, max, Math.Tanh));
			register(new OperatorInfo("exp", OperatorType.Func, max, Math.Exp));
			register(new OperatorInfo("round", OperatorType.Func, max, Math.Round));
			register(new OperatorInfo("max", OperatorType.Func, max, Math.Max));
			register(new OperatorInfo("min", OperatorType.Func, max, Math.Min));
			register(new OperatorInfo("random", OperatorType.Source, max, (double[] data) => random.NextDouble()));
			register(new OperatorInfo("randInt", OperatorType.Func, max, (a, b) => random.Next((int)Math.Min(a, b), (int)Math.Max(a, b))));
			register(new OperatorInfo("pi", OperatorType.Source, max, (double[] data) => Math.PI));
			register(new OperatorInfo("rad", OperatorType.Func, max, (a) => { return a * Math.PI / 180; }));
			register(new OperatorInfo("degree", OperatorType.Func, max, (a) => { return a * 180 / Math.PI; }));
			register(new OperatorInfo("x", OperatorType.Source, max, data => data[0]));
			register(new OperatorInfo("y", OperatorType.Source, max, data => data[1]));
			register(new OperatorInfo("z", OperatorType.Source,max, data => data[2]));
		}
		
		OperatorInfo register(string mark, OperatorType type, int priority, SourceOperator so,ObjectFactory factory=null , string tag = "raw")
		{
			return register(new OperatorInfo(mark, type, priority, so, factory,tag));
		}
	
		OperatorInfo register(string mark, OperatorType type, int priority,OneElementOperator so, ObjectFactory factory = null, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so,factory, tag));
		}
		OperatorInfo register(string mark, OperatorType type, int priority, TwoElementsOperator so, ObjectFactory factory = null, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so,factory, tag));
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
                }return new Operator((d) => { if(d.Length>0) return d[d.Length-1]; return -1; }, mathObjects);
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



					return new Operator(mo.getValue);
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
							manager.registerMathFunc(names[0], manager.GetObject(v[1].name, scanInfos, r));
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
					manager.registerSource(v[0].name, r,"runtime");
				}
				return new SourceObject((a) => r);
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
					registerSource(funcname.ToString(),r, tag);
			}
		
			return r;
		}

		OperatorInfo registerSource(string name,int index,string tag="temp")
		{

			return register(name, OperatorType.Source,max, (double [] data) => {
				if(data.Length>index) { return data[index];}return double.NaN;
			}, null,tag);
		}
		OperatorInfo registerSource(string name,double value, string tag = "temp")
		{

			return register(name, OperatorType.Source, max, (double[] data) => {
				return value;
			}, null,tag);
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
		public void registerMathFunc(string name, MathObject mathObject, string tag = "customize")
		{ 
		register(new OperatorInfo(name,OperatorType.Func,max,mathObject.getValue,null,tag));
		}

			public void registerFunc(string name,ScanSet mathObject,ScanResult sr,string tag="customize")
		{

			MathObject m = GetObject(mathObject.Item1, mathObject.Item2, sr);

			if (match(name) == null || canOver)
				register(new OperatorInfo(name, OperatorType.Func, max, (double[] data) => (m.getValue(data)), (string code, OperatorScanInfo selected, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult sr) =>
				{
					var r = ObjectInfo<MathObject, OperatorInfo, MathObjectManager>.solveFunc(code, selected, infos, manager);
					int funcSize = r.Length;
					MathObject[] mathObjects = new MathObject[funcSize];
					for (int i = 0; i < funcSize; i++)
					{
						mathObjects[i] = manager.GetObject(r[i].name, r[i].infos,sr);
					}
					var rmo = new RuntimeMathObject((e) =>
					{
						
						addReturn(o => { e.setValueOut(o);});
						e.setAfter(manager => removeReturn());
						if (m is RuntimeMathObject)
						{

							for(int i = 0;i <mathObject.Item2.Count;i++)
							{
								var cc = mathObject.Item2[i];
							}
							var v=updatePms(mathObject, r, this);
							return Run(v.Item1);
						}
						return new Operator(selected.operatorInfo.Operator0, mathObjects);
					}
							);

					return rmo;
				}, tag));
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

		public MathObject Run(string text,bool runtime=false)
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
				var go = GetObject(text, info,r);
				popRunTime();
				return go;
			}

			return GetObject(text, info,r);
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

		public OperatorInfo()
		{

		}
		public OperatorInfo(OperatorInfo other)
		{
			copyForm(other);
		}
		public OperatorInfo(string mark, OperatorType type,int priority,SourceOperator so,ObjectFactory factory=null,string tag="raw"):base()
	
		{
			this.mark = mark;
			this.type = type;
			Operator0=so;
			this.priority=priority;
			this.tag = tag;
			this.factory = factory;
			if(this.factory==null)
			{
				this.factory = getDefault(this);
			}
		}
	
		public OperatorInfo(string mark, OperatorType type, int priority, OneElementOperator so, ObjectFactory factory = null, string tag = "raw") : this(mark, type, priority,Helper.toSourceOperator(so),factory, tag)
	    {
			
		}
		public OperatorInfo setDot(char size)
		{
			dot = size;
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
			return this;
		}

		public OperatorInfo(string mark, OperatorType type, int priority,TwoElementsOperator so, ObjectFactory factory = null, string tag = "raw") : this(mark, type, priority, Helper.toSourceOperator(so), factory, tag)

		{
		}
		public OperatorInfo(string mark, OperatorType type, int priority, ThreeElementsOperator so, ObjectFactory factory = null, string tag = "raw") : this(mark, type, priority, Helper.toSourceOperator(so), factory, tag)

		{
		}
		static MathObject LR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{

		

			var v=solveLR(text, ois, infos, manager);
			return new Operator(ois.operatorInfo.Operator0, manager.GetObject(v[0].name, v[0].infos,r),
				manager.GetObject(v[1].name, v[1].infos, r));
		}

		static MathObject RunTimeR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{
			return new RuntimeMathObject((e) =>
			{
				var v = solveR(text, ois, infos, manager);

				var mo = manager.GetObject(v.name, v.infos,r);
			
				return new Operator(ois.operatorInfo.Operator0,mo );

			});
		
		}

		internal	static MathObject RuntimeLR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult result)
		{
			var r = solveLR(text, ois, infos, manager);
			var a = manager.Run(r[0].name);
			var b = manager.Run(r[1].name);
		return	new RuntimeMathObject((e) =>
			{

				return  new Operator(ois.operatorInfo.Operator0,a ,
				b);

			});

		}
static	MathObject R(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult r)
		{
			var v=solveR(text, ois, infos, manager);
			return new Operator(ois.operatorInfo.Operator0,manager. GetObject(v.name,v.infos,r));
		}
	static	MathObject L(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager, ScanResult result)
		{
			var v=solveL(text, ois, infos, manager);
			return new Operator(ois.operatorInfo.Operator0,manager. GetObject(v.name,v.infos,result));
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

	static	Operator Func(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager,ScanResult result)
		{
			var r=solveFunc(text, ois, infos, manager);
			int funcSize = r.Length;
			MathObject[] mathObjects = new MathObject[funcSize];			
			
			for (int i = 0; i < funcSize; i++)
			{
					mathObjects[i] = manager.GetObject(r[i].name, r[i].infos,result);
			}
			return new Operator(ois.operatorInfo.Operator0, mathObjects

				);
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
				return (a,b,c,d,r)=> { int index = -1; if (b.operatorInfo.tag.StartsWith("tempindex")){ index = int.Parse(b.operatorInfo.tag.Substring(9)); }; return new SourceObject(operatorInfo.Operator0).setIndex(index); };
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
		class ERRORObject : MathObject
		{
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

		public virtual MathObject getDYDX(int index=0)
		{
			return ERROR;
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
		public virtual MathObject AsRunCodeMode()
		{
			return this;
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
		public LinearMap( int size)
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
		public RuntimeMathObject(MathMaster mathMaster)
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
		public If(MathObject[] IF, MathObject Code, RuntimeMathObject shell)
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
		public Else(MathObject If, MathObject ELse,RuntimeMathObject shell)
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

	public class Operator : MathObject
	{
		MathObject[] A;
		SourceOperator Operator1;
		
		static DerivativeGetter defaultDerivative = delegate{ return MathObject.ERROR; };

		DerivativeGetter getter;

		
		public Operator(SourceOperator op,params MathObject[] a)
		{
			A = a;
			Operator1 = op;
		}
		public Operator setDerivativeGetter(DerivativeGetter getter)
		{
			this.getter = getter;
			return this;
		}
		public override MathObject getDYDX(int index = 0)
		{
			return base.getDYDX(index);
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
			if(Operator1 is null)
			{
				return double.NaN;
			}
			if (A is null )
			{
				return  Operator1(inputValues) ;
			}

			return  Operator1(getValues(inputValues)) ;
		}
	}
	public class SourceObject : MathObject
	{
		SourceOperator operator1;
		internal int index = -1;
		public SourceObject(SourceOperator one)
		{
			operator1 = one; ;
		}
		public SourceObject setIndex(int i)
		{
			index = i;
			return this ;
		}
		public override double getValue(params double[] inputValues)
		{
			return operator1(inputValues) ;
		}
	}
	public class MethodObject : MathObject
	{
		SourceOperator operator1;
		public object value{get;internal set;}
		public MethodObject(SourceOperator one,object v)
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

		public NumberObject(double value)
		{
			this.value = value;
		}

		public override double getValue(params double[] inputValues)
		{
			return value ;
		}
	}


}
