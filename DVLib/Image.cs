using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using Physics;
using System.IO;
using System.Drawing.Imaging;
using MachineLearning;
using DVOSLib;

using MathBase.Old;
using Physics.Physics2;
using vector2 = MathBase.Vector2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
namespace Images
{
	public struct Color0
	{
	public readonly	int R;
	public readonly int G;
	public readonly	int B;
	public readonly int A;
	public readonly int Code;
		public Color0(int a ,int r, int g, int b)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
			Code = ((A & 0xff) << 24) | ((r & 0xff) << 16) | ((g & 0xff) << 8) | ((b & 0xff));

		}
		public Color0(int r,int g,int b)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = 255;
			Code = ((A & 0xff) << 24) | ((r & 0xff) << 16) | ((g & 0xff) << 8) | ((b & 0xff));

		}
		public Vector3 toVec()
		{
			return new Vector3(R, G, B);
		}

		public static Color0 FromArgb(int v1, int v2, int v3, int v4)
		{
			return new Color0(v1,v2,v3,v4);
		}	
		public static implicit operator Int32(Color0 color )
	{
		return color.Code;
	}
	}
	public delegate int win(int x);
	public class Converter
	{
		bitmap Bitmap;
		public Converter(bitmap bitmap)
		{
			Bitmap = bitmap;
		}
		public void toBitmap(PixelAccessor<Argb32> a)
		{

			for (int i = 0; i < a.Height; i++)
			{
				var s = MemoryMarshal.Cast<Argb32, int>(a.GetRowSpan(i));
				var b = Bitmap.getRowSpan(i);
				s.CopyTo(b);
			}
		}
		public void toImage(PixelAccessor<Argb32> a)
		{

			for (int i = 0; i < a.Height; i++)
			{
				var s = MemoryMarshal.Cast<Argb32, int>(a.GetRowSpan(i));
			
				var b = Bitmap.getRowSpan(i);
				b.CopyTo(s);
			}
		}

	}

	public static class ImageHelper
	{

	
		public static bitmap fromImage(this Image<Argb32> image)
		{
			bitmap bitmap=new bitmap(image.Width, image.Height);
			Converter converter = new Converter(bitmap);
			image.ProcessPixelRows(converter.toImage);
			return bitmap;
		}


	
		static double min = 0.000001;

		public static Vector2 getMassCenterWidth(this bitmap bitmap,int y, Channel channel = Channel.All, double rate = 0.98)
		{
			double sum = 0;
			double sumPos = 0;
			double I = 0;
			double max, min_;
			(int, int, int) color;
			double r = 1.0 / 3;
				sumPos = sum = 0;
				max = 0;
			double[] Intensity = new double[bitmap.Width];

			for (int j = 0; j < bitmap.Width; j++)
			{
				color = bitmap[j, y].Int2RGB();
				if (channel == Channel.All)
				{
					I = color.Item1 + color.Item2 + color.Item3 + min;
					I *= r;
				}
				else if (channel == Channel.Gray)
				{
					I = color.Item1 + color.Item2 + color.Item3 + min;
					I *= r;

				}
				else if (channel == Channel.Red)
				{
					I = color.Item1 + min;

				}
				else if (channel == Channel.Green)
				{
					I = color.Item2 + min;

				}
				else if (channel == Channel.Blue)
				{
					I = color.Item3 + min;

				}
				Intensity[j] = I;
				if (I > max)
				{
					max = I;
				}

			}



				


				min_ = max * rate;

				for (int j = 0; j < bitmap.Width; j++)
				{
				I = Intensity[j];
					if (I > min_)
					{
						sum += I;
						sumPos += j * I;
					}

				}

			
			return new vector2(sumPos / sum, y);
		}

		public static Vector2[] getMassCenterWidth(this bitmap bitmap, Channel channel = Channel.All,double rate=0.98)
		{
			Vector2[] vector2s = new Vector2[bitmap.Height];
			
			for (int i = 0; i < bitmap.Height; i++)
			{
				vector2s[i] = getMassCenterWidth(bitmap,i,channel,rate);

			}
			return vector2s;
		}
	}
	public class GrayHistogram
	{
		int[] vs1;
		int max = 256;
		int total;
		public GrayHistogram(int m)
		{
			max = m;
			vs1 = new int[max];
		}
		public GrayHistogram():this(256)
		{
			
		}

		public void readBitmapR(bitmap bitmap)
		{
			for(int i=0;i<bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height;j++)
				{
					int v = bitmap.GetR(i, j);
					vs1[v] += 1;
				}
			}
			total = bitmap.Width * bitmap.Height;
		}
		public void readBitmapG(bitmap bitmap)
		{

			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					int v = bitmap.GetG(i, j);
					vs1[v] += 1;
				}
			}
			total = bitmap.Width * bitmap.Height;
		}
		public functionmap output(int w, int h,int min ,int max)
		{
			functionmap functionmap = new functionmap(w, h);
			for (int i = min; i < max; i++)
			{
				functionmap.add(i, vs1[i]);
			}
			return functionmap;

		}
		public functionmap output(int w,int h)
		{
			functionmap functionmap = new functionmap(w, h);
			for(int i=0;i<max;i++)
			{
				functionmap.add(i, vs1[i]);
			}
			return functionmap;
			
		}
		public void readBitmapB(bitmap bitmap)
		{

			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					int v = bitmap.GetB(i, j);
					vs1[v] += 1;
				}
			}
			total=bitmap.Width*bitmap.Height;
		}
		public void readBitmapB(int[] bitmap)
		{

			for (int i = 0; i < bitmap.Length; i++)
			{
				
					int v = bitmap[i].Int2ARGB().b;
					vs1[v] += 1;
				
			}
			total = bitmap.Length;
		}
		public void readBitmapR(int[] bitmap)
		{

			for (int i = 0; i < bitmap.Length; i++)
			{

				int v = bitmap[i].Int2ARGB().r;
				vs1[v] += 1;

			}
			total = bitmap.Length;
		}
		public void readBitmapG(int[] bitmap)
		{

			for (int i = 0; i < bitmap.Length; i++)
			{

				int v = bitmap[i].Int2ARGB().g;
				vs1[v] += 1;

			}
			total = bitmap.Length;
		}
		int[] sk;
		public MathFunction <int> getHisteq()
		{
			double[] Pr = new double[max];
			double[] Pa = new double[max];
			sk = new int[max];
			for (int i = 0; i <max; i++)
			{
				int c = vs1[i];
				Pr[i] = c * 1.0 / total;
				if(i>0)
				{
					Pa[i] = Pa[i - 1] + Pr[i];
				}else
				{
					Pa[i] = Pr[i];
				}
				sk[i] =(int) Math.Round(Pa[i] * (max - 1));
			}
			return (v) => {  return sk[v]; };
		}
	}



	public delegate int GT(int v);

	public class filehelper
	{
		string path;
		List<string> files = new List<string>();
		List<string> num = new List<string>();
		public filehelper(string Path)
		{
			path = Path;
		}

		public void updataFile()
		{
			if (Directory.Exists(path))
			{
				files = Directory.GetFiles(path).ToList<string>();
				num = new List<string>();
				foreach (string name in files)
				{
					string temp = "";
					for (int i = 0; i < name.Length; i++)
					{
						if (name[i].isnumber())
						{
							temp += name[i];
						}
					}
					num.Add(temp);
				}
			}
		}
	}
	class pointgroup
	{
		List<vector2> points;
		public void set(int index, vector2 vector2)
		{
			points[index] = vector2;
		}
		public pointgroup()
		{

			points = new List<vector2>();
		}
		public pointgroup(vector2[] vectors)
		{

			points = vectors.ToList<vector2>();
		}
		public pointgroup(vector2 p0, params vector2[] vectors)
		{
			points = new List<vector2>();
			points.Add(p0);
			foreach (vector2 vector2 in vectors)
			{
				points.Add(vector2);
			}
		}
		public pointgroup clone()
		{
			pointgroup p = new pointgroup();
			foreach (vector2 vector2 in points)
				p.Add(vector2);
			return p;
		}
		public void Add(vector2 vector)
		{
			points.Add(vector);
		}
		public vector2[] toArray()
		{
			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = points[i];
			}
			return result;
		}
		public vector2[] toArray(vector2 P0)
		{
			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = points[i] + P0;
			}
			return result;
		}
		public vector2[] toArray(vector2 p1, vector2 p2)
		{
			vector2 P1 = points[0];
			vector2 P2 = points[points.Count - 1];
			double k = (p2 - p1).value() / (P2 - P1).value();
			double angle = (p2 - p1).angle() - (P2 - P1).angle();
			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{

				result[i] = p1 + ((points[i] - P1).row(angle) * k);
			}
			return result;
		}
		public vector2[] toArray(vector2 P1, vector2 P2, vector2 p1, vector2 p2)
		{

			double k = (p2 - p1).value() / (P2 - P1).value();
			double angle = (p2 - p1).angle() - (P2 - P1).angle();
			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = p1 + (points[i] - P1).row(-angle) * k;
			}
			return result;
		}
		public vector2[] toArray(vector2 P1, double k, double angle)
		{

			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = P1 + (points[i] - P1).row(angle) * k;
			}
			return result;
		}
		public vector2[] toArray(vector2 P0, vector2 P1, double k, double angle)
		{

			vector2[] result = new vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = P0 + P1 + (points[i] - P1).row(angle) * k;
			}
			return result;
		}
	}
	public class worldIO
	{
		v_world world;
		FileStream file;
		public worldIO(v_world world)
		{
			this.world = world;
		}
		public void save(string path)
		{

			file = new FileStream(path, FileMode.Create);
			StreamWriter writer = new StreamWriter(file);
			writer.WriteLine(world.usingcr);
			writer.WriteLine(world.usingcollision);
			writer.WriteLine(world.usingCoulombforce);
			writer.WriteLine(world.usingelasticforce);
			writer.WriteLine(world.usingfieldforce);
			writer.WriteLine(world.usinggravity);
			writer.WriteLine(world.gettimespeed());
			writer.WriteLine(world.gett());
			writer.WriteLine(world.getdt());
			writer.WriteLine(world.getG());
			writer.WriteLine(world.getK());
			mp_object2[] obs = world.getobjects();

			writer.WriteLine(obs.Length);
			for (int i = 0; i < obs.Length; i++)
			{
				mp_object2 ob = obs[i];
				writer.WriteLine(ob.getname());
				writer.WriteLine(ob.gettag());
				writer.WriteLine(ob.getmass());
				writer.WriteLine(ob.getcollision());
				writer.WriteLine(ob.getstrength());
				writer.WriteLine(ob.GetCharge());
				vector2 pos = ob.getdisplacement();
				writer.WriteLine(pos.X);
				writer.WriteLine(pos.Y);
				vector2 v = ob.getvelocity2();
				writer.WriteLine(v.X);
				writer.WriteLine(v.Y);
				var oc = world.obj_c[i].Int2RGB();
				writer.WriteLine(255);
				writer.WriteLine(oc.r);
				writer.WriteLine(oc.g);
				writer.WriteLine(oc.b);
				var tc = world.obj_tc[i].Int2RGB();
				writer.WriteLine(255);
				writer.WriteLine(tc.r);
				writer.WriteLine(tc.g);
				writer.WriteLine(tc.b);

			}
			field2[] fis = world.getfields();
			writer.WriteLine(fis.Length);
			for (int i = 0; i < fis.Length; i++)
			{
				field2 fi = fis[i];
				writer.WriteLine(fi.getname());
				writer.WriteLine(fi.gettag());
				writer.WriteLine(fi.getmass());
				writer.WriteLine(fi.getcollision());
				writer.WriteLine(fi.getstrength());
				writer.WriteLine(fi.GetCharge());
				vector2 pos = fi.getdisplacement();
				writer.WriteLine(pos.X);
				writer.WriteLine(pos.Y);
				vector2 v = fi.getvelocity2();
				writer.WriteLine(v.X);
				writer.WriteLine(v.Y);
				var oc = world.fie_c[i].Int2RGB();
				writer.WriteLine(255);
				writer.WriteLine(oc.r);
				writer.WriteLine(oc.g);
				writer.WriteLine(oc.b);
				var tc = world.fie_tc[i].Int2RGB();
				writer.WriteLine(255);
				writer.WriteLine(tc.r);
				writer.WriteLine(tc.g);
				writer.WriteLine(tc.b);
				writer.WriteLine(world.getindex(fi.getfather()));
				writer.WriteLine((int)fi.gettype());
				writer.WriteLine(fi.getr_f());
			}
			writer.Flush();
			writer.Close();
			file.Close();
		}
		public void read(string path)
		{
			world.clean();
			file = new FileStream(path, FileMode.Open);
			StreamReader reader = new StreamReader(file);
			world.usingcr = Convert.ToBoolean(reader.ReadLine());
			world.usingcollision = Convert.ToBoolean(reader.ReadLine());
			world.usingCoulombforce = Convert.ToBoolean(reader.ReadLine());
			world.usingelasticforce = Convert.ToBoolean(reader.ReadLine());
			world.usingfieldforce = Convert.ToBoolean(reader.ReadLine());
			world.usinggravity = Convert.ToBoolean(reader.ReadLine());
			world.settimespeed(Convert.ToDouble(reader.ReadLine()));
			world.sett(Convert.ToInt32(reader.ReadLine()));

			world.setdt(Convert.ToDouble(reader.ReadLine()));
			world.setG(Convert.ToDouble(reader.ReadLine()));
			world.setK(Convert.ToDouble(reader.ReadLine()));
			int obi = Convert.ToInt32(reader.ReadLine());

			for (int i = 0; i < obi; i++)
			{
				mp_object2 ob = new mp_object2();
				ob.setname(reader.ReadLine());
				ob.settag(reader.ReadLine());
				ob.setmass(Convert.ToDouble(reader.ReadLine()));
				ob.setcollision(Convert.ToDouble(reader.ReadLine()));
				ob.setstrength(Convert.ToDouble(reader.ReadLine()));
				ob.setcharge(Convert.ToDouble(reader.ReadLine()));
				vector2 pos = new vector2(Convert.ToDouble(reader.ReadLine()), Convert.ToDouble(reader.ReadLine()));
				ob.setdisplacement(pos);
				vector2 v = new vector2(Convert.ToDouble(reader.ReadLine()), Convert.ToDouble(reader.ReadLine()));
				ob.setvelocity2(v);
				world.addobject(ob);
				int oc =new Color0(Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()));
				world.obj_c[i] = oc;
				Color0 tc =new  Color0(Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()));
				world.obj_tc[i] = tc;


			}
			int fii = Convert.ToInt32(reader.ReadLine());
			for (int i = 0; i < fii; i++)
			{
				field2 fi = new field2();
				fi.setname(reader.ReadLine());
				fi.settag(reader.ReadLine());
				fi.setmass(Convert.ToDouble(reader.ReadLine()));
				fi.setcollision(Convert.ToDouble(reader.ReadLine()));
				fi.setstrength(Convert.ToDouble(reader.ReadLine()));
				fi.setcharge(Convert.ToDouble(reader.ReadLine()));
				vector2 pos = new vector2(Convert.ToDouble(reader.ReadLine()), Convert.ToDouble(reader.ReadLine()));
				fi.setdisplacement(pos);
				vector2 v = new vector2(Convert.ToDouble(reader.ReadLine()), Convert.ToDouble(reader.ReadLine()));
				fi.setvelocity2(v);
				world.addfield(fi);
				Color0 oc = Color0.FromArgb(Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()));
				world.fie_c[i] = oc;
				Color0 tc = Color0.FromArgb(Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()), Convert.ToInt32(reader.ReadLine()));
				world.fie_tc[i] = tc;
				int index = Convert.ToInt32(reader.ReadLine());
				if (index != -1)
				{
					fi.bond(world.getobjects()[index]);
				}

				fi.settype((Physics.fieldtype)Convert.ToInt32(reader.ReadLine()));
				fi.setr_f(Convert.ToDouble(reader.ReadLine()));

			}
			file.Close();


		}

	}
	public class MovingPixel
	{
		public int x { get; private set; }
		public int y { get; private set; }
public	double value{ get; internal set; }

		public MovingPixel(int x,int y,double v)
		{
			this.x = x;
			this.y = y;
			value = v;
		}
}
	public interface IFilter
	{
		bitmap apply( bitmap bitmap)


			;

		bitmap applyR( bitmap bitmap)
		;
		bitmap applyG( bitmap bitmap)
		;
		bitmap applyB( bitmap bitmap)
		;

		bitmap applyGray(bitmap bitmap)
		;
	}
	public class MedialFilter:IFilter
	{
		int range=1;
		int size = 9;
		int m=4; public MedialFilter(int r, int p)
		{
			range = r;
			size = (1 + 2 * range) * (1 + 2 * range);
			if(p<size)
			m = p;
			else
			m = size / 2;
		}
		public MedialFilter(int r)
		{
			range = r;
			size = (1 + 2 * range) * (1 + 2 * range);
			m = size / 2;
		}
		public bitmap apply(bitmap bitmap)
		{int[] R = new int[size];int[] G = new int[size];
					int[] B = new int[size];
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;
		
					
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					
					int n = 0;
					int x_;
					int y_;
					int r, g, b;
					for (int i = -range; i <= range; i++)
					{
						for (int j = -range; j <= range; j++)
						{
							x_ = x + i;
							y_ = y + j; int color = bitmap.GetColor(x_, y_);
							r = ((color >> 16) & 0xff);
							g = ((color >> 8) & 0xff);
							b = ((color) & 0xff);


							R[n] = r;
							B[n] = g;
							G[n] = b;
							n++;
						}
					}
					Array.Sort(R);
					Array.Sort(G);
					Array.Sort(B);
					Colors[x,y]= (R[m] << 16) | (G[m] << 8) | B[m];
				}
			}
			return bb;
		}
		public bitmap applyR(bitmap bitmap)
		{


			int width = bitmap.Width;
			int height = bitmap.Height; 
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors =bb.Data;
int[] R = new int[size];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
int n = 0;
			int x_;
			int y_;
			int r;

			for (int i = -range; i <= range; i++)
			{
				for (int j = -range; j <= range; j++)
				{
					x_ = x + i;
					y_ = y + j;
					r = bitmap.GetSafeR(x_, y_);

					R[n] = r;
					n++;
				}
			}
			Array.Sort(R);
					int c = unchecked((int)(Colors[x, y] & 0xff00ffff) | (R[m] << 16));
					Colors[x, y] = c;
				}
			}

			return bb;
		}
		public bitmap applyG( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height; bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;
			int[] R = new int[size];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int n = 0;
					int x_;
					int y_;
					int r;

					for (int i = -range; i <= range; i++)
					{
						for (int j = -range; j <= range; j++)
						{
							x_ = x + i;
							y_ = y + j;
							r = bitmap.GetSafeG(x_, y_);

							R[n] = r;
							n++;
						}
					}
					Array.Sort(R);
					int c = unchecked((int)(Colors[x, y] & 0xffff00ff) | (R[m] << 8));
					Colors[x, y] = c;
				}
			}
			return bb;
		}
		public bitmap applyB( bitmap bitmap)
		{
			bitmap b = new bitmap(bitmap.Width, bitmap.Height);
			int width = bitmap.Width;
			int height = bitmap.Height;
			int[,] Colors = b.Data;
			int[] R = new int[size];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int n = 0;
					int x_;
					int y_;
					int r;

					for (int i = -range; i <= range; i++)
					{
						for (int j = -range; j <= range; j++)
						{
							x_ = x + i;
							y_ = y + j;
							r = bitmap.GetSafeB(x_, y_);

							R[n] = r;
							n++;
						}
					}
					Array.Sort(R);
					int c = unchecked((int)(Colors[x, y] & 0xffffff00) | (R[m]));
					Colors[x, y] = c;
				}
			}
			return b;
		}
		public bitmap applyGray(bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap b = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors =b.Data;
			int[] R = new int[size];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int n = 0;
					int x_;
					int y_;
					int r;

					for (int i = -range; i <= range; i++)
					{
						for (int j = -range; j <= range; j++)
						{
							x_ = x + i;
							y_ = y + j;
							r = bitmap.GetSafeB(x_, y_);

							R[n] = r;
							n++;
						}
					}
					Array.Sort(R);
					int cc = (R[m]) & 0xff ;
					int c = (cc<<16)|(cc<<8)|cc;
					Colors[x, y] = c;
				}
			}
			return b;
		}
	}
	public enum Channel:int
		{
			Red,Green,Blue,Gray,All,Alpha
		}
	public class SobelFilter : IFilter
	{
		public static SpatialFilter s1 = SpatialFilter.Sobel_x;
		public static SpatialFilter s2 = SpatialFilter.Sobel_y;
		public static SobelFilter sobelFilter = new SobelFilter();
		bitmap IFilter.apply( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
		
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);	int[,] Colors = bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{double r1 = 0; 
			double g1 = 0;
			double b1 = 0;
			double r2 = 0;
			double g2 = 0;
			double b2 = 0;
	
			foreach (MovingPixel mp in s1.  Pixels)
			{		int color = bitmap.getSafeValue(x+mp.x, y+mp.y);
			int cr = ((color >> 16) & 0xff);
			int cg = ((color >> 8) & 0xff);
			int cb=( (color) & 0xff);
				r1 +=cr  * mp.value;
				g1 +=cg * mp.value;
				b1 +=cb * mp.value;
			}
			foreach (MovingPixel mp in s2.Pixels)
			{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y);
						int cr = ((color >> 16) & 0xff);
						int cg = ((color >> 8) & 0xff);
						int cb = ((color) & 0xff);
						r2 += cr* mp.value;
				g2 += cg * mp.value;
				b2 += cb* mp.value;
			}
			double r = Math.Sqrt(r1 * r1 + r2 * r2);

			double g = Math.Sqrt(g1 *g1 + g2 *g2);
			double b = Math.Sqrt(b1 * b1 + b2 * b2);


			if (r < 0)
			{
				r = -r;
			}
			if (r > 255)
			{
				r = 255;
			}
			if (b < 0)
			{
				b = -b;
			}
			if (b> 255)
			{
				b= 255;
			}
			if (g < 0)
			{
				g= -g;
			}
			if (g > 255)
			{
				g = 255;
			}

			Colors[x,y]= ((int)r << 16) | ((int)g << 8) | (int)b;
				}
			}


			return bb;
		}
		bitmap IFilter.applyGray(bitmap bitmap)
		{

			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r1 = 0;
					double r2 = 0;
			
					foreach (MovingPixel mp in s1.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y);
						
						r1 += (color&0xff) * mp.value;
					}
					foreach (MovingPixel mp in s2.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y);
						r2 += (color & 0xff) * mp.value;
					}
				
					double b = Math.Sqrt(r1 * r1 + r2 * r2);


					if (b < 0)
					{
						b = -b;
					}
					if (b > 255)
					{
						b = 255;
					}

					Colors[x, y] = ((int)b << 16) | ((int)b << 8) | (int)b;
				}
			}
			return bb;
		}

		bitmap IFilter.applyB(bitmap bitmap)
		{

			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors =bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r1 = 0;
					double r2 = 0;
				
					foreach (MovingPixel mp in s1.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y);	int c = ((color ) & 0xff);
						r1 += c * mp.value;
					}
					foreach (MovingPixel mp in s2.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y); int c = ((color) & 0xff);
						r2 += c * mp.value;
					}
					int color0 = bitmap.getSafeValue(x , y ); 
					double r = (color0 >> 16) & 0xff;

					double g = (color0>>8) & 0xff;
					double b = Math.Sqrt(r1 * r1 + r2 * r2);


					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}

					Colors[x,y]= ((int)r << 16) | ((int)g << 8) | (int)b;
				}
			}
			return bb;
		}

	bitmap IFilter.applyG( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r1 = 0;
					double r2 = 0;
					int color0 = bitmap.GetColor(x, y);
					foreach (MovingPixel mp in s1.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y); int c = ((color) & 0xff);
						r1 += c * mp.value;
					}
					foreach (MovingPixel mp in s2.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y); int c = ((color) & 0xff);
						r2 += c * mp.value;
					}
					double g = Math.Sqrt(r1 * r1 + r2 * r2);

					double r = (color0 >> 16) & 0xff;
					double b = (color0) & 0xff;


					if (g < 0)
					{
						g = -g;
					}
					if (g > 255)
					{
						g = 255;
					}

					Colors[x,y]=((int)r << 16) | ((int)g << 8) | (int)b;
				}
			}
			return bb;
		}

		bitmap IFilter.applyR( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors =bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r1 = 0;
					double r2 = 0;
					int color0 = bitmap.GetColor(x, y);
					foreach (MovingPixel mp in s1.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y); int c = ((color) & 0xff);
						r1 += c * mp.value;
					}
					foreach (MovingPixel mp in s2.Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y); int c = ((color) & 0xff);
						r2 += c * mp.value;
					}
					double g = (color0 >> 8) & 0xff;

					double r =  Math.Sqrt(r1 * r1 + r2 * r2);
					double b =(color0 ) & 0xff;


					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}

					Colors[x,y]= ((int)r << 16) | ((int)g << 8) | (int)b;
				}
		
			}
			return bb;
		}
	}

	public class SpatialFilter :IFilter
	{

		int r = 0;
		int g = 0;
		int b = 0;

		public void setBaseRGB(int r,int g,int b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}
		public bitmap apply( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;
			int[,] source = bitmap.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{

					double r = this.r; ;
					double g = this.g;
					double b = this.b;
					
					foreach (MovingPixel mp in Pixels)
					{
						int color = bitmap.getSafeValue(x + mp.x, y + mp.y);
					int cr = ((color >> 16) & 0xff);
					int cg = ((color >> 8) & 0xff);
					int cb = ((color) & 0xff);
						r += cr * mp.value;
						g += cg * mp.value;
						b += cb * mp.value;
					}

					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}
					if (b < 0)
					{
						b = -b;
					}
					if (b > 255)
					{
						b = 255;
					}
					if (g < 0)
					{
						g = -g;
					}
					if (g > 255)
					{
						g = 255;
					}

					Colors[x,y]= ((int)r << 16) | ((int)g << 8) | (int)b;
				}
				
			}
				return bb;
			
		}
		public bitmap applyG( bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = bitmap.Clone();
			int[,] Colors =bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r = 0;
					foreach (MovingPixel mp in Pixels)
					{
						int cr = ((bitmap.getSafeValue(x + mp.x, y + mp.y)) & 0xff);
						r += cr * mp.value;
					}
					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}


					int c = unchecked((int)(Colors[x, y] & 0xffff00ff) | ((int)r << 8));
					Colors[x, y] = c;
				}
			}

			return bb;
		}
		public bitmap applyGray(bitmap bitmap)
		{

			int width = bitmap.Width;
			int height = bitmap.Height;
			bitmap bb = new bitmap(bitmap.Width, bitmap.Height);
			int[,] Colors = bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r = 0;
					
					foreach (MovingPixel mp in Pixels)
					{int cr = ((bitmap.getSafeValue(x+mp.x, y+mp.y)) & 0xff);
						r += cr * mp.value;
					}
					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}


					int c = ((int)r)&0xff;
					Colors[x, y] = (c<<16)|(c<<8)|c;
				}
			}

			return bb;

		}
		public bitmap applyB( bitmap bitmap)
		{

			int width = bitmap.Width;
			int height = bitmap.Height; 
			bitmap bb = bitmap.Clone();
			int[,] Colors =bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r = 0;
					foreach (MovingPixel mp in Pixels)
					{
						int cr = ((bitmap.getSafeValue(x + mp.x, y + mp.y)) & 0xff);
						r += cr * mp.value;
					}
					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}


					int c = unchecked((int)(Colors[x, y] & 0xffffff00) | ((int)r ));
					Colors[x, y] = c;
				}
			}
			return bb;


		}
		public bitmap applyR( bitmap bitmap)
		{

			int width = bitmap.Width;
			int height = bitmap.Height; bitmap bb =bitmap.Clone();
			int[,] Colors = bb.Data;
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					double r = 0;
		
					foreach (MovingPixel mp in Pixels)
					{
						int cr = ((bitmap.getSafeValue(x + mp.x, y + mp.y)) & 0xff);
						r += cr * mp.value;
					}
					if (r < 0)
					{
						r = -r;
					}
					if (r > 255)
					{
						r = 255;
					}


					int c = unchecked((int)(Colors[x, y] & 0xff00ffff) | ((int)r));
					Colors[x, y] = c;
				}
			}
			return bb;



		}


		static SpatialFilter ()
			{
			Box = new SpatialFilter(
				new double[]{1.0/9, 1.0 / 9, 1.0/9 },
				new double[]{ 1.0 / 9, 1.0 / 9, 1.0/9 },
				new double[]{ 1.0 / 9, 1.0 / 9, 1.0 / 9 }
			);
			Sobel_x = new SpatialFilter(
		new double[] { -1, 0, 1 },
		new double[] { -2, 0, 2 },
		new double[]{-1,0,1
});
			Sobel_y = new SpatialFilter(
	new double[] { -1, -2, -1 },
	new double[] { 0, 0, 0 },
	new double[]{1, 2, 1
});
			Sobel = new SpatialFilter(
				new double[]{2,2,0 },
				new double[]{2,0,-2 },
				new double[]{0,-2,-2 
		}) ;
			Priwitt = new SpatialFilter(
		new double[] { -2, -1, 0 },
		new double[] { -1, 0, 1 },
		new double[]{0,1,2
});
			Laplacian = new SpatialFilter(
		new double[] { -1, -1, -1 },
		new double[] { -1, 8, -1 },
		new double[] { -1, -1  ,-1
});
			Roberts = new SpatialFilter(
new double[] { -1, -1},
new double[] { 1, 1}
);_128 = new SpatialFilter();
			_128.setBaseRGB(128, 128, 128);
			Self = new SpatialFilter(new MovingPixel[] {new MovingPixel(0,0,1) });
			Guass = new SpatialFilter(
	new double[] { 1/16.0, 2 / 16.0, 1 / 16.0 },
	new double[] { 2 / 16.0, 4 / 16.0, 2 / 16.0 },
	new double[] { 1 / 16.0, 2 / 16.0, 1 / 16.0 });
		}
		public static SpatialFilter Box;
		public static SpatialFilter Sobel;
		public static SpatialFilter Sobel_x;
		public static SpatialFilter Sobel_y;
		public static SpatialFilter _128;
		public static SpatialFilter Laplacian;
		public static SpatialFilter Guass;
		public static SpatialFilter Self;
		public static SpatialFilter Priwitt;
		public static SpatialFilter Roberts;
		public List<MovingPixel> Pixels { get; private set; }
		public SpatialFilter(List<MovingPixel> pixels)
		{
			this.Pixels = pixels;
		}

		public SpatialFilter(MovingPixel[] pixels)
		{

			this.Pixels = pixels.ToList<MovingPixel>();
		}
		public double getPoint(int x,int y)
		{
			foreach(MovingPixel p in Pixels)
			{
				if(p.x==x&&p.y==y)
				{
					return p.value;
				}
			}
			return 0;
		}
		public void setValue(int x,int y, double v)
		{
			foreach (MovingPixel p in Pixels)
			{
				if (p.x == x && p.y == y)
				{
					p.value = v;
				}
			}
		}
		public void addValue(int x, int y, double v)
		{
			foreach (MovingPixel p in Pixels)
			{
				if (p.x == x && p.y == y)
				{
					p.value += v;
					return;
				}
			}
			Pixels.Add(new MovingPixel(x, y, v));
		}
		public void reduceValue(int x, int y, double v)
		{
			addValue(x, y, -v);
		}

		public void mulValue(int x, int y, double v)
		{
			foreach (MovingPixel p in Pixels)
			{
				if (p.x == x && p.y == y)
				{
					p.value *= v;
					return;
				}
			}
		}
		public void divValue(int x, int y, double v)
		{
			foreach (MovingPixel p in Pixels)
			{
				if (p.x == x && p.y == y)
				{
					p.value /= v;
					return;
				}
			}
		}
		public static SpatialFilter operator -(SpatialFilter a, SpatialFilter b)
		{
			SpatialFilter spatial = new SpatialFilter();
			foreach (MovingPixel p in
				a.Pixels)
			{
				spatial.Pixels.Add(new MovingPixel(p.x, p.y, p.value));
			}
			foreach (MovingPixel p in b.Pixels)
			{
				spatial.divValue(p.x, p.y, -p.value);
			}
			return spatial;
		}
		public static SpatialFilter operator -(SpatialFilter a)
		{
			SpatialFilter spatial = new SpatialFilter();
			foreach (MovingPixel p in
				a.Pixels)
			{
				spatial.Pixels.Add(new MovingPixel(p.x,p.y,-p.value));
			}
		
			return spatial;
		}
		public static SpatialFilter operator /(SpatialFilter a, SpatialFilter b)
		{
			SpatialFilter spatial = new SpatialFilter();
			foreach (MovingPixel p in
				a.Pixels)
			{
				spatial.Pixels.Add(new MovingPixel(p.x, p.y, p.value));
			}
			foreach (MovingPixel p in b.Pixels)
			{
				spatial.divValue(p.x, p.y, p.value);
			}
			return spatial;
		}
		public static SpatialFilter operator *(SpatialFilter a, SpatialFilter b)
		{
			SpatialFilter spatial = new SpatialFilter();
			foreach (MovingPixel p in
				a.Pixels)
			{
				spatial.Pixels.Add(new MovingPixel(p.x, p.y, p.value));
			}
			foreach (MovingPixel p in b.Pixels)
			{
				spatial.mulValue(p.x, p.y, p.value);
			}
			return spatial;
		}
		public static SpatialFilter operator+(SpatialFilter a,SpatialFilter b)
		{
			SpatialFilter spatial = new SpatialFilter();
			foreach(MovingPixel p in
				a.Pixels)
			{
				spatial.Pixels.Add(new MovingPixel(p.x, p.y, p.value));
			}
			foreach(MovingPixel p in b.Pixels)
			{
				spatial.addValue(p.x, p.y, p.value);
			}
			return spatial;
		}
		public SpatialFilter(params double[][] vs)
		{
			Pixels = new List<MovingPixel>();
			int cy = vs.Length / 2;
			for(int i=0;i<vs.Length;i++)
			{
				int cx = vs[i].Length / 2;
				for(int j=0;j<vs[i].Length;j++)
				{
					Pixels.Add(new MovingPixel( j - cx,i - cy, vs[i][j]));
					
				}
			}
			

		}

		public SpatialFilter()
		{
			Pixels = new List<MovingPixel>();
		}
	}
	public class v_world : world
	{



		System.Timers.Timer drawing_timer;
		int height;
		int width;
		vector2[][] objecttracer;
		public bitmap Bitmap = new bitmap(1, 1);
		public int[] obj_c;
		public int[] obj_tc;
		public int[] fie_c;
		public int[] fie_tc;
		public bool usingtrace = false;
		public mp_object2 traced;
		vector2[][] fieldtracer;
		public bool b1 = false;
		public bool b2 = false;
		public bitmap backgroung1;
		int bn = 1;
		public double zoom;
		public vector2 pos;
		public bool usingtracer = true;
		public bool showv = false;
		public bool showf = false;
		public v_world() : base()
		{
			drawing_timer = new System.Timers.Timer();
			drawing_timer.Interval = 1;
			drawing_timer.Elapsed += tick;
			height = 300;
			width = 300;
			backgroung1 = new bitmap(width, height);
			objecttracer = new vector2[0][];
			fieldtracer = new vector2[0][];
			obj_c = new int[0];
			obj_tc = new int[0];
			fie_c = new int[0];
			fie_tc = new int[0];
			zoom = 1;
			pos = 0;
		}
		public vector2 getcenter()
		{
			return new vector2((double)width / 2, (double)height / 2);
		}
		public v_world(int t, int w, int h) : base(t)
		{
			drawing_timer = new System.Timers.Timer();
			drawing_timer.Interval = t;
			drawing_timer.Elapsed += tick;
			height = h;
			width = w;
			backgroung1 = new bitmap(width, height);
			objecttracer = new vector2[0][];
			fieldtracer = new vector2[0][];
			obj_c = new int[0];
			obj_tc = new int[0];
			fie_c = new int[0];
			fie_tc = new int[0];
			zoom = 1;
			pos = 0;
		}
		new public void removeobject(int i)
		{
			base.removeobject(i);
			int[] temp = new int[obj_c.Length - 1];
			for (int j = 0, n = 0; j < obj_c.Length; j++)
			{
				if (j != i)
				{
					temp[n] = obj_c[j];
					n++;
				}
			}
			obj_c = temp;
			temp = new int[obj_tc.Length - 1];
			for (int j = 0, n = 0; j < obj_tc.Length; j++)
			{
				if (j != i)
				{
					temp[n] = obj_tc[j];
					n++;
				}
			}
			obj_tc = temp;

			vector2[][] temp2 = new vector2[objecttracer.GetLength(0) - 1][];
			for (int j = 0, n = 0; j < objecttracer.GetLength(0); j++)
			{
				if (j != i)
				{
					temp2[n] = objecttracer[j];
					n++;
				}
			}
			objecttracer = temp2;

		}
		new public void clean()
		{
			base.clean();
			objecttracer = new vector2[0][];
			fieldtracer = new vector2[0][];
			obj_c = new int[0];
			obj_tc = new int[0];
			fie_c = new int[0];
			fie_tc = new int[0];
		}
		new public void removefield(int i)
		{
			base.removefield(i);
			int[] temp = new int[fie_c.Length - 1];
			for (int j = 0, n = 0; j < fie_c.Length; j++)
			{
				if (j != i)
				{
					temp[n] = fie_c[j];
					n++;
				}
			}
			fie_c = temp;
			temp = new int[fie_tc.Length - 1];
			for (int j = 0, n = 0; j < fie_tc.Length; j++)
			{
				if (j != i)
				{
					temp[n] = fie_tc[j];
					n++;
				}
			}
			fie_tc = temp;

			vector2[][] temp2 = new vector2[fieldtracer.Length - 1][];
			for (int j = 0, n = 0; j < fieldtracer.Length; j++)
			{
				if (j != i)
				{
					temp2[n] = fieldtracer[j];
					n++;
				}
			}
			fieldtracer = temp2;

		}
		new public void addobject(mp_object2 o)
		{
			base.addobject(o);
			int[] tempc;
			tempc = new int[obj_c.Length + 1];
			obj_c.CopyTo(tempc, 0);
			tempc[obj_c.Length] = Colors.Green;
			obj_c = tempc;
			tempc = new int[obj_tc.Length + 1];
			obj_tc.CopyTo(tempc, 0);
			tempc[obj_tc.Length] = Colors.Red;
			obj_tc = tempc;

			vector2[][] tempv;
			tempv = new vector2[objecttracer.Length + 1][];
			objecttracer.CopyTo(tempv, 0);
			tempv[objecttracer.Length] = new vector2[0];
			objecttracer = tempv;

		}
		vector2 ptp(Vector2i p)
		{
			vector2 dr = (p.Vector2 - getcenter()) / zoom;
			return -pos + dr;
		}
		Vector2i ptp(vector2 p)
		{

			vector2 r = p + pos;
			vector2 dr = r * zoom + getcenter();
			return new Vector2i(dr.X,dr.Y);

		}
		void drawv(mp_object2 o,int c, bitmap b)
		{
			Picture.drawline(b, ptp(o.getdisplacement()), ptp(o.getdisplacement() + o.getvelocity2() / 4), 1, c);
			Picture.drawring(b, ptp(o.getdisplacement() + o.getvelocity2() / 4), 4, 3, c);
		}

		new public void addfield(field2 o)
		{
			base.addfield(o);
			int[] tempc;
			tempc = new int[fie_c.Length + 1];
			fie_c.CopyTo(tempc, 0);
			tempc[fie_c.Length] = Colors.Blue;
			fie_c = tempc;
			tempc = new int[fie_tc.Length + 1];
			fie_tc.CopyTo(tempc, 0);
			tempc[fie_tc.Length] = Colors.Green;
			fie_tc = tempc;
			vector2[][] tempv;
			tempv = new vector2[fieldtracer.Length + 1][];
			fieldtracer.CopyTo(tempv, 0);
			tempv[fieldtracer.Length] = new vector2[0];
			fieldtracer = tempv;
		}
		public int[] searchname(stuff t, string name)
		{
			int[] result = new int[0];
			if (t == stuff.场 || t == stuff.场轨迹)
			{
				for (int i = 0; i < this.fie_c.Length; i++)
				{
					if (getfield(i).getname() == name)
					{
						int[] temp = new int[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = i;
						result = temp;

					}
				}
			}
			else
			{
				for (int i = 0; i < this.obj_c.Length; i++)
				{
					if (getobject(i).getname() == name)
					{
						int[] temp = new int[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = i;
						result = temp;

					}
				}
			}

			return result;
		}
		public void setcolor(stuff t, string name, int c)
		{
			if (t == stuff.场)
			{
				foreach (int i in
				searchname(stuff.场, name))
				{
					fie_c[i] = c;
				}
			}
			else if (t == stuff.场轨迹)
			{
				foreach (int i in
				searchname(stuff.场, name))
				{
					fie_tc[i] = c;
				}
			}
			else if (t == stuff.物体)
			{
				foreach (int i in
				searchname(stuff.物体, name))
				{
					obj_c[i] = c;
				}
			}
			else
			{
				foreach (int i in
				searchname(stuff.物体, name))
				{
					obj_tc[i] = c;
				}
			}
		}
		public void startdraw()
		{
			drawing_timer.Start();
		}

		public void stopdraw()
		{
			drawing_timer.Start();
		}
		public double getzoom()
		{
			return zoom;
		}
		public int n = 500;
		public void updatatracer()
		{

			vector2[] tempv;

			if (true)
			{
				for (int i = 0; i < objecttracer.Length; i++)

				{
					tempv = new vector2[objecttracer[i].Length + 1];
					objecttracer[i].CopyTo(tempv, 0);
					objecttracer[i] = tempv;
					objecttracer[i][objecttracer[i].Length - 1] = getobject(i).getdisplacement();
				}
				for (int i = 0; i < fieldtracer.Length; i++)

				{
					tempv = new vector2[fieldtracer[i].Length + 1];
					fieldtracer[i].CopyTo(tempv, 0);
					fieldtracer[i] = tempv;
					fieldtracer[i][fieldtracer[i].Length - 1] = getfield(i).getdisplacement();
				}
			}

			for (int i = 0; i < objecttracer.Length; i++)
			{
				if (objecttracer[i].Length > n)
				{
					tempv = new vector2[n];
					for (int i2 = n - 1; i2 >= 0; i2--)
					{

						tempv[i2] = objecttracer[i][i2 + 1];
					}
					objecttracer[i] = tempv;
				}
			}
			for (int i = 0; i < fieldtracer.Length; i++)
			{
				if (fieldtracer[i].Length > n)
				{
					tempv = new vector2[n];
					for (int i2 = n - 1; i2 >= 0; i2--)
					{
						tempv[i2] = fieldtracer[i][i2 + 1];
					}
					fieldtracer[i] = tempv;
				}
			}
		}
		public void tick(object sender, EventArgs e)
		{





		}
	}
	public struct up
	{
		public double maxx { get; }
		public double maxy { get; }
		public double minx { get; }
		public double miny { get; }
		public up(double ax, double ay, double ix, double iy)
		{
			maxx = ax;
			maxy = ay;
			minx = ix;
			miny = iy;
		}
	}
	public class BmpFile
	{
		ushort bfType= 0x4d42;
		uint bfSize;
		ushort bfR1=0;
		ushort bfR2=0;
		uint bfOddBits;


		uint bSize=40;
		uint width, height;
		ushort biPlanes=1;
		ushort Bitcount;
		uint Compress=0, SizeImage;
		uint ClrUsed=0, ClrImportant=0;
		uint xpm = 2834;
		uint ypm = 2834;
	public bitmap bitmap { get; private set; }

		public BmpFile(string path)
		{
			bitmap = new bitmap(1,1);
			if(File.Exists(path))
			{
				FileStream s = new FileStream(path, FileMode.Open);
				readFormSream(s);
				s.Close();
			}
			
		}
		public BmpFile(bitmap bitmap)
		{
			this.bitmap = bitmap;
			width =(uint) bitmap.Width;
			height =(uint) bitmap.Height;
			bfOddBits =54
				;
			SizeImage = (uint)(width * height * 3);
			bfSize =(uint) (width * height * 3 + 54);
			Bitcount = 8 * 3;


		}

	
		public void setBitCount(int b)
		{
			Bitcount = (ushort)b;
		}
		public void Save(string path)
		{
			FileStream s = new FileStream(path, FileMode.Create);
			if(Bitcount==24)
			{
writeToStream(s);
			}
			
			s.Flush();
			s.Close();
		}


		public void readFormSream(Stream stream)
		{
			byte[] temp = new byte[4];
			stream.Read(temp, 0, 2);
			bfType = BitConverter.ToUInt16(temp, 0);
			stream.Read(temp, 0, 4);
			bfSize = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 2);
			bfR1= BitConverter.ToUInt16(temp, 0); ;
			stream.Read(temp, 0, 2);
			bfR2 = BitConverter.ToUInt16(temp, 0); ;
			stream.Read(temp, 0, 4);
			bfOddBits = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			bSize = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			width = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			height = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 2);
			biPlanes = BitConverter.ToUInt16(temp, 0); ;
			stream.Read(temp, 0, 2);
			Bitcount = BitConverter.ToUInt16(temp, 0); ;
			stream.Read(temp, 0, 4);
			Compress = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			SizeImage = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			xpm = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			ypm = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			ClrUsed = BitConverter.ToUInt32(temp, 0);
			stream.Read(temp, 0, 4);
			ClrImportant = BitConverter.ToUInt32(temp, 0);

			bitmap = new bitmap((int)width,(int) height);

			int[,] C = bitmap.Data;
			if(Bitcount==24)
			{
for (int j = (int)height - 1; j >= 0; j--)
			{
				for (int i = 0; i < width; i++)
				{
					stream.Read(temp, 0, 3);
					temp[3] = 0;
					C[i, j] = BitConverter.ToInt32(temp, 0);
				}
			}
			}
			else if(Bitcount==32)
			{

				for (int j = (int)height - 1; j >= 0; j--)
				{
					for (int i = 0; i < width; i++)
					{
						stream.Read(temp, 0, 4);
						C[i, j] = BitConverter.ToInt32(temp, 0);
					}
				}
			}
			
		}
		public void writeToStreamAsGray(Stream stream)
		{
			stream.Write(BitConverter.GetBytes(bfType), 0, 2);
			stream.Write(BitConverter.GetBytes(bfSize), 0, 4);
			stream.Write(BitConverter.GetBytes(bfR1), 0, 2);
			stream.Write(BitConverter.GetBytes(bfR2), 0, 2);
			stream.Write(BitConverter.GetBytes(bfOddBits), 0, 4);
			stream.Write(BitConverter.GetBytes(bSize), 0, 4);
			stream.Write(BitConverter.GetBytes(width), 0, 4);
			stream.Write(BitConverter.GetBytes(height), 0, 4);
			stream.Write(BitConverter.GetBytes(biPlanes), 0, 2);
			stream.Write(BitConverter.GetBytes(Bitcount), 0, 2);
			stream.Write(BitConverter.GetBytes(Compress), 0, 4);
			stream.Write(BitConverter.GetBytes(SizeImage), 0, 4);
			stream.Write(BitConverter.GetBytes(xpm), 0, 4);
			stream.Write(BitConverter.GetBytes(ypm), 0, 4);
			stream.Write(BitConverter.GetBytes(ClrUsed), 0, 4);
			stream.Write(BitConverter.GetBytes(ClrImportant), 0, 4);

			int[,] C = bitmap.Data;

			for (int j = (int)height - 1; j >= 0; j--)
			{
				for (int i = 0; i < width; i++)
				{
					byte[] color = BitConverter.GetBytes(C[i, j]);
					stream.Write(color, 0, 1);
				}
			}


		}
		public void writeToStream(Stream stream)
		{
			stream.Write(BitConverter.GetBytes(bfType),0,2);
			stream.Write(BitConverter.GetBytes(bfSize), 0, 4);
			stream.Write(BitConverter.GetBytes(bfR1), 0, 2);
			stream.Write(BitConverter.GetBytes(bfR2), 0, 2);
			stream.Write(BitConverter.GetBytes(bfOddBits), 0, 4);
			stream.Write(BitConverter.GetBytes(bSize), 0, 4);
			stream.Write(BitConverter.GetBytes(width), 0, 4);
			stream.Write(BitConverter.GetBytes(height), 0, 4);
			stream.Write(BitConverter.GetBytes(biPlanes), 0, 2);
			stream.Write(BitConverter.GetBytes(Bitcount), 0, 2);
			stream.Write(BitConverter.GetBytes(Compress), 0, 4);
			stream.Write(BitConverter.GetBytes(SizeImage), 0, 4);
			stream.Write(BitConverter.GetBytes(xpm), 0, 4);
			stream.Write(BitConverter.GetBytes(ypm), 0, 4);
			stream.Write(BitConverter.GetBytes(ClrUsed), 0, 4);
			stream.Write(BitConverter.GetBytes(ClrImportant), 0, 4);

			int[,] C = bitmap.Data;

			if(Bitcount==24)
			{
	for(int j=(int)height-1;j>=0;j--)
			{
for(int i=0;i<width;i++)
			{
					byte[] color = BitConverter.GetBytes(C[i, j]);
					stream.Write(color, 0, 3);
				}
			}
			}	
			else if(Bitcount==32)
			{
				for (int j = (int)height - 1; j >= 0; j--)
				{
					for (int i = 0; i < width; i++)
					{
						byte[] color = BitConverter.GetBytes(C[i, j]);
						stream.Write(color, 0,4);
					}
				}
			}
		

				
		}
	}

	public class NamedBitmap
	{
		public string name { get; private set; }
		char[] name_;
		int width
		{
			get { return bitmap.Width; }
		}
		int height
		{
			get { return bitmap.Height; }
		}
		public 	bitmap bitmap { get; internal set; }
		public NamedBitmap(string name,bitmap bitmap)
		{
			if (name == null)
				name = "";
			this.name = name;
			this.bitmap = bitmap;
			name_ = name.ToArray();
			
			
		}
		public void write(Stream stream )
		{
			stream.Write(BitConverter.GetBytes(name_.Length), 0, 4);
			for(int i=0;i<name_.Length;i++)
			{
				stream.Write(BitConverter.GetBytes(name_[i]), 0, 2);
			}
			stream.Write(BitConverter.GetBytes(width), 0, 4);
			stream.Write(BitConverter.GetBytes(height), 0, 4);
			for(int i=0;i<width;i++)
			{
				for(int j=0;j<height;j++)
				{
					stream.Write(BitConverter.GetBytes(bitmap[i, j]), 0, 4);
				}
			}
			stream.Flush();

		}
		public static NamedBitmap readNamedBitmap(Stream stream)
		{
			byte[] temp = new byte[4];
			stream.Read(temp, 0, 4);
			int l_name = BitConverter.ToInt32(temp, 0);
			char[] chars = new char[l_name];
			
			for (int i = 0; i < l_name; i++)
			{
				stream.Read(temp, 0, 2);
				chars[i] = BitConverter.ToChar(temp, 0);
			}
			string name = new string(chars);
			stream.Read(temp, 0, 4);
			int width = BitConverter.ToInt32(temp, 0);
			stream.Read(temp, 0, 4);
			int height = BitConverter.ToInt32(temp, 0);
			bitmap bitmap = new bitmap(width, height);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					stream.Read(temp, 0, 4);
					bitmap[i,j] = BitConverter.ToInt32(temp, 0);
				}
			}
			return new NamedBitmap(name, bitmap);

		}
	}

	
	public class BitmapPack
	{
		public int count { get { return Bitmaps.Count; }
		
		}
		List<NamedBitmap> Bitmaps=new List<NamedBitmap>();
		public bitmap this[string name]
		{
			get
			{
				foreach (NamedBitmap bitmap in Bitmaps)
				{
					if (bitmap.name.Equals(name))
					{
						return bitmap.bitmap;
					}
				}
				return null;
			}
			set {
				foreach (NamedBitmap bitmap in Bitmaps)
				{
					if (bitmap.name.Equals(name))
					{
						bitmap.bitmap = value;
						return;
						
					}
				}
				Bitmaps.Add(new NamedBitmap(name,value));
			}
		}
		public void newVersion()
		{
			
			foreach(NamedBitmap bitmap in Bitmaps)
			{

				bitmap.bitmap.ParallelForeach((x, y, data) =>
				{
					int c= data[x, y];
					if (c==Colors.Alpha)
					{
						data[x, y] = 0x00000000;
					}
					else
					{
						data[x, y] = c | Colors.Alpha;
					}
				});
			}
		}
		public void remove(string name)
		{
			for(int i=0;i<Bitmaps.Count;i++)
			{
				if (Bitmaps[i].name.Equals(name))
				{
					Bitmaps.RemoveAt(i);
					return;
				}
			}
		}

		public void addOrUpdate(bitmap bitmap, string name)
		{
			this[name] = bitmap;
		}
		public void add(bitmap bitmap, string name)
		{
			add(new NamedBitmap(name, bitmap));
		}
		public void add(NamedBitmap bitmap)
		{
			Bitmaps.Add(bitmap);
		}

		public void write(Stream stream)
		{
			stream.Write(BitConverter.GetBytes(count), 0, 4);
			stream.Flush();
			for(int i=0;i<count;i++)
			{
				Bitmaps[i].write(stream);
			}
			
		}

		public void read(Stream stream)
		{
			byte[] bytes = new byte[4];
			stream.Read(bytes, 0, 4);
			int c = BitConverter.ToInt32(bytes, 0);
			for(int i=0;i<c;i++)
			{
				add(NamedBitmap.readNamedBitmap(stream));
			}
		}
		public void readFile(String stream)
		{
			if (File.Exists(stream))
			{
FileStream fileStream = new FileStream(stream, FileMode.Open);
				read(fileStream);
				fileStream.Close();
			}
			
		}

		public static BitmapPack readPack(Stream stream)
		{
			BitmapPack pack = new BitmapPack();
			byte[] bytes = new byte[4];
			stream.Read(bytes, 0, 4);
			int c = BitConverter.ToInt32(bytes, 0);
			for (int i = 0; i < c; i++)
			{
				pack.add(NamedBitmap.readNamedBitmap(stream));
			}
			return pack;
		}
		public void save(string path)
		{
			DirectoryInfo info = new DirectoryInfo(path);
			if(!info.Parent.Exists)
			{
				info.Parent.Create();
			}
			FileStream stream = new FileStream(path, FileMode.Create);
			write(stream);
			stream.Flush();
			stream.Close();
		}
		public override string ToString()
		{
			string name = "{";
			for(int i=0;i<Bitmaps.Count;i++)
			{
				name += Bitmaps[i].name;
				if (i < Bitmaps.Count - 1)
				{
					name += ",";
				}
			}
			name += "}";
			return name;
		}
	}
	public class bitmap:Map<int>
	{
		public static int RGBtoColor(double r,double g,double b)
		{
			int R = (int)r;

			int G = (int)g;

			int B = (int)b;
			if (R < 0) R = -R;
			if (R > 255) R = 255;
			if (G < 0) G= -R;
			if (G > 255)G = 255;
			if (B< 0) B = -B;
			if (B > 255) B = 255;

			return (R << 16) | (G << 8) | B;

		}
		public int HWidth { get; }
		public string name = "";
		public int HHeight { get; }

		public int[,] getData()
		{
			return Data;
		}
	
		public bitmap(string path)
		{
			name = path;
			try
			{
                Image<Argb32> image=Image.Load<Argb32>(path);
				if (image != null)
				{
					Width= image.Width;
					Height= image.Height;
					HWidth = image.Width/2;
					HHeight = image.Height/2;
					Data=new int[Width,Height];
					image.ProcessPixelRows(new Converter(this).toBitmap);
					image.Dispose();
				}
			}
			catch
			{
				
			}

		}
		public bitmap getRect(Rectanglei r)
		{
			bitmap bitmap = new bitmap(r.width, r.height);
			bitmap.Foreach((x, y, d) => {
				d[x, y] = this[x+r.X,y+r.Y];
			});
			return bitmap;
		}
		public static implicit operator Image<Argb32>(bitmap bitmap)
		{
			Image<Argb32> image = new Image<Argb32>(bitmap.Width, bitmap.Height);
			image.ProcessPixelRows(new Converter(bitmap).toImage);
			return image;
		}
		public bitmap[] getRandomAreas(int count,int width,int height,int minWidth=1,int minHeight=1,int maxWidth=-1,int maxHeight=-1)
		{
			if(maxWidth<1)
			{
				maxWidth = Width;
			}
			if(maxHeight<1)
			{
				maxHeight = Height;
			}

			bitmap[] bitmaps = new bitmap[count];
			SearchBox searchBox = new SearchBox(this, width, height);
			double wh = width * 1.0 / height;
			Random r = new Random((int)DateTime.Now.Ticks);
			int w, h;
			foreach(int i in count.getRange())
			{
				h = r.Next(minHeight,r.Next(minHeight, maxHeight+1));
				w = (int)(h * wh);
				if(w<minWidth)
				{
					w = minWidth;
				}
				if(w>=maxWidth)
				{
					w = maxWidth-1;
				}
				searchBox.setSize(w, h);

				bitmaps[i] = searchBox[r.Next(0, Width), r.Next(0, Height)].Clone();
			}
			return bitmaps;
		}
		public bitmap grow(int x, int y)
		{
			bitmap map = new bitmap(Width * x, Height * y);
			int temp;

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


		public ComplexMap moveFFT(double x, double y, Channel channel = Channel.Red)
		{
		
				ComplexMap map = new ComplexMap(this, channel);
				map = map.FFT().fftShift().move(x, y).ifftShift();
				return map;
			
		}
		
		public bitmap move(double x,double y,Channel channel=Channel.All)
		{
			if (channel==Channel.All)
			{	bitmap bitmap = new bitmap(Width, Height);
			    int[,]data=bitmap.Data;
				ComplexMap a = new ComplexMap(this, Channel.Alpha);
				ComplexMap r = new ComplexMap(this, Channel.Red);
				ComplexMap g = new ComplexMap(this, Channel.Green);
				ComplexMap b = new ComplexMap(this, Channel.Blue);
				a=a.FFT().fftShift().move(x,y).ifftShift().iFFT();
				r = r.FFT().fftShift().move(x, y).ifftShift().iFFT();
				g = g.FFT().fftShift().move(x, y).ifftShift().iFFT();
				b = b.FFT().fftShift().move(x, y).ifftShift().iFFT();

				Complex[,] ad=a.Data, rd=r.Data, gd=g.Data, bd=b.Data;
				bitmap.Foreach((i, j, d) =>
				{
					data[i, j] = (((int)ad[i, j].length() & 255) << 24) |
					(((int)rd[i, j].length() & 255) << 16) |
					(((int)gd[i, j].length() & 255) << 8) |
					(((int)bd[i, j].length() & 255));
				});
				return bitmap;
			}
			else
			{
				ComplexMap map = new ComplexMap(this, channel);
				map = map.FFT().fftShift().move(x, y).ifftShift().iFFT();
				return map.toBitmap();
			}

		}
		public bitmap grow_One2Four()
		{
			bitmap map = new bitmap(Width<<1, Height<<1);
			int[,] data = map.Data;
			int x, y,v;
				for(int i = 0; i < Width; i++)
				{
				x = i << 1;
					for(int j=0; j < Height;j++)
					{
					y = j << 1;
					v = Data[i, j];
					data[x+1, y+1] = data[x, y+1] = data[x+1, y] = data[x, y] = v;
					}
				}
			return map;
			
		}

		public bitmap scaleAndRotate(double degree,double factor,int x,int y)
		{
			Vector2 center = new vector2(x, y),v;
			Vector2i vp;

			bitmap bitmap = new bitmap(Width,Height);

			bitmap.Foreach((i, j, d) => {
				v = new vector2(i, j) - center;
				v=v.row(degree)/factor+center;
				vp = v.getClost();
				d[i, j] = getSafeValue(vp.X, vp.Y);

			});
			return bitmap;

		}
		public bitmap scaleAndRotate_(double degree, double factor, double x, double y)
		{
			Vector2 center = new vector2(x, y), v;
			Vector2i vp;

			bitmap bitmap = new bitmap(Width, Height);

			bitmap.Foreach((i, j, d) => {
				v = new vector2(i, j) - center;
				v = v.row(degree) / factor + center;
				int vv = getGoodR(v.X, v.Y);
				d[i, j] = (255,vv,vv,vv).ARGB2Int();

			});
			return bitmap;

		}
		public bitmap scale_( double factor, double x, double y)
		{
			Vector2 center_ = new vector2(x, y);
			Vector2 center = new vector2(x*factor, y*factor), v;
			Vector2i vp;

			bitmap bitmap = new bitmap((int)(Width*factor),(int)( Height*factor));

			bitmap.Foreach((i, j, d) => {
				v = new vector2(i, j) - center;
				v = v / factor + center_;
				int vv = getGoodR(v.X, v.Y);
				d[i, j] = (255, vv, vv, vv).ARGB2Int();

			});
			return bitmap;

		}
		public bitmap igrow_FourInOne()
		{
			int w = Width;
			int h = Height;
			if(Width%2==1)
			{
				w++;
			}
			if(Height%2==1)
			{
				h++;
			}
			bitmap map = new bitmap(w, h);
			int[,] md = map.Data;
			Foreach((i, j, d) => {
				md[i, j] = d[i, j];
			});

			w = w >> 1;
			h = h >> 1;
			int x, y;
			int p1, p2, p3, p4,r,g,b,a;
			bitmap bitmap=new bitmap(w,h);
			int[,]db=bitmap.Data;
			for (int i = 0; i < w; i++)
			{
				x = i << 1;
				for (int j = 0; j < h; j++)
				{
					y=j << 1;
					p1=Data[x, y];
					p2 = Data[x+1, y];
					p3 = Data[x, y+1];
					p4 = Data[x+1, y+1];

					 b = ((p1 & 0xff) + (p3 & 0xff) + (p2 & 0xff) + (p4 & 0xff)) >> 2;
					 g = (((p1>>8) & 0xff) + ((p3>>8) & 0xff) + ((p2>>8) & 0xff) + ((p4>>8) & 0xff)) >> 2;
					 r = (((p1 >> 16) & 0xff) + ((p3 >> 16) & 0xff) + ((p2 >> 16) & 0xff) + ((p4 >> 16) & 0xff)) >> 2;
					 a = (((p1 >> 24) & 0xff) + ((p3 >> 24) & 0xff) + ((p2 >> 24) & 0xff) + ((p4 >> 24) & 0xff)) >> 2;

					db[i,j]=(a<<24)|(r<<16)|(g<<8)|(b);

				}
			}
			return bitmap;
		}
		public bitmap igrow(int x, int y)
		{
			bitmap map = new bitmap(Width / x, Height / y);
			int temp;
			int w = Width / x;
			int h = Height / y;
			int s = x * y;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					temp = 0;
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
		public bitmap scale(int w,int h)
		{
			if(w==Width&&h==Height)
			{
				return Clone();
			}
			bitmap bitmap = new bitmap(w, h);
			int[,] C = bitmap.Data;
			double kx = (double)Width / w;
			double ky = (double) Height /h;
			for (int i = 0; i < w;i++) 
			{
				for (int j = 0; j < h; j++) 
				{
					C[i,j]=Data[(int)(i*kx), (int)(j*ky)];
				}
			}
			return bitmap;
		}
		public new  bitmap  Move(int x,int y)
		{
			return new bitmap( base.Move(x,y));
		}


		
		public bitmap(bitmap bitmap)
		{
			this.Width = bitmap.Width;
			this.Height = bitmap.Height;
			this.HHeight = bitmap.HHeight;
			this.HWidth = bitmap.HWidth;
			this.Data =(int[,]) bitmap.Data.Clone();
		}

		public bitmap pam()
		{

			bitmap bitmap = new bitmap(Width, Height);
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)

				{

					bitmap.SetPixel(x, y, ((Color0)(new Vector3(255, 255, 255) - GetPixel(x, y).toVec())).Code);
				}

			}
			return bitmap;
		}

		public Vector2 getMassPoint()
		{
			Vector2 v=new vector2(0,0);
			double c=0;double vvv;
			Foreach((x, y, d) => {
				var vv = d[x, y].Int2ARGB();
				vvv=vv.a * (vv.r + vv.g + vv.b);
				v += new vector2(x, y) * vvv;
				c += vvv;
			});
			return v /c;
		}

		
	
		public bitmap(int width, int height)
		{
			Width = width;
			Height = height;
			HWidth = width / 2;
			HHeight = height / 2;
			Data = new int[Width, Height];
		}
	
		public static bitmap operator +(bitmap a, bitmap b)
		{
			int w = Math.Min(a.Width, b.Width);
			int W = Math.Max(a.Width, b.Width);
			int h = Math.Min(a.Height, b.Height);
			int H = Math.Max(a.Height, b.Height);
			bitmap bb = new bitmap(W, H);
			for(int i=0;i<w;i++)
			{
				for(int j=0;j<h;j++)
				{
					int r = a.GetR(i, j) + b.GetR(i, j);
					int g = a.GetG(i, j) + b.GetG(i, j);
					int b_ = a.GetB(i, j) + b.GetB(i, j);
					bb.SetSafePixel(i, j, r, g, b_);
				}
			}
			return bb;
		}

		public static bitmap operator -(bitmap a, bitmap b)
		{
			int w = Math.Min(a.Width, b.Width);
			int W = Math.Max(a.Width, b.Width);
			int h = Math.Min(a.Height, b.Height);
			int H = Math.Max(a.Height, b.Height);
			bitmap bb = new bitmap(W, H);
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					int r = a.GetR(i, j) - b.GetR(i, j);
					int g = a.GetG(i, j) - b.GetG(i, j);
					int b_ = a.GetB(i, j) - b.GetB(i, j);
					bb.SetSafePixel(i, j, r, g, b_);
				}
			}
			return bb;
		}

		
		public bitmap applyWin(win win)
		{
			int a;



			bitmap bitmap = new bitmap(Width, Height);
			int[,] data1 = bitmap.getData();
			ParallelForeach((int x, int y, int[,] data) => {
				(int,int,int) c=data[x,y].Int2RGB();
				data1[x, y] = (win(c.Item1), win(c.Item2), win(c.Item3)).RGB2Int();
			});

			

			return bitmap;
		}
		public bitmap applyFiltering(IFilter filter,Channel channel)
		{
			
		switch(channel)
			{
				case Channel.All: return applyFiltering(filter);
				case Channel.Red: return applyFilteringR(filter);
				case Channel.Green: return applyFilteringG(filter);
				case Channel.Blue: return applyFilteringB(filter);
				case Channel.Gray: return applyFilteringGray(filter);
			}
			bitmap bp = new bitmap(Width, Height);
			return bp;
		}
		public bitmap toGray()
		{
			bitmap bp = new bitmap(Width, Height);

			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					int c = Data[i, j];
					int v =(( (c&0x00ff0000))>>16 )*299+( ((c& 0x0000ff00))>> 8)*587+ ( + ((c & 0x000000ff)))*144 ;
					v /= 1000;
					if(v>255)
					{
						v = 255;
					}
					v = v & 0xff;
					bp.Data[i, j] = (v) | ((v) << 8)  | ((v )<< 16)|(0xff<<24) ;
				}
			}
			return bp;
		}

		public int getGoodR(double x,double y)
		{
			Vector2i p1=new Vector2i((int)x,(int)y);
			Vector2i p2 = p1.add(1, 0);
			Vector2i p3 = p2.add(0, 1);
			Vector2i p4 = p3.add(-1, 0);

			double rx = x - p1.X;
			double ry = y - p1.Y;
			double rx_ = 1 - rx;
			double ry_ = 1 - ry;

			double v = GetSafeR(p1.X, p1.Y) * rx_* ry_+ GetSafeR(p2.X, p2.Y) * rx * ry_
				+ GetSafeR(p3.X, p3.Y) * rx * ry + GetSafeR(p4.X, p4.Y) * rx_ * ry;

			return (int)v;

		}

		public bitmap applyFilteringGray(IFilter filter)
		{
			
			
			return filter.applyGray(this);
		}
		public bitmap applyFilteringB(IFilter filter)
		{
			
			return filter.applyB(this);
		}
		public bitmap applyFilteringR(IFilter filter)
		{
		
			;
			return filter.applyR(this);
		}
		public bitmap applyFilteringG(IFilter filter)
		{
			
			return filter.applyG(this);

		}

	
		public int getSafeValue(int x,int y)
		{
			if (x >= 0 && y >= 0 && x < Width && y < Height)
			{
				return Data[x, y];
			}
			return 0;
		}
		public int getSafeValue(int x, int y,int Default)
		{
			if (x >= 0 && y >= 0 && x < Width && y < Height)
			{
				return Data[x, y];
			}
			return Default;
		}
		public bitmap applyFiltering(IFilter filter)
		{
		
			;
			return filter.apply(this);
		}

		public void SetPixel(int x, int y, int c)
		{

			this.Data[x, y] = c;
		}
		public void SetPixel(int x, int y, Color0 c)
		{

			this.Data[x, y] = c.Code;
		}
		public void SetPixel_m(int x, int y, int c, double p)
		{
			int R = 0, G = 0, B = 0;

			var color = c.Int2RGB();
			int cc = this.Data[x, y];
			if (p >= 0 && p <= 1)

			{
				R = (int)(((cc>>16)&0xff )* (1 - p) + color.r * p);
				G = (int)(((cc>>8)&0xff) * (1 - p) + color.r * p);
				B = (int)((cc&0xff) * (1 - p) + color.r * p);
			}
			else if (p > 1)
			{
				p = 1 - (p - 1);
				
				R = (int)(((cc >> 16) & 0xff) * (1 - p) );
				G = (int)(((cc >> 8) & 0xff) * (1 - p) );
				B = (int)((cc & 0xff) * (1 - p) );
			}

			this.Data[x, y] = (R << 16) | (G << 8) | B;
		}
		public void SetSafePixel(int x, int y, int c)
		{
			if(x>=0&&x<Width&&y>=0&&y<Height)
			Data[x, y] = c;
		}
		public void SetSafePixel(int x, int y, int a,int r, int g, int b)
		{
			if (a > 255)
			{
				a = 255;

			}
			if (a < 0)
			{
				a = 0;
			}
			if (r > 255)
			{
				r = 255;

			}
			if (r < 0)
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
			if (x >= 0 && x < Width && y >= 0 && y < Width)
				Data[x, y] = (r << 24) | (r << 16) | (g << 8) | b;
		}
		public void SetSafePixel(int x, int y, int r, int g, int b)
		{
			SetSafePixel(x, y, 255, r, g, b);
		}
		public void SetPixel(int x, int y, int r, int g, int b)
		{
			SetPixel(x, y, 255, r, g, b);
		}
		public void SetPixel(int x, int y,int a, int r, int g, int b)
		{
			Data[x, y] = ((a & 0xff) << 24|(r & 0xff) << 16) | ((g & 0xff)) << 8 | (b & 0xff);
		}
		public void SetColor(int x, int y,int c)
		{
			Data[x, y] = c;
		}
		public int GetColor(int x, int y)
		{
		return	Data[x, y];
		}
		public int GetSafeA(int x, int y)
		{
			if (x >= Width)
			{
				x = Width - 1;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y >= Height)
			{
				y = Height - 1;
			}
			if (y < 0)
			{
				y = 0;
			}
			return (Data[x, y] >> 24)
				& 0xff;
		}
		public int GetSafeR(int x, int y)
		{if(x>=Width)
			{
				x = Width - 1;
			}
		if(x<0)
			{
				x = 0;
			}
			if (y >= Height)
			{
				y =Height - 1;
			}
			if (y< 0)
			{
				y = 0;
			}
			return (Data[x, y]>>16)
				&0xff;
		}
		public int GetSafeG(int x, int y)
		{
			if (x >= Width)
			{
				x = Width - 1;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y >= Height)
			{
				y = Height - 1;
			}
			if (y < 0)
			{
				y = 0;
			}
			return (Data[x,y]>>8)&0xff;
		}
		public int GetSafeB(int x, int y)
		{
			if (x >= Width)
			{
				x = Width - 1;
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y >= Height)
			{
				y = Height - 1;
			}
			if (y < 0)
			{
				y = 0;
			}
		return	(Data[x, y] ) & 0xff;
		}
		public int GetA(int x, int y)
		{
			return (Data[x, y] >> 24)
					& 0xff;
		}
		public int GetR(int x, int y) {
			return (Data[x, y] >> 16)
					& 0xff;
		}
		public int GetG(int x, int y)
		{
			return (Data[x, y] >> 8)
				& 0xff;
		}
		public int GetB(int x, int y)
		{

			return (Data[x, y])
				& 0xff;
		}
		public  Color0 GetPixel(int x, int y)
		{
			int c = Data[x, y];

			if (x < Width && y < Height)
				return new Color0((c >> 24) & 0xff,(c>>16)&0xff,( c>>8)&0xff, c&0xff);
			else
				return new Color0(255,255,255);
		}


	
	
		public void drawpoint(int x, int y, int c)
		{
			SetSafePixel(x, y, c);
		}
	

		public Vector2i getcenter()
		{
			return new Vector2i(Width / 2, Height / 2);
		}
		public void paint(int c)
		{
			for (int xi = 0; xi < this.Width; xi++)
			{
				for (int yi = 0; yi < this.Height; yi++)
				{
					drawpoint(xi, yi, c);
				}
			}
		}
		public void paint(Color0 c)
		{
			for (int xi = 0; xi < this.Width; xi++)
			{
				for (int yi = 0; yi < this.Height; yi++)
				{
					Data[xi, yi] = c;
				}
			}
		}
		public bitmap(Map<int>  data)
		{
			this.Width = data.Width;
			this.Height = data.Height;
			this.HWidth = Width / 2;
			this.HHeight = Height / 2;
			Data = data.Data;
		}

    

        public new  bitmap Clone()
		{

			return new bitmap(base.Clone());
		}

		public bitmap getCenterBox(int width,int height)
		{
			return getBox((int)Math.Round((this.Width - width) * 0.5),(int)Math.Round( (this.Height - height) * 0.5), width, height);
		}
		public new  bitmap getBox(int x, int y, int width, int height)
		{
			return new bitmap(base.getBox(x,y,width,height));
		}
		public new bitmap getPolar( int pieces,int radius)
		{
			return new bitmap(base.getPolar( pieces,radius));
		}
		public new bitmap getPolarLog(int pieces, int radius ,double realMax,double realMin=1, double startDegree = 0, double degreeRange = 360)
		{
			return new bitmap(base.getPolarLog(pieces, radius,realMax,realMin,startDegree,degreeRange));
		}
		public bitmap getPolarLog_(int pieces, int radius, double realMax, double realMin = 1,double range=360,double startAngle=0)
		{
			range /= 360;

			bitmap mapR = new bitmap(pieces, radius);
			mapR.ParallelForeach((x, y, d) => {
				d[x, y] = Data[0, 0];
			});
			double logMax = Math.Log(realMax);
			double logMin = Math.Log(realMin);
			Vector2 center = new Vector2(Width / 2.0, Height / 2.0);

			double dLog = (logMax - logMin) / (radius);
			double f = (Math.Min(Width, Height) / 2) / (realMax);
			double log;
			double theta;
			double dtheta = Math.PI * 2 *range/ pieces;
			Vector2 temp, dir;
			Vector2i v; 
			theta = startAngle*Math.PI/180.0;
			for (int j = 0; j < pieces; j++)
			{

				temp = Vector2.fromAngle(1, theta);
				log = logMin;
				for (int i = 0; i < radius; i++)

				{
					dir = center + temp * ((Math.Exp(log)) * f);

					int vv = getGoodR(dir.X, dir.Y);
						mapR[j, i] = (255,vv,vv,vv).ARGB2Int();
					
					log += dLog;
				}
				theta += dtheta;
			}
			return mapR;
		}

	}
	public class ChessBoard
	{
		public static bitmap create(int width,int height,int pixel)
		{
			int w = width + 2;
			int h = height + 2;
			bitmap bitmap = new bitmap(w* pixel, h * pixel) ;
			for (int i = 0; i <w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					bool white = (i + j+1) % 2 == 0;
					bool edge = i == 0||i==width+1||j==0||j==height+1;
					for(int x=0,x0=pixel*i;x<pixel;x++,x0++)
					{
						for(int y=0,y0=pixel*j;y<pixel;y++,y0++)
						{

							if(white||edge)
							{
								bitmap[x0, y0] = 0xffffff;
							}
							else
							{
								bitmap[x0, y0] = 0;
							}
						}
					}
				}
			}
			return bitmap;
			
		}
	}

	public static class Picture
	{
		public static Vector2i Vector2i(this Vector2 v)
		{
			return new Vector2i((int)Math.Round(v.X), (int)Math.Round(v.Y));
		}


		public class random
		{
			Random R;
			public random()
			{
				R = new Random(Picture.getint(time, 4232424, 83244242, n));
			}
			static int n0 = 1;
			static int n
			{
				get
				{
					if (n0 == 100)
					{
						n0 = 1;
					}
					return n0++;
				}
			}
			static string time { get { return DateTime.Now.TimeOfDay.ToString(); } }
			public int Int(int min, int max)
			{
				return R.Next(min, max);
			}

		}
		public class line : Line2d
		{
			public line() : base()
			{ }
			public line(Vector2i a, Vector2i b) : base(a, b)
			{
			}
			public line(vector2 a, vector2 b) : base(a, b)
			{


			}
			public line(double k0, double b0) : base(k0, b0)
			{
			}
			public void draw(bitmap bp, double r, int c)
			{
				for (int xi = 0; xi < bp.Width; xi++)
				{
					for (int yi = 0; yi < bp.Height; yi++)
					{
						if (distancetopoint(new Vector2i(xi, yi)) <= r)
						{
							Picture.drawpoint(bp, xi, yi, c);
						}
					}
				}
			}
			public void draw(bitmap bp, double r,int c, int min, int max)
			{
				if (max < min)
				{
					int temp = min;
					min = max;
					max = temp;
				}
				for (int xi = 0; xi < bp.Width; xi++)
				{
					for (int yi = 0; yi < bp.Height; yi++)
					{
						if (distancetopoint(new Vector2i(xi, yi)) <= r && xi > min && xi < max)
						{
							Picture.drawpoint(bp, xi, yi, c);

						}
					}

				}
				Picture.drawcircle(bp, min, Convert.ToInt32(y(min)), r * 1.2, c);
				Picture.drawcircle(bp, max, Convert.ToInt32(y(max)), r * 1.2, c);
			}
			public line Verticalline(vector2 p)
			{
				if (type == 0)
				{
					double k0 = -1 / k;
					double b0 = p.Y - p.X * k0;
					return new line(k0, b0);
				}
				else if (type == 1)
				{
					return new line(p, new vector2(p.X, p.Y + 1));
				}
				else
				{
					return new line(p, new vector2(p.X + 1, p.Y));
				}
			}
		}
		const double nan = Double.NaN;

		public static int process = 0;
		public static string information = "";
		public static bool isdoing = false;
		public static bool isworking = false;

		const double pi = System.Math.PI;

		public static int crange(this int n)
		{
			if (n > 255)
			{
				return 255;
			}
			else if (n < 0)
			{
				return 0;
			}
			else
			{
				return n;
			}
		}
		public static void blue(this bitmap bitmap)
		{
			for (int x = 0; x < bitmap.Width; x++)
				for (int y = 0; y < bitmap.Height; y++)
				{
					{
						int b = bitmap.GetPixel(x, y).B;
						bitmap.SetPixel(x, y,new  Color0 (0, 0, b));
					}
				}
		}
		public static Color0 channal(this Color0 color, Color0 direction)
		{

			Vector3 d0 = new Vector3(direction.R, direction.G, direction.B);
			Vector3 dmax;
			if (d0.x > d0.y && d0.x > d0.z)
			{
				dmax = d0 * 255 / d0.x;
			}
			else if (d0.y > d0.x && d0.y > d0.z)
			{
				dmax = d0 * 255 / d0.y;
			}
			else
			{
				dmax = d0 * 255 / d0.z;
			}
			Vector3 cmax = d0.dot(new Vector3(255, 255, 255)) / d0.length() / d0.length() * d0;
			Vector3 result = cmax.dot(color) / cmax.length() / cmax.length() * dmax;
			return result;
		}

		public static void bc(this bitmap bitmap, Color0 color0
			)
		{
			for (int x = 0; x < bitmap.Width; x++)
				for (int y = 0; y < bitmap.Height; y++)
				{
					bitmap.SetPixel(x, y, bitmap.GetPixel(x, y).channal(color0).Code);
				}
		}
		public static vector2 Pointtoveector(Vector2i p)
		{
			return new vector2(p.X, p.Y);
		}
		public static Vector2i createpoint(string code, int n, int xmin, int ymin, int xmax, int ymax)
		{

			return new Vector2i(Picture.getint(code, xmin, xmax, 2 * n - 1), Picture.getint(code, ymax, ymin, 2 * n));
		}
		public static vector2 m_p(vector2 p1, vector2 p2, vector2 p3)
		{
			return new vector2((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3);
		}
		public static bool isin(line l1, line l2, line l3, vector2 p)
		{
			if (l1.iscrosswith(l2) && l2.iscrosswith(l3) && l3.iscrosswith(l1))
			{
				int pos1, pos2, pos3;
				vector2 pm = m_p(l1.crosspoint(l2), l2.crosspoint(l3), l3.crosspoint(l1));
				pos1 = l1.pos(pm);
				pos2 = l2.pos(pm);
				pos3 = l3.pos(pm);
				return (pos1 == l1.pos(p)) && (pos2 == l2.pos(p)) && (pos3 == l3.pos(p));
			}
			else
			{
				return false;
			}
		}
		public static Vector2i createpoint(string code, int n, Vector2i p1, Vector2i p2)
		{
			int xmax = p1.X, xmin = p2.X, ymin = p1.Y, ymax = p2.Y;
			return new Vector2i(Picture.getint(code, xmin, xmax, 2 * n - 1), Picture.getint(code, ymax, ymin, 2 * n));
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
		public static void drawpoint(bitmap b, int x, int y, int c)
		{
			if (x < b.Width && y < b.Height && x >= 0 && y >= 0)
			{
				b.SetPixel(x, y, c);
			}
		}
		public static void drawpoint_m(bitmap b, int x, int y,int c, double p)
		{
			if (x < b.Width && y < b.Height && x >= 0 && y >= 0)
			{

				b.SetPixel_m(x, y, c, p);


			}
		}
		public static double distance(Vector2i a,Vector2i b)
		{
			double result;
			result = Sqrt(Pow(a.X - b.X, 2) + Pow(a.Y - b.Y, 2));
			return result;
		}
		public static double distance(int x1, int y1, int x2, int y2)
		{
			Vector2i a = new Vector2i(x1, y1), b = new Vector2i(x2, y2);
			double result;
			result = Sqrt(Pow(a.X - b.X, 2) + Pow(a.Y - b.Y, 2));
			return result;
		}
		public static Vector2i getcenter(bitmap b)
		{
			return new Vector2i(b.Width / 2, b.Height / 2);
		}
		public static void paint(bitmap b, int c)
		{
			for (int xi = 0; xi < b.Width; xi++)
			{
				for (int yi = 0; yi < b.Height; yi++)
				{
					drawpoint(b, xi, yi, c);
				}
			}
		}
	
		public static void paintarea_m(bitmap b, int c, vector2 p1, vector2 p2, vector2 p3, Field p)
		{
			paintarea_m(b, c, new line(p1, p2), new line(p2, p3), new line(p3, p1), p);
		}
		public static void paintarea(bitmap b, int c, vector2[] ps)
		{
			if (ps.Length > 2)
			{
				for (int i = 0; i < ps.Length - 2; i++)
				{
					paintarea(b, c, ps[0], ps[i + 1], ps[i + 2]);
				}
			}
		}
		public static void paintarea_m(bitmap b, int c, vector2[] ps, Field p)
		{
			if (ps.Length > 2)
			{
				for (int i = 0; i < ps.Length - 2; i++)
				{
					paintarea_m(b, c, ps[0], ps[i + 1], ps[i + 2], p);
				}
			}
		}

		public static void paintarea(bitmap B, int c, int c2, vector2[] ps)
		{
			var color = c.Int2RGB();
			var color2 = c2.Int2RGB();
			double r = color.r, g = color.g, b = color.b, dr = (color2.r - r) / (double)ps.Length, dg = (color2.g - g) / (double)ps.Length, db = (color2.b - b) / (double)ps.Length;
			{
				for (int i = 0; i < ps.Length - 2; i++)
				{
					r += dr;
					b += db;
					g += dg;
					paintarea(B, ( (int)r, (int)g, (int)b).RGB2Int(), ps[0], ps[i + 1], ps[i + 2]);
				}
			}
		}
		public static void paintarea1(bitmap b, int c, Vector2[] ps)
		{
			if (ps.Length > 2)
			{
				double x = 0, y = 0;
				for (int i = 0; i < ps.Length; i++)
				{
					x += ps[i].X;
					y += ps[i].Y;
				}
				x /= ps.Length;
				y /= ps.Length;
				Vector2 mp = new Vector2(x, y);
				for (int i = 0; i < ps.Length - 1; i++)
				{
					b.drawTriangle( c, mp, ps[i], ps[i + 1]);
				}
				b.drawTriangle(c, mp, ps[0], ps[ps.Length - 1]);
			}
		}
		
	
		public static void paintareaWITHedge(bitmap b, int c, int ec, vector2[] ps)
		{

			Picture.drawlines(b, ec, ps, 0.8, true);

			if (ps.Length > 2)
			{
				double x = 0, y = 0;
				for (int i = 0; i < ps.Length; i++)
				{
					x += ps[i].X;
					y += ps[i].Y;
				}
				x /= ps.Length;
				y /= ps.Length;
				vector2 mp = new vector2(x, y);
				for (int i = 0; i < ps.Length - 1; i++)
				{
					b.drawTriangle(c, mp, ps[i], ps[i + 1]);
				}
				b.drawTriangle(c, mp, ps[0], ps[ps.Length - 1]);
			}
		}
		public static void paintarea(bitmap b, int c,Vector2 v1,Vector2 v2,Vector2 v3)
		{
			
			up up1 = getup(new Vector2[] {v1,v2,v3});
			Triangle2D triangle=new Triangle2D(v1,v2,v3);
			for (int xi = max(0, (int)up1.minx); xi < min(b.Width, (int)up1.maxx); xi++)
			{
				for (int yi = max(0, (int)up1.miny); yi < min(b.Height, (int)up1.maxy); yi++)
				{
					
					if (triangle.IsIn(new vector2(xi, yi)) )
					{
						b[xi, yi] = c;
					}
				}
			}
			drawline(b,v1,v2, 0.8, c);
			drawline(b, v2, v3, 0.8, c);
			drawline(b, v1, v3, 0.8, c);
		}
		public static void paintarea_m(bitmap b, int c, line l1, line l2, line l3, Field p)
		{
			vector2[] ps = new vector2[3];
			ps[0] = l1.crosspoint(l2);
			ps[1] = l2.crosspoint(l3);
			ps[2] = l3.crosspoint(l1);
			up up1 = getup(ps);
			int xi = 0, yi = 0;
			for (xi = max(0, (int)up1.minx); xi < min(b.Width, (int)up1.maxx); xi++)
			{
				for (yi = max(0, (int)up1.miny); yi < min(b.Height, (int)up1.maxy); yi++)
				{
					if (isin(l1, l2, l3, new vector2(xi, yi)))
					{
						drawpoint_m(b, xi, yi, c, p(xi, yi));
					}
				}
			}
			drawline_m(b, ps[0].Vector2i(), ps[1].Vector2i(), 0.8, c, p(xi, yi));
			drawline_m(b, ps[2].Vector2i(), ps[1].Vector2i(), 0.8, c, p(xi, yi));
			drawline_m(b, ps[0].Vector2i(), ps[2].Vector2i(), 0.8, c, p(xi, yi));
		}
		public static up getup(vector2[] ps)
		{
			double maxx = ps[0].X, maxy = ps[0].Y, minx = ps[0].X, miny = ps[0].Y;
			for (int i = 0; i < ps.Length; i++)
			{
				if (ps[i].X > maxx)
				{
					maxx = ps[i].X;
				}
				if (ps[i].Y > maxy)
				{
					maxy = ps[i].Y;
				}
				if (ps[i].X < minx)
				{
					minx = ps[i].X;
				}
				if (ps[i].Y < miny)
				{
					miny = ps[i].Y;
				}
			}

			return new up(maxx, maxy, minx, miny);
		}
		public static int max(int a, int b)
		{
			return Math.Max(a, b);
		}
		public static int min(int a, int b)
		{
			return Math.Min(a, b);
		}
		public static void drawcircle(bitmap b, vector2 p, double r, int c)
		{
			if (((vector2)p).distance(b.getcenter()) <= r + Math.Sqrt(b.Width * b.Width + b.Height * b.Height))

			{
				int x = (int)p.X;
				int y = (int)p.Y;
				int sx = max(0, Convert.ToInt32(p.X - r - 1)), sy = max(0, Convert.ToInt32(p.Y - r - 1)), ex = min(b.Width, Convert.ToInt32(p.X + r + 1)), ey = min(b.Height, Convert.ToInt32(p.Y + r + 1));
				Vector2i pp = new Vector2i(x, y);

				for (int xi = sx; xi < ex; xi++)
				{
					for (int yi = sy; yi < ey; yi++)
					{
						if (distance(pp, new Vector2i(xi, yi)) <= r)
						{
							b[xi,yi]=c;
						}
					}
				}
			}

		}
		public static void drawcircle_m(bitmap b, Vector2i p, double r, int c, double ppp)
		{
			if (((vector2)p).distance(b.getcenter()) <= r + Math.Sqrt(b.Width * b.Width + b.Height * b.Height))

			{
				int x = p.X;
				int y = p.Y;
				int sx = max(0, Convert.ToInt32(p.X - r - 1)), sy = max(0, Convert.ToInt32(p.Y - r - 1)), ex = min(b.Width, Convert.ToInt32(p.X + r + 1)), ey = min(b.Height, Convert.ToInt32(p.Y + r + 1));
				Vector2i pp = new Vector2i(x, y);

				for (int xi = sx; xi < ex; xi++)
				{
					for (int yi = sy; yi < ey; yi++)
					{
						if (distance(pp, new Vector2i(xi, yi)) <= r)
						{
							b.SetPixel_m(xi, yi, c, ppp);
						}
					}
				}
			}

		}
		public static void drawcirclet(bitmap b, int x, int y, double R, double r, int c)
		{
			double C = Math.Sqrt(Math.Abs(R * R - r * r));
			vector2 c1, c2;
			if (R >= r)
			{
				c1 = new vector2(x + C, y);
				c2 = new vector2(x - C, y);
			}
			else
			{
				c1 = new vector2(x, y + C);
				c2 = new vector2(x, y - C);
			}
			double temp = Math.Max(R, r);
			r = Math.Min(R, r);
			R = temp;
			int sx = max(0, Convert.ToInt32(x - R - 1)), sy = max(0, Convert.ToInt32(y - R - 1)), ex = min(b.Width, Convert.ToInt32(x + R + 1)), ey = min(b.Height, Convert.ToInt32(y + R + 1));


			for (int xi = sx; xi < ex; xi++)
			{
				for (int yi = sy; yi < ey; yi++)
				{
					if (c1.distance(new vector2(xi, yi)) + c2.distance(new vector2(xi, yi)) <= 2 * R)
					{
						b.SetPixel(xi, yi, c);
					}
				}
			}
		}
		public static void drawe_circle(bitmap b, Vector2i p, double r, int c)
		{
			if (((vector2)p).distance(b.getcenter()) <= r + Math.Sqrt(b.Width * b.Width + b.Height * b.Height))
			{
				double C = Math.PI * r * 2;
				double da = 1 / r;
				int x = p.X, y = p.Y;
				if (r * Math.PI > Math.Max(b.Width, b.Height))
				{
					vector2 e1 = new vector2(0, 0), e2 = new vector2(0, b.Height), e3 = new vector2(b.Width, 0), e4 = new vector2(b.Width, b.Height), p5 = new vector2(10, 0);
					double max1, max2, min1, min2, min, max;
					vector2 p1, p2;
					p1 = p - new vector2(r, r);
					p2 = p + new vector2(r, r);
					bool b1 = p2.X > b.Width || p2.Y > b.Height || p1.X < 0 || p1.Y < 0;
					if (b1)
					{
						e1 = e1 - p;
						e2 = e2 - p;
						e3 = e3 - p;
						e4 = e4 - p;
						max1 = Math.Max(Math.Acos(p5.cos(e1)), Math.Acos(p5.cos(e2)));
						max2 = Math.Max(Math.Acos(p5.cos(e3)), Math.Acos(p5.cos(e4)));
						max = Math.Max(max1, max2);
						min1 = Math.Min(Math.Acos(p5.cos(e1)), Math.Acos(p5.cos(e2)));
						min2 = Math.Min(Math.Acos(p5.cos(e3)), Math.Acos(p5.cos(e4)));
						min = Math.Min(min1, min2);
					}
					min = 0;
					max = Math.PI;
					for (double i = Math.Max(min, 0); i <= Math.Min(max, Math.PI); i += da)
					{
						b.drawpoint((int)(x + Math.Cos(i) * r), (int)(y + Math.Sin(i) * r), c);
					}

					for (double i = -Math.Max(min, 0); i >= -Math.Min(max, Math.PI); i -= da)
					{
						b.drawpoint((int)(x + Math.Cos(i) * r), (int)(y + Math.Sin(i) * r), c);
					}

				}
				else
				{
					for (double i = 0; i <= Math.PI * 2; i += da)
					{
						b.drawpoint((int)(x + Math.Cos(i) * r), (int)(y + Math.Sin(i) * r), c);
					}


				}
			}

		}
		public static void drawcircle(bitmap b, int x, int y, double r, int c)
		{


			int sx = max(0, Convert.ToInt32(x - r - 1)), sy = max(0, Convert.ToInt32(y - r - 1)), ex = min(b.Width, Convert.ToInt32(x + r + 1)), ey = min(b.Height, Convert.ToInt32(y + r + 1));
			for (int xi = sx; xi < ex; xi++)
			{
				for (int yi = sy; yi < ey; yi++)
				{
					if (distance(new Vector2i(x, y), new Vector2i(xi, yi)) <= r)
					{
						b.SetPixel(xi, yi, c);
					}
				}
			}
		}
		public static void drawring(bitmap b, int x, int y, double r1, double r2, int c)
		{
			if (r1 < r2)
			{
				double temp = r1;
				r1 = r2;
				r2 = temp;

			}
			int sx = max(0, Convert.ToInt32(x - r1 - 1)), sy = max(0, Convert.ToInt32(y - r1 - 1)), ex = min(b.Width, Convert.ToInt32(x + r1 + 1)), ey = min(b.Height, Convert.ToInt32(y + r1 + 1));


			for (int xi = sx; xi < ex; xi++)
			{
				for (int yi = sy; yi < ey; yi++)
				{
					if (distance(new Vector2i(x, y), new Vector2i(xi, yi)) <= r1 && distance(new Vector2i(x, y), new Vector2i(xi, yi)) >= r2)
					{
						drawpoint(b, xi, yi, c);
					}
				}
			}

		}
		public static void drawring(bitmap b, Vector2i p, double r1, double r2, int c)
		{
			int x = p.X;
			int y = p.Y;
			if (r1 < r2)
			{
				double temp = r1;
				r1 = r2;
				r2 = temp;

			}
			int sx = max(0, Convert.ToInt32(x - r1 - 1)), sy = max(0, Convert.ToInt32(y - r1 - 1)), ex = min(b.Width, Convert.ToInt32(x + r1 + 1)), ey = min(b.Height, Convert.ToInt32(y + r1 + 1));


			for (int xi = sx; xi < ex; xi++)
			{
				for (int yi = sy; yi < ey; yi++)
				{
					if (distance(new Vector2i(x, y), new Vector2i(xi, yi)) <= r1 && distance(new Vector2i(x, y), new Vector2i(xi, yi)) >= r2)
					{
						drawpoint(b, xi, yi, c);
					}
				}
			}

		}
		public static Vector2i drawline(bitmap p, int x, int y, double angle, double lenth,int c)
		{
			Vector2i pos = new Vector2i(x, y);
			double n1, n2, b1, b2;
			double an = angle / 180 * pi;
			n1 = abs(Cos(an) * lenth);
			n2 = abs(Sin(an) * lenth);
			b1 = Cos(an) >= 0 ? 1 : -1;
			b2 = Sin(an) >= 0 ? 1 : -1;
			if (abs(Cos(an)) > abs(Sin(an)))
			{
				for (int i = 0; i < n1; i++)
				{
					drawpoint(p, Convert.ToInt32(x + i * b1), Convert.ToInt32(y + Tan(an) * i * b1), c);
					pos = new Vector2i(Convert.ToInt32(x + i * b1), Convert.ToInt32(y + Tan(an) * i * b1));

				}
			}
			else
			{
				for (double i = 0; i < n2; i++)
				{

					drawpoint(p, Convert.ToInt32(x + i / Tan(an) * b2), Convert.ToInt32(y + i * b2), c);
					pos = new Vector2i(Convert.ToInt32(x + i / Tan(an) * b2), Convert.ToInt32(y + i * b2));
				}
			}
			return pos;
		}
		
		public static void drawlines(bitmap p, int c, vector2[] ps, double r1, bool connect)
		{

			for (int i = 0; i < ps.Length - 1; i++)
			{

				p.drawline( ps[i], ps[i + 1], (int)r1, c);
			}
			if (connect)
			{
				p.drawline(ps[0], ps[ps.Length - 1], (int)r1, c);
			}

		}
		public static Vector2i drawline(bitmap p, Vector2i p0, double angle, double lenth, double r, int c)
		{
			double an = angle * pi / 180;
			line l = new line(Tan(an), p0.Y - p0.X * Tan(an));
			l.draw(p, r, c, p0.X, Convert.ToInt32(p0.X + lenth * Cos(an)));
			return new Vector2i(Convert.ToInt32(p0.X + lenth * Cos(an)), Convert.ToInt32(l.y(Convert.ToInt32(p0.X + lenth * Cos(an)))));
		}
		public static void drawline(bitmap p, vector2 p0, vector2 p1, double r, int c)
		{
			line l = new line(p0, p1), vl1 = l.Verticalline(p0), vl2 = l.Verticalline(p1);
			double maxb = Math.Max(vl1.b, vl2.b), minb = Math.Min(vl1.b, vl2.b), maxc = Math.Max(vl1.c, vl2.c), minc = Math.Min(vl1.c, vl2.c);
			int n1 = max((int)Math.Min((p0.X - r - 1), (p1.X - r - 1)), 0);
			int n2 = min((int)Math.Max((p0.X + r + 1), (p1.X + r + 1)), p.Width);
			int n3 = max((int)Math.Min((p0.Y - r - 1), (p1.Y - r - 1)), 0);
			int n4 = min((int)Math.Max((p0.Y + r + 1), (p1.Y + r + 1)), p.Height);
			for (int xi = n1; xi < n2; xi++)
			{
				for (int yi = n3; yi < n4; yi++)
				{
					if (l.distancetopoint(new vector2(xi, yi)) <= r)
					{
						line vl = l.Verticalline(Pointtoveector(new Vector2i(xi, yi)));
						if (vl.type == -1 && vl.c < maxc && vl.c > minc)
						{
							drawpoint(p, xi, yi, c);
						}
						else if (vl.type != -1 && vl.b > minb && vl.b < maxb)
						{
							drawpoint(p, xi, yi, c);
						}
					}
				}
			}
		}
		public static void drawline_m(bitmap p, Vector2i p0, Vector2i p1, double r, int c, double p2)
		{
			line l = new line(p0, p1), vl1 = l.Verticalline(Pointtoveector(p0)), vl2 = l.Verticalline(Pointtoveector(p1));
			double maxb = Math.Max(vl1.b, vl2.b), minb = Math.Min(vl1.b, vl2.b), maxc = Math.Max(vl1.c, vl2.c), minc = Math.Min(vl1.c, vl2.c);
			int n1 = max((int)Math.Min((p0.X - r - 1), (p1.X - r - 1)), 0);
			int n2 = min((int)Math.Max((p0.X + r + 1), (p1.X + r + 1)), p.Width);
			int n3 = max((int)Math.Min((p0.Y - r - 1), (p1.Y - r - 1)), 0);
			int n4 = min((int)Math.Max((p0.Y + r + 1), (p1.Y + r + 1)), p.Height);
			for (int xi = n1; xi < n2; xi++)
			{
				for (int yi = n3; yi < n4; yi++)
				{
					if (l.distancetopoint(new Vector2i(xi, yi)) <= r)
					{
						line vl = l.Verticalline(Pointtoveector(new Vector2i(xi, yi)));
						if (vl.type == -1 && vl.c < maxc && vl.c > minc)
						{
							drawpoint_m(p, xi, yi, c, p2);
						}
						else if (vl.type != -1 && vl.b > minb && vl.b < maxb)
						{
							drawpoint_m(p, xi, yi, c, p2);
						}
					}
				}
			}
		}

		public static void drawdotline(bitmap p, Vector2i p0, Vector2i p1, double r, int c, double P)
		{
			line l = new line(p0, p1), vl1 = l.Verticalline(Pointtoveector(p0)), vl2 = l.Verticalline(Pointtoveector(p1));
			double maxb = Math.Max(vl1.b, vl2.b), minb = Math.Min(vl1.b, vl2.b), maxc = Math.Max(vl1.c, vl2.c), minc = Math.Min(vl1.c, vl2.c);
			int n1 = max((int)Math.Min((p0.X - r - 1), (p1.X - r - 1)), 0);
			int n2 = min((int)Math.Max((p0.X + r + 1), (p1.X + r + 1)), p.Width);
			int n3 = max((int)Math.Min((p0.Y - r - 1), (p1.Y - r - 1)), 0);
			int n4 = min((int)Math.Max((p0.Y + r + 1), (p1.Y + r + 1)), p.Height);
			for (int xi = n1; xi < n2; xi++)
			{
				for (int yi = n3; yi < n4; yi++)
				{
					if (l.distancetopoint(new Vector2i(xi, yi)) <= r)
					{
						bool flag = random1.NextDouble() < P;
						if (flag)
						{
							line vl = l.Verticalline(Pointtoveector(new Vector2i(xi, yi)));
							if (vl.type == -1 && vl.c < maxc && vl.c > minc)
							{

								drawpoint(p, xi, yi, c);
							}
							else if (vl.type != -1 && vl.b > minb && vl.b < maxb)
							{
								drawpoint(p, xi, yi, c);
							}
						}
					}
				}
			}
		}
		public static void drawdotline_m(bitmap p, Vector2i p0, Vector2i p1, double r, int c, double P, double p2)
		{
			line l = new line(p0, p1), vl1 = l.Verticalline(Pointtoveector(p0)), vl2 = l.Verticalline(Pointtoveector(p1));
			double maxb = Math.Max(vl1.b, vl2.b), minb = Math.Min(vl1.b, vl2.b), maxc = Math.Max(vl1.c, vl2.c), minc = Math.Min(vl1.c, vl2.c);
			int n1 = max((int)Math.Min((p0.X - r - 1), (p1.X - r - 1)), 0);
			int n2 = min((int)Math.Max((p0.X + r + 1), (p1.X + r + 1)), p.Width);
			int n3 = max((int)Math.Min((p0.Y - r - 1), (p1.Y - r - 1)), 0);
			int n4 = min((int)Math.Max((p0.Y + r + 1), (p1.Y + r + 1)), p.Height);
			for (int xi = n1; xi < n2; xi++)
			{
				for (int yi = n3; yi < n4; yi++)
				{
					if (l.distancetopoint(new Vector2i(xi, yi)) <= r)
					{
						bool flag = random1.NextDouble() < P;
						if (flag)
						{
							line vl = l.Verticalline(Pointtoveector(new Vector2i(xi, yi)));
							if (vl.type == -1 && vl.c < maxc && vl.c > minc)
							{

								drawpoint_m(p, xi, yi, c, p2);
							}
							else if (vl.type != -1 && vl.b > minb && vl.b < maxb)
							{
								drawpoint_m(p, xi, yi, c, p2);
							}
						}
					}
				}
			}
		}
		public static Random random1 = new Random();
		public static Vector2i drawline(bitmap p, int x, int y, double angle, double lenth, double r, int c)
		{
			Vector2i p0 = new Vector2i(x, y);
			double an = angle * pi / 180;
			line l = new line(Tan(an), p0.Y - p0.X * Tan(an));
			l.draw(p, r, c, p0.X, Convert.ToInt32(p0.X + lenth * Cos(an)));
			return new Vector2i(Convert.ToInt32(p0.X + lenth * Cos(an)), Convert.ToInt32(l.y(Convert.ToInt32(p0.X + lenth * Cos(an)))));
		}
		public static void drawrectangle(bitmap p, int x, int y, int width, int height, int c)
		{
			int dx = x, dy = y;
			for (int xi = 0; xi < width; xi++)
			{
				for (int yi = 0; yi < height; yi++)
				{
					if (xi + x < p.Width && yi + y < p.Height)
					{
						drawpoint(p, xi + x, yi + y, c);
					}
				}
			}
		}
		public static void drawrectangle(bitmap p, Vector2i pos, int width, int height, int c)
		{
			int x = pos.X, y = pos.Y;
			int dx = x, dy = y;
			for (int xi = 0; xi < width; xi++)
			{
				for (int yi = 0; yi < height; yi++)
				{
					if (xi + x < p.Width && yi + y < p.Height)
					{
						drawpoint(p, xi + x, yi + y, c);
					}
				}
			}
		}
		public static void drawrectanglering(bitmap p, int x, int y, int width, int height, int width2, int height2, int c)
		{
			int dx = x, dy = y;
			for (int xi = 0; xi < width; xi++)
			{
				for (int yi = 0; yi < height; yi++)
				{
					if (xi + x < p.Width && yi + y < p.Height)
					{
						if ((xi <= (width - width2) / 2 || xi >= (width - width2) / 2 + width2) || (yi <= (height - height2) / 2 || yi >= (height - height2) / 2 + height2))
						{
							drawpoint(p, xi + x, yi + y, c);
						}
					}
				}
			}
		}
		public static void drawrectanglering(bitmap p, Vector2i pos, int width, int height, int width2, int height2, int c)
		{
			int x = pos.X, y = pos.Y;
			int dx = x, dy = y;
			for (int xi = 0; xi < width; xi++)
			{
				for (int yi = 0; yi < height; yi++)
				{
					if (xi + x < p.Width && yi + y < p.Height)
					{
						if ((xi <= (width - width2) / 2 || xi >= (width - width2) / 2 + width2) || (yi <= (height - height2) / 2 || yi >= (height - height2) / 2 + height2))
						{
							drawpoint(p, xi + x, yi + y, c);
						}
					}
				}
			}
		}
		public static void drawaonb(bitmap pa, bitmap pb, int x, int y, int key)
		{
			bitmap temp = new bitmap(1, 1);
			temp.SetPixel(0, 0, key);
			key = temp.GetPixel(0, 0);
			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (pa.GetPixel(ix, iy) != key)
					{
						drawpoint(pb, x + ix, y + iy, pa.GetPixel(ix, iy));
					}
				}
			}
		}
		public static void mixaandb(bitmap pa, bitmap pb, int x, int y, int key)
		{
			
			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (ix + x >= 0 && iy + y >= 0 && ix + x < pb.Width && iy + y < pb.Height && pa.GetColor(ix, iy) != key)
					{

						int r = (pa.GetPixel(ix, iy).R + pb.GetPixel(ix + x, iy + y).R) / 2;
						int g = (pa.GetPixel(ix, iy).G + pb.GetPixel(ix + x, iy + y).G) / 2;
						int b = (pa.GetPixel(ix, iy).B + pb.GetPixel(ix + x, iy + y).B) / 2;
						drawpoint(pb, x + ix, y + iy,(r,g,b).RGB2Int());
					}
				}
			}
		}
		public static void mixaandb(bitmap pa, bitmap pb, int x, int y)
		{

			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (ix + x >= 0 && iy + y >= 0 && ix + x < pb.Width && iy + y < pb.Height)
					{

						int r = (pa.GetPixel(ix, iy).R + pb.GetPixel(ix + x, iy + y).R) / 2;
						int g = (pa.GetPixel(ix, iy).G + pb.GetPixel(ix + x, iy + y).G) / 2;
						int b = (pa.GetPixel(ix, iy).B + pb.GetPixel(ix + x, iy + y).B) / 2;
						drawpoint(pb, x + ix, y + iy, (r, g, b).RGB2Int());
					}
				}
			}
		}
		public static void colorreplace(bitmap pa, int key, int c, line l, double crr, double crg, double crb)
		{
			var color = c.Int2RGB();
			double  r = color.r, g = color.g, b = color.b, ri = crr, gi = crg, bi = crb;

			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (pa.GetColor(ix, iy) == key)
					{
						r = color.r + l.distancetopoint(new Vector2i(ix, iy)) * crr;
						g = color.g + l.distancetopoint(new Vector2i(ix, iy)) * crg;
						b = color.b + l.distancetopoint(new Vector2i(ix, iy)) * crb;
						while (r >= 510)
						{
							r -= 510;
						}
						while (r < 0)
						{
							r += 510;
						}
						if (r >= 255)
						{
							r = 510 - r;
						}
						while (g >= 510)
						{
							g -= 510;
						}
						while (g < 0)
						{
							g += 510;
						}
						if (g >= 255)
						{
							g = 510 - g;
						}
						while (b >= 510)
						{
							b -= 510;
						}
						while (b < 0)
						{
							b += 510;
						}
						if (b >= 255)
						{
							b = 510 - b;
						}

						int c2 = ( Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)).RGB2Int();
						drawpoint(pa, ix, iy, c2);
					}
				}
			}

		}
		public static void colorkey(bitmap pa, int key, int c, int size, double crr, double crg, double crb)
		{
			var color = c.Int2RGB();
			double r = color.r, g = color.g, b = color.b, ri = crr, gi = crg, bi = crb;
	
			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (pa.GetColor(ix, iy) == key)
					{

						int dx = ix, dy = iy;

						while (dx >= size)
						{
							dx -= size;
						}
						while (dy >= size)
						{
							dy -= size;
						}
						r = color.r + distance(new Vector2i(size / 2, size / 2), new Vector2i(dx, dy)) * crr;
						g = color.g + distance(new Vector2i(dx, dy), new Vector2i(size / 2, size / 2)) * crg;
						b = color.b + distance(new Vector2i(dx, dy), new Vector2i(size / 2, size / 2)) * crb;
						while (r >= 510)
						{
							r -= 510;
						}
						while (r < 0)
						{
							r += 510;
						}
						if (r >= 255)
						{
							r = 510 - r;
						}
						while (g >= 510)
						{
							g -= 510;
						}
						while (g < 0)
						{
							g += 510;
						}
						if (g >= 255)
						{
							g = 510 - g;
						}
						while (b >= 510)
						{
							b -= 510;
						}
						while (b < 0)
						{
							b += 510;
						}
						if (b >= 255)
						{
							b = 510 - b;
						}
						int c2 =( Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)).RGB2Int();
						drawpoint(pa, ix, iy, c2);
					}
				}
			}

		}
		public static void colorreplace(bitmap pa, int key, int c, int cpx, int cpy, double crr, double crg, double crb)
		{
			var color = c.Int2RGB();
			double r = color.r, g = color.g, b = color.b, ri = crr, gi = crg, bi = crb;
	
			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (pa.GetColor(ix, iy) == key)
					{


						r = color.r + distance(new Vector2i(cpx, cpy), new Vector2i(ix, iy)) * crr;
						g = color.g + distance(new Vector2i(ix, iy), new Vector2i(cpx, cpy)) * crg;
						b = color.b + distance(new Vector2i(ix, iy), new Vector2i(cpx, cpy)) * crb;
						while (r >= 510)
						{
							r -= 510;
						}
						while (r < 0)
						{
							r += 510;
						}
						if (r >= 255)
						{
							r = 510 - r;
						}
						while (g >= 510)
						{
							g -= 510;
						}
						while (g < 0)
						{
							g += 510;
						}
						if (g >= 255)
						{
							g = 510 - g;
						}
						while (b >= 510)
						{
							b -= 510;
						}
						while (b < 0)
						{
							b += 510;
						}
						if (b >= 255)
						{
							b = 510 - b;
						}

						int c2 =( Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)).RGB2Int();
						drawpoint(pa, ix, iy, c2);
					}
				}
			}

		}
		public static void colorreplace(bitmap pa,int key, int c, Vector2i pos, double crr, double crg, double crb)
		{
			int cpx = pos.X, cpy = pos.Y;
			var color = c.Int2RGB();
			double r = color.r, g = color.g, b = color.b, ri = crr, gi = crg, bi = crb;

			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{
					if (pa.GetColor(ix, iy) == key)
					{


						r = color.r + distance(new Vector2i(cpx, cpy), new Vector2i(ix, iy)) * crr;
						g = color.g + distance(new Vector2i(ix, iy), new Vector2i(cpx, cpy)) * crg;
						b =color.b + distance(new Vector2i(ix, iy), new Vector2i(cpx, cpy)) * crb;
						while (r >= 510)
						{
							r -= 510;
						}
						while (r < 0)
						{
							r += 510;
						}
						if (r >= 255)
						{
							r = 510 - r;
						}
						while (g >= 510)
						{
							g -= 510;
						}
						while (g < 0)
						{
							g += 510;
						}
						if (g >= 255)
						{
							g = 510 - g;
						}
						while (b >= 510)
						{
							b -= 510;
						}
						while (b < 0)
						{
							b += 510;
						}
						if (b >= 255)
						{
							b = 510 - b;
						}

						int c2 =( Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)).RGB2Int();
						drawpoint(pa, ix, iy, c2);
					}
				}
			}

		}
		public static void colorreplace(bitmap pa, int key, int c)
		{
			var color = c.Int2RGB();
			double r = color.r, g = color.g, b = color.b;



			for (int ix = 0; ix < pa.Width; ix++)
			{
				for (int iy = 0; iy < pa.Height; iy++)
				{

					if (pa.GetColor(ix, iy) == key)
					{
						drawpoint(pa, ix, iy, c);
					}

				}
			}
		}

	
	
		public static double todouble(int i)
		{
			return Convert.ToDouble(i);
		}
	
	
		public static string getnumber2(string code, int length)
		{
			string s = "", s1 = "";
			string[] sl = new string[length / 9 + 1];
			double d = 3.347;
			for (int i = 0; i < length / 9 + 1; i++)
			{
				sl[i] = getnumber1(code, 10, d * i + 1);
			}

			double t = 0.1;
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}

			while (s.Length < length)
			{

				s += Convert.ToUInt64(s[0]) * t * Sin(t);
				t += 1.1 * t;
				s = s.Replace('.', '0');
			}
			for (int i = 0; i < length / 9; i++)
			{
				s1 += sl[i];
			}
			s = s + s1;
			while (s.Length > length)
			{

				s = s.Substring(0, s.Length - 1);
			}
			s = s.Replace('-', '9');
			return s;
		}
		static string getnumber1(string code, int length, double t1)
		{
			string s = "";

			for (int i = code.Length - 1; i >= 0; i--)
			{
				s += (int)code[i];
			}
			while (s.Length < length)
			{

				s += Convert.ToDouble(s) * 1.2 * t1;
				s = s.Replace('.', '0');
			}
			while (s.Length > length)
			{

				s = s.Substring(0, s.Length - 1);
			}

			return s;
		}
		public static string getnumber(string code, int length)
		{
			code = gets_code(code) + code.Length + gets_code2(code) + getansi(code);
			while (code.Length < length)
			{
				code = code + getansi(code);
			}
			while (code.Length > length)
			{
				code = code.Substring(0, code.Length - 1);
			}
			return code;
		}
		public static string getansi(string code)
		{
			string s = "";
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return s;
		}
		public static string gets_code(string code)
		{
			int s = 0;
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return (s % 100).ToString();
		}
		public static string gets_code2(string code)
		{
			int s = 0;
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return (s % 5).ToString();
		}

		public static Color createcolor(string code, int n)
		{
			int a, r, g, b;
			if (getint(code, 0, 10, n) > 8)
			{
				r = getint(code, 0, 254, 4 * n);
				g = getint(code, 0, 254, 4 * n - 1);
				b = getint(code, 0, 254, 4 * n - 2);
				a = getint(code, 0, 254, 4 * n - 3);
			}
			else
			{
				r = getint(code, 0, 254, 4 * n);
				g = r;
				b = r;
				a = getint(code, 0, 254, 4 * n - 3);
			}
			return Color.FromRgba((byte)r, (byte)g, (byte)b, (byte)a);
		}

		public static int[] createint(string code, int min, int max, int number)
		{
			int lenth;
			int[] result = new int[number];
			if (max < min)
			{
				int temp = max;
				max = min;
				min = temp;
			}
			min--;
			if (max > 0)
			{
				lenth = max.ToString().Length + 1;
			}
			else
			{
				lenth = 1;
			}
			for (int i = 0; i < number; i++)
			{
				result[i] = toint(getnumber(code, lenth * number).Substring(i, lenth).Replace('0', '1'));
				result[i] = min + (result[i] - min) % (max - min);
			}
			return result;
		}
		public static int getint(string code, int min, int max, int number)
		{
			int lenth;
			int result;
			if (max < min)
			{
				int temp = max;
				max = min;
				min = temp;
			}
			max++;
			if (max > 0)
			{
				lenth = max.ToString().Length + 1;
			}
			else
			{
				lenth = 2;
			}
			result = toint(getnumber(code, lenth * number).Substring(number - 1, lenth).Replace('0', '1'));
			result = min + (result - min) % (max - min);

			return result;
		}
		public static double getdouble(string code, int number)
		{
			int a = getint(code, 0, 1000, number);
			return a / 1000.0;
		}
		public static int toint(string code)
		{
			return Convert.ToInt32(Convert.ToDouble(code));
		}
		public static line createline(string code, int n, Vector2i min, Vector2i max)
		{
			return new line(createpoint(code, n + 1, min, max), createpoint(code, n + 2, min, max));
		}

		
	
	}
}

