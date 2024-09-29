
using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Images
{
	public struct Pixel
	{
		
		public byte Blue;
	    public byte Green;
		public byte Red;
		public byte Alpha;
		public unsafe int value { get {
				
				fixed(Pixel* p=&this)
				{
					return *(int*)p;
				}
				
				; } }
		public unsafe static implicit operator Pixel(int color)
		{
			return *((Pixel*)&color);
		}
	}
	public unsafe struct PixelReader
	{
		byte* head;
		public byte Blue
		{
			get { return head[0]; }
			set { head[0] = value; }
		}
		public byte Green
		{
			get { return head[1]; }
			set { head[1] = value; }
		}
		public byte Red
		{
			get { return head[2]; }
			set { head[2] = value; }
		}
		public byte Alpha
		{
			get { return head[3]; }
			set { head[3] = value; }
		}
		public int Value{
			get { return *((int*)head); }
	       set { *((int*)head) = value; }
		}
	}

	public unsafe struct PixelMap
	{
		public PixelMap(Array2<int> map)
		{
			intP = map.intp;
			width = map.Width;
			height = map.Height;
			IndexMap = map.MapIndex;
		}
		readonly int* intP;
		readonly int* IndexMap;
		readonly int width;
		readonly int height;

		public Pixel* this[int x, int y]
		{
			
			get
			{
				return ((Pixel*)(intP+ x + IndexMap[y]));
			}
			set
			{
				*((Pixel*)(intP + x + IndexMap[y]))=*value; 
			}
		}
		}
	
}
