using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper
{
	public class DoubleStack<T>
	{
		Stack<T> left=new Stack<T>();
		Stack<T> right=new Stack<T>();
		public T current { get; private set; }

		public void add(T item)
		{
			while(right.TryPop(out T i))
			{
				left.Push(i);
			}
			if((!left.TryPeek(out T o))||(!o.Equals(item)))
			{
             left.Push(item);
			}
		}

		public T rollOut()
		{
			if(left.TryPop(out T outT))
			{
				if(current!=null)
				{
				right.Push(current);
				}
				current = outT;
			}
			return current;
		}

		public T rollBack()
		{
			if(right.TryPop(out T outT))
			{
				if(current!=null)
				{
					left.Push(current);
				}
				current = outT;
			}
			return current;
		}
	}
}
