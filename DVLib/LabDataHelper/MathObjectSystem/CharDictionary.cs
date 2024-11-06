using DVOSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.MathObjectSystem
{

	public class SCD:CharDictionary<(string,string),SCD>
	{

	}
	public class StringDictionary<T,D>where D :CharDictionary<T,D>,new()
	{
		Dictionary<char, D> valuePairs = new Dictionary<char, D>();

		public void Add(string  s,T value)
		{
			char head = s[0];
			if(valuePairs.TryGetValue(head,out D c))
			{
				c.Add(s, value);
			}
			else
			{
				D cd = new D().setHead(head);
				cd.Add(s, value);
				valuePairs.Add(head, cd);
			}
		}

		public bool match(string name,out T value,int head=0)
		{
		
			if (valuePairs.TryGetValue(name[head],out D cd))
			{
				if (cd.match(name.AsSpan(head),out value))
				{
					return true;
				}
			}


			

			value = default(T);
			return false;
		}
	

	}

	public class CharDictionary<T,D>where D :CharDictionary<T,D>
	{
		internal char head;
		internal int maxLength;
		internal int capacity = 8;
		internal Dictionary<string, T>[] context;
		

		public CharDictionary()
		{

			this.maxLength = 0;
			this.context = new Dictionary<string, T>[capacity];
		}

		public D setHead(char head)
		{
			this.head = head;
			return (D)this;
		}

		public CharDictionary(char head)
		{
			this.head = head;
			this.maxLength = 0;
			this.context = new Dictionary<string,T>[capacity];
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

			public T Add(string mk,T info)
		{
			int l = mk.Length;
			if (l > capacity)
			{
				var nc = new Dictionary<string,T>[l];
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
				context[l - 1] = new Dictionary<string,T>();
			}

			if (context[l - 1].ContainsKey(mk))
			{
				T t =context[l - 1][mk];
				context[l - 1][mk] = info;
				return t;
			}
			context[l - 1].Add(mk, info);
			return default(T);
		}

		public virtual T getDefault()
		{
			return default(T);  
		}
		public T Match(ReadOnlySpan<char> input)
		{
			int indexMax = Math.Min(maxLength, input.Length) - 1;
			for (int i = indexMax; i >= 0; i--)
			{
				var v = context[i];
				if (v != null)
				{
					T oi;
					if (v.TryGetValue(input.Slice(0, i + 1).ToString(), out oi))
					{
						return oi;
					}

				}
			}
			return getDefault();
		}

		public bool match(ReadOnlySpan<char> input,out T oi)
		{
			int indexMax = Math.Min(maxLength, input.Length) - 1;
			for (int i = indexMax; i >= 0; i--)
			{
				var v = context[i];
				if (v != null)
				{
					if (v.TryGetValue(input.Slice(0, i + 1).ToString(), out oi))
					{
						return true;
					}

				}
			}
			oi = getDefault();
			return false;
		}
	}
}
