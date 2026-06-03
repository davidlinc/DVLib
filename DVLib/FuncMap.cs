using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using Physics;
using System.IO;
using System.Drawing.Imaging;
using Physics.Physics2;
using Images;
using vector2 = MathBase.Vector2;
using DVOSLib;

namespace Images
{
	public class Plot2D
	{
		public static preRenderFont fontRenderer;
	    static Plot2D()
		{
			fontRenderer = FontManager.renderFont;
		}
		public bool no_zore = false;
		List<Vector2> points = new List<Vector2>();
		vector2 Rmaxp;
		vector2 Rminp;
		bool usingrange = false;
		string unitX = "X";
		string unitY = "Y";
		public bool usingn = true;
		int xf = 3;
		int yf = 4;
		int Xn = 5;
		int Yn = 5;
		int dis = 24;
		int width;
		int height;
		int stringc = Colors.Black;
		int xyc = Colors.Red;
		int linec = Colors.Green;
		public void clean()

		{
			points.Clear();
		}
		public void setrange(vector2 r0, vector2 r1)
		{
			Rminp = r0; Rmaxp = r1;
			usingrange = true;
		}
		public void setstring(string sx, string sy)
		{
			unitX = sx;
			unitY = sy;
		}
		public void setform(int x, int y)
		{
			xf = x;
			yf = y;
		}
		public void setn(int x, int y)
		{
			Xn = x;
			Yn = y;
		}
		public void setdis(int d)
		{
			dis = d;
		}
		public void setcolor(int c1, int c2, int c3)
		{
			stringc = c1;
			xyc = c2;
			linec = c3;
		}
		public Plot2D(int w, int h)
		{
			width = w;
			height = h;
		}
		vector2 ptp(vector2 v)
		{
			vector2 maxP = maxp();
			vector2 minP = minp();
			double xzoom = (width - (double)dis * 4) / (maxP.X - minP.X);
			double yzoom = (height - (double)dis * 2) / (maxP.Y - minP.Y);
			vector2 result = new vector2((v.X - minP.X) * xzoom + dis, height - ((v.Y - minP.Y) * yzoom + dis));
			return result;
		}
		public void add(double x, double y)
		{
			points.Add(new vector2(x, y));
		}
		public void add(vector2 v)
		{
			points.Add(v);
		}
		void drawxy(bitmap map)
		{
			vector2 maxP = maxp();
			vector2 minP = minp();
			map.drawline(ptp(minP), ptp(new vector2(minP.X, maxP.Y + 1)),  xyc);
			map.drawline(ptp(minP), ptp(new vector2(maxP.X + 1, minP.Y)),  xyc);
			double dx = (maxP.X - minP.X) / Xn;
			double dy = (maxP.Y - minP.Y) / Yn;
			for (int i = 1; i <= Xn; i++)
			{
				map.drawline( ptp(minP + new vector2(i * dx, 0)), ptp(minP + new vector2(i * dx, 0)) - new vector2(0, dis / 2),  xyc);
			}
			for (int i = 1; i <= Yn; i++)
			{
				map.drawline(ptp(minP + new vector2(0, i * dy)), ptp(minP + new vector2(0, i * dy)) + new vector2(dis / 2, 0),  xyc);
			}
		}
		void drawpoints(bitmap map)
		{
			for (int i = 0; i < points.Count - 1; i++)
			{
				if (!(points[i].X == 0 && points[i].Y == 0) || !no_zore)
				{
					map.drawline( ptp(points[i]), ptp(points[i + 1]),  linec);
					map.drawCross( ptp(points[i]),  linec,1);
				}
			}
		}
		vector2 maxp()
		{
			if (usingrange)
			{
				return Rmaxp;
			}
			double maxX = points[0].X;
			double maxY = points[0].Y;
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].X > maxX)
				{
					maxX = points[i].X;
				}
				if (points[i].Y > maxY)
				{
					maxY = points[i].Y;
				}
			}
			return new vector2(maxX, maxY);
		}
		vector2 minp()
		{
			if (usingrange)
			{
				return Rminp;
			}
			double minX = points[0].X;
			double minY = points[0].Y;
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].X < minX)
				{
					minX = points[i].X;
				}
				if (points[i].Y < minY)
				{
					minY = points[i].Y;
				}
			}
			return new vector2(minX, minY);
		}
		static public Plot2D fromfile(string path, int w, int h)
		{
			FileStream file = new FileStream(path, FileMode.Open);
			StreamReader reader = new StreamReader(file);
			Plot2D functionmap = new Plot2D(w, h);
			bool flag = true;
			string x, y;
			string temp;
			List<double> X, Y;
			X = new List<double>(0);
			Y = new List<double>(0);
			x = reader.ReadLine();
			while (flag)
			{
				temp = reader.ReadLine();
				if (temp.isnumber())
				{
					X.Add(Convert.ToDouble(temp));
				}
				else
				{
					y = temp;
					flag = false;
				}
			}
			flag = true;
			while (flag)
			{
				temp = reader.ReadLine();
				if (temp.isnumber())
				{
					Y.Add(Convert.ToDouble(temp));
				}
				else
				{
					flag = false;
				}
			}
			if (X.Count == Y.Count)
			{

				for (int i = 0; i < X.Count; i++)
				{
					functionmap.add(new vector2(X[i], Y[i]));
				}

			}
			file.Close();
			reader.Close();
			return functionmap;
		}

		public bitmap get()
		{
			bitmap map = new bitmap(width, height);
			if(points.Count<2)
			{
				return map;
			}
			map.paint(Colors.White);

			drawxy(map);
			drawpoints(map);	
			vector2 maxP = maxp();
			vector2 minP = minp();
			double dx = (maxP.X - minP.X) / Xn;
			double dy = (maxP.Y - minP.Y) / Yn;
			if (usingn)
			{
				for (int i = 1; i <= Xn; i++)
				{
					fontRenderer.drawString(map, (float)ptp(minP + new vector2(i * dx, 0)).X - dis *2, height - dis * 2f, dis, dis, dis*3/5, dis, 10, (i * dx + minP.X).ToString("f" + xf), Colors.Black);
		        }
				for (int i = 1; i <= Yn; i++)
				{
					fontRenderer.drawString(map, (float)dis, (float)ptp(minP + new vector2(0, i * dy)).Y, dis, dis, dis*3/5, dis, 10, (i * dy + minP.Y).ToString("f" + yf), Colors.Black);
				}
			}
			/*
			Bitmap result = map;
			Graphics graphics = Graphics.FromImage(result);
			graphics.DrawString(unitX, new Font("宋体", dis / 4 * 3), new SolidBrush(stringc), width - dis * unitX.Length, height - dis);
			graphics.DrawString(unitY, new Font("宋体", dis / 4 * 3), new SolidBrush(stringc), dis, 0);
			
		*/

			return map;
		}
	}
}