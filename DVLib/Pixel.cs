
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
		}
		readonly int* intP;
		readonly int width;
		readonly int height;

		public Color32ARGB* this[int x, int y]
		{
			
			get
			{
				return ((Color32ARGB*)(intP+ x + width *y));
			}
			set
			{
				*((Color32ARGB*)(intP + x + width*y))=*value; 
			}
		}
		}
	
}
