using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using DVOSLib;
using System.Threading.Tasks;
using Images;
using MathBase;
using System.IO;
using vector2 =MathBase.Vector2;

namespace MathBase
{


	public delegate double Field(double x, double y);


	public class ComplexCube:CubeData<Complex>,IStreamObject<ComplexCube>
	{
		public ComplexCube(int width, int height, int depth) : base(width, height, depth)
		{

		}

		public ComplexCube readStream(Stream stream)
		{
			byte[] buffer = new byte[16];
			Complex[,] temp;
			stream.Read(buffer, 0, 12);
			for (int i=0; i<Depth; i++)
			{
				temp = Data[i];
				for(int j=0;j<Width;j++)
				{
					for(int k=0;k<Height;k++)
					{
						temp[j,k]=Complex.fromStream(stream,buffer);
					}
				}
			}
			return this;
		}

		public ComplexCube readStream(Stream stream, byte[] buffer)
		{
			Complex[,] temp;
			stream.Read(buffer, 0, 12);
			for (int i = 0; i < Depth; i++)
			{
				temp = Data[i];
				for (int j = 0; j < Width; j++)
				{
					for (int k = 0; k < Height; k++)
					{
						temp[j, k] = Complex.fromStream(stream, buffer);
					}
				}
			}
			return this;
		}

		public static ComplexCube fromStream(Stream stream)
		{
			Complex[,] temp;
			byte[] buffer = new byte[16];
			stream.Read(buffer, 0, 12);

			int w = BitConverter.ToInt32(buffer, 0);
			int h = BitConverter.ToInt32(buffer, 4);
			int d = BitConverter.ToInt32(buffer, 8);
			ComplexCube cube = new ComplexCube(w, h, d);
			Complex[][,] Data= cube.Data;
			for (int i = 0; i < d; i++)
			{
				temp = Data[i];
				for (int j = 0; j < w; j++)
				{
					for (int k = 0; k < h; k++)
					{
						temp[j, k] = Complex.fromStream(stream, buffer);
					}
				}
			}
			return cube;
		}
		public static ComplexCube fromStream(Stream stream, byte[] buffer)
		{
			Complex[,] temp;
			stream.Read(buffer, 0, 12);

			int w = BitConverter.ToInt32(buffer, 0);
			int h = BitConverter.ToInt32(buffer, 4);
			int d = BitConverter.ToInt32(buffer, 8);
			ComplexCube cube = new ComplexCube(w, h, d);
			Complex[][,] Data = cube.Data;
			for (int i = 0; i < d; i++)
			{
				temp = Data[i];
				for (int j = 0; j < w; j++)
				{
					for (int k = 0; k < h; k++)
					{
						temp[j, k] = Complex.fromStream(stream, buffer);
					}
				}
			}
			return cube;
		}
		public void writeStream(Stream stream)
		{
			byte[] buffer = new byte[16];
			stream.Write(BitConverter.GetBytes(Width), 0, 4);
			stream.Write(BitConverter.GetBytes(Height), 0, 4);
			stream.Write(BitConverter.GetBytes(Height), 0, 4);
			Complex[,] temp;
			foreach(var data in Data)
			{
				foreach(var comp in data)
				{
					comp.writeStream(stream, buffer);
				}
				stream.Flush();
			}
		}

		public void save(string path)
		{
			FileInfo file=new FileInfo(path);
			if(!Directory.Exists(file.DirectoryName))
			{
				Directory.CreateDirectory(file.DirectoryName);
			}
			FileStream s=new FileStream(file.FullName, FileMode.Create);
			writeStream(s);
			s.Close();


		}
		public void writeStream(Stream stream, byte[] buffer)
		{
			stream.Write(BitConverter.GetBytes(Width), 0, 4);
			stream.Write(BitConverter.GetBytes(Height), 0, 4);
			stream.Write(BitConverter.GetBytes(Height), 0, 4);
			Complex[,] temp;
			foreach (var data in Data)
			{
				foreach (var comp in data)
				{
					comp.writeStream(stream, buffer);
				}
				stream.Flush();
			}
		}
	}


	/// <summary>
	/// 二维复数数据格式
	/// </summary>
	public class ComplexMap : Map<Complex>
	{



		public ComplexMap(int w, int h)
		{

			if (w % 2 == 1)
				w++;
			if (h % 2 == 1)
				h++;
			Width = w;
			Height = h;
			Data = new Complex[w, h]; Data.AsParallel();
		}
		public ComplexMap(Map<Complex> map)
		{
			Data = map.Data;
			Width = map.Width;
			Height = map.Height;
		}
		public new ComplexMap Clone()
		{
			return new ComplexMap(base.Clone());
		}



		public ComplexMap(bitmap b, Channel channel = Channel.Gray)
		{
			int[,] colors = b.Data;
			if (channel == Channel.Gray)
			{
				colors = b.toGray().Data;
			}
			Width = b.Width;
			Height = b.Height;
			Data = new Complex[Width, Height];
			Data.AsParallel();
			;

			switch (channel)
			{
				case Channel.Red:
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{
Data[i, j] = (colors[i, j] >> 16) & 0xff;

						}
					}
					 break;
				case Channel.Green:
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{
							Data[i, j] = (colors[i, j] >> 8) & 0xff;

						}
					}
					; break;
				case Channel.Blue:
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{
Data[i, j] = (colors[i, j]) & 0xff;

						}
					}
					 break;
				case Channel.Alpha:
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{

	Data[i, j] = (colors[i, j] >> 24) & 0xff;
						}
					}
				 break;
				default:
					for (int i = 0; i < Width; i++)
					{
						for (int j = 0; j < Height; j++)
						{
	Data[i, j] = (colors[i, j]) & 0xff;

						}
					}
				 break;
			}

			for (int i = 0; i <Width; i++)
			{ 
				for (int j = 0; j < Height; j++)
				{
					

				}
			}
		}
		public Complex getSafeValue(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < Width && y < Width)
			{
				return Data[x, y];
			}
			return 0;

		}

		public ComplexMap getSized(int x, int y, int xOffset, int yOffset)
		{
			ComplexMap bitmap = new ComplexMap(x, y);

			double rateX = Width * 1.0 / x;
			double rateY = Height * 1.0 / y;
			int px, py, n, xx, yy;
			Complex v;
			bitmap.Foreach((x_, y_, d) =>
			{
				n = 0;
				v = 0;
				px = (int)(rateX * x_) + xOffset;
				py = (int)(rateY * y_) + yOffset;
				for (int i = 0; i < rateX; i++)
				{
					for (int j = 0; j < rateY; j++)
					{
						xx = px + i;
						yy = py + j;
						if (xx < Width && yy < Height)
						{
							v += this[xx, yy];
							n++;
						}
					}
				}
				v /= n;
				bitmap[x_, y_] = v;
			});
			return bitmap;
		}

		public Vector2 getMassPointR()
		{
			Vector2 v = new vector2(0, 0);
			double c=0;
			Foreach((x, y, d) => {
				var vv = d[x, y].realPart;
				v += new vector2(x, y) * vv;
				c += vv;
			});
			return v / c;
		}
		public double[] scanRR()
		{
			Vector2 v = getMassPointR();
			int l = (int)Math.Min(Math.Min(Width - v.X, v.X), Math.Min(Width - v.Y, v.Y));

			if(l>0)
			{
				int rr;
				double[] r = new double[l];
				Vector2 p;
				Foreach((x, y, d) =>
				{
					p = new vector2(x, y);
					rr = (int)p.distance(v);
					if(rr<l)
					{
						r[rr] += d[x, y].realPart;
					}
				});
			
				return r;
			}

			return new double[0];
		}
		public double[] scanR()
		{
			Vector2 v = getMassPointR();
			int l = (int)Math.Min(Math.Min(Width - v.X, v.X), Math.Min(Width - v.Y, v.Y));

			if (l > 0)
			{
				int rr;
				double[] r = new double[l];
				int[]c= new int[l];
				Vector2 p;
				Foreach((x, y, d) =>
				{
					p = new vector2(x, y);
					rr = (int)p.distance(v);
					if (rr < l)
					{
						r[rr] += d[x, y].realPart;
						c[rr]++;
					}
				});

				for(int i=0;i<l;i++)
				{
					if (c[i]>0)
					{
						r[i] /= c[i];
					}
				}
				return r;
			}

			return new double[0];
		}
		public Complex getSafeValue(int x, int y, Complex Default)
		{
			if (x >= 0 && y >= 0 && x < Width && y < Width)
			{
				return Data[x, y];
			}
			return Default;
		}

		public ComplexMap scale(int w, int h)
		{
			if (w == Width && h == Height)
			{
				return Clone();
			}
			ComplexMap bitmap = new ComplexMap(w, h);
			Complex[,] C = bitmap.Data;
			double kx = (double)Width / w;
			double ky = (double)Height / h;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					C[i, j] = Data[(int)(i * kx), (int)(j * ky)];
				}
			}
			return bitmap;
		}
		public Complex getValue(int x, int y)
		{
			return Data[x, y];
		}

		public static ComplexMap getSqureFromBytes(byte[] bytes)
		{

			int w = (int)Math.Round(Math.Sqrt(bytes.Length / 16));
			ComplexMap map = new ComplexMap(w, w);

			int i = 0;
			map.Foreach((x, y, data) =>
			{

				data[x, y] = Complex.fromBytes(bytes, i);
				i += 16;
			});
			return map;
		}
		public byte[] getBytes()
		{
			byte[] bytes = new byte[Width * Height * 16];

			int i = 0;
			Foreach((x, y, data) => {
				data[x, y].getBytes().CopyTo(bytes, i);
				i += 16;

			});
			return bytes;
		}

		public ComplexMap grow(int x, int y)
		{
			ComplexMap map = new ComplexMap(Width * x, Height * y);
			Complex temp;

			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					temp = Data[i, j];
					for (int k = 0; k < x; k++)
					{
						for (int l = 0; l < y; l++)
						{
							map[i * x + k, j * y + l] = temp;
						}
					}
				}

			}
			return map;
		}
		public Vector2 maxR()
		{
			Vector2 result = new Vector2();
			double max = Double.NegativeInfinity,v;
            Foreach((x, y, d) => {
				v = d[x, y].realPart;
				if(v> max)
				{
					max = v;
					result = new vector2(x, y);
				}
			});
	return		result;
		}

		public Vector2 maxRQuick()
		{
			ComplexMap map = scale(Width / 32, Height /32);
			;
			Vector2 v = map.maxR();
			return  v* 32+getBox((int)v.X,(int)v.Y,32,32).maxR();
		}
		public ComplexMap igrow(int x, int y)
		{
			ComplexMap map = new ComplexMap(Width / x, Height / y);
			Complex temp;
			int w = Width / x;
			int h = Height / y;
			int s = x * y;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					temp = new Complex(0, 0);
					for (int k = 0; k < x; k++)
					{
						for (int l = 0; l < y; l++)
						{
							temp += Data[i * x + k, j * y + l];
						}
					}
					map[i, j] = temp / s;
				}

			}
			return map;
		}
		/// <summary>
		/// 通过两次一维离散傅里叶变换进行二维离散傅里叶变换
		/// </summary>
		/// <returns></returns>
		public ComplexMap dft()
		{

			ComplexMap m1 = new ComplexMap(Width, Height);
			ComplexMap map = new ComplexMap(Width, Height);
			double v1 = -2 * Math.PI / Width;
			double v2 = -2 * Math.PI / Height;

			Parallel.For(0, Height, (int j) => {
				for (int i = 0; i < Width; i++)
				{
					double v11 = v1 * i;
					Complex sum1 = 0;

					for (int x = 0; x < Width; x++)
					{
						sum1 += Data[x, j] * Complex.exp(new Complex(0, v11 * x));

					}


					m1.Data[i, j] = sum1;
				}
			});

			Parallel.For(0, Height, (int j) => {
				double v22 = v2 * j;
				for (int i = 0; i < Width; i++)
				{

					Complex sum = 0;



					for (int y = 0; y < Height; y++)
					{

						sum += m1.Data[i, y] * Complex.exp(new Complex(0, v22 * y));
					}
					map.Data[i, j] = sum / Width;
				}
			});

			return map;
		}
		/// <summary>
		/// 傅里叶反变换
		/// </summary>
		/// <returns></returns>
		public ComplexMap idft()
		{

			ComplexMap m1 = new ComplexMap(Width, Height);
			ComplexMap map = new ComplexMap(Width, Height);
			double v1 = 2 * Math.PI / Width;
			double v2 = 2 * Math.PI / Height;

			Parallel.For(0, Height, (int j) => {
				for (int i = 0; i < Width; i++)
				{
					double v11 = v1 * i;
					Complex sum1 = 0;

					for (int x = 0; x < Width; x++)
					{
						sum1 += Data[x, j] * Complex.exp(new Complex(0, v11 * x));

					}


					m1.Data[i, j] = sum1;
				}
			});

			Parallel.For(0, Height, (int j) => {
				double v22 = v2 * j;
				for (int i = 0; i < Width; i++)
				{

					Complex sum = 0;



					for (int y = 0; y < Height; y++)
					{

						sum += m1.Data[i, y] * Complex.exp(new Complex(0, v22 * y));
					}
					map.Data[i, j] = sum / Height;
				}
			});
			double d = 1.0 / map.Width / map.Height;
			//map.Foreach((x, y, da) => { da[x, y] *= d; });
			return map;
		}
		/// <summary>
		/// 转化为位图
		/// </summary>
		/// <returns></returns>
		/// 


		public ComplexMap limtedMax(double factor)
		{
			ComplexMap complex = new ComplexMap(Width, Height);
			double max = double.NegativeInfinity;
			double temp;
			foreach (Complex complex1 in Data)
			{
				temp = complex1.length_2();
				if (temp > max)
				{
					max = temp;
				}
			}
			max = Math.Sqrt(max);
			if (max <= 0)
			{
				max = 1;
			}
			max = 1 / max;
			complex.Foreach((x, y, data) =>
			{
				data[x, y] = Data[x, y] * max * factor; ;
			});
			return complex;
		}
		public bitmap toBitmap()
		{
			bitmap b = new bitmap(Width, Height);
			Parallel.For(0, Width, (int i) => {
				for (int j = 0; j < Height; j++)
				{
					int c = ((int)Data[i, j].length());
					if (c > 255)
					{
						c = 255;
					}
					b.Data[i, j] = (0xff << 24) | c | (c << 8) | (c << 16);
				}
			});
			return b;
		}
		/// <summary>
		/// 移动0频到图像中心
		/// </summary>
		/// <returns></returns>
		public ComplexMap fftShift(bool multiThreads=false)
		{
			ComplexMap newMap = new ComplexMap(Width, Height);
			Complex[,] vs = newMap.Data;

			int hw = Width / 2;
			int hh = Height / 2;
			bool xodd = Width % 2 == 1;
			bool yodd = Height % 2 == 1;
			int dx0, dx1, dy0, dy1;
			if(xodd)
			{
				dx0 = hw + 1;
				dx1 = -hw;

			}
			else
			{
				dx0 = hw;
				dx1
					= -hw;
			}
			if(yodd)
			{
				dy0 = hh + 1;
				dy1 = -hh;
			}
			else
			{
				dy0 = hh;
				dy1 = -hh;
			}
			int[][] idx = hw.getRange().split(8);

			if(multiThreads)
			{
				Parallel.ForEach(Width.getRange().split(20), x => {
				
					foreach(int i in x)
					{
						if (i < hw)
						{
							for (int j = 0; j < Height; j++)
							{
								if (j < hh)
								{
									vs[i, j] = Data[i + dx0, j + dy0];
								}
								else
								{
									vs[i, j] = Data[i + dx0, j + dy1];
								}

							}
						}
						else
						{
							for (int j = 0; j < Height; j++)
							{
								if (j < hh)
								{
									vs[i, j] = Data[i + dx1, j + dy0];
								}
								else
								{
									vs[i, j] = Data[i + dx1, j + dy1];
								}

							}
						}
					}
				
				});
			}
			else
			{
				for(int i=0;i<Width;i++)
				{

					if(i<hw)
					{
						for (int j = 0; j < Height; j++)
						{
							if(j<hh)
							{
								vs[i, j] = Data[i + dx0, j + dy0];
							}
							else
							{
								vs[i, j] =Data[i + dx0, j + dy1];
							}
							
						}
					}
					else
					{
						for (int j = 0; j < Height; j++)
						{
							if (j < hh)
							{
								vs[i, j] = Data[i + dx1, j + dy0];
							}
							else
							{
								vs[i, j] = Data[i + dx1, j + dy1];
							}

						}
					}

				}
			}
			

			return newMap;
		}

		public bitmap toNotColorfullBitmap()
		{
			bitmap b = new bitmap(Width, Height);
			int[,] ints = b.Data;
			double max = 255 * Width * Height;
			double max_ = Double.NegativeInfinity;
			double temp;
			double h, v, s;
			foreach (Complex c in Data)
			{
				temp = c.length_2();
				if (temp > max_)
				{
					max_ = temp;
				}
			}
			max_ = Math.Sqrt(max_);
			Complex comp;
			double l;
			double p;
			Foreach((x, y, d) => {
				comp = d[x, y];
				l = comp.length();
				v = l / max;
				s = l / max_;
				s = 0.75 + Math.Sqrt(s) * 0.25;
				v = Math.Sqrt(v);
				ints[x, y] = Colors.HSVToRGB(s, 0, v);


			});
			return b;
		}
		public ComplexMap intX_simple()
		{
			ComplexMap complex = new ComplexMap(Width, Height);
			int w = Width - 1;
			Complex current;
			Complex last; 
			//double t = 0;
			Complex temp;
			for (int j = 0; j < Height; j++)
			{

				last = 0;
				for (int i = 0; i < Width; i++)
				{
					current = Data[i, j];
					temp = current + last;
					complex[i, j] = temp;
					last = temp;

					//t += complex[i, j].length();
				}

			}
			//DVOS.writeLine(t / Math.PI / 2 / complex.Height);
			return complex;

		}
		public ComplexMap derX_simple()
		{
			ComplexMap complex = new ComplexMap(Width,Height);
			int w = Width - 1;
			Complex current;
			Complex last; double t = 0;

			for (int j=0;j<Height;j++)
				{

				last = Data[w, j];
				for (int i = 0; i < Width; i++)
				{
					current = Data[i,j];
					complex[i, j] = current - last;
					last = current;

					t += complex[i, j].length();
				}

				}
			DVOS.writeLine(t/Math.PI/2/complex.Height);
			return complex;
			
		}
		public ComplexMap intY_simple()
		{
			ComplexMap complex = new ComplexMap(Width, Height);
			int h = Height - 1;

			Complex current;
			Complex temp;
			Complex last;
			for (int i = 0; i < Width; i++)
			{
				last = 0;
				for (int j = 0; j < Height; j++)
				{
					current = Data[i, j];
					temp = last + current;
					complex[i, j] = temp;
					last = temp;
				}
			}
			return complex;

		}
		public ComplexMap derY_simple()
		{
			ComplexMap complex = new ComplexMap(Width, Height);
			int h = Height - 1;
			
			Complex current;
			Complex last;
            for (int i = 0; i < Width; i++)
				{last = Data[i, h];
			for (int j = 0; j < Height; j++)
			{
					current = Data[i, j];
					complex[i, j] = current - last;
					last = current;
			}
			}
			return complex;

		}
		public bitmap toColorfullBitmap()
		{
			bitmap b=new bitmap(Width,Height);
			int[,] ints=b.Data;
			double max = 255 * Width * Height;
		    double max_ = Double.NegativeInfinity;
			double temp;
			double h, v, s;
			foreach(Complex c in Data)
			{
				temp = c.length_2();
				if(temp>max_)
				{
					max_ = temp;
				}
			}
			max_=Math.Sqrt(max_);
			Complex comp;
			double l;
			double p;
			Foreach((x, y, d) => {
				comp = d[x, y];
				l= comp.length();
				v = l / max;
				s = l / max_;
				s=0.75+Math.Sqrt(s)*0.25;
				v=Math.Sqrt(v);
				h = comp.mu() / Math.PI * 180;
				ints[x,y] = Colors.HSVToRGB(s, h, v);


			});
			return b;
		}
		public ComplexMap ifftShift(bool multiThreads = false)
		{
			ComplexMap newMap = new ComplexMap(Width, Height);
			Complex[,] vs = newMap.Data;

			int hw = Width / 2;
			int hh = Height / 2;
			bool xodd = Width % 2 == 1;
			bool yodd = Height % 2 == 1;
			int dx0, dx1, dy0, dy1;
			if (xodd)
			{
				dx0 = hw ;
				dx1 = -hw-1;
				hw++;

			}
			else
			{
				dx0 = hw;
				dx1
					= -hw;
			}
			if (yodd)
			{
				dy0 = hh ;
				dy1 = -hh-1;
				hh++;
			}
			else
			{
				dy0 = hh;
				dy1 = -hh;
			}
			int[][] idx = hw.getRange().split(8);

			if (multiThreads)
			{
				Parallel.ForEach(Width.getRange().split(20), x => {

					foreach (int i in x)
					{
						if (i < hw)
						{
							for (int j = 0; j < Height; j++)
							{
								if (j < hh)
								{
									vs[i, j] = Data[i + dx0, j + dy0];
								}
								else
								{
									vs[i, j] = Data[i + dx0, j + dy1];
								}

							}
						}
						else
						{
							for (int j = 0; j < Height; j++)
							{
								if (j < hh)
								{
									vs[i, j] = Data[i + dx1, j + dy0];
								}
								else
								{
									vs[i, j] = Data[i + dx1, j + dy1];
								}

							}
						}
					}

				});
			}
			else
			{
				for (int i = 0; i < Width; i++)
				{

					if (i < hw)
					{
						for (int j = 0; j < Height; j++)
						{
							if (j < hh)
							{
								vs[i, j] = Data[i + dx0, j + dy0];
							}
							else
							{
								vs[i, j] = Data[i + dx0, j + dy1];
							}

						}
					}
					else
					{
						for (int j = 0; j < Height; j++)
						{
							if (j < hh)
							{
								vs[i, j] = Data[i + dx1, j + dy0];
							}
							else
							{
								vs[i, j] = Data[i + dx1, j + dy1];
							}

						}
					}

				}
			}


			return newMap;
		}
		/// <summary>
		/// 取模
		/// </summary>
		/// <returns></returns>
		public ComplexMap ToAbs()
		{

			ComplexMap complexMap = new ComplexMap(Width, Height);
			Complex[,] t = complexMap.Data;
			Complex[,] s = Data;
			Parallel.For(0, Width, (int i) => {
				for (int j = 0; j < Height; j++)
				{
					t[i, j] = s[i, j].length();
				}
			});
			return complexMap;

		}

		/// <summary>
		/// 两张图片相加
		/// </summary>
		/// <param name="m1"></param>
		/// <param name="m2"></param>
		/// <returns></returns>
		/// 
		public static ComplexMap operator *(ComplexMap m1, ComplexMap m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Complex[,] rd2 = m2.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{
						if (i < m2.Width && j < m2.Height)
						{
							rd[i, j] = rd1[i, j] * rd2[i, j];
						}
						else
						{
							rd[i, j] = rd1[i, j];
						}

					}
				}

			});
			return r;
		}
		public ComplexMap ToAbs_2()
		{

			ComplexMap complexMap = new ComplexMap(Width, Height);
			Complex[,] t = complexMap.Data;
			Complex[,] s = Data;
			Parallel.For(0, Width, (int i) => {
				for (int j = 0; j < Height; j++)
				{
					t[i, j] = s[i, j].length_2();
				}
			});
			return complexMap;

		}
		public ComplexMap ToAbs_4()
		{

			ComplexMap complexMap = new ComplexMap(Width, Height);
			Complex[,] t = complexMap.Data;
			Complex[,] s = Data;
			Parallel.For(0, Width, (int i) => {
				for (int j = 0; j < Height; j++)
				{
					t[i, j] = s[i, j].length_4();
				}
			});
			return complexMap;

		}
		public static ComplexMap operator /(ComplexMap m1, ComplexMap m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Complex[,] rd2 = m2.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{
						if (i < m2.Width && j < m2.Height)
						{
							rd[i, j] = rd1[i, j] / rd2[i, j];
						}
						else
						{
							rd[i, j] = rd1[i, j];
						}

					}
				}

			});
			return r;
		}
		public static ComplexMap operator -(ComplexMap m1, ComplexMap m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Complex[,] rd2 = m2.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{
						if (i < m2.Width && j < m2.Height)
						{
							rd[i, j] = rd1[i, j] - rd2[i, j];
						}
						else
						{
							rd[i, j] = rd1[i, j];
						}

					}
				}

			});
			return r;
		}

		public static ComplexMap operator /(Complex m2, ComplexMap m1)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						rd[i, j] = m2 / rd1[i, j];




					}
				}

			});
			return r;
		}
		public static ComplexMap operator /(ComplexMap m1, Complex m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						rd[i, j] = rd1[i, j] / m2;



					}
				}

			});
			return r;
		}
		public static ComplexMap operator *(Complex m2, ComplexMap m1)
		{
			return m1 * m2;
		}
		public static ComplexMap operator ^(ComplexMap m1, Complex m2)
		{
			ComplexMap m = new ComplexMap(m1.Width, m1.Height);

			int[][] dnx = m1.Width.getRange().split(8);
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						m[i, j] = m1[i, j]^m2;




					}
				}

			});
			return m;
		}
		public static ComplexMap operator *(ComplexMap m1, Complex m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						rd[i, j] = rd1[i, j] * m2;




					}
				}

			});
			return r;
		}


		public  int rangeX(int x)
		{
			if (x < 0)
				x =0;
			if(x>=Width)
			{
				x = Width - 1;
			}
			return x;
		}
		public int rangeY(int y)
		{
			if (y < 0)
				y = 0;
			if (y >= Height)
			{
				y = Height - 1;
			}
			return y;
		}
		public ComplexMap getBox(int x, int y, int width, int height)
		{
			ComplexMap map = new ComplexMap(width, height);
			int x0, y0;
			x0 = x;
			for (int i = 0; i < width; i++)
			{
				y0 = y;
				for (int j = 0; j < height; j++)
				{
					if (contains(x0, y0))
					{
						map[i, j] = Data[x0, y0];
					}
					y0++;
				}
				x0++;
			}
			return map;
		}
		public static ComplexMap addAtRange(ComplexMap complexMap0, ComplexMap complexMap,int minx,int miny,int maxx,int maxy)
		{
			minx = complexMap0.rangeX(minx);
			miny = complexMap0.rangeY(miny);
			maxx = complexMap0.rangeX(maxx);
			maxy = complexMap0.rangeY(maxy);
			ComplexMap map = complexMap0.Clone();
			for(int i = minx; i <= maxx; i++)
			{
				for (int j = miny; j <= maxy; j++)
				{
					map[i, j] += complexMap[i, j];
				}
			}
			return map;
		}
		public ComplexMap addAtRange( ComplexMap complexMap, int minx, int miny, int maxx, int maxy)
		{
			minx = rangeX(minx);
			miny = rangeY(miny);
			maxx = rangeX(maxx);
			maxy = rangeY(maxy);
			ComplexMap map = Clone();
			for (int i = minx; i <= maxx; i++)
			{
				for (int j = miny; j <= maxy; j++)
				{
					map[i, j] += complexMap[i, j];
				}
			}
			return map;
		}

		public static ComplexMap mulAtRange(ComplexMap complexMap0,Complex value, int minx, int miny, int maxx, int maxy)
		{
			minx = complexMap0.rangeX(minx);
			miny = complexMap0.rangeY(miny);
			maxx = complexMap0.rangeX(maxx);
			maxy = complexMap0.rangeY(maxy);
			ComplexMap map = complexMap0.Clone();
			for (int i = minx; i <= maxx; i++)
			{
				for (int j = miny; j <= maxy; j++)
				{
					map[i, j] *=value;
				}
			}
			return map;
		}

		public  ComplexMap mulAtRange( Complex value, int minx, int miny, int maxx, int maxy)
		{
			minx = rangeX(minx);
			miny = rangeY(miny);
			maxx = rangeX(maxx);
			maxy = rangeY(maxy);
			ComplexMap map = Clone();
			for (int i = minx; i <= maxx; i++)
			{
				for (int j = miny; j <= maxy; j++)
				{
					map[i, j] *= value;
				}
			}
			return map;
		}
		public static ComplexMap operator -(Complex m2, ComplexMap m1)
		{
			return m1 - m2;
		}
		public static ComplexMap operator -(ComplexMap m1, Complex m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						rd[i, j] = rd1[i, j] - m2;




					}
				}

			});
			return r;
		}
		public static ComplexMap operator +(Complex m2, ComplexMap m1)
		{
			return m1 + m2;
		}
		public static ComplexMap operator +(ComplexMap m1, Complex m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{

						rd[i, j] = rd1[i, j] + m2;


						rd[i, j] = rd1[i, j];


					}
				}

			});
			return r;
		}
		public static ComplexMap operator +(ComplexMap m1, ComplexMap m2)
		{
			ComplexMap r = new ComplexMap(m1.Width, m1.Height);
			Complex[,] rd = r.Data;
			int[][] dnx = m1.Width.getRange().split(8);
			Complex[,] rd1 = m1.Data;
			Complex[,] rd2 = m2.Data;
			Parallel.ForEach(dnx, (int[] ran) =>
			{
				foreach (int i in ran)
				{
					for (int j = 0; j < m1.Height; j++)
					{
						if (i < m2.Width && j < m2.Height)
						{
							rd[i, j] = rd1[i, j] + rd2[i, j];
						}
						else
						{
							rd[i, j] = rd1[i, j];
						}

					}
				}

			});
			return r;
		}
		/// <summary>
		/// 对数变换
		/// </summary>
		/// <returns></returns>
		public ComplexMap ToLog()
		{
			double k = 255 / Math.Log(256 * Height);
			ComplexMap complexMap = new ComplexMap(Width, Height);
			Complex[,] t = complexMap.Data;
			Complex[,] s = Data;
			Parallel.For(0, Width, (int i) => {
				for (int j = 0; j < Height; j++)
				{
					t[i, j] = Math.Log(s[i, j].length() + 1) * k;
				}
			});
			return complexMap;

		}
	}

	public static class vechelper
	{

	
		static public bool isnumber(this string text)
		{
			if (text == null)
				return false;
			text = text.Trim();

			if (text.Length == 0)
				return false;
			for (int i = 0; i < text.Length; i++)
			{
				if (!text[i].isnumber())
					return false;
			}
			return true;
		}
		static public bool isnumber(this char a)
		{
			char b = '0';
			
				if (a-b<10)
					return true;
			
			if (a == '.')
				return true;
			return false;
		}
		static public bool isNumber(this char a)
		{
			char b = '0';

			if (a - b < 10)
				return true;

			return false;
		}
		static public bool isLetter(this char a)
		{
			char b = 'a';
			char c = 'A';
			if (a - b < 26&&a>b)
				return true;
			if (a - c < 26&&a>c)
				return true;
			return false;
		}
		static public double Xmax(this vector2[] vector2s)
		{
			double result = vector2s[0].X;
			foreach (vector2 vector2 in vector2s)
			{
				if (vector2.X > result)
				{
					result = vector2.X;
				}
			}
			return result;
		}
		static public double Ymax(this vector2[] vector2s)
		{
			double result = vector2s[0].Y;
			foreach (vector2 vector2 in vector2s)
			{
				if (vector2.Y > result)
				{
					result = vector2.Y;
				}
			}
			return result;
		}
		static public double Xmin(this vector2[] vector2s)
		{
			double result = vector2s[0].X;
			foreach (vector2 vector2 in vector2s)
			{
				if (vector2.X < result)
				{
					result = vector2.X;
				}
			}
			return result;
		}
		static public double Ymin(this vector2[] vector2s)
		{
			double result = vector2s[0].Y;
			foreach (vector2 vector2 in vector2s)
			{
				if (vector2.Y < result)
				{
					result = vector2.Y;
				}
			}
			return result;
		}

		static public double angle(this vector2 vector2)
		{
			if (vector2.X == 0)
			{
				if (vector2.Y == 0)
				{
					return 0;
				}
				else if (vector2.Y > 0)
				{
					return 90;
				}
				else
				{
					return 270;
				}
			}
			else if (vector2.Y == 0)
			{
				if (vector2.X > 0)
				{
					return 0;
				}
				else
				{
					return 180;
				}
			}
			else
			{



				return Math.Atan2(vector2.Y, vector2.X) / Math.PI * 180;
			}
		}
		static public vector2[] row(this vector2[] points, double angle)
		{
			vector2[] vectors = null;
			if (points != null)
			{
				vectors = new vector2[points.Length];
				for (int i = 0; i < vectors.Length; i++)
				{
					vectors[i] = points[i].row(angle);
				}
			}
			return vectors;
		}
		static public vector2 row(this vector2 point, double angle)
		{
			return new vector2(point.value() * Math.Cos((angle + point.angle()) / 180 * Math.PI), point.value() * Math.Sin((angle + point.angle()) / 180 * Math.PI));
		}
		static public vector2 trans(this vector2 vector2, double angle, double k)
		{
			return k * vector2.row(angle);
		}

		public static void saveDotCloud(this IEnumerable<Vector3> vector3s, string path)
		{
			FileStream stream = new FileStream(path,FileMode.Create);
			vector3s.writeDotCloud(stream);
			stream.Flush();
			stream.Close();
		}
		public static Vector3[] loadDotCloud(string path)
		{
			FileStream stream = new FileStream(path, FileMode.Open);
			Vector3[] vs = readDotCloud(stream);
			stream.Close();
			return vs;
		}
		public static void writeDotCloud(this IEnumerable<Vector3> vector3s,Stream stream)
		{
			byte[] bytes;
			bytes = BitConverter.GetBytes(vector3s.Count());
			stream.Write(bytes,0, bytes.Length);
			int c = vector3s.Count();
			Vector3 temp;
			for(int i=0;i<c;i++)
			{
				temp = vector3s.ElementAt(i);
				bytes = BitConverter.GetBytes(temp.x);
				stream.Write(bytes, 0, bytes.Length);
				bytes = BitConverter.GetBytes(temp.y);
				stream.Write(bytes, 0, bytes.Length);
				bytes = BitConverter.GetBytes(temp.z);
				stream.Write(bytes, 0, bytes.Length);
			}
		}

		public static Vector3[] readDotCloud( Stream stream)
		{
			byte[] bytes=new byte[8];
			stream.Read(bytes, 0, 4);
			int c = BitConverter.ToInt32(bytes, 0);
			Vector3[] result = new Vector3[c];
			double x, y, z;
			for (int i = 0; i < c; i++)
			{
				stream.Read(bytes, 0, 8);
				x = BitConverter.ToDouble(bytes, 0);
				stream.Read(bytes, 0, 8);
				y = BitConverter.ToDouble(bytes, 0);
				stream.Read(bytes, 0, 8);
				z = BitConverter.ToDouble(bytes, 0);
				result[i] = new Vector3(x, y, z);
			}
			return result;

			}

		static public vector2[] trans(this vector2[] vector2, double angle, double k)
		{
			vector2[] result = new vector2[vector2.Length];
			vector2 v0 = vector2[0];
			vector2 temp;
			for (int i = 0; i < vector2.Length; i++)
			{
				temp = v0 + (vector2[i] - v0).trans(angle, k);
				result[i] = temp;
			}
			return result;
		}
		static public Vector3[] trans(this Vector3[] vector3, double angle, double k)
		{
			Vector3[] result = new Vector3[vector3.Length];
			vector2[] vec2s = vector3.toVec2().trans(angle, k);
			for (int i = 0; i < vector3.Length; i++)
			{

				result[i] = new Vector3(vec2s[i].X, vec2s[i].Y, vector3[i].z);
			}
			return result;
		}
		static public vector2 zoom(this vector2 vector2, vector2 direction, double k)
		{
			direction = direction.nolrmalized();
			double u = vector2.dot(direction);
			vector2 vector = vector2 - (u - u * k) * direction;
			return vector;
		}
		static public bool issameside(this vector2 vector2, vector2 p, vector2 l1, vector2 l2)
		{
			vector2 v1 = l2 - l1;
			vector2 v2 = p - l1;
			vector2 v3 = vector2 - l1;

			return (v1.angle() - v2.angle()) * (v1.angle() - v3.angle()) >= 0;
		}
		static public vector2 c(this vector2[] vecs)
		{
			vector2 result = new vector2(0, 0);
			for (int i = 0; i < vecs.Length; i++)
			{
				result += vecs[i];
			}
			return result / vecs.Length;
		}
		static public bool isin(this vector2 point, vector2[] vectors)
		{
			vector2 c = vectors.c();

			for (int i = 0; i < vectors.Length - 1; i++)
			{
				if ((vectors[i] - vectors[i + 1]).value() != 0)
				{
					if (!c.issameside(point, vectors[i], vectors[i + 1]))
						return false;
				}

			}
			if ((vectors[0] - vectors[vectors.Length - 1]).value() != 0)
			{
				if (!c.issameside(point, vectors[0], vectors[vectors.Length - 1]))
				{
					return false;
				}
			}

			return true;

		}
		static public vector2[] zoom(this vector2[] vector2, vector2 direction, double k)
		{
			vector2[] result = new vector2[vector2.Length];
			vector2 v0 = vector2[0];
			for (int i = 0; i < vector2.Length; i++)
			{

				result[i] = v0 + (vector2[i] - v0).zoom(direction, k);
			}
			return result;
		}
		static public Vector3[] zoom(this Vector3[] vector3, vector2 direction, double k)
		{
			Vector3[] result = new Vector3[vector3.Length];
			vector2[] vec2s = vector3.toVec2().zoom(direction, k);
			for (int i = 0; i < vector3.Length; i++)
			{

				result[i] = new Vector3(vec2s[i].X, vec2s[i].Y, vector3[i].z);
			}
			return result;
		}
		static public Vector3 tov3(this Color color)
		{
			return new Vector3(color.R, color.G, color.B);
		}
		static public vector2[] normalize(this vector2[] vectors)
		{
			vector2 v0 = vectors[0];
			vector2[] result = new vector2[vectors.Length];
			for (int i = 0; i < vectors.Length; i++)
			{
				result[i] = vectors[i] - v0;
			}
			return result;
		}
		static public vector2[] toVec2(this Vector3[] vector3s)
		{
			vector2[] result = new vector2[vector3s.Length];
			for (int i = 0; i < vector3s.Length; i++)
			{
				result[i] = new vector2(vector3s[i].x, vector3s[i].y);
			}
			return result;
		}
	}
	namespace Old
	{

		public struct vector2_old
		{
			public double X { get; }
			public double Y { get; }
			//运算
			public static vector2_old operator +(vector2_old a, vector2_old b)
			{
				return new vector2_old(a.X + b.X, a.Y + b.Y);
			}
			public static vector2_old operator *(double a, vector2_old b)
			{
				return new vector2_old(a * b.X, a * b.Y);
			}
			public static vector2_old operator *(int a, vector2_old b)
			{
				return new vector2_old(a * b.X, a * b.Y);
			}
			public static vector2_old operator *(vector2_old b, double a)
			{
				return new vector2_old(a * b.X, a * b.Y);
			}
			public static vector2_old operator /(vector2_old b, double a)
			{
				return new vector2_old(b.X / a, b.Y / a);
			}
			public static vector2_old operator *(vector2_old b, int a)
			{
				return new vector2_old(a * b.X, a * b.Y);
			}
			public static vector2_old operator -(vector2_old a, vector2_old b)
			{
				return new vector2_old(a.X - b.X, a.Y - b.Y);
			}
			public static vector2_old operator -(vector2_old a)
			{
				return new vector2_old(-a.X, -a.Y);
			}
			//取值
			public double distance(vector2_old v2)
			{
				return (this - v2).value();
			}
			public double dot(vector2_old v2)
			{
				return v2.X * X + v2.Y * Y;
			}
			public double cos(vector2_old v2)
			{
				return dot(v2) / value() / v2.value();
			}
			public double value()
			{
				return Math.Sqrt(X * X + Y * Y);
			}
			public vector2_old nolrmalized()//返回单位向量
			{
				if (value() != 0)
					return new vector2_old(X / value(), Y / value());
				else
					return new vector2_old(0, 0);


			}
			//构造函数
			public vector2_old(double x, double y)
			{
				X = x;
				Y = y;
			}

			public vector2_old(int x, int y)
			{
				X = x;
				Y = y;
			}
			//转化
			public static implicit operator Point(vector2_old v)
			{
				return new Point((int)v.X, (int)v.Y);
			}
			public static implicit operator vector2_old(int i)
			{
				return new Point(i, i);
			}
			public static implicit operator vector2_old(Vector2 vector)
			{
				return new vector2_old(vector.X, vector.Y);
			}
			public static implicit operator vector2_old(Point v)
			{
				return new vector2_old(v.X, v.Y);
			}
			public string pos()
			{
				return "(" + X + "," + Y + ")";
			}
			public Point Point()
			{
				if (double.IsNaN(X) || double.IsNaN(Y))
					return new Point(0, 0);
				if (double.IsInfinity(X) || double.IsInfinity(Y))
					return new Point(0, 0);
				return new Point((int)X, (int)Y);
			}
		}
	}
	public static class MathHelper

	{
		public static readonly double radian2degree = 180 / Math.PI;
		public static readonly double degree2radian = Math.PI / 180;
		public static int mixColor(int color)
		{
			int alpha = color >> 24;
			if (alpha > 50 && alpha < 96)
			{
				alpha = 25 + (alpha - 50) * 3;
			}
			else if (alpha >= 96)
			{
				alpha = 163 + (alpha - 96) / 2;
			}
			else
			{
				alpha = alpha / 2;
			}
			alpha -= 128;
			int r = (((color & 0x00ff0000) >> 16)) + alpha;
			int g = (((color & 0x0000ff00) >> 8)) + alpha;
			int b = ((color & 0x000000ff)) + alpha;


			if (r > 255)
			{
				r = 255;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (r < 0)
			{
				r = 0;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b < 0)
			{
				b = 0;
			}
			int c = (int)(0xff000000 | r << 16 | g << 8 | b);
			return c;
		}
		public static double[] integral(this double[] data)
		{
			double[] result = new double[data.Length];
			result[0]= 0;
		    for(int i = 1; i < data.Length; i++)
			{
				result[i] = result[i - 1] + data[i];
			}
			return result;
		}

		public static double keep(this double n,int count)
		{
			double a = Math.Pow(10, count);
			return ((int)(n * a)) / a;
		}
		public static double mean(this double[] data)
		{
			double sum = 0;
			for (int i = 1; i < data.Length; i++)
			{
				sum += data[i];
			}
			return sum/data.Length;
		}
		public static double[] smooth(this double[] data, int range)
		{ double[] r = new double[data.Length];
			int count = range * 2 + 1;
			int l = data.Length - range;
			int count_ = range + 1;
			int fc = 0;
			for (int i =0 ; i < range; i++)
			{
				r[i] = 0;
				for (int j = 0; j < count_; j++)
				{
					r[i ] += data[i + j - fc];
				}
				r[i] /= count_;
				fc++;
				count_++;
			}
			for (int i = range; i < l; i++)
			{
				r[i] = 0;
				for(int j=0;j<count;j++)
				{
					r[i] += data[i + j-range];
				}
				r[i] /= count;
			}
			count_ = count - 1;
			for(int i=l;i<data.Length;i++)
			{
				r[i] = 0;
				for (int j = 0; j < count_; j++)
				{
					r[i] += data[i + j-range];
				}
				r[i] /= count_;
				count_--;
			}
			return r;
		}
		public static double[] add(this double[] data,double number)
		{
			double[] r = new double[data.Length];
			for (int i = 1; i < data.Length; i++)
			{
				r[i] = data[i]+number;
			}
			return r;
		}
		public static double[] mul(this double[] data, double number)
		{
			double[] r = new double[data.Length];
			for (int i = 1; i < data.Length; i++)
			{
				r[i] = data[i] * number;
			}
			return r;
		}
		public static double[] div(this double[] data, double number)
		{
			double[] r = new double[data.Length];
			for (int i = 1; i < data.Length; i++)
			{
				r[i] = data[i] / number;
			}
			return r;
		}
		public static double[] sub(this double[] data, double number)
		{
			double[] r = new double[data.Length];
			for (int i = 1; i < data.Length; i++)
			{
				r[i] = data[i] -number;
			}
			return r;
		}
		public static double[] derivation(this double[] data)
		{
			double[] result = new double[data.Length];
			result[0] = 0;
			int l = data.Length - 1;
			for (int i = 1; i < l; i++)
			{
				result[i] = (data[i + 1] - data[i-1])*0.5;
			}
			result[0] = data[1] - data[0];
			result[l] = data[l] - data[l - 1];
			return result;
		}
		public static double[] abs(this double[] data)
		{
			double[] result = new double[data.Length];
			
			for (int i = 0; i < data.Length; i++)
			{
				result[i] = Math.Abs(data[i]);
			}
			return result;
		}
		public static double max(this double[] data)
		{
			double max = double.NegativeInfinity;
			for (int i = 0; i < data.Length; i++)
			{
				if(data[i] > max)
					max = data[i];
			}
			return max;
		}
		public static double min(this double[] data)
		{
			double min = double.PositiveInfinity;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] <min)
					min = data[i];
			}
			return min;
		}
		public static double[] matchValue(this double[] data,double value)
		{
			List<double> result = new List<double>();
			double v1, v2;
			int end=data.Length-1;
			for (int i = 0; i < end; i++)
			{
				v1=data[i];
				v2 = data[i + 1];
				if(v1<value&&v2>value)
				{
					result.Add(i + (value-v1) / (v2 - v1));
				}
				else if (v1 > value && v2 < value)
				{
					result.Add(i + (value-v1) / (v2 - v1));
				}
				else if(v1==value)
				{
					result.Add(i);
				}
			}
			if (data[end]==value)
			{
				result.Add(end);
			}
			return result.ToArray();

		}
		public static int mixColor(int color, float light)
		{
			int alpha = color >> 24;


			int r = (int)((((color & 0x00ff0000) >> 16)) * light);
			int g = (int)((((color & 0x0000ff00) >> 8)) * light);
			int b = (int)(((color & 0x000000ff)) * light);


			if (r > 255)
			{
				r = 255;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (r < 0)
			{
				r = 0;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b < 0)
			{
				b = 0;
			}
			int c = alpha << 24 | r << 16 | g << 8 | b;
			return c;
		}

		public static int mixColor(int color, int light)
		{
			int alpha = color >> 24;

			int r = (((color & 0x00ff0000) >> 16)) + light;
			int g = (((color & 0x0000ff00) >> 8)) + light;
			int b = ((color & 0x000000ff)) + light;


			if (r > 255)
			{
				r = 255;
			}
			if (g > 255)
			{
				g = 255;
			}
			if (b > 255)
			{
				b = 255;
			}
			if (r < 0)
			{
				r = 0;
			}
			if (g < 0)
			{
				g = 0;
			}
			if (b < 0)
			{
				b = 0;
			}
			int c = alpha << 24 | r << 16 | g << 8 | b;
			return c;
		}
		public static double STDEV_ONE(this ICollection<double> arrData)
		{
			return STDEV_ONE(arrData.ToArray());
		}

		public static double STDEV_ONE( params double[] arrData) //计算标准偏差
	{
		double std_dev;
		double xSum = 0F;//样本总和
		double xAvg = 0F;//样本平均值
		double sSum = 0F;//方差的分子
						 //float tmpStDev = 0F;
		int arrNum = arrData.Length;//得到样本数量，分母
		for (int i = 0; i < arrNum; i++)//循环计算得到样本总和
		{
			xSum += arrData[i];
		}
		xAvg = xSum / arrNum;//计算得到样本平均值
		for (int j = 0; j < arrNum; j++)//得到方差的分子
		{
			sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
		}
		std_dev = Math.Sqrt((sSum / (arrNum - 1)))/xAvg;//样本标准差

		//STDP = Convert.ToSingle(Math.Sqrt((sSum / arrNum)).ToString());//总体标准差
			return std_dev;
	}
		public static double STDEV(this ICollection<double> arrData) //计算标准偏差
		{
			return STDEV(arrData.ToArray());
		}
			public static double STDEV(params double[] arrData) //计算标准偏差
		{
			double std_dev;
			double xSum = 0F;//样本总和
			double xAvg = 0F;//样本平均值
			double sSum = 0F;//方差的分子
							 //float tmpStDev = 0F;
			int arrNum = arrData.Length;//得到样本数量，分母
			for (int i = 0; i < arrNum; i++)//循环计算得到样本总和
			{
				xSum += arrData[i];
			}
			xAvg = xSum / arrNum;//计算得到样本平均值
			for (int j = 0; j < arrNum; j++)//得到方差的分子
			{
				sSum += ((arrData[j] - xAvg) * (arrData[j] - xAvg));
			}
			std_dev = Math.Sqrt((sSum / (arrNum )));//样本标准差

			//STDP = Convert.ToSingle(Math.Sqrt((sSum / arrNum)).ToString());//总体标准差
			return std_dev;
		}
	}
	public struct Ray3d
	{
		public Vector3 Direction { get; private set; }
		public Vector3 Position { get; private set; }
		public Ray3d(Vector3 pos,Vector3 dir)
		{
			Position = pos;
			Direction = dir;
		}

		public Vector3 getTarget(Ray3d Ray2)
		{
			double a1, b1, c1, a2, b2, c2, k1, k2, k3, k4, k5, k6, r1, r2, x, y, z;
			a1 =Direction.x;
			b1 = Direction.y;
			c1 = Direction.z;
			a2 = Ray2. Direction.x;
			b2 = Ray2.Direction.y;
			c2 = Ray2.Direction.z;
			if ((a1 == a2 && c1 == c2 && b1 == b2) || (a1 == -a2 && b1 == -b2 && c1 == -c2))
			{
				x = (Position.x + Ray2.Position.x) / 2;
				y = (Position.y + Ray2.Position.y) / 2;
				z = (Position.z + Ray2.Position.z) / 2;
				return new Vector3(x, y, z);
			}
			k1 = -2 * a1 * a2 - 2 * b1 * b2 - 2 * c1 * c2;
			k2 = 2 * a2 * a2 + 2 * b2 * b2 + 2 * c2 * c2;
			k3 = 2 * a2 * (Ray2.Position.x - Position.x) + 2 * b2 * (Ray2.Position.y - Position.y) + 2 * c2 * (Ray2.Position.z - Position.z);
			k4 = 2 * a1 * a1 + 2 * b1 * b1 + 2 * c1 * c1;
			k5 = -2 * a1 * a2 - 2 * b1 * b2 - 2 * c1 * c2;
			k6 = -2 * a1 * (Ray2.Position.x - Position.x) + -2 * b1 * (Ray2.Position.y- Position.y) + -2 * c1 * (Ray2.Position.z - Position.z);
			r1 = -(k3 * k5 - k2 * k6) / (k1 * k5 - k2 * k4);
			r2 = -(k3 * k4 - k1 * k6) / (k2 * k4 - k1 * k5);
			x = (Position.x + a1 * r1 + Ray2.Position.x+ a2 * r2) / 2;
			y = (Position.y + b1 * r1 + Ray2.Position.y + b2 * r2) / 2;
			z = (Position.z + c1 * r1 + Ray2.Position.z + c2 * r2) / 2;
			return new Vector3(x, y, z);
		}
	}
	public class Line2d
	{
		public static Line2d Xaxis;
		public static Line2d Yaxis;
		static Line2d()
			{
			Xaxis = new Line2d(0.0, 0);
			Yaxis = new Line2d(new vector2(0, 0), new vector2(0, 1));
			}
		private static int abs(int s)
		{
			return s >= 0 ? s : -s;
		}
		private static double abs(double s)
		{
			return s >= 0 ? s : -s;
		}
		private static double Sin(double s)
		{
			return System.Math.Sin(s);
		}
		private static double Cos(double s)
		{
			return System.Math.Cos(s);
		}
		private static double Tan(double s)
		{
			return System.Math.Tan(s);
		}
		private static double Sqrt(double s)
		{
			return System.Math.Sqrt(s);
		}
		private static double Pow(double x, double y)
		{
			return System.Math.Pow(x, y);
		}
		const double nan = double.NaN;
		public double k { get; }
		public double b { get; }

		public int type { get; }
		public double c { get; }
		public Line2d()

		{

			k = 1;
			this.b = 0;
			type = 0;
			c = 0;
		}
		public Line2d(Point a, Point b)
		{

			if (a.X == b.X)
			{
				type = -1;
				k = nan;
				this.b = nan;
				c = a.X;
			}
			else if (a.Y == b.Y)
			{
				type = 1;
				k = 0;
				this.b = a.Y;
				c = nan;
			}
			else
			{
				double ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
				k = (ay - by) / (ax - bx);
				this.b = ay - k * ax;
				type = 0;
				c = -this.b / k;
			}

		}
		public Line2d(vector2 a, vector2 b)
		{

			if (a.X == b.X)
			{
				type = -1;
				k = nan;
				this.b = nan;
				c = a.X;
			}
			else if (a.Y == b.Y)
			{
				type = 1;
				k = 0;
				this.b = a.Y;
				c = nan;
			}
			else
			{
				double ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
				k = (ay - by) / (ax - bx);
				this.b = ay - k * ax;
				type = 0;
				c = -this.b / k;
			}

		}
		public Line2d(double k0, double b0)
		{

			k = k0;
			b = b0;
			if (k0 == 0)
			{
				c = nan;
				type = 1;
			}
			else { c = -this.b / k; type = 0; }
		}
		public bool iscrosswith(Line2d l1)
		{
			if (type == l1.type)
			{
				if (type != 0)
				{
					return false;
				}
				else if (k == l1.k)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else { return true; }
		}
		public int pos(vector2 p)
		{
			int n;
			if (type == -1)
			{
				if (p.X > c)
				{
					n = 1;
				}
				else if (p.X < c)
				{
					n = -1;
				}
				else
				{
					n = 0;
				}
			}
			else if (type == 1)
			{
				if (p.Y > b)
				{
					n = 1;
				}
				else if (p.Y < b)
				{
					n = -1;
				}
				else
				{
					n = 0;
				}
			}
			else
			{
				if (y(p.X) > p.Y)
				{
					n = -1;
				}
				else if (y(p.X) < p.Y)
				{
					n = 1;
				}
				else
				{
					n = 0;
				}
			}
			return n;
		}
		public vector2 crosspoint(Line2d l1)
		{
			double x = 0, y = 0;
			if (iscrosswith(l1))
			{
				if (type == 0)
				{
					if (l1.type == 0)
					{
						x = ((l1.b - b) / (k - l1.k));
						y = (k * x + b);
					}
					else if (l1.type == 1)
					{
						y = l1.b;
						x = ((y - b) / k);
					}
					else
					{
						x = l1.c;
						y = (k * x + b);
					}
				}
				else if (type == 1)
				{
					if (l1.type == 0)
					{
						y = b;
						x = ((y - l1.b) / l1.k);
					}
					else
					{
						y = b;
						x = l1.c;
					}
				}
				else
				{
					if (l1.type == 0)
					{
						x = c;
						y = (l1.k * x + l1.b);
					}
					else
					{
						x = c;
						y = l1.b;
					}
				}
			}
			return new vector2(x, y);
		}
		public Line2d(double ang, Point a)
		{
			ang = ang / 180 * Math.PI;
			Point b = new Point(a.X + (int)(100 * Cos(ang)), a.Y + (int)(100 * Sin(ang)));

			if (a.X == b.X)
			{
				type = -1;
				k = nan;
				this.b = nan;
				c = a.X;
			}
			else if (a.Y == b.Y)
			{
				type = 1;
				k = 0;
				this.b = a.Y;
				c = nan;
			}
			else
			{
				k = (a.Y - b.Y) / (a.X - b.X);
				this.b = a.Y - k * a.X;
				type = 0;
				c = -this.b / k;
			}
		}
		public double distancetopoint(vector2 p)
		{
			if (type == 0)
			{
				return Math.Abs(p.X * k + b - p.Y) / Sqrt(k * k + 1.0);
			}
			else if (type == 1)
			{
				return Math.Abs(b - p.Y);
			}
			else
			{
				return Math.Abs(c - p.X);
			}
		}



		public double y(double x)
		{
			return k * x + b;
		}
		public Line2d verticalline(vector2 p)
		{
			if (type == 0)
			{
				double k0 = -1 / k;
				double b0 = p.Y - p.X * k0;
				return new Line2d(k0, b0);
			}
			else if (type == 1)
			{
				return new Line2d(p, new vector2(p.X, p.Y + 1));
			}
			else
			{
				return new Line2d(p, new vector2(p.X + 1, p.Y));
			}
		}
		public double x(double y)
		{
			return (y - b) / k;
		}




	}


	

}
