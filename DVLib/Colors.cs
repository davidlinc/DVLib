using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVOSLib;
using MathBase;
using System.Threading.Tasks;

namespace Images
{
	public static class Colors
	{
		public static readonly int Gray = unchecked((int)(0xffbebebe));
		public static readonly int Black = unchecked((int)(0xff000000));
		public static readonly int Red= unchecked((int)(0xffff0000));
		public static readonly int Green= unchecked((int)(0xff00ff00));
		public static readonly int Blue = unchecked((int)(0xff0000ff));
		public static readonly int White = unchecked((int)(0xffffffff));
		public static readonly int Alpha=unchecked((int)(0xff000000));
		static Random  rand=new Random();
		public static int Random { get { return (255, rand.Next(256), rand.Next(256), rand.Next(256)).ARGB2Int()  ; } }
		public static Vector3 RGBToHSV(int c)
		{
			return RGBToHSV((c >> 16 )&255,(c>>8)&255,c&255);
		}
			public static Vector3 RGBToHSV(int r,int g,int b)
		{

			double r_ = r / 255.0;
			double g_ = g / 255.0;
			double b_ = b / 255.0;
			double CMam = Math.Max(r_, Math.Max(g_, b_));
			double CMin = Math.Min(r_, Math.Min(g_, b_));
			double delta = CMam - CMin;
			
			double h=0;
			if(delta==0)
			{
				h = 0;
			}
			else
			if(CMam==r_)
			{
				h = (g_ - b_) / delta * 60;
			}
			else if(CMam==g_)
			{
				h=((b_ - r_) / delta + 2) * 60;
			}
			else if(CMam==b_)
			{
				h = ((r_ - g_) / delta + 4) * 60;
			}

			if(h<0)
			{
				h += 360;
			}
			double s;
			if(CMam==0)
			{
				s = 0;
			}
			else
			{
				s = delta / CMam;
			}
			double v = CMam;
			return new Vector3(s, h, v);
		}
		public static int HSVToRGB(Vector3 v)
		{
			return HSVToRGB(v.x, v.y, v.z);
		}

		public static int HSVToRGB(double s,double h,double v)
		{
			if(h<0)
			{
				h += 360;
			}

			if(h>300)
			{
				h -= 360;
			}
			double r_, g_, b_;
			double delta;
			double Cmax=v, Cmin;
			if(h==0)
			{
				delta = 0;
			}
			delta = s * Cmax;
			Cmin = Cmax - delta;
			if (h == 0)
			{
				r_ = g_ = b_ = Cmax;
			}
			else if(h<60)
			{
				r_ = Cmax;

				if(h>0)
				{
					b_= Cmin;
					g_ = h / 60 * delta + b_;
				}
				else
				{
					g_ = Cmin;

					b_ = g_ - h / 60 * delta;
				}
			}
			else if(h<180)
			{
				g_ = Cmax;
				 if(h>120)
				{
					r_ = Cmin;
					b_ = (h - 120) / 60 * delta + r_;

				}
				 else
				{
					b_= Cmin;
					r_ = b_ - (h - 120) / 60 * delta;
				}
			}
			else
			{
				b_ = Cmax;
				if (h>240)
				{
					g_ = Cmin;
					r_ = (h - 240) / 60 * delta + g_;
				}
				else
				{
					r_= Cmin;
					g_ = r_ - (h - 240) / 60 * delta;

				}
			}
			int r = (int)Math.Round(r_ * 255);
			int g = (int)Math.Round(g_ * 255);
			int b = (int)Math.Round(b_ * 255);
			if(r>255)
			{
				r = 255;
			}
			if(r<0)
			{
				r = 0;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (b < 0)
			{
				b = 0;
			}
			return Colors.Alpha|(r<<16)|(g<<8)|(b<<0);
		}
		public static int similarColor(this int color, int dc)
		{
			var Color = color.Int2RGB();
			if (dc > 127)
			{
				dc =127;
			}
			if (dc < 1)
			{
				dc =1;
			}
			int r, g, b;
			if (Color.r - dc < 0)
			{
				r = Color.r + dc;
			}
			else
			{
				r = Color.r - dc;
			}
			if (Color.g - dc < 0)
			{
				g = Color.g + dc;
			}
			else
			{
				g = Color.g - dc;
			}
			if (Color.b - dc < 0)
			{
				b = Color.b + dc;
			}
			else
			{
				b = Color.b - dc;
			}
			return (r, g, b).RGB2Int();
		}
	}
}
