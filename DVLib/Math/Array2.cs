using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MathBase
{
	public delegate void Array2Operator<T>(int x, int y, T value, Array2<T> data);
	public unsafe struct Array2<T>:IDisposable,IList<T>
	{
		public T* intp { get;private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Length { get; private set; }

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly => throw new NotImplementedException();

		public T this[int index] { get => intp[index]; set =>intp[index]=value; }

		internal int* MapIndex;
		
		Array2(int width,int height,int length,T* intp,int* map)
		{
			this.intp = intp;
			MapIndex = map;
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
			MapIndex= (int*)Marshal.AllocHGlobal(Height * Marshal.SizeOf(typeof(int)));
			int index = 0;
			for(int i = 0; i < height;i++)
			{
				MapIndex[i] = index;
				index += width;
			}
		}

		public Array2<T> Foreach(Array2Operator<T> array2Operator)
		{
			int* mi=stackalloc int[Height];
			Array2<T> a2 = new Array2<T>(Width,Height,Length,intp,MapIndex);
			int size = sizeof(int) * Height;
			Buffer.MemoryCopy(MapIndex,mi,size,size);
			int i = 0;
			for(int y= 0; y < Height;y++)
			for(int x=0;x<Width;x++)
			{
					array2Operator(x, y, intp[i],a2 );
					i++;
			}
			return this;
		}
		
		public T this[int x,int y]
		{
			get { return intp[x + MapIndex[y]]; }
			set { intp[x + MapIndex[y]] = value; }
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
