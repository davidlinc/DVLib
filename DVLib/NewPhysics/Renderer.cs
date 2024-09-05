using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVOSLib;
using MathBase;
using System.Threading.Tasks;
using Images;

namespace Images
{
	internal class Renderer
	{

	}

	static public class RendererHelper
	{
	}

	public class Zmap:Map<double>
	{
		public Zmap(int x,int y):base(x,y)
		{
			bitmap = new bitmap(x, y);
		}
	public override void onCreate()
		{
			ParallelForeach((int x, int y, double[,] data) => {
				data[x, y] =double.PositiveInfinity;
			});
			bitmap = new bitmap(Width, Height);
		}
		 bitmap bitmap;

	public	bitmap getSource()
		{
			return bitmap;
		}
	public	bitmap getBitmap()
		{
			bitmap b = bitmap.Clone();
			return b;
		}
		
	}
}
