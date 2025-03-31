using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Images;
using DVOSLib;
using DVLib.LabDataHelper.MathObjectSystem;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Dynamic;
//using ScainfoM = ScanInfo<DVLib.LabDataHelper.MathObject, I, M>;

namespace DVLib.LabDataHelper
{
    public delegate  T  Factory<T,InfoT,ManagerT> (string text, ScanInfo<T,InfoT,ManagerT> scanInfo, SList<ScanInfo< T,InfoT, ManagerT>> list,ManagerT Paser,ScanResult result)where InfoT:ObjectInfo<T,InfoT,ManagerT>,new() where ManagerT:ObjectManager<T,InfoT,ManagerT>;
    public delegate bool OperatorInfoCondition<T, InfoT, M>(InfoT info) where InfoT : ObjectInfo<T, InfoT, M>, new() where M : ObjectManager<T, InfoT, M>;
	public enum OperatorType
	{
		LeftRight, Left, Right, Func, Source,Number, LeftRightOrRight,RUNCODE,RETURN
	}


    public interface IHasPos<T>
    {
        int Position { get; set; }
        int Level { get; set; }
        int NextOffset { get; }
        void setIndex(int index);
        int getIndex();
        string Token {  get; }
		T getCopy();

	}

	public class ScanInfo<T,I,M>:IHasPos<ScanInfo<T, I, M>> where I : ObjectInfo<T,I,M>,new()where M:ObjectManager<T,I,M>
    {
        internal ScanInfo(int p, int l, I o)
        {
            Position = p;
            Level = l;
            operatorInfo = o;
            //tag=o.tag;
        }
        public int Position {get; set;}
        public string Token { get => operatorInfo.Token; }
        internal int FixedPosition { get { if (operatorInfo.reverse) return -Position; return Position; } }
        public int Level { get; set; }
		public string Mark { get => operatorInfo.mark; }
		public string Tag { get => operatorInfo.tag; }
        public int Index { get; private set; }
        internal I operatorInfo;
        public int NextOffset { get; private set; } = -1;

		public ScanInfo<T, I, M> getCopy()
		{
			var n= new ScanInfo<T, I, M>(Position,Level,operatorInfo);
            n.NextOffset = NextOffset;
            n.Index = Index;
            //n.tag = tag;
            return n;
		}
        public void setIndex(int i)
        { this.Index = i; }
        public void setNext(int offset)
        {
        NextOffset=offset;
        }

		public int getIndex()
		{
			return this.Index;
		}
	}

    public class LevelGetter
    {
        Dictionary<char,int> map=new Dictionary<char, int>();

        public LevelGetter(params (char Char,int level)[] values) {
        foreach(var v in values)
            {
                register(v.Char, v.level);
            }
        }
        public void register(char c,int l)
        {
            map.Add(c, l);
        }

        public int getLevel(char c)
        {
          if(map.TryGetValue(c, out int l)) return l;
            return 0;
        }
    }

   
    public class ScanResult
    {



         HashSet<OperatorType> types=new HashSet<OperatorType>();
         HashSet<string> names=new HashSet<string>();
        public readonly int id;
        public ScanResult(int id)
        {
            this.id = id;
        }

        public bool isChanged(ChagedObject obj)
        {
            return obj.isChanged(id);
        }
      

        internal void add(OperatorType type)
        {
            types.Add(type);
		}
		internal void add(string type)
		{
			names.Add(type);
		}
		public bool containsType(OperatorType type)
        {
            return types.Contains(type);
        }
    
        public bool containsName(string name)
        {
            return names.Contains(name);
        }
    }
    public class CodeBlockInfo<T, I, M> where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
	{
       public (string name, List<ScanInfo<T, I, M>> infos)? code { get; private set; }
		public (string name, List<ScanInfo<T, I, M>> infos)[] vars { get; private set; }
		public CodeBlockInfo((string name, List<ScanInfo<T, I, M>> infos)? code, params (string name, List<ScanInfo<T, I, M>> infos)[] vars)
        {
            this.vars = vars;
            this.code = code;
        }
    }
	public delegate bool ObjContition<T, S, M>(ReadOnlySpan<char> chars, int pos, SList<ScanInfo<T, S, M>> lis,Stack<ScanInfo<T, S, M>> stack) where S : ObjectInfo<T, S, M>, new() where M : ObjectManager<T, S, M>;
    public delegate bool ScanContition(ReadOnlySpan<char> chars, int pos);

    public class ObjectInfoOverride<T, S, M> where S : ObjectInfo<T, S, M>, new() where M : ObjectManager<T, S, M>
	{
        List<(ObjContition<T, S, M> contition, S override_)> overrides = new();
        public void addOverride(ObjContition<T,S,M> contition,S override_)
        {
            overrides.Add((contition, override_));
        }

        public S GetOverride(ReadOnlySpan<Char> chars,int index,SList<ScanInfo<T,S,M>> scanInfos,Stack<ScanInfo<T, S, M>> stack)
        {
            foreach(var v in overrides)
            {
                if(v.contition(chars,index,scanInfos,stack))
                {
                    return v.override_;
                }
            }
            return null;
        }
    }

    public class ObjectInfo<T,S,M>where S:ObjectInfo<T,S,M>,new() where M : ObjectManager<T, S,M>
    {
    

        public string tag { get; internal set; }
        public string mark { get; internal set; }
        public int priority { get; internal set; }
        public char dot { get; internal set; } = ',';
        public bool reverse { get; internal set; }

        public int LevelChnage { get; private set; } = 0;

        public ObjectInfoOverride<T,S,M> override_=new();
        public bool hasInstance { get; private set; } = true;

        internal int TokenEndCount = 0;

        public Func<ScanInfo<T,S,M>, ScanInfo<T, S, M>, bool> isNextPart { get; private set; } = null;

        public ObjContition<T,S,M> ShouldCreate { get; private set; } = (s, p,l,st) => true;
        public Factory<T,S,M> factory { get; internal set; }

        public virtual string Token { get=>mark.Substring(0,mark.Length-TokenEndCount);  }

		static bool false_(ReadOnlySpan<char> span, int index)
		{
			return false;
		}

		public ObjectInfo()
        {
        }
        public ObjectInfo(string mark, int priority, Factory<T,S,M> factory=null,string tag = "raw")
        {
            this.tag = tag;
            this.priority = priority;
            this.mark = mark;
            this.factory = factory;
        }
        public S setLevelChange(int level)
        {
            this.LevelChnage = level;
            return (S)this;
        }

        public S addOveriide(ObjContition<T, S, M> objContition,S Override_)
        {
            this.override_.addOverride(objContition, Override_);
            return (S)this;
        }

        public S getOverride(ReadOnlySpan<Char> chars, int index, SList<ScanInfo<T, S, M>> scanInfos,Stack<ScanInfo<T, S, M>> stack)
        {
            var v = override_.GetOverride(chars, index, scanInfos,stack);
            if (v != null) { return v; }
            return (S)this;
        }

        public virtual S setCondition(ObjContition<T,S,M> objContition)
        {
            this.ShouldCreate = objContition;

            return (S)this;
        }
		public virtual S setNoInstance()
		{
			this.hasInstance=false;

			return (S)this;
		}
		public virtual S copyForm(S info)
        {
            tag = info.tag;
            mark = info.mark;
            priority = info.priority;
            dot = info.dot;
            reverse = info.reverse;
            factory = info.factory;
            ShouldCreate=info.ShouldCreate;
            return (S)this;
        }

        public virtual S nextPart(Func<ScanInfo<T, S, M>, ScanInfo<T, S, M>, bool> func)
        {
            this.isNextPart = func;
            return (S)this;
        }

		public static CodeBlockInfo<T, I, M> solveCodeBlock<T, I, M>(string text, ScanInfo<T, I, M> ois,SList<ScanInfo<T, I, M>> infos, MathObjectManager manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			int r = ois.operatorInfo.Token.Length;
            (string, List<ScanInfo<T, I, M>>)? code= null;
			if (text.Length <= r)
			{
				return new CodeBlockInfo<T, I, M>(null);
			}
            string name=text.Substring(0, r);
			text = text.Substring(r);

            if (text[0]=='(')
            {
                int p = text.AsSpan().findEnd('(', ')');
                if(p<text.Length)
                {
                    if(p<text.Length - 1)
                    {
                      string part2= text.Substring(p+1);
                        if (part2.StartsWith('{'))
                        {
                                List<ScanInfo<T, I, M>> l = new List<ScanInfo<T, I, M>>();
                                ScanInfo<T, I, M> temp;


								for (int i = 0;i<infos.Count;i++)
                                {
                                    temp= infos[i];
                                    if (infos[i].Position-r>p)
                                    {
                                        l.Add(temp);
                                        temp.Position -= name.Length + p + 1 ;
                                        infos.RemoveAt(i);
                                        i--;
                                    }
                                }
                                code = (part2, l);
                            }
                        
					}
                    text = text.Substring(0, p+1);
					int tt = Helper.clean(ref text);
					List<int> pos = Helper.findDot(text, ois.operatorInfo.dot);
					int funcSize = pos.Count + 1;
					List<ScanInfo<T, I, M>>[] left = new List<ScanInfo<T, I, M>>[funcSize];
					for (int i = 0; i < funcSize; i++)
					{
						left[i] = new List<ScanInfo<T, I, M>>();
					}


					foreach (var v in infos)
					{
						v.Level -= tt;
						v.Position -= r;
						bool notFound = true;
						for (int i = pos.Count - 1; i >= 0; i--)
						{
							if (v.Position > pos[i])
							{
								notFound = false;
								left[i + 1].Add(v);
								v.Position -= pos[i] + 1 + tt;
								break;
							}
						}
						if (notFound)
						{
							left[0].Add(v);
							v.Position -= tt;
						}
					}

					(string, List<ScanInfo<T, I, M>>)[] mathObjects = new (string, List<ScanInfo<T, I, M>>)[funcSize];
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
                    return new CodeBlockInfo<T, I, M>(code, mathObjects);
                }

            }
            return new CodeBlockInfo<T, I, M>(null);
			
		}
		public static (string name, SList<ScanInfo<T, I, M>> infos)[] solveFunc_NP<T, I, M>(string text,string split, SList<ScanInfo<T, I, M>> infos, M manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			List<ScanInfo<T,I,M>> pos=new ();
            foreach(var v in infos)
            {
                DVOS.writeLine(v.Level);
                if(v.Token==split&&v.Level==0)
                {
                    pos.Add(v);
                }
            }    

            return new (string name, SList<ScanInfo<T, I, M>> infos)[0];
		}


		public static (string name, SList<ScanInfo<T, I, M>> infos)[] solveFunc<T, I, M>(string text, ScanInfo<T, I, M> ois, SList<ScanInfo<T, I, M>> infos, M manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{    
            int r = 0;
            char dot = ',';
			if (ois!=null)
            { dot = ois.operatorInfo.dot;
	         r = ois.operatorInfo.Token.Length;
			if (text.Length <= r)
			{
				return new (string name, SList<ScanInfo<T, I, M>> infos)[0];
			}text = text.Substring(r);
            }
            if(text.StartsWith('('))
            {
 int end = text.AsSpan().findEnd('(', ')');
            if(end<text.Length)
				{
				
					text =text.Substring(0, end+1);
              
					int tt = Helper.clean(ref text);
				    if(text.Length==0)
                    {
                        return new (string name, SList<ScanInfo<T, I, M>> infos)[0];
                    }	
					List<int> pos = Helper.findDot(text,dot );


			int funcSize = pos.Count + 1;
                    if(end==1)
                    {
                        funcSize = 0;
                    }
			SList<ScanInfo<T, I, M>>[] left = new SList<ScanInfo<T, I, M>>[funcSize];
			for (int i = 0; i < funcSize; i++)
			{
				left[i] = new SList<ScanInfo<T, I, M>>();
			}
                   // DVOS.writeLine(text+":::"+infos.Count);
			foreach (var v in infos)
			{
				v.Level -= tt;
				v.Position -= r;
				bool notFound = true;
				for (int i = pos.Count - 1; i >= 0; i--)
				{
					if (v.Position > pos[i]&&v.Position!=end)
					{
						notFound = false;
						left[i + 1].Add(v);
						v.Position -= pos[i] + 1 + tt;
						break;
					}
				}
				if (notFound && v.Position != end)
				{
					left[0].Add(v);
					v.Position -= tt;
				}
			}

			(string, SList<ScanInfo<T, I, M>>)[] mathObjects = new (string, SList<ScanInfo<T, I, M>>)[funcSize];
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
			return mathObjects;
                }
            }
            return new (string name, SList<ScanInfo<T, I, M>> infos)[0];
           }

		public static (string name, SList<ScanInfo<T,I,M>> infos)[] solveLR<T,I,M>(string text, ScanInfo<T,I,M> ois, SList<ScanInfo<T,I,M>> infos, M manager)where I:ObjectInfo<T,I,M>,new() where M:ObjectManager<T,I,M>
		{
			SList<ScanInfo<T, I, M>> left = new SList<ScanInfo<T, I, M>>();
			SList<ScanInfo<T, I, M>> right = new SList<ScanInfo<T, I, M>>();
			int l = ois.Position;
			int r = ois.Position + ois.operatorInfo.Token.Length;
			foreach (var v in infos)
			{
				if (v.Position < l)
				{
					left.Add(v);
				}
				else if (v.Position >= r)
				{
					v.Position -= r;
					right.Add(v);
				}
			}
			return new (string, SList<ScanInfo<T, I, M>>)[]{(text.Substring(0, l), left),
				(text.Substring(r), right)};
		}

		public static (string name, SList<ScanInfo<T, I, M>> infos) solveL<T, I, M>(string text, ScanInfo<T, I, M> ois, SList<ScanInfo<T, I, M>> infos, M manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			SList<ScanInfo<T, I, M>> left = new SList<ScanInfo<T, I, M>>();
			int l = ois.Position;
			foreach (var v in infos)
			{
				if (v.Position < l)
				{
					left.Add(v);
				}
			}
			return (text.Substring(0, l), left);
		}
		public static (string name, SList<ScanInfo<T, I, M>> infos) solveR<T, I, M>(string text, ScanInfo<T, I, M> ois, SList<ScanInfo<T, I, M>> infos, M manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			SList<ScanInfo<T, I, M>> right = new SList<ScanInfo<T, I, M>>();

			int r = ois.operatorInfo.Token.Length;
			foreach (var v in infos)
			{
				if (v.Position >= r)
				{
					v.Position -= r;
					right.Add(v);
				}
			}
			return (text.Substring(r), right) ;
		}


	}

    public class TokenIgnore
	{
     public   ScanContition ShouldIgnore { get;private set; }
     public   ScanContition ShouldStop { get;private set; }

        public TokenIgnore(ScanContition shouldIgnore, ScanContition shouldStop)
        {
            this.ShouldIgnore = shouldIgnore;
            this.ShouldStop = shouldStop;
        }
    }

    public class SList<T> :IEnumerable<T>where T :class, IHasPos<T>
    {
        public List<T> List { get; private set; } = new();
      //  Dictionary<T, int> pos=new();
        public int Count=>List.Count;

        public void Add(T scanInfo)
        {
            scanInfo.setIndex(List.Count);
           // pos.Add(scanInfo, List.Count);
            List.Add(scanInfo);
        }
        public T this[int index] {get{ return List[index]; }}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)List).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)List).GetEnumerator();
		}

		internal void RemoveAt(int j)
		{
           // pos.Remove(List[j]);
			List.RemoveAt(j);
            for(int i = j;i<Count;i++)
            {
               // pos[List[i]] = i;
                List[i].setIndex(i);
            }
		}

		internal void Clear()
		{
			List.Clear();
           // pos.Clear();
		}

        public T getNext(T scanInfo)
        {
            if(scanInfo.NextOffset>-1&&scanInfo.getIndex()+scanInfo.NextOffset<Count)
            {
                return List[scanInfo.getIndex() + scanInfo.NextOffset ];
            }
            return null;
        }

        public (string name,SList<T> list) getContext(string s,T head,T end,int lc=-1)
        {
            int st=head.Position+head.Token.Length;
            int count = end.Position - st;
            return (s.Substring(st, count), SliceIndex(st,head.getIndex()+1, end.getIndex()-head.getIndex()-1,lc));
        }
        public (string name, SList<T> list)[] Split(string s,string split)
        {
            List<(string, SList<T> list)> list = new();
            int lastPos = 0; 
            int lastIndex = 0;
			for (int i=0;i<Count;i++)
            {
                var v=List[i];

                if(v.Token==split&&v.Level==0)
                {
                    list.Add((s.Substring(lastPos, v.Position - lastPos), SliceIndex(lastPos,lastIndex,v.getIndex()-lastIndex)));
                    lastPos = v.Position+1;
                    lastIndex=v.getIndex()+1;
                }
            }
            if(lastPos<s.Length)
            {
                list.Add((s.Substring(lastPos),SliceIndex(lastPos,lastIndex)));
            }

            return list.ToArray();
        }
		public int getPos(T scanInfo)
        {
            return scanInfo.getIndex();
            //return pos[scanInfo];
        }
        public (int index, T? t) getLastWithLevel(int index, string token, int level, T Conjugae = null)
        {
			for (int i = index; i >=0; i--)
			{
				var v = List[i];
				if (Conjugae != null && Conjugae.Token == v.Token && Conjugae.Level == v.Level && Conjugae != v)
				{
					break;
				}
				if (v.Level == level && v.Token == token)
				{
					return (i, v);
				}
			}
			return (-1, default);
		}
		public (int index,T? t) getFirstWithLevel(int index,string token,int level,T Conjugae=null)
        {
            for (int i = index; i < Count; i++)
            {
                var v = List[i];
                if(Conjugae!=null&&Conjugae.Token==v.Token&&Conjugae.Level==v.Level&&Conjugae!=v)
                {
                    break;
                }
				if (v.Level == level&&v.Token==token)
                {
                    return (i,v);
                }
            }
            return (-1,default);
		}
		public (int index, T? t) getLast(int index, string token)
		{
			for (int i = index; i>=0; i--)
			{
				var v = List[i];
				if (v.Token == token)
				{
					return (i, v);
				}
			}
			return (-1, default);
		}
		public (int index, T? t) getFirst(int index, string token)
		{
			for (int i = index; i < Count; i++)
			{
				var v = List[i];
				if ( v.Token == token)
				{
					return (i, v);
				}
			}
			return (-1, default);
		}
		public  SList<T> Slice( int index, int length = -1, int levelChange = 0, bool inplace = true) 
		{
			SList<T> result = new();
			int start = index;
			int end = index + length;
			bool toEnd = length < 0;
			T info;
			foreach (var s in this)
			{

				if (s.Position >= start && (toEnd || (s.Position < end)))
				{
					info = inplace ? s : s.getCopy();
					result.Add(info);
					info.Position -= index;
					info.Level += levelChange;
				}
			}
			return result;
		}
		public SList<T> SliceIndex(int pos,int index, int length = -1, int levelChange = 0, bool inplace = true)
		{
			SList<T> result = new();
			int start = index;  
            if(length<0)
            {
                length=Count-index;
            }
			int end = index + length;
         
			T info;
            int firstPos = -1;
            for(var i = index;i<end;i++)
            {
                var s=List[i];
				info = inplace ? s : s.getCopy();
				result.Add(info);
				info.Position -= pos;
				info.Level += levelChange;
			}
			return result;
		}
		public SList<T> Slice( IList<(int index, int length)> pos, int levelChange = 0, bool inplace = true) 
		{
			SList<T> result = new();
			int order = 0;
			var pair = pos[0];

			int start = pair.index;
			int end = pair.index + pair.length;
			bool toEnd = pair.length < 0;

			T info;
			foreach (var s in this)
			{
				if (s.Position >= end)
				{
					order++;
					pair = pos[order];
					start = pair.index;
					end = pair.index + pair.length;
					toEnd = pair.length < 0;
				}
				if (s.Position >= start && (toEnd || (s.Position < end)))
				{
					info = inplace ? s : s.getCopy();
					result.Add(info);
					info.Position -= start;
					info.Level += levelChange;
				}
			}
			return result;
		}

		internal SList<T> getCopy()
		{
			var v=new SList<T>();
            foreach (var s in this)
            {
                v.Add(s.getCopy());
            }
            return v;
		}
	}
    public delegate string replaceFunc(ReadOnlySpan<char> chars, int index);

	public class ObjectManager<T,S,M>:ChagedObject where S:ObjectInfo<T,S,M> ,new() where M:ObjectManager<T,S,M>
    {

     

		public int maxPriority { get; private set; } = 0;
        Dictionary<char, HeadCharSet<T,S,M>> stringLic = new Dictionary<char, HeadCharSet<T, S,M>>();
        List<Dictionary<char, HeadCharSet<T, S, M>>> stack = new List<Dictionary<char, HeadCharSet<T, S, M>>>();
        List<TokenIgnore> Ignore = new List<TokenIgnore>();
		bool canOver = true;
        public LevelGetter levelGetter { get; private set; } = new LevelGetter();
        internal int ObjectDepth = 0;
		internal StringDictionary<(string, replaceFunc)> toReplace = new StringDictionary<(string, replaceFunc)>();

		internal StringBuilder SB = new StringBuilder();
		public ObjectManager()
        {
            registerLG(levelGetter);
            registerDefault();
        }
        public void resetDepth()
        {
            ObjectDepth = 0;
        }
        public (string ,int ) getRecommend(string s)
        {
            var v = s.AsSpan();
            int i = 0;
            while(v.Length>0)
            {
                var v2 = getRecommend_(v);
                if (v2 != null)
                {
                    return (v2,i);
                }
                v = v.Slice(1);
                i++;
            }
            return (null,-1);
        }

        string getRecommend_(ReadOnlySpan<char> s)
        {

            if (s.Length > 0&& stringLic.TryGetValue(s[0], out HeadCharSet<T, S, M> hs))
            {
                return hs.getRecommend(s);
            }
            return null;
        }

	 Dictionary<char, HeadCharSet<T, S, M>> getClone()
        {

            Dictionary<char, HeadCharSet<T, S, M>> source;
            if(stack.Count > 0)
            {
                source = stack[stack.Count-1];
            }
            else
            {
                source=stringLic;
            }

		   var v= new Dictionary<char, HeadCharSet<T, S, M>>(source);
            foreach(var kv in source)
            {
                v[kv.Key] = kv.Value.getCopy();
            }
            return v;
        }

	   internal bool bracket0(ScanInfo<T,S,M> info, ScanInfo<T, S, M> info1)
        {
            return info1.Token == "(" && info1.Level == info.Level;
        }
		internal virtual void pushStack()
        {
            stack.Add(getClone());
        }
        internal virtual bool  popStack(out Dictionary<char, HeadCharSet<T, S, M>> result)
        {
            if(stack.Count>0 )
            {
                result = stack[stack.Count-1];
                stack.RemoveAt(stack.Count - 1);
                return true;
            }
            result = null;
            return false;
        }
		internal virtual bool peekStack(out Dictionary<char, HeadCharSet<T, S, M>> result,int offset=0)
		{
			if (stack.Count-offset>0)
			{
               result= stack[stack.Count-1-offset];
				return true;
			}
            result = null;
			return false;
		}
        public void addReplacePair((string a,replaceFunc b) pair)
        {
            toReplace.Add(pair.a, pair);
        }
		public S register(S info)
        {


            if (info.priority + 1 > maxPriority)
            {
                maxPriority = info.priority + 1;
            }
            string mk = info.mark;
            char head = mk[0];
            HeadCharSet<T, S,M> hc;
            if (stringLic.TryGetValue(head, out hc))
            {
                return hc.Add(info);
            }
            else
            {
                hc = new HeadCharSet<T, S,M>(head);
                hc.Add(info);
                stringLic.Add(head, hc);
            }
            return null;
        }

		public S registerInStack(S info)
		{
			string mk = info.mark;
			char head = mk[0];
			HeadCharSet<T, S, M> hc;
            if(peekStack(out var s))
            {
	        if (s.TryGetValue(head, out hc))
			{
				return hc.Add(info);
			}
			else
			{
				hc = new HeadCharSet<T, S, M>(head);
				hc.Add(info);
				s.Add(head, hc);
			}
            }
			return null;
		}

        public void addTokenIgnore(ScanContition inC,ScanContition outC)
        {
            
                Ignore.Add(new TokenIgnore(inC, outC));
       
        }
		public virtual void registerLG(LevelGetter getter)
        {

        }

        internal virtual void onCreated(T obj, S info)
        {

        }
        public virtual void registerDefault()
        {

        }
     
		public virtual string formalizeCode(string s)
		{
			SB.Clear();
			SB.EnsureCapacity(s.Length);
			var ss = s.AsSpan();
			char v;
            bool forceKeep = false;
            TokenIgnore tokenIgnore = null;


            for (int i = 0; i < ss.Length; i++)
            {
                v = ss[i];
                if ((v != ' ' && v != '\n') || forceKeep)
                {
                        SB.Append(v);
                }
			
		    }
            forceKeep = false;
            s = SB.ToString();
            ss=s.AsSpan();

            SB.Clear();
				for (int i = 0; i < ss.Length; i++)
			{
				v = ss[i];
                if (!forceKeep&&  toReplace .match(s, out var pair, out var st, i))
                {
                    SB.Append(pair.Item2(ss, i));

                    i += pair.Item1.Length - 1;
                }
                else
                {
                    SB.Append(v);
                }

				if (!forceKeep)
				{
					foreach (var j in Ignore)
					{
						if (j.ShouldIgnore(ss, i))
						{
							forceKeep = true;
							tokenIgnore = j;
							break;
						}
					}
				}
				else
				{
					if (tokenIgnore.ShouldStop(ss, i))
					{
						forceKeep = false;
						tokenIgnore = null;
					}
				}
			}
			var sb = SB.ToString();
			return sb;
		}
		public S match(string name)
        {
            if (name.Length > 0)
            {
                char head = name[0];
                HeadCharSet<T,S,M> h;
                if (stringLic.TryGetValue(head, out h))
                {
                    return h.Match(name);
                }
            }

            return null;
        }
       public bool removeInfo(string name)
        {


            if (name.Length > 0)
            {
                char head = name[0];
                HeadCharSet<T,S,M> h;
                if (stringLic.TryGetValue(head, out h))
                {
                    if (h.tryRemoveKey(name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

       public  List<string> findWithTag(string tag)
        {
            List<string> list = new List<string>();
            foreach (var item in stringLic)
            {

                item.Value.findWithTag(tag, list);
            }
            return list;
        }

       public  List<string> findWithCondition(OperatorInfoCondition<T,S,M> condition)
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
            var l = findWithTag(tag);
            foreach (var item in l)
            {
                removeInfo(item);
            }
        }

        public virtual void clear()
        {

            foreach (var v in stringLic)
            {
                v.Value.clear();
            }
        }
        public void removeWithCondition(OperatorInfoCondition<T,S,M> tag)
        {
            var l = findWithCondition(tag);
            foreach (var item in l)
            {
                removeInfo(item);
            }
		}

  
       internal T GetObject(string text)
        {
            SList<ScanInfo<T, S, M>> info = new SList<ScanInfo<T, S, M>>();
    	var r = ScanForOperators(ref text, info);
		return GetObject(text, info, r);
        }
            
        
            
     
        internal virtual T  GetObject(string text, SList<ScanInfo<T,S,M>> infos,ScanResult result)
        {
            ObjectDepth++;
            int t = Helper.clean(ref text);
            infos = infos.getCopy();
			List<ScanInfo<T,S,M >>[] plist = new List<ScanInfo<T,S, M>>[maxPriority];
            for (int i = 0; i < maxPriority; i++)
            {
                plist[i] = new List<ScanInfo<T,S,M>>();
            }
            foreach (var v in infos)
            {
                v.Position -= t;
                v.Level -= t;
				if (v.Level == 0 && v.operatorInfo.hasInstance)
                {
                    plist[v.operatorInfo.priority].Add(v);
                    //DVOS.writeLine(v.Token + ":" + v.Level +"=>"+v.operatorInfo.priority);
                }
            }
            ScanInfo< T,S,M> ois = null;

            foreach (var v in plist)
            {
                if (v.Count > 0)
                {
                    int maxPos = int.MinValue;

                    foreach (var i in v)
                    {
                        if (i.FixedPosition > maxPos)
                        {
                            maxPos = i.FixedPosition;
                            ois = i;
                        }
                    }
                    break;
                }
            }

			if (ois != null)
			{
				// DVOS.writeLine(ois.operatorInfo.Token);
				var v = ois.operatorInfo.factory(text, ois, infos, (M)this,result);
                onCreated(v, ois.operatorInfo);

                ObjectDepth--;
                return v;
            }
            else
            {
                try
				{
					
                    var v = getBaseType(text);ObjectDepth--;
					return v;
                }
                catch
				{
                    var v = getErrorType();
					ObjectDepth--;
					return v;
                }
            }
            return getErrorType();

        }

        public virtual T getBaseType(string s)
        {
            return default(T);
        }
        public virtual T getErrorType()
        {
            return default(T);
        }
        public ScanResult ScanForOperators(ref string text, SList<ScanInfo<T,S,M>> list)
        {
            ScanResult r = new ScanResult(ID);
           // Helper.clean(ref text);
            text=formalizeCode(text);
            ReadOnlySpan<char> text_ = text.AsSpan();

            Stack<ScanInfo<T, S, M>> withNext = new();
            int level=0;
            ScanInfo<T,S,M> osi;
            char c;
            //char last;
            HeadCharSet<T,S,M> hs;
            S info;

            //bool InString = false;

            bool CheckToken = true;
            TokenIgnore tokenIgnore = null;
            //bool InCall = false;
            //bool lastIsNum=false;

            void OnNew()
            {
				if (withNext.Count > 0)
				{
					var v = withNext.Peek();
					if (v.operatorInfo.isNextPart(v, osi))
					{
                        v.setNext(list.Count-1- list.getPos(v));
                        withNext.Pop();
					}
				}
				if (info.isNextPart != null)
				{
					withNext.Push(osi);
				}
                level += info.LevelChnage;
			}

			for (int i = 0; i < text_.Length; i++)
            {
                c = text_[i];
                /*
                if(c=='"')
                {
                    InString = !InString;
                }
                CheckToken=!(InString||InCall);
                */

              
                if(CheckToken)
                {
                    bool flag = true;
                  
                      if(peekStack(out var d,0)&&d.TryGetValue(c, out hs)&&(info = hs.Match(text_.Slice(i)))!=null)
                    {

                        info = info.getOverride(text_, i, list, withNext);
                            if (info.ShouldCreate(text_, i,list,withNext))
                            {
                                osi = new ScanInfo<T, S, M>(i, level, info);
                                list.Add(osi);
                                i += info.Token.Length - 1;
                                r.add(info.mark);
                                flag = false;
                                OnNew();
                                //break;
                            }
					}
                    
                 

                    if (flag&& stringLic.TryGetValue(c, out hs))
                    {
                    info = hs.Match(text_.Slice(i));
                        if(info!=null)
                        {
		info = info.getOverride(text_, i, list, withNext);
					if (info.ShouldCreate(text_,i,list,withNext))
                    {
              
                        osi = new ScanInfo<T,S,M>(i, level, info);
                        list.Add(osi);
                        i += info.Token.Length - 1;
                        r.add(info.mark);
                         OnNew();
						}

					}
                        }
				
                }
                if(CheckToken)
                {
                    foreach(var v in Ignore)
                    {
                        if(v.ShouldIgnore(text_,i))
                        {
                            tokenIgnore = v;
                            CheckToken = false;
                        }
                    }
				}
				else if (tokenIgnore.ShouldStop(text_, i))
				{
					CheckToken = true;
					tokenIgnore = null;
				}
				/*
                if(InCall)
                {
                    if (c == '@' || c == '(')
                    {
                        InCall = false;
                    }
                }
                if (c == '.' && text_.Length > i + 1 && !text_[i+1].isNumber())
                {
                    InCall=true;
                }
                */
				level += levelGetter.getLevel(c);
               // last = c;
                //lastIsNum = c > -'0' && c <= '9';
            }
			return r;
        }
    }
    public class HeadCharSet<T,InfoT,M>:CharDictionary<InfoT> where InfoT:ObjectInfo<T,InfoT,M>,new() where M:ObjectManager<T,InfoT,M>
    {
      

        public HeadCharSet(char head):base(head)
        {
        }

        public void clear()
        {
            foreach (var v in context)
            {
                if (v != null)
                    v.Clear();
            }
        }

        public string getRecommend(ReadOnlySpan<char> s)
        {
            int index = s.Length - 1;
            if(index>=0&&context.Length > index)
            {
                for(int i=index;i<context.Length; i++)
                {
                    if (context[i]!=null)
                    {
                foreach(var m in context[i])
                {
                    var ms = m.Key.AsSpan().Slice(0,s.Length);
                    if(ms.SequenceEqual(s))
                            {
                                return m.Key;
                            }
                }
                    }
              
                }
            }
            return null;
        }
        public bool tryRemoveKey(string key)
        {
            int i = key.Length - 1;
            if (i >= 0 && i < maxLength)
            {
                var d = context[i];
                if (d.ContainsKey(key))
                {
                    d.Remove(key);
                    return true;
                }
            }
            return false;
        }

        internal void findWithTag(string tag, List<string> list)
        {
            foreach (var v in context)
            {
                if (v != null)
                    foreach (var k in v)
                    {
                        if (k.Value.tag == tag)
                        {
                            list.Add(k.Key);
                        }
                    }
            }
        }
        internal void findWithCondition(OperatorInfoCondition<T,InfoT,M> condition, List<string> list)
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

        public HeadCharSet<T, InfoT, M> getCopy()
        {
            HeadCharSet<T, InfoT, M> r = new(head);
            r.maxLength = maxLength;
            r.capacity = capacity;
            r.context = new Dictionary<string, InfoT>[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                if(context[i] != null)
                r.context[i]=new Dictionary<string, InfoT>(context[i]);
            }
            return r;
        }
        public InfoT Add(InfoT info)
        {
            string mk = info.mark;
            int l = mk.Length;
            if (l > capacity)
            {
                var nc = new Dictionary<string, InfoT>[l];
                Array.Copy(context, nc, maxLength);
                context = nc;
                capacity = l;
            }
            if (l > maxLength)
            {
                maxLength = l;
            }
            if (context[l - 1] == null)
            {
                context[l - 1] = new Dictionary<string, InfoT>();
            }

            if (context[l - 1].ContainsKey(info.mark))
            {
                var t = new InfoT();
                t.copyForm (context[l - 1][info.mark]);
                context[l - 1][info.mark].copyForm(info);
                return (InfoT)t;
            }

            context[l - 1].Add(info.mark, info);
            return null;
        }

        public InfoT  Match(ReadOnlySpan<char> input)
        {
            int indexMax = Math.Min(maxLength, input.Length) - 1;
            for (int i = indexMax; i >= 0; i--)
            {
                var v = context[i];
                if (v != null)
                {
                    InfoT oi;
                    if (v.TryGetValue(input.Slice(0, i + 1).ToString(), out oi))
                    {
                        return oi;
                    }

                }
            }
            return null;
        }
    }


}
