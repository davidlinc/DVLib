using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MathBase
{
	public unsafe delegate void Array2Operator<T>(int y, T* value);
	public unsafe struct Array2<T>:IDisposable,IList<T>
	{
		public T* intp { get;private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Length { get; private set; }

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly => throw new NotImplementedException();

		public T this[int index] { get => intp[index]; set =>intp[index]=value; }

		
		Array2(int width,int height,int length,T* intp)
		{
			this.intp = intp;
			Width = width;
			Height = height;
			Length = length;
		}
		public Array2 (int width,int height)
		{
			this.Width = width;
			this.Height = height;
			this.Length = height * width;
			intp = (T*)Marshal.AllocHGlobal(Length * Marshal.SizeOf(typeof(T)));
			int index = 0;
		}

		public unsafe void RTC(Array2<T> r)
		{
			T* p = r.intp;
			T*  t=intp;
			T* d;
			int h = Height;
			int w=Width;
				for (int j = 0; j <h; j++)
			{
				d = p + j;
				for (int i = 0; i < w; i++)
			{
					*d = *t;
					t++;
					d += h;
				}
			}
		}

		public Array2<T> Foreach(Array2Operator<T> array2Operator)
		{
			//int* mi=stackalloc int[Height];
			Array2<T> a2 = new Array2<T>(Width,Height,Length,intp);
			int size = sizeof(int) * Height;
			//Buffer.MemoryCopy(MapIndex,mi,size,size);
			int i = 0;

			for(int y=0;y<Height;y++)
			{
					array2Operator(y, intp+i);
				i += Width;
			}
			return this;
		}
		
		public T this[int x,int y]
		{
			get { return intp[x + Width * y]; }
			set { intp[x + Width * y] = value; }
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal((IntPtr)intp);
		}

		public int IndexOf(T item)
		{
			for(int i=0;i<Length;i++)
			{
				if (item!=null)
				{
					if(item.Equals(intp[i]))
					return i;
				}
				else if(intp[i]==null)
				{
					return i;
				}

			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			intp[index]=default(T);
		}

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
            for (int i = 0; i < Length; i++)
            {
				intp[i] = default(T);
            }
        }

		public bool Contains(T item)
		{
			return IndexOf(item)>-1;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			int ind = 0;
			int l = Math.Min(Length, array.Length - arrayIndex);
			int lb = l * sizeof(T);
			fixed(T* a=array)
			{
			Buffer.MemoryCopy(intp,a + arrayIndex, lb, lb);
			}
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
