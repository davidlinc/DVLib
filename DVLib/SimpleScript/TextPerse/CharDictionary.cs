using DVOSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.MathObjectSystem
{

	


	public class CharDictionary<T>:CharDictionary_<T, CharDictionary<T>>
	{
		public CharDictionary(char head):base() {
		setHead(head);
		}
	}

	public class StringDictionary<T>
	{
		Dictionary<char, CharDictionary<T>> valuePairs = new Dictionary<char, CharDictionary<T>>();

		public void Add(string  s,T value)
		{
			char head = s[0];
			if(valuePairs.TryGetValue(head,out CharDictionary<T> c))
			{
				c.Add(s, value);
			}
			else
			{
				CharDictionary<T> cd = new CharDictionary<T>(head).setHead(head);
				cd.Add(s, value);
				valuePairs.Add(head, cd);
			}
		}

		public bool match(string name,out T value,out string key,int head=0)
		{
		
		if (valuePairs.TryGetValue(name[head],out CharDictionary<T> cd))
			{
				if (cd.match(name.AsSpan(head),out value,out key))
				{
					return true;
				}
			}
		


			

			value = default(T);
			key = null;
			return false;
		}
	

	}

	public class CharDictionary_<T,D>where D :CharDictionary_<T,D>
	{
		internal char head;
		internal int maxLength;
		internal int capacity = 8;
		internal Dictionary<string, T>[] context;
		

		public CharDictionary_()
		{

			this.maxLength = 0;
			this.context = new Dictionary<string, T>[capacity];
		}

		public D setHead(char head)
		{
			this.head = head;
			return (D)this;
		}

		public CharDictionary_(char head)
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

		public bool match(ReadOnlySpan<char> input,out T oi,out string s)
		{
			int indexMax = Math.Min(maxLength, input.Length) - 1;
			for (int i = indexMax; i >= 0; i--)
			{
				var v = context[i];
				if (v != null)
				{
					s = input.Slice(0, i + 1).ToString();
					if (v.TryGetValue(s, out oi))
					{
						return true;
					}

				}
			}
			oi = getDefault();
			s = null;
			return false;
		}
	}
}
