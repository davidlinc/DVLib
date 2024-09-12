using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Images;

namespace DVLib.LabDataHelper
{
    public delegate  T  Factory<T,InfoT,ManagerT> (string text, ScanInfo<T,InfoT,ManagerT> scanInfo, List<ScanInfo< T,InfoT, ManagerT>> list,ManagerT Paser)where InfoT:ObjectInfo<T,InfoT,ManagerT>,new() where ManagerT:ObjectManager<T,InfoT,ManagerT>;
    public delegate bool OperatorInfoCondition<T, InfoT, M>(InfoT info) where InfoT : ObjectInfo<T, InfoT, M>, new() where M : ObjectManager<T, InfoT, M>;

    public class ScanInfo<T,InfoT,M>where InfoT : ObjectInfo<T,InfoT,M>,new()where M:ObjectManager<T,InfoT,M>
    {
        internal ScanInfo(int p, int l, InfoT o)
        {
            position = p;
            level = l;
            operatorInfo = o;

        }
        internal int position;
        internal int fixedPosition { get { if (operatorInfo.reverse) return -position; return position; } }
        internal int level;
        internal InfoT operatorInfo;
    }

    public class ObjectInfo<T,S,M>where S:ObjectInfo<T,S,M>,new() where M:ObjectManager<T,S,M>
    {
    

        public string tag { get; internal set; }
        public string mark { get; internal set; }
        public int priority { get; internal set; }
        public char dot { get; internal set; }
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


    }
    public class ObjectManager<T,InfoT,M>where InfoT:ObjectInfo<T,InfoT,M> ,new() where M:ObjectManager<T,InfoT,M>
    {
        static int max = 6;
        static int maxPriority = 0;
        static Dictionary<char, HeadCharSet<T,InfoT,M>> stringLic = new Dictionary<char, HeadCharSet<T, InfoT,M>>();
        bool canOver = true;
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

  

     
        public T GetObject(string text, List<ScanInfo<T,InfoT,M>> infos)
        {
            int t = Helper.clean(ref text);
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

                var v = ois.operatorInfo.factory(text, ois, infos, (M)this);
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
        public void ScanForOperators(string text, List<ScanInfo<T,InfoT,M>> list)
        {

            ReadOnlySpan<char> text_ = text.AsSpan();
            int leftParentheses = 0;
            int rightParentheses = 0;
            int level;
            ScanInfo<T,InfoT,M> osi;
            char c;
            HeadCharSet<T,InfoT,M> hs;
            InfoT info;
            for (int i = 0; i < text_.Length; i++)
            {
                if (text_[i] == '(')
                {
                    leftParentheses++;
                }
                if (text_[i] == ')')
                {
                    rightParentheses++;
                }
                c = text_[i];
                if (stringLic.TryGetValue(c, out hs))
                {
                    info = hs.Match(text_.Slice(i));
                    if (info != null)
                    {
                        level = leftParentheses - rightParentheses;
                        osi = new ScanInfo<T,InfoT,M>(i, level, info);
                        list.Add(osi);
                        i += info.mark.Length - 1;
                    }

                }
            }
        }
    }
    public class HeadCharSet<T,InfoT,M>where InfoT:ObjectInfo<T,InfoT,M>,new() where M:ObjectManager<T,InfoT,M>
    {
        internal char head;
        internal int maxLength;
        internal int capacity = 8;
        Dictionary<string,InfoT>[] context;

        public HeadCharSet(char head)
        {
            this.head = head;
            this.maxLength = 0;
            this.context = new Dictionary<string, InfoT>[capacity];
        }

        public void clear()
        {
            foreach (var v in context)
            {
                if (v != null)
                    v.Clear();
            }
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
