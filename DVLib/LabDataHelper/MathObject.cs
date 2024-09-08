using DVOSLib;
using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace DVLib.LabDataHelper
{

	
	public delegate double TwoElementsOperator(double a,double b);
	public delegate double OneElementOperator(double a);
	public delegate double SourceOperator(params double[] doubles);

	public delegate bool OperatorInfoCondition(OperatorInfo info);
	public enum OperatorType
	{
		LeftRight,Left,Right,Func,Source,LeftRightOrRight
	}
	public class MathObjectManager
	{
		static int maxPriority = 0;
		static Random random = new Random();
		static MathObject error = new NumberObject(double.NaN);
		static Dictionary<char,HeadCharSet> stringLic = new Dictionary<char, HeadCharSet>();

		public MathObjectManager()
		{
			register(new OperatorInfo("+", OperatorType.LeftRight, 0,  Helper.toSourceOperator((a, b) => { return a + b; })));
			register(new OperatorInfo("-", OperatorType.LeftRightOrRight, 1,
				(data) => { if (data.Length == 1) return -data[0];return data[0]-data[1];}
				));
			register(new OperatorInfo("/", OperatorType.LeftRight, 2, (a, b) => { return a / b; }));
			register(new OperatorInfo("*", OperatorType.LeftRight, 2,  (a, b) => { return a * b; }));
			register(new OperatorInfo("%", OperatorType.LeftRight, 2,  (a, b) => { return a % b; }));
			register(new OperatorInfo("^", OperatorType.LeftRight, 3, Math.Pow));
			register(new OperatorInfo("/-", OperatorType.LeftRight, 2, (a, b) => { return a / -b; }));
			register(new OperatorInfo("*-", OperatorType.LeftRight, 2, (a, b) => { return a * -b; }));
			register(new OperatorInfo("%-", OperatorType.LeftRight, 2, (a, b) => { return a % -b; }));
			register(new OperatorInfo("^-", OperatorType.LeftRight, 3,(a,b)=> Math.Pow(a,-b)));
			register(new OperatorInfo("sin", OperatorType.Func, 4, Math.Sin));
			register(new OperatorInfo("cos", OperatorType.Func, 4, Math.Cos));
			register(new OperatorInfo("tan", OperatorType.Func, 4, Math.Tan));
			register(new OperatorInfo("arcsin", OperatorType.Func, 4, Math.Asin));
			register(new OperatorInfo("arccos", OperatorType.Func, 4, Math.Acos));
			register(new OperatorInfo("arctan", OperatorType.Func, 4, Math.Atan));
			register(new OperatorInfo("arccosh", OperatorType.Func, 4, Math.Acosh));
			register(new OperatorInfo("arcsinh", OperatorType.Func, 4, Math.Asinh));
			register(new OperatorInfo("arctanh", OperatorType.Func, 4, Math.Atanh));
			register(new OperatorInfo("arctan2", OperatorType.Func, 4,  Math.Atan2).setFuncSize(2));
			register(new OperatorInfo("asin", OperatorType.Func, 4, Math.Asin));
			register(new OperatorInfo("acos", OperatorType.Func, 4, Math.Acos));
			register(new OperatorInfo("atan", OperatorType.Func, 4, Math.Atan));
			register(new OperatorInfo("abs", OperatorType.Func, 4, Math.Abs));
			register(new OperatorInfo("acosh", OperatorType.Func, 4, Math.Acosh));
			register(new OperatorInfo("asinh", OperatorType.Func, 4, Math.Asinh));
			register(new OperatorInfo("atanh", OperatorType.Func, 4, Math.Atanh));
			register(new OperatorInfo("atan2", OperatorType.Func, 4,  Math.Atan2).setFuncSize(2));
			register(new OperatorInfo("sinh", OperatorType.Func, 4, Math.Sinh));
			register(new OperatorInfo("cosh", OperatorType.Func, 4, Math.Cosh));
			register(new OperatorInfo("tanh", OperatorType.Func, 4, Math.Tanh));
			register(new OperatorInfo("exp", OperatorType.Func, 4, Math.Exp));
			register(new OperatorInfo("round", OperatorType.Func, 4, Math.Round));
			register(new OperatorInfo("max", OperatorType.Func, 4, Math.Max).setFuncSize(2));
			register(new OperatorInfo("min", OperatorType.Func, 4,  Math.Min).setFuncSize(2));
			register(new OperatorInfo("random", OperatorType.Source, 4,  (double[] data) => random.NextDouble()));
			register(new OperatorInfo("randInt", OperatorType.Func, 4,   (a,b) => random.Next((int)Math.Min(a,b),(int)Math.Max(a,b))).setFuncSize(2));
			register(new OperatorInfo("pi", OperatorType.Source, 4 ,(double[] data)=>Math.PI));
			register(new OperatorInfo("rad", OperatorType.Func, 4, (a) => { return a * Math.PI / 180; }));
			register(new OperatorInfo("degree", OperatorType.Func, 4, (a) => { return a *  180/Math.PI ; }));
			register(new OperatorInfo("x", OperatorType.Source, 4,  data => data[0]));
			register(new OperatorInfo("y", OperatorType.Source, 4, data => data[1]));
			register(new OperatorInfo("z", OperatorType.Source, 4,  data => data[2]));
		}

		
		OperatorInfo register(string mark, OperatorType type, int priority, SourceOperator so , string tag = "raw")
		{
			return register(new OperatorInfo(mark, type, priority, so, tag));
		}
		OperatorInfo register(string mark, OperatorType type, int priority,OneElementOperator so, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so, tag));
		}
		OperatorInfo register(string mark, OperatorType type, int priority, TwoElementsOperator so, string tag = "raw")
		{
		return	register(new OperatorInfo(mark, type, priority, so, tag));
		}
		OperatorInfo register(OperatorInfo info)
		{
			if(info.priority+1>maxPriority)
			{
				maxPriority=info.priority+1;
			}
			string mk = info.mark;
			char head = mk[0];
			HeadCharSet hc;
			if(stringLic.TryGetValue(head,out hc))
			{
              return hc.Add(info);
			}
			else
			{
				hc=new HeadCharSet(head);
				hc.Add(info);
				stringLic.Add(head, hc);
			}
			return null;
		}

	OperatorInfo match(string name)
		{
			if(name.Length>0)
			{ 
				char head= name[0];
				HeadCharSet h;
				if(stringLic.TryGetValue(head,out h))
				{
					return h.Match(name);
				}
            }
			
			return null;
		}
		bool removeInfo(string name)
		{
			if (name.Length > 0)
			{
				char head = name[0];
				HeadCharSet h;
				if (stringLic.TryGetValue(head, out h))
				{
					if(h.tryRemoveKey(name))
					{
						return true;
					}
				}
			}
			return false;
		}
		List<string> findWithTag(string tag)
		{
			List<string> list = new List<string>();
            foreach (var item in stringLic)
            {
				
				item.Value.findWithTag(tag, list);
            }
            return list;
		}

		List<string> findWithCondition(OperatorInfoCondition condition)
		{
			List<string> list = new List<string>();
			foreach (var item in stringLic)
			{
				item.Value.findWithCondition(condition, list);
			}
			return list;
		}
		public void removeWithTag(string tag)
		{
			var l=findWithTag(tag);
			foreach (var item in l)
			{
				removeInfo(item);
			}
		}

		public void clear()
		{
			removeWithCondition(i => (i.tag != "raw"));
		}
		public void removeWithCondition(OperatorInfoCondition tag)
		{
			var l = findWithCondition(tag);
			foreach (var item in l)
			{
				removeInfo(item);
			}
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
				else if (l2.Count == 0 || l2.First() > pos) ;
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
		public void AddFunction(string s, string tag = "customize")
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
					MathObject m = Scan(func.ToString());
					AddUserFunc(names[0],m , funSize, tag);
					removeWithTag("temp");
					foreach(OperatorInfo o in old)
					{
						register(o);
					}
				}


			}
		

		}
		public void AddVar(string s, string tag = "customize")
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
					MathObject m = Scan(func.ToString());
					registerSource(funcname.ToString(),m .getValue(), tag);
			}
		

		}

		OperatorInfo registerSource(string name,int index,string tag="temp")
		{

			return register(name, OperatorType.Source, 4, (data) => {
				return data[index];
			}, tag);
		}
		OperatorInfo registerSource(string name,double value, string tag = "temp")
		{

			return register(name, OperatorType.Source, 4, (double[] data) => {
				return value;
			}, tag);
		}
		public void AddUserFunc(string name,MathObject mathObject,int funcSize,string tag="customize")
		{
			if(match(name)==null)
			register(new OperatorInfo(name, OperatorType.Func, 4,(double[] data)=>mathObject.getValue(data), tag).setFuncSize(funcSize));
		}

		public MathObject Scan(string text)
		{
			clean(ref text);
			List<OperatorScanInfo> info = new List<OperatorScanInfo>();
			ScanForOperators( text, info);
			return GetMathObject(text, info);

		}
		public MathObject GetMathObject(string text,List<OperatorScanInfo> infos) 
		{
			int t = clean(ref text);




			List<OperatorScanInfo>[] plist = new List<OperatorScanInfo>[maxPriority];
			
			
			
			for(int i = 0; i < maxPriority; i++)
			{
				plist[i]	=new List<OperatorScanInfo>();
			}

			foreach(var v in infos)
			{
				v.level -= t;
				v.position -= t;
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
					int maxPos = -1;
					
					foreach(var i in v)
					{
						if(i.position>maxPos)
						{
							maxPos = i.position;
							ois = i;
						}
					}
					break;
				}
			}
			if(ois!=null)
			{
				if(ois.operatorInfo.type==OperatorType.LeftRight||( ois.operatorInfo.type == OperatorType.LeftRightOrRight&&ois.position>0))
				{
					List<OperatorScanInfo> left = new List<OperatorScanInfo>();
					List<OperatorScanInfo> right=new List<OperatorScanInfo>();
					int l = ois.position;
					int r = ois.position + ois.operatorInfo.mark.Length;
					foreach(var v in infos)
					{
						if(v.position<l)
						{
							left.Add(v);
						}
						else if(v.position>=r)
						{
							v.position -= r;
							right.Add(v);
						}
					}
					return new Operator(ois.operatorInfo.Operator0,GetMathObject(text.Substring(0,l),left),
						GetMathObject(text.Substring(r),right));	
				}
				else if(ois.operatorInfo.type == OperatorType.Right|| (ois.operatorInfo.type == OperatorType.LeftRightOrRight && ois.position == 0))
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
					return new Operator(ois.operatorInfo.Operator0,GetMathObject(text.Substring(r), right));
				}
				else if (ois.operatorInfo.type == OperatorType.Left)
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
					return new Operator(ois.operatorInfo.Operator0, GetMathObject(text.Substring(0, l), left));
				}
				else if (ois.operatorInfo.type ==  OperatorType.Func)
				{	int funcSize=ois.operatorInfo.funcSize;
					List<OperatorScanInfo>[] left =new List<OperatorScanInfo>[funcSize];
                    for (int i=0;i<funcSize;i++)
                    {
						left[i] = new List<OperatorScanInfo>();
                    }

                    int r = ois.operatorInfo.mark.Length;
					text = text.Substring(r);
					int tt = clean(ref text);
					List<int> pos = findDot(text);
					foreach (var v in infos)
					{
						v.level -= tt;
						v.position -= r;
						bool notFound = true;
						for(int i=pos.Count-1;i>=0;i--)
						{
							if (v.position > pos[i])
							{
								notFound = false;
								left[i+1].Add(v);
								v.position -= pos[i] +1;
								break;
							}
						}
						if(notFound)
						{
							left[0].Add(v);
						}
					}
					MathObject[]mathObjects=new MathObject[funcSize];
					int id = 0;
					for(int i=0;i<funcSize;i++)
					{
						if(i<pos.Count)
						{
                        mathObjects[i] = GetMathObject(text.Substring(id, pos[i] - id), left[i]);
							id = pos[i]+1;
						}
						else
						{

							mathObjects[i] = GetMathObject(text.Substring(id), left[i]);
						}
						
					}
					return new Operator(ois.operatorInfo.Operator0,mathObjects

						);
				}
				else if(ois.operatorInfo.type==OperatorType.Source)
				{
					return new SourceObject(ois.operatorInfo.Operator0);
				}



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

	
		public List<int> findDot(string s)
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
			else	if (text_[i]==',')
				{
					level = leftParentheses - rightParentheses;

					if(level==0)
					{
						ints.Add(i);
					}

				}
			}
			return ints;
		}

		public int clean(ref string text)
		{
			text=text.Trim();
			int i = 0;
			while(text.StartsWith('(')&&text.EndsWith(")"))
			{
				text = text.Substring(1,text.Length-2);
				i++;
			}
			text=text.Trim();
			return i;
		}
		public void ScanForOperators( string text,List<OperatorScanInfo> list)
		{
			
			ReadOnlySpan<char> text_=text.AsSpan();
			int leftParentheses = 0;
			int rightParentheses = 0;
			int level;
			OperatorScanInfo osi;
			char c;
			HeadCharSet hs;
			OperatorInfo info;
			for(int i=0;i<text_.Length;i++)
			{
				if (text_[i]=='(')
				{
					leftParentheses++;
				}
				if (text_[i]==')')
				{
					rightParentheses++;
				}
				c = text_[i];
				if(stringLic.TryGetValue(c, out hs))
				{
					info = hs.Match(text_.Slice(i));
						if(info !=null)
					{
						level = leftParentheses - rightParentheses;
						osi = new OperatorScanInfo(i, level, info);
						list.Add(osi);
						i += info.mark.Length - 1;
					}

				}
			}
		}
	}
	public class OperatorScanInfo
	{
		internal OperatorScanInfo(int p,int l,OperatorInfo o)
		{
			position = p;
			level = l;
			operatorInfo = o;

		}
	    internal int position;
		internal int level;
		internal OperatorInfo operatorInfo;
	}

	public static class Helper
	{
		static internal SourceOperator toSourceOperator(this OneElementOperator OperatorInfo)
		{
			return data => OperatorInfo(data[0]);
		}
		static internal SourceOperator toSourceOperator(this TwoElementsOperator OperatorInfo)
		{
			return data => OperatorInfo(data[0], data[1]);
		}
		public static List<int> findString(this string text, string toFind)
		{
			List<int> ints = new List<int>();
			ReadOnlySpan<char> text_ = text.AsSpan();
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
	}

	public class OperatorInfo
	{
		public string mark{get; private set;}
		public OperatorType type { get; private set; }
		internal SourceOperator Operator0 { get; private set; }
		public int priority { get; private set; }
		public string tag { get; private set; }
		public int funcSize = 1;

		
		public OperatorInfo(string mark, OperatorType type,int priority,SourceOperator so,string tag="raw")
	
		{
			this.mark = mark;
			this.type = type;
			Operator0=so;
			this.priority=priority;
			this.tag = tag;
		}
		public OperatorInfo(string mark, OperatorType type, int priority, OneElementOperator so, string tag = "raw")

		{
			this.mark = mark;
			this.type = type;
			Operator0 = Helper.toSourceOperator(so);
			this.priority = priority;
			this.tag = tag;
		}
		public OperatorInfo setFuncSize(int size)
		{
			funcSize = size;
			return this;
		}
		public OperatorInfo(string mark, OperatorType type, int priority,TwoElementsOperator so, string tag = "raw")

		{
			this.mark = mark;
			this.type = type;
			Operator0 = Helper.toSourceOperator(so);
			this.priority = priority;
			this.tag = tag;
		}
	}
	public abstract class MathObject
	{
		public abstract double getValue(params double[] inputValues);
		
	}

	public class HeadCharSet
	{
		internal char head;
		internal int maxLength;
		internal int capacity = 8;
		Dictionary<string, OperatorInfo>[] context;

		public HeadCharSet(char head)
		{
			this.head = head;
			this.maxLength = 0;
			this.context = new Dictionary<string, OperatorInfo>[capacity];
		}

		public bool tryRemoveKey(string key)
		{
			int i = key.Length - 1;
			if(i>=0&&i<maxLength)
			{
				var d = context[i];
				if(d.ContainsKey(key))
				{
					d.Remove(key);
					return true;
				}
			}
			return false;
		}

		internal void findWithTag(string tag,List<string> list)
		{
			foreach(var v in context)
			{
				if(v!=null)
				foreach (var k in v)
				{
					if(k.Value.tag==tag)
					{
						list.Add(k.Key);
					}
				}
			}
		}
		internal void findWithCondition(OperatorInfoCondition condition, List<string> list)
		{
			foreach (var v in context)
			{
				if (v != null)
					foreach (var k in v)
				{
					if (condition(k.Value))
					{
						list.Add(k.Key);
					}
				}
			}
		}
		public OperatorInfo Add(OperatorInfo info)
		{
			string mk = info.mark;
			int l=mk.Length;
			if(l>capacity)
			{
				var nc = new Dictionary<string, OperatorInfo>[l];
				Array.Copy(context, nc,maxLength);
				context = nc;
			}
			if(l>maxLength)
			{
				maxLength = l;
			}
			if (context[l - 1] == null)
			{
				context[l - 1] = new Dictionary<string, OperatorInfo>(); 
			}

			if(context[l - 1].ContainsKey(info.mark))
			{
				var t = context[l - 1][info.mark];
				context[l - 1][info.mark] = info;
				return t;
			}

			context[l-1].Add(info.mark, info);
			return null;
		}

		public OperatorInfo Match(ReadOnlySpan<char> input)
		{
			int indexMax=Math.Min(maxLength, input.Length)-1;
			for(int i=indexMax; i>=0;i--)
			{
				var v= context[i];
				if(v!=null)
				{
					OperatorInfo oi;
					if(v.TryGetValue(input.Slice(0,i+1).ToString(),out oi))
					{
						return oi;
					}

				}
			}
			return null;
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
				return double.NaN;
			}

			return Operator1(getValues(inputValues));
		}
	}
	public class SourceObject : MathObject
	{
		SourceOperator operator1;
		public SourceObject(SourceOperator one)
		{
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
			return value;
		}
	}

}
