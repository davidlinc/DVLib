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
namespace DVLib.LabDataHelper
{

	
	public delegate double TwoElementsOperator(double a,double b);
	public delegate double OneElementOperator(double a);
	public delegate double SourceOperator(params double[] doubles);
	public delegate MathObject MathMaster(MathObjectManager manager, params (string, List<OperatorScanInfo>)[] vars );
	public delegate (object,SourceOperator) MethodFactory(MathObjectManager manager, params (string, List<OperatorScanInfo>)[] vars);

	public enum OperatorType
	{
		LeftRight,Left,Right,Func,Source,LeftRightOrRight
	}



	public class MathObjectManager:ObjectManager<MathObject,OperatorInfo,MathObjectManager>
	{
		 int max = 6;
		int maxPriority = 0;
		Random random = new Random();

		internal static MathObject error = new NumberObject(double.NaN);
		internal static MathObject trueO = new NumberObject(1);
		internal static MathObject falseO = new NumberObject(0);
		internal static MathObject nullO = new NumberObject(-1);
	bool canOver = true;

		public MathObjectManager()
		{
			registerDefault();
		}

		void registerDefault()
		{
			register(new OperatorInfo("+", OperatorType.LeftRight, max-4, Helper.toSourceOperator((a, b) => { return a + b; })));
			register(";", OperatorType.LeftRight, 0, Helper.toSourceOperator((a, b) => { return 0; }),OperatorInfo.RuntimeLR);
			register(new OperatorInfo("=", OperatorType.LeftRight, 1, Helper.toSourceOperator((a, b) => { return 0; }),EQ	
				).setReverse());
			register(new OperatorInfo("-", OperatorType.LeftRightOrRight, max-3,
				(double[] data) => { if (data.Length == 1) return -data[0]; return data[0] - data[1]; }
				));
			register(new OperatorInfo("/", OperatorType.LeftRight, max-2, (a, b) => { return a / b; }));
			register(new OperatorInfo("*", OperatorType.LeftRight, max-2, (a, b) => { return a * b; }));
			register(new OperatorInfo("%", OperatorType.LeftRight, max-2, (a, b) => { return a % b; }));
			register(new OperatorInfo("^", OperatorType.LeftRight, max-1, Math.Pow));
			register(new OperatorInfo("/-", OperatorType.LeftRight, max-2, (a, b) => { return a / -b; }));
			register(new OperatorInfo("*-", OperatorType.LeftRight, max-2, (a, b) => { return a * -b; }));
			register(new OperatorInfo("%-", OperatorType.LeftRight, max-2, (a, b) => { return a % -b; }));
			register(new OperatorInfo("^-", OperatorType.LeftRight, max-1, (a, b) => Math.Pow(a, -b)));
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

		static MathObject EQ(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
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
						MathObject m = manager.Run(v[1].name);

						manager.registerFunc(names[0], m, "runtime");
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
					MathObject m = manager.GetMathObject(v[1].name, v[1].infos);
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
					MathObject m = Run(func.ToString());
					registerFunc(names[0],m , tag);
					removeWithTag("temp");
					foreach(OperatorInfo o in old)
					{
						register(o);
					}
					return m;
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
		public void registerFunc(string name,MathObject mathObject,string tag="customize")
		{
			if(match(name)==null||canOver)

			register(new OperatorInfo(name, OperatorType.Func, max,(double[] data)=> ( mathObject.getValue(data) ), (string code, OperatorScanInfo selected, List<OperatorScanInfo> infos, MathObjectManager manager) => {
				var r = OperatorInfo.solveFunc(code, selected, infos, manager);
				int funcSize = r.Length;
				MathObject[] mathObjects = new MathObject[funcSize];
				int id = 0;
				for (int i = 0; i < funcSize; i++)
				{
					mathObjects[i] = manager.GetMathObject(r[i].name, r[i].infos);
				}
			return new RuntimeMathObject((m,vs) => {

					(string, List<OperatorScanInfo> infos)[] nr = new (string, List<OperatorScanInfo> infos)[funcSize];
					for(int i = 0;i < funcSize;i++)
					{
						nr[i] = r[i];	
						if(mathObjects[i] is SourceObject )
						{
							int ind = ((SourceObject)mathObjects[i]).index;
							if(ind>=0&&vs!=null&&ind<vs.Length)
							{
								nr[i] = vs[ind];
							}
						}
					}
					

				if(mathObject is RuntimeMathObject)
				{
					((RuntimeMathObject)mathObject).setValueIn(m,nr);
					return mathObject;
				}
				return new Operator(selected.operatorInfo.Operator0, mathObjects);
			}

					).setValueIn(this, r);


			}, tag));
		}

		public void regiseterMethod(string name ,MethodFactory method,SourceOperator op=null)
		{
			register(name, OperatorType.Func, max, op, (string code, OperatorScanInfo selected, List<OperatorScanInfo> infos, MathObjectManager manager) => {
				var r = OperatorInfo.solveFunc(code, selected, infos, manager);
				int funcSize = r.Length;
				MathObject[] mathObjects = new MathObject[funcSize];
				int id = 0;
				for (int i = 0; i < funcSize; i++)
				{
					mathObjects[i] = manager.GetMathObject(r[i].name, r[i].infos);
				}

				return new RuntimeMathObject((m,vs) => {

					(string, List<OperatorScanInfo> infos)[] nr = new (string, List<OperatorScanInfo> infos)[funcSize];
					for(int i = 0;i < funcSize;i++)
					{
						nr[i] = r[i];	
						if(mathObjects[i] is SourceObject )
						{
							int ind = ((SourceObject)mathObjects[i]).index;
							if(ind>=0&&vs!=null&&ind<vs.Length)
							{
								nr[i] = vs[ind];
							}
						}
					}
					

					
				var rr = method(this, nr); 
					return new MethodObject(rr.Item2, rr.Item1); });
				
			
			},"Method");
		}

		public MathObject Run(string text,bool runtime=false)
		{
			
			if(runtime)
			{
				return new RuntimeMathObject((a,b) => Run(text, false));
			}
		   Helper.clean(ref text);
			
			
			List<OperatorScanInfo> info = new List<OperatorScanInfo>();
			
			ScanForOperators( text, info);
			return GetMathObject(text, info);

		}
		public MathObject GetMathObject(string text,List<OperatorScanInfo> infos) 
		{
			int t = Helper.clean(ref text);
			



			List<OperatorScanInfo>[] plist = new List<OperatorScanInfo>[maxPriority];
			
			
			
			for(int i = 0; i < maxPriority; i++)
			{
				plist[i]	=new List<OperatorScanInfo>();
			}

			foreach(var v in infos)
			{
				v.position -= t;
				v.level -= t;
				if(v.level==0)
				{
					plist[v.operatorInfo.priority].Add(v);
				}
			}
			OperatorScanInfo ois=null;

			foreach(var v in plist)
			{
				if(v.Count>0)
				{
					int maxPos = int.MinValue;
					
					foreach(var i in v)
					{
						if(i.fixedPosition>maxPos)
						{
							maxPos = i.fixedPosition;
							ois = i;
						}
					}
					break;
				}
			}

			if(ois!=null)
			{
				
				var v = ois.operatorInfo.factory(text, ois, infos, this);
				return	v;
			}
			else
			{
				try
				{
					return new NumberObject(double.Parse(text));
				}
				catch
				{
					return error;
				}
			}
			return error;

		}

	

	}


	public static class Helper
	{
		static internal SourceOperator toSourceOperator(this OneElementOperator OperatorInfo)
		{
			return data => OperatorInfo((double)data[0]);
		}
		static internal SourceOperator toSourceOperator(this TwoElementsOperator OperatorInfo)
		{
			return data => OperatorInfo((double)data[0], (double)data[1]);
		}
		public static List<int>  findDot(string s,char dot=',')
		{

			List<int> ints = new List<int>();

			ReadOnlySpan<char> text_ = s.AsSpan();
			int leftParentheses = 0;
			int rightParentheses = 0;
			int level;
			OperatorScanInfo osi;
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
			text = text.Trim();
			int i = 0;
			while (text.StartsWith('(') && text.EndsWith(")"))
			{
				text = text.Substring(1, text.Length - 2);
				i++;
			}
			text = text.Trim();
			return i;
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

		public OperatorInfo copyForm(OperatorInfo operatorInfo)
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
		static MathObject LR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			var v=solveLR(text, ois, infos, manager);
			return new Operator(ois.operatorInfo.Operator0, manager.GetMathObject(v[0].name, v[0].infos),
				manager.GetMathObject(v[1].name, v[1].infos));
		}
	 internal	static MathObject RuntimeLR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			
		return	new RuntimeMathObject((v,s) =>
			{
var r = solveLR(text, ois, infos, manager);

			var a = manager.Run(r[0].name);

			var b = manager.Run(r[1].name);

				if(a is RuntimeMathObject)
				{
					((RuntimeMathObject)a).setValueIn(v, s);
				}
				if (b is RuntimeMathObject)
				{
					((RuntimeMathObject)b).setValueIn(v, s);
				}

				return  new Operator(ois.operatorInfo.Operator0,a ,
				b);

			});

		}
		internal static (string name, List<OperatorScanInfo> infos)[] solveLR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			List<OperatorScanInfo> left = new List<OperatorScanInfo>();
			List<OperatorScanInfo> right = new List<OperatorScanInfo>();
			int l = ois.position;
			int r = ois.position + ois.operatorInfo.mark.Length;
			foreach (var v in infos)
			{
				if (v.position < l)
				{
					left.Add(v);
				}
				else if (v.position >= r)
				{
					v.position -= r;
					right.Add(v);
				}
			}
			return new (string, List<OperatorScanInfo>)[]{(text.Substring(0, l), left),
				(text.Substring(r), right)};
		}
	static	MathObject R(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			List<OperatorScanInfo> right = new List<OperatorScanInfo>();

			int r = ois.operatorInfo.mark.Length;
			foreach (var v in infos)
			{
				if (v.position >= r)
				{
					v.position -= r;
					right.Add(v);
				}
			}
			return new Operator(ois.operatorInfo.Operator0,manager. GetMathObject(text.Substring(r), right));
		}
	static	MathObject L(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			List<OperatorScanInfo> left = new List<OperatorScanInfo>();
			int l = ois.position;
			foreach (var v in infos)
			{
				if (v.position < l)
				{
					left.Add(v);
				}
			}
			return new Operator(ois.operatorInfo.Operator0,manager. GetMathObject(text.Substring(0, l), left));
		}
	static	MathObject RLOR(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			if(ois.position==0)
			{
			return	R(text,ois,infos,manager);
			}
			else
			{
			return	LR(text,ois,infos,manager);
			}
		}
	static	MathObject Error(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			return MathObjectManager.error;
		}

	internal static	(string name,List<OperatorScanInfo> infos)[] solveFunc(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			int r = ois.operatorInfo.mark.Length;
			if(text.Length<=r)
			{
				return new (string name, List<OperatorScanInfo> infos)[0];
			}
			   text = text.Substring(r);
			int tt = Helper.clean(ref text);
			List<int> pos = Helper.findDot(text,ois.operatorInfo.dot);
			int funcSize = pos.Count + 1;
			List<OperatorScanInfo>[] left = new List<OperatorScanInfo>[funcSize];
			for (int i = 0; i < funcSize; i++)
			{
				left[i] = new List<OperatorScanInfo>();
			}


			foreach (var v in infos)
			{
				v.level -= tt;
				v.position -= r;
				bool notFound = true;
				for (int i = pos.Count - 1; i >= 0; i--)
				{
					if (v.position > pos[i])
					{
						notFound = false;
						left[i + 1].Add(v);
						v.position -= pos[i] + 1 + tt;
						break;
					}
				}
				if (notFound)
				{
					left[0].Add(v);
					v.position -= tt;
				}
			}

			(string,List<OperatorScanInfo>)[] mathObjects = new (string, List<OperatorScanInfo>)[funcSize];
			int id = 0;
			for (int i = 0; i < funcSize; i++)
			{
				if (i < pos.Count)
				{
					mathObjects[i] = (text.Substring(id, pos[i] - id), left[i]);
					id = pos[i] + 1;
				}
				else
				{

					mathObjects[i] = (text.Substring(id), left[i]);
				}

			}
			return  mathObjects;
		}
	static	MathObject Func(string text, OperatorScanInfo ois, List<OperatorScanInfo> infos, MathObjectManager manager)
		{
			var r=solveFunc(text, ois, infos, manager);
			int funcSize = r.Length;
			MathObject[] mathObjects = new MathObject[funcSize];			
			int id = 0;
			for (int i = 0; i < funcSize; i++)
			{
					mathObjects[i] = manager.GetMathObject(r[i].name, r[i].infos);
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
				return (a,b,c,d)=> { int index = -1; if (b.operatorInfo.tag.StartsWith("tempindex")){ index = int.Parse(b.operatorInfo.tag.Substring(9)); }; return new SourceObject(operatorInfo.Operator0).setIndex(index); };
			}

			return Error;


		}
	}

	public abstract class MathObject
	{
		internal bool isRuntime=false;
		public abstract double getValue(params double[] ms);
	}

	
	public class RuntimeMathObject:MathObject
	{
		MathMaster MathMaster;
		public object valueOut { get;internal set; }
		public (MathObjectManager manager,  (string, List<OperatorScanInfo>)[] vars)? valueIn { get; internal set; }
		public RuntimeMathObject(MathMaster mathMaster)
		{
			this.MathMaster = mathMaster;
		}

        public RuntimeMathObject setValueIn(MathObjectManager manager,params (string, List<OperatorScanInfo>)[] vars)
		{
			valueIn = (manager,vars);
			return this;
		}
		public override double getValue(params double[] ms)
		{
			var v =valueIn.HasValue? MathMaster(valueIn.Value.manager,valueIn.Value.vars):
				MathMaster(null,null);
			if(v is MethodObject)
			{
				valueOut = ((MethodObject)v).value;
			}
			return v.getValue(ms);
		}
	}
	public class Operator : MathObject
	{
		MathObject[] A;
		SourceOperator Operator1;

		public Operator(SourceOperator op,params MathObject[] a)
		{
			A = a;
			Operator1 = op;
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
			if (A is null )
			{
				return  double.NaN ;
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
