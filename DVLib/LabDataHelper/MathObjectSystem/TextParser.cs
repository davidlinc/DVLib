using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Images;
using DVOSLib;
using DVLib.LabDataHelper.MathObjectSystem;
//using ScainfoM = ScanInfo<DVLib.LabDataHelper.MathObject, I, M>;

namespace DVLib.LabDataHelper
{
    public delegate  T  Factory<T,InfoT,ManagerT> (string text, ScanInfo<T,InfoT,ManagerT> scanInfo, List<ScanInfo< T,InfoT, ManagerT>> list,ManagerT Paser,ScanResult result)where InfoT:ObjectInfo<T,InfoT,ManagerT>,new() where ManagerT:ObjectManager<T,InfoT,ManagerT>;
    public delegate bool OperatorInfoCondition<T, InfoT, M>(InfoT info) where InfoT : ObjectInfo<T, InfoT, M>, new() where M : ObjectManager<T, InfoT, M>;
	public enum OperatorType
	{
		LeftRight, Left, Right, Func, Source,Number, LeftRightOrRight,RUNCODE,RETURN
	}

   

	public class ScanInfo<T,I,M>where I : ObjectInfo<T,I,M>,new()where M:ObjectManager<T,I,M>
    {
        internal ScanInfo(int p, int l, I o)
        {
            position = p;
            level = l;
            operatorInfo = o;
            tag=o.tag;
        }
        internal int position;
        internal int fixedPosition { get { if (operatorInfo.reverse) return -position; return position; } }
        internal int level;
        public string tag { get; private set; }
        internal I operatorInfo;

		internal ScanInfo<T, I, M> getCopy()
		{
			var n= new ScanInfo<T, I, M>(position,level,operatorInfo);
            n.tag = tag; return n;
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
    public class ObjectInfo<T,S,M>where S:ObjectInfo<T,S,M>,new() where M:ObjectManager<T,S,M>
    {
    

        public string tag { get; internal set; }
        public string mark { get; internal set; }
        public int priority { get; internal set; }
        public char dot { get; internal set; } = ',';
        public bool reverse { get; internal set; }
        public OperatorType type { get; internal set; }
        public Factory<T,S,M> factory { get; internal set; }


        public ObjectInfo()
        {

        }
        public ObjectInfo(string mark, OperatorType type, int priority, Factory<T,S,M> factory=null,string tag = "raw")
        {
            this.tag = tag;
            this.priority = priority;
            this.mark = mark;
            this.type = type;
            this.factory = factory;
        }

        public virtual S copyForm(S info)
        {
            tag = info.tag;
            mark = info.mark;
            priority = info.priority;
            dot = info.dot;
            reverse = info.reverse;
            factory = info.factory;
            type= info.type;    
            return (S)this;
        }

        
		public static CodeBlockInfo<T, I, M> solveCodeBlock<T, I, M>(string text, ScanInfo<T, I, M> ois, List<ScanInfo<T, I, M>> infos, MathObjectManager manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			int r = ois.operatorInfo.mark.Length;
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
                                    if (infos[i].position-r>p)
                                    {
                                        l.Add(temp);
                                        temp.position -= name.Length + p + 1 ;
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
		public  static (string name, List<ScanInfo<T, I, M>> infos)[] solveFunc<T, I, M>(string text, ScanInfo<T, I, M> ois, List<ScanInfo<T, I, M>> infos, MathObjectManager manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			int r = ois.operatorInfo.mark.Length;
			if (text.Length <= r)
			{
				return new (string name, List<ScanInfo<T, I, M>> infos)[0];
			}
			text = text.Substring(r);

            if(text.StartsWith('('))
            {
 int end = text.AsSpan().findEnd('(', ')');
            if(end<text.Length)
                {
    text=text.Substring(0, end+1);

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
			return mathObjects;
                }
            }

            return new (string name, List<ScanInfo<T, I, M>> infos)[0];
           

		
		}

		public static (string name, List<ScanInfo<T,I,M>> infos)[] solveLR<T,I,M>(string text, ScanInfo<T,I,M> ois, List<ScanInfo<T,I,M>> infos, MathObjectManager manager)where I:ObjectInfo<T,I,M>,new() where M:ObjectManager<T,I,M>
		{
			List<ScanInfo<T, I, M>> left = new List<ScanInfo<T, I, M>>();
			List<ScanInfo<T, I, M>> right = new List<ScanInfo<T, I, M>>();
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
			return new (string, List<ScanInfo<T, I, M>>)[]{(text.Substring(0, l), left),
				(text.Substring(r), right)};
		}
		public static (string name, List<ScanInfo<T, I, M>> infos) solveL<T, I, M>(string text, ScanInfo<T, I, M> ois, List<ScanInfo<T, I, M>> infos, MathObjectManager manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			List<ScanInfo<T, I, M>> left = new List<ScanInfo<T, I, M>>();
			int l = ois.position;
			foreach (var v in infos)
			{
				if (v.position < l)
				{
					left.Add(v);
				}
			}
			return (text.Substring(0, l), left);
		}
		public static (string name, List<ScanInfo<T, I, M>> infos) solveR<T, I, M>(string text, ScanInfo<T, I, M> ois, List<ScanInfo<T, I, M>> infos, MathObjectManager manager) where I : ObjectInfo<T, I, M>, new() where M : ObjectManager<T, I, M>
		{
			List<ScanInfo<T, I, M>> right = new List<ScanInfo<T, I, M>>();

			int r = ois.operatorInfo.mark.Length;
			foreach (var v in infos)
			{
				if (v.position >= r)
				{
					v.position -= r;
					right.Add(v);
				}
			}
			return (text.Substring(r), right) ;
		}


	}
	public class ObjectManager<T,InfoT,M>:ChagedObject where InfoT:ObjectInfo<T,InfoT,M> ,new() where M:ObjectManager<T,InfoT,M>
    {

     

		public int maxPriority { get; private set; } = 0;
        static Dictionary<char, HeadCharSet<T,InfoT,M>> stringLic = new Dictionary<char, HeadCharSet<T, InfoT,M>>();
        bool canOver = true;
        public LevelGetter levelGetter { get; private set; } = new LevelGetter();


        public ObjectManager()
        {
            registerLG(levelGetter);
            registerDefault();
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

            if (s.Length > 0&& stringLic.TryGetValue(s[0], out HeadCharSet<T, InfoT, M> hs))
            {
                return hs.getRecommend(s);
            }
            return null;
        }
	
        public InfoT register(InfoT info)
        {


            if (info.priority + 1 > maxPriority)
            {
                maxPriority = info.priority + 1;
            }
            string mk = info.mark;
            char head = mk[0];
            HeadCharSet<T, InfoT,M> hc;
            if (stringLic.TryGetValue(head, out hc))
            {
                return hc.Add(info);
            }
            else
            {
                hc = new HeadCharSet<T, InfoT,M>(head);
                hc.Add(info);
                stringLic.Add(head, hc);
            }
            return null;
        }
        public virtual void registerLG(LevelGetter getter)
        {

        }

        internal virtual void onCreated(T obj, InfoT info)
        {

        }
        public virtual void registerDefault()
        {

        }
        public virtual string formalizeCode(string s)
        {
            return s;
        }
		public InfoT match(string name)
        {
            if (name.Length > 0)
            {
                char head = name[0];
                HeadCharSet<T,InfoT,M> h;
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
                HeadCharSet<T,InfoT,M> h;
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

       public  List<string> findWithCondition(OperatorInfoCondition<T,InfoT,M> condition)
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
        public void removeWithCondition(OperatorInfoCondition<T,InfoT,M> tag)
        {
            var l = findWithCondition(tag);
            foreach (var item in l)
            {
                removeInfo(item);
            }
        }

  

     
        public virtual T  GetObject(string text, List<ScanInfo<T,InfoT,M>> infos,ScanResult result)
        {
            int t = Helper.clean(ref text);
            List<ScanInfo<T, InfoT, M>> infos_ = new List<ScanInfo<T, InfoT, M>>();
            foreach (var info in infos) {
                infos_.Add(info.getCopy());
            }
            infos = infos_;
			List<ScanInfo<T,InfoT,M >>[] plist = new List<ScanInfo<T,InfoT, M>>[maxPriority];
            for (int i = 0; i < maxPriority; i++)
            {
                plist[i] = new List<ScanInfo<T,InfoT,M>>();
            }

            foreach (var v in infos)
            {
                v.position -= t;
                v.level -= t;
                if (v.level == 0)
                {
                    plist[v.operatorInfo.priority].Add(v);
                }
            }
            ScanInfo< T,InfoT,M> ois = null;

            foreach (var v in plist)
            {
                if (v.Count > 0)
                {
                    int maxPos = int.MinValue;

                    foreach (var i in v)
                    {
                        if (i.fixedPosition > maxPos)
                        {
                            maxPos = i.fixedPosition;
                            ois = i;
                        }
                    }
                    break;
                }
            }

            if (ois != null)
            {

                var v = ois.operatorInfo.factory(text, ois, infos, (M)this,result);
                onCreated(v, ois.operatorInfo);
                return v;
            }
            else
            {
                try
                {
                    return getBaseType(text);
                }
                catch
                {
                    return getErrorType();
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
        public ScanResult ScanForOperators(ref string text, List<ScanInfo<T,InfoT,M>> list)
        {
            ScanResult r = new ScanResult(ID);
            bool hasCode=false;
            text=formalizeCode(text);
            ReadOnlySpan<char> text_ = text.AsSpan();
        
            int level=0;
            ScanInfo<T,InfoT,M> osi;
            char c;
            HeadCharSet<T,InfoT,M> hs;
            InfoT info;
            for (int i = 0; i < text_.Length; i++)
            {
                c = text_[i];
                if (stringLic.TryGetValue(c, out hs))
                {
                    info = hs.Match(text_.Slice(i));
                    if (info != null)
                    {
                        if(!hasCode&&info.type==OperatorType.RUNCODE)
                        {
                            hasCode = true;
                        }
                        osi = new ScanInfo<T,InfoT,M>(i, level, info);
                        list.Add(osi);
                        i += info.mark.Length - 1;
                        r.add(info.type);
                        r.add(info.mark);
                    }

                }
                level += levelGetter.getLevel(c);
            }
            return r;
        }
    }
    public class HeadCharSet<T,InfoT,M>:CharDictionary<InfoT,HeadCharSet<T, InfoT, M>> where InfoT:ObjectInfo<T,InfoT,M>,new() where M:ObjectManager<T,InfoT,M>
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
