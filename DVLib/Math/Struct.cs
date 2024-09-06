using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using Images;
using System.IO;
using DVOSLib;
using System.Xml;
using MachineLearning;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MathBase
{ 
public class CubeData<T>
{
	public int Width { get; internal set; }
	public int Height { get; internal set; }
	public int Depth { get; internal set; }

	internal T[][,] Data;
	public T this[int x, int y, int z]
	{
		get { return Data[z][x, y]; }
		set { Data[z][x, y] = value; }
	}

	public Map<T> getXY(int z)
	{
		Map<T> map = new Map<T>(Width, Height);
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				map.Data[x, y] = Data[z][x, y];
			}
		}
		return map;
	}

	public void setXY(int z, T[,] data)
	{
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				Data[z][x, y] = data[x, y];
			}
		}
	}
	public void setXY(int z, Map<T> data)
	{
		setXY(z, data.Data);
	}

	public Map<T> getXZ(int y)
	{
		Map<T> map = new Map<T>(Width, Depth);
		for (int x = 0; x < Width; x++)
		{
			for (int z = 0; z < Depth; z++)
			{
				map.Data[x, z] = Data[z][x, y];
			}
		}
		return map;
	}

	public void setXZ(int y, T[,] data)
	{
		for (int x = 0; x < Width; x++)
		{
			for (int z = 0; z < Depth; z++)
			{
				Data[z][x, y] = data[x, z];
			}
		}
	}
	public void setXZ(int z, Map<T> data)
	{
		setXZ(z, data.Data);
	}

	public Map<T> getYZ(int x)
	{
		Map<T> map = new Map<T>(Height, Depth);
		for (int y = 0; y < Height; y++)
		{
			for (int z = 0; z < Depth; z++)
			{
				map.Data[y, z] = Data[z][x, y];
			}
		}
		return map;
	}

	public void setYZ(int x, T[,] data)
	{
		for (int y = 0; y < Height; y++)
		{
			for (int z = 0; z < Depth; z++)
			{
				Data[z][x, y] = data[y, z];
			}
		}
	}
	public void setYZ(int z, Map<T> data)
	{
		setYZ(z, data.Data);
	}
	public CubeData(int width, int height, int depth)
	{
		Width = width;
		Height = height;
		Depth = depth;
		Data = new T[depth][,];
		for (int i = 0; i < depth; i++)
		{
			Data[i] = new T[width, height];
		}
	}
}



public class Map<T>
{
	public virtual void onCreate()
	{

	}
	internal T[,] Data;

	public int Width { get; internal set; }
	public int Height { get; internal set; }

	public Map()

	{

	}
	public Map(int width, int height)
	{
		this.Width = width;
		this.Height = height;
		Data = new T[Width, Height];
		onCreate();
	}

	public Map<T> Clone()
	{


		Map<T> c = new Map<T>(Width, Height);

		T[,] cs = c.Data;
		Array.Copy(Data, cs, Width * Height);
		return c;
	}

	public Map<T> Move(int x, int y)
	{

		int start_x = Math.Max(x, 0);
		int start_y = Math.Max(y, 0);
		int end_x = Math.Min(Width, Width + x);
		int end_Y = Math.Min(Height, Height + y);
		int x0 = start_x - x;
		int y0;
		Map<T> result = new Map<T>(Width, Height);
		T[,] data = result.Data;
		for (int i = start_x; i < end_x; i++, x++, x0++)
		{
			y0 = start_y - y;
			for (int j = start_y; j < end_Y; j++, y0++)
			{
				data[i, j] = Data[x0, y0];
			}
		}
		return result;
	}
		public bool contains(int x,int y)
		{
			return x>=0&&y>=0&&x<Width && y<Height;
		}

		public Map<T> getBox(int x,int y,int width,int height)
		{
			Map<T> map = new Map<T>(width, height);
			int x0, y0;
			x0 = x;
			for(int i = 0; i < width; i++)
			{
				y0 = y;
				for(int j = 0; j < height; j++)
				{
					if(contains(x0,y0))
					{
						map[i,j]=Data[x0,y0];
					}
					y0 ++;
				}
				x0++;
			}
			return map;
		}
		public Map<T> getPolarLog(int pieces, int radius, double realMax, double realMin = 1, double startDegree = 0, double degreeRange=360)
		{
			degreeRange /= 360;
			Map<T> mapR = new Map<T>(pieces, radius);
			mapR.ParallelForeach((x,y,d) => {
				d[x, y] = Data[0,0];
			});
			double logMax=Math.Log(realMax);
			double logMin=Math.Log(realMin);
			Vector2 center=new Vector2(Width/2.0,Height/2.0);

			double dLog=(logMax-logMin)/(radius);
			double f = (Math.Min(Width,Height)/2 ) / (realMax);
			double log;
			double theta;
			double dtheta = Math.PI* 2 *degreeRange / pieces;
			Vector2 temp,dir;
			Vector2i v;
			theta = startDegree*Math.PI/180;
			for(int j=0;j<pieces;j++)
				{
				
				temp = Vector2.fromAngle(1, theta);
				log = logMin;
				for(int i=0;i<radius;i++)
			
				{
					dir = center + temp * ((Math.Exp(log) ) * f);

					v = dir.getVector2i();
					if(contains(v.X,v.Y))
					{
                        mapR[j, i] = Data[v.X, v.Y];
					}
					log += dLog;
				}
				theta += dtheta;
			}
			return mapR;
		}
		public Map<T> getPolar(int pieces,int radius)
		{
			Map<T> map = new Map<T>(pieces,radius);
			Vector2 center = new Vector2(Width/2.0, Height/2.0);
			double l = Math.Min(Width, Height)/2;
			l /= radius;
			Vector2 pos, dir;
			Vector2i posi;
			double theta = 0;
			double dtheta = Math.PI * 2 / pieces;
			for(int i=0;i<pieces;i++ )
			{
				dir = Vector2.fromAngle(l, theta);
				pos= center;
				for(int j=0;j<radius;j++)
				{

					posi = pos.getClost();
					if(contains(posi.X,posi.Y))
					{
						map[i, j] = Data[posi.X, posi.Y];
					}

					pos += dir;
				}

				theta += dtheta;
			}

			return map;
		}


		public Span<T> getRowSpan(int row)
		{
			return getSpan().Slice(row*Width,Width);
		}
		public Span<T> getSpan()
		{
			var s = MemoryMarshal.CreateSpan(ref Data[0, 0], Data.Length);
			return s;
		}
	

		public void drawTriangleNew(T color, Vector2 a1, Vector2 a2, Vector2 a3)
		{

			var r=from Vector2 v in new Vector2[]{ a1,a2,a3} orderby v.Y descending select v;

			var top=r.ElementAt(0);
			var mid=r.ElementAt(1);
			var bottom=r.ElementAt(2);
			Vector2 left, right;
			if(top.Y==mid.Y)
			{
				if(top.X<mid.X)
				{
					left=top; right=mid;
				}
				else if(top.X==mid.X)
				{
					return;
				}
				else
				{
					left=mid;
					right=top;
				}
				drawTriangleDown(color, bottom, left, right);
			}
			else if(mid.Y==bottom.Y)
			{
				if(mid.X<bottom.X)
				{
					left = mid;
					right=bottom;
				}
				else if(mid.X==bottom.X)
				{
					return;
				}
				else
				{
					left = bottom;
					right=mid;
				}
                drawTriangleUp(color, bottom, left, right);
            }
			else
			{
				var other=bottom+(top-bottom)/(top.Y-bottom.Y)*(mid.Y-bottom.Y);
				if(other.X<mid.X)
				{
					left = other;
					right = mid;
				}
				else if(other.X>mid.X)
				{
					right = other;
					left = mid;
				}
				else
				{
					return;
				}
				drawTriangleUp(color,top, left, right);
			    drawTriangleDown(color,bottom, left, right);
			}

		}
        public void drawTriangleUp(T color, Vector2 up, Vector2 left, Vector2 right)
        {
			double height = up.Y ;
			double hl = height - left.Y;
			var start = left;
			var end = right;
			var dLeft = (  up.X-left.X) / hl ;
			var dRight = (up.X-right.X  ) / hl;
			int startIndex, endIndex,l;
			if(height>=Height)
			{
				height = Height;
			}
			if(left.Y<0)
			{
				start = new Vector2(-left.Y * dLeft+start.X, 0);
			}
			for(int h=(int)(left.Y);h<height;h++)
			{
				var span=getRowSpan(h);
				startIndex= (int)start.X;
				endIndex= (int)end.X;
			
				if(startIndex<0)
				{
					startIndex = 0;
				}
				if(endIndex>Width)
				{
					endIndex = Width;
				}	
				l = endIndex - startIndex;
				if(l>0)
				{
					span.Slice(startIndex,l).Fill(color);
				}
				start=start.add(dLeft, 1);
				end=end.add(dRight, 1);
			}
        }
        public void drawTriangleDown(T color, Vector2 up, Vector2 left, Vector2 right)
        {
            double height = up.Y;
            double hl = left.Y-height  ;
            var start = left;
            var end = right;
            var dLeft = (up.X- left.X ) / hl;
            var dRight = ( up.X-right.X ) / hl;
            int startIndex, endIndex, l;
            if (height<0)
            {
                height =0;
            }
            if (left.Y >=Height)
            {
                start = new Vector2((left.Y-Height) * dLeft + start.X, Height);
            }
            for (int h = (int)(left.Y); h >= height; h--)
            {
                var span = getRowSpan(h);
                startIndex = (int)start.X;
                endIndex = (int)end.X;

                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                if (endIndex > Width)
                {
                    endIndex = Width;
                }
                l = endIndex - startIndex;
                if (l > 0)
                {
                    span.Slice(startIndex, l).Fill(color);
                }

              start=  start.add(dLeft, -1);
             end=   end.add(dRight, -1);
            }
        }

        public void drawTriangle(T color, Vector2 a1, Vector2 a2, Vector2 a3, bool checkRange = true)
		{
			Map<T> map = this;
			Vector2 left, mid, right;

			var list = (from x in new Vector2[] { a1, a2, a3 } orderby x.X ascending select x).ToArray();

			left = list[0];
			mid = list[1];
			right = list[2];
			double l = right.X - left.X;
			double ll = mid.X - left.X;
			double lll = right.X - mid.X;
			double k = (ll) / l;
			Vector2 M = left + (right - left) * k;
			int x1 = (int)(ll);
			int x2 = (int)(l - ll);
			double dy = (right.Y - left.Y) / l;
			double dy2 = (mid.Y - left.Y) / ll;
			double dy3 = (right.Y - mid.Y) / lll;
			double dh = Math.Abs(dy - dy2);
			double dir = mid.Y - M.Y > 0 ? 1 : -1;
			Vector2 start = left;
			Vector2 pos;
			double h = 0;
			if(checkRange)
			{
				for (int i = 0; i < ll; i++)
				{
					start = start.add(1, dy);
					pos = start;
					setCheckRange((int)pos.X, (int)pos.Y, color);
					for (int j = 0; j < h; j++)
					{
						setCheckRange((int)pos.X, (int)pos.Y, color);
						pos = pos.add(0, dir);
					}
					pos = start.add(0, h * dir); 
					setCheckRange((int)pos.X, (int)pos.Y, color);
					h += dh;
				}
				h = dh * ll;
				dh = -Math.Abs(dy - dy3);
				for (int i = 0; i < lll; i++)
				{
					h += dh;

					start = start.add(1, dy);
					pos = start; 
					setCheckRange((int)pos.X, (int)pos.Y, color);
					for (int j = 0; j < h; j++)
					{
					setCheckRange((int)pos.X, (int)pos.Y, color);
						pos = pos.add(0, dir);
					}
					pos = start.add(0, h * dir);
					setCheckRange((int)pos.X, (int)pos.Y, color);
				}
			}
			else
			{
for (int i = 0; i < ll; i++)
			{
				start = start.add(1, dy);
				pos = start;
				map[(int)pos.X, (int)pos.Y] = color;
				for (int j = 0; j < h; j++)
				{
					map[(int)pos.X, (int)pos.Y] = color;
					pos = pos.add(0, dir);
				}
				pos = start.add(0, h * dir);
				map[(int)pos.X, (int)pos.Y] = color;
				h += dh;
			}
			dh = -Math.Abs(dy - dy3);
			for (int i = 0; i < lll; i++)
			{
				h += dh;

				start = start.add(1, dy);
				pos = start;
				map[(int)pos.X, (int)pos.Y] = color;
				for (int j = 0; j < h; j++)
				{
					map[(int)pos.X, (int)pos.Y] = color;
					pos = pos.add(0, dir);
				}
				pos = start.add(0, h * dir);
				map[(int)pos.X, (int)pos.Y] = color;
			}
			}
			
		}
		public void drawTriangle_(T color, Vector2 a, Vector2 b, Vector2 c, bool checkRange = true)
	{
		Vector2 bc = c - b;
		Vector2 ba = a - b;
		Vector2 paintLine = ba.nolrmalized();
		double cos = ba.cos(bc);


		double sin = Math.Sqrt(1 - cos * cos);
		Vector2 dx = bc.nolrmalized() * (1 / sin) * 0.45;
		Vector2 p;
		Vector2 p0 = b;
		double a_b = ba.value();
		double b_c = bc.value();
		double d1 = dx.value() * a_b / b_c;

		if (checkRange)
		{
			for (; b_c > 0; b_c -= 0.45)
			{
				p = p0;

				for (double t = a_b; t > 0; t -= 1)
				{


					setCheckRange((int)Math.Round(p.X), (int)Math.Round(p.Y), color);
					p += paintLine;


				}


				p0 = p0 + dx;
				a_b -= d1;

			}

		}
		else
		{
			for (; b_c > 0; b_c -= 0.45)
			{
				p = p0;

				for (double t = a_b; t > 0; t -= 1)
				{

					this[(int)(p.X), (int)(p.Y)] = color;
					p += paintLine;


				}


				p0 = p0 + dx;
				a_b -= d1;

			}
		}


	}
	public Vector2 drawline(Vector2 p1, Vector2 p2, int width, T c, double maxValue)
	{

		Vector2 d = (p2 - p1);
		Vector2 t = d.row(90).nolrmalized();
		Vector2 ptemp;
		p1 = p1 - t * (width / 2.0);

		double l = d.value();
		d = d.nolrmalized();
		l++;
		if (l > maxValue)
		{
			l = maxValue;
		}

		int x, y;
		for (int i = 0; i < l; i++)
		{
			ptemp = p1;
			for (int j = 0; j < width; j++)
			{

				x = (int)Math.Round(ptemp.X);
				y = (int)Math.Round(ptemp.Y);
				if (x >= 0 && y >= 0 && x < Width && y < Height)
				{
					Data[x, y] = c;
				}
				ptemp += t;
			}


			p1 += d;
		}
		return p2;
	}
	public Vector2 drawline(Vector2 p1, Vector2 p2, T c, double maxValue = double.PositiveInfinity)
	{
		return drawline(p1, p2, 1, c, maxValue);
	}
	public void drawCross(int x, int y, T c, int width)
	{
		for (int i = -width; i <= width; i++)
		{
			setCheckRange(x + i, y, c);
		}
		for (int i = -width; i <= width; i++)
		{
			setCheckRange(x, y + i, c);
		}
	}
	public void drawCross(Vector2 vector, T c, int width)
	{
		int x = (int)Math.Round(vector.X);
		int y = (int)Math.Round(vector.Y);
		drawCross(x, y, c, width);
	}

	public void setCheckRange(int x, int y, T value)
	{
		if (x >= 0 && y >= 0 && x < Width && y < Height)
		{
			this[x, y] = value;
		}
	}
	public void Foreach(mapForeach<T> function)// 使用函数遍历全图
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				function(i, j, Data);
			}
		}

	}
	int[][] widthRange_;
	int[][] WidthRange { get { if (widthRange_ == null) { widthRange_ = Width.getRange().split(8); } return widthRange_; } }

	public void ParallelForeach(mapForeach<T> function)
	{
		Parallel.ForEach(WidthRange, (int[] index) =>
		{
			foreach (int i in index)
			{
				for (int j = 0; j < Height; j++)
				{
					function(i, j, Data);
				}
			}
		});
	}
	public T this[int x, int y]
	{
		get { return Data[x, y]; }
		set { Data[x, y] = value; }
	}

	public T[] getArray()
	{
		T[] r = new T[Width * Height];
		int n = 0;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				r[n] = this[i, j];
				n++;
			}
		}
		return r;
	}

	public T[] GetColumn(int index)
	{
		T[] ret = new T[Height];
		for (int i = 0; i < Height; i++)
		{
			ret[i] = this[index, i];
		}
		return ret;
	}
	public int SetColumn(int index, T[] src)
	{
		for (int i = 0; i < src.Length; i++)
		{
			this[index, i] = (i < src.Length) ? src[i] : default(T);
		}
		return 0;
	}
	//获取行和列
	public T[] GetRow(int index)
	{
		return getRowSpan(index).ToArray();
	}

	public static Map<T> operator &(Map<T> map, MathFunction<T> function)
	{
		Map<T> r = new Map<T>(map.Width, map.Height);
		T[,] d = map.Data;
		r.ParallelForeach((int x, int y, T[,] data) =>
		{
			data[x, y] = function(d[x, y]);

		});
		return r;
	}
	public static Map<T> operator &(MathFunction<T> function, Map<T> map)
	{
		Map<T> r = new Map<T>(map.Width, map.Height);
		T[,] d = map.Data;
		r.ParallelForeach((int x, int y, T[,] data) =>
		{
			data[x, y] = function(d[x, y]);

		});
		return r;
	}
	public int SetRow(int index, T[] src)
	{
			var a=getRowSpan(index);
			Span<T> span = src;
			span.CopyTo(a);
		return 0;
	}
}

/// <summary>
/// 用于遍历的的函数（委托）
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="data"></param>
public delegate void mapForeach<T>(int x, int y, T[,] data);

/// <summary>
/// T类型的二维数据结构，bitmap就是T类型为int的Map（int 32位 低8位储存B， 次低8位储存G ，次高8位储存R ，最高8位暂时没有用到）
/// </summary>
/// <typeparam name="T"></typeparam>
/// 

public delegate T MathFunction<T>(T value);
}