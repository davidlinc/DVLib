using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using MathBase;
using Images;
namespace DVOSLib
{
	public class AreaMap
	{
		int width;
		int height;
		
		public Vector2i[] offsets = new Vector2i[] {new Vector2i(-1,0),new Vector2i(-1,-1),new Vector2i(0,-1),new Vector2i(1,-1 )};
		Area[,] map;
		List<Area> areas=new List<Area>();

		public Area[] getAreas { get { return areas.ToArray(); } }
		public AreaMap(int w,int h)
		{
			map = new Area[w, h];
			width = w;
			height = h;
		}
		public static Random Random = new Random();
		public bitmap getImage()
		{
			bitmap b = new bitmap(width, height);
			int[,] data = b.getData();
			foreach (Area area in areas)
			{
				int c=Random.Next(1, 0x00ffffff);
				foreach(Vector2i v in area.positions)
				{
					data[v.X, v.Y] = c;
				}
			}
			return b;
		}

		public Area getArea(int x, int y)
		{
			if(x<0||x>=width||y<0||y>=height)
			{
				return null;
			}
			return map[x,y];
		}
		
		public void add_XY(int x,int y)
		{
			int n = 0;
			Area temp;
			List<Area> notnull = new List<Area>(4);
			foreach(Vector2i v in offsets)
			{
				int px = x + v.X;
				int py = y + v.Y;
				temp = getArea(px, py);
				if(temp!=null&&!notnull.Contains(temp))
				{
					notnull.Add(temp);
					n++;
				}
			}
;
			if (n==0)
			{
				Area a = new Area();
				a.add(x, y);
				areas.Add(a);
				map[x, y] = a;
			}
			else if(n==1)
			{
				temp = notnull[0];
				map[x, y] = temp;
				temp.directlyAdd(x, y);
			}
			else
			{
				Area a1 = notnull[0];
				Area a2 = notnull[1];
				Area a3 = a1.QuikMerge(a2);
				areas.Remove(a1);
				areas.Remove(a2);
				areas.Add(a3);
				foreach(Vector2i pos in a3.positions)
				{
					map[pos.X, pos.Y] = a3;

				}

			}
		}
	}
	public class AreaSet:List<Area>
	{
		public AreaSet()
		{

		}
		public AreaSet(int capacity) :base(capacity)
		{

		}
		public void add(Area area)
		{
			foreach(Area a in  this)
			{
				if(a==area)
				{
					return;
				}
			}
			base.Add(area);
		}
	}
	public class Area
	{
		int Count;
		public Vector2[] PosList_X { get; private set; }
		public Vector2[] PosList_Y { get; private set; }
		public Vector2[] DirectionList_X { get; private set; }
		public Vector2[] DirectionList_Y { get; private set; }
		public Vector2? Direction_X { get; private set; }
		public Vector2 Direction_Y { get; private set; }
		public Vector2i[] points { get { return positions.ToArray(); } }

	internal	List<Vector2i> positions;
		bool[,] map;
	    public int minx { get; private set; }
		public int miny { get; private set; }
		public int maxx { get; private set; }
		public int maxy { get; private set; }
		public double cos{ get; private set; }
		public int width { get
			{
				return maxx - minx + 1;
			} }
		public int height
		{
			get
			{
				return maxy - miny+ 1;
			}
		}
		public Area()
		{
			positions = new List<Vector2i>();
			minx = int.MaxValue;
			maxx = -1;
			miny = int.MaxValue;
			maxy = -1;
		}
		
		public Area(int count)
		{
			positions = new List<Vector2i>(count);
			minx = int.MaxValue;
			maxx = -1;
			miny = int.MaxValue ;
			maxy = -1;
		}
		public void updateMap()
		{
			int w = width;
			int h = height;
			map = new bool[w, h];
			foreach(Vector2i v in positions)
			{
				map[v.X - minx, v.Y - miny] = true;
			}

		}
		public bool contains(int x,int y)
		{
			foreach (Vector2i vector in positions)
			{
				if (vector.X == x && vector.Y == y)
				{
					return true;
				}
			}
			return false;
		}
		public static Area operator+(Area a,Area b)
		{
			Area area = new Area(a.Count+b.Count);
			foreach(Vector2i v in a.positions)
			{
				area.positions.Add(v);

			}
			foreach (Vector2i v in b.positions)
			{
				area.add(v.X,v.Y);

			}
			area.maxx = Math.Max(a.maxx, b.maxx);
			area.maxy = Math.Max(a.maxy, b.maxy);
			area.miny = Math.Min(a.miny, b.miny);
			area.minx = Math.Min(a.minx, b.minx);
			return area;
		}
		public void updateDirection()
		{
			int w = width;
			int h = height;
			int cx = w / 2;
			int cy= h / 2;
			PosList_X = new Vector2[w];
			PosList_Y = new Vector2[h];
			DirectionList_X = new Vector2[w];
			DirectionList_Y = new Vector2[h];
			Direction_X = new Vector2(0, 0);
			Direction_Y = new Vector2(0, 0);
			updateMap();
			for(int i=0;i<w;i++)
			{
				double y=0,n=0;
				for(int j=0; j<h;j++)
				{
					if(map[i,j])
					{
						y += j;
						n++;
					}

				}
				if(n>0)
				{
y /= n;PosList_X[i] = new Vector2(i, y);
				}
				
				else
				{
					PosList_X[i] = new Vector2(i, cy);
				}
				
				if(i>0)
				{
					DirectionList_X[i - 1] = (PosList_X[i] - PosList_X[i - 1]);
					Direction_X += DirectionList_X[i - 1].nolrmalized();
					if(i==w-1)
					{
						DirectionList_X[i] = (PosList_X[i] - PosList_X[0]);
					}
				}


			}for (int j = 0; j < h; j++)
				{double x = 0, n = 0;
			for (int i = 0; i < w; i++)
			{
				
				
					if (map[i, j])
					{
						x += i;
						n++;
					}

				}
				if (n > 0)
				{
					x /= n; PosList_Y[j] = new Vector2(x, j);
				}

				else
				{
					PosList_Y[j] = new Vector2(cx, j);
				}
				if (j > 0)
				{
					DirectionList_Y[j - 1] = (PosList_Y[j] - PosList_Y[j- 1]);
					Direction_Y += DirectionList_Y[j - 1].nolrmalized();
					if (j == h - 1)
					{
						DirectionList_Y[j] = (PosList_Y[j] - PosList_Y[0]);
					}
				}
			}
			cos = Direction_Y.cos((Vector2)Direction_X);
		}
		public void updateRange()
		{
			int x, y;
			minx = int.MaxValue;
			maxx = -1;
			miny = int.MaxValue;
			maxy = -1;
			foreach (Vector2i v in positions)
			{
				x = v.X;
				y = v.Y;
				if (x > maxx)
				{
					maxx = x;
				}
				if (x < minx)
				{
					minx = x;
				}
				if (y > maxy)
				{
					maxy = y;
				}
				if (y < miny)
				{
					miny = y;
				}
			}
		}
		public static Area operator -(Area a, Area b)
		{
			Area area = new Area(a.Count );
			foreach (Vector2i v in a.positions)
			{
				area.positions.Add(v);
				area.Count++;
			}
			foreach (Vector2i v in b.positions)
			{
				area.remove(v.X, v.Y);

			}
			area.updateRange();
			return area;
		}
		public bitmap getImage()
		{
			bitmap b = new bitmap(width, height);
			int[,] data = b.getData();
			
			
				foreach (Vector2i v in positions)
				{
					data[v.X-minx, v.Y-miny] = 0x00ffffff;
				}
			
			return b;
		}
		public Area QuikMerge(Area other)
		{
			Area area = new Area(Count + other.Count);
			area.maxx = Math.Max(maxx, other.maxx);
			area.maxy = Math.Max(maxy, other.maxy);
			area.miny = Math.Min(miny, other.miny);
			area.minx = Math.Min(minx, other.minx);
			foreach (Vector2i v in positions)
			{
				area.positions.Add(v);

			}
			foreach (Vector2i v in other.positions)
			{
				area.positions.Add(v);

			}
			area.Count = Count + other.Count;
			return area;
		}
		public void directlyAdd(int x, int y)
		{
			
			positions.Add(new Vector2i(x, y));
			if(x>maxx)
			{
				maxx = x;
			}
			if (x < minx)
			{
				minx = x;
			}
			if (y > maxy)
			{
				maxy = y;
			}
			if (y < miny)
			{
				miny = y;
			}
			Count++;
		}
		public void add(int x,int y)
		{
			foreach(Vector2i vector in positions)
			{
				if(vector.X==x&&vector.Y==y)
						{
					return;
				}
			}
			if (x > maxx)
			{
				maxx = x;
			}
			if (x < minx)
			{
				minx = x;
			}
			if (y > maxy)
			{
				maxy = y;
			}
			if (y < miny)
			{
				miny = y;
			}
			positions.Add(new Vector2i(x, y));
			Count++;
		}
		public void remove(int x, int y)
		{
			
			for (int i=0;i<positions.Count;i++)
			{

				if (positions[i].X == x && positions[i].Y == y)
				{
					Count--;
					positions.RemoveAt(i);
					break;
				}
			}
		}
	}
	public struct ScanLine
	{
	
		Vector2 direction;
		Vector2 position;
		double length;
		public ScanLine(Vector2 pos,double l)
		{
			position = pos;
			length = l;
			direction = new Vector2(1, 0);
		}
		public ScanLine(Vector2 dir,Vector2 pos, double l)
		{
			position = pos;
			length = l
				;direction = dir;
			direction = new Vector2(1, 0);
		}
	}

	public class RoomTask
	{

	}

	public class HeadHunter
	{
		int width;
		int height;
		double minr = 20;
		bitmap Source;
		double r;
		List<Vector2i> FaceDetector_up;
		List<Vector2i> FaceDetector_down_L;
		List<Vector2i> FaceDetector_down_R;
		List<Vector2i> EyeDetector_L;
		List<Vector2i> EyeDetector_R;
		List<Vector2i> NoseDetector_L;
		List<Vector2i> NoseDetector_R;
		public bool isSkin(int color)
		{
			return FaceHelper.Helper.isSkinColor(color);
		}
		public void setR(double r)
		{
			this.r = r;
			double R = r;
			double R2 = R * R;

			int Ri = (int)R;
			FaceDetector_up = new List<Vector2i>();
			FaceDetector_down_L= new List<Vector2i>();
			FaceDetector_down_R= new List<Vector2i>();
			EyeDetector_L = new List<Vector2i>();
			EyeDetector_R = new List<Vector2i>();
			NoseDetector_L = new List<Vector2i>();
			NoseDetector_R = new List<Vector2i>();


			List<Vector2i> eyes = new List<Vector2i>();
			Vector2 pos;
			Vector2i posi;
			double er = r / 3.2;
			double er2 = er * er;
			double tr; int offset = (int)(r / 2.5);
			int offset2 = (int)(r / 2.4);
			int eri = (int)er ;
			for(int i=-Ri;i<=Ri;i++)
			{
				for (int j = -Ri; j <= Ri; j++)
				{
					pos = new Vector2(i, j);
					posi = new Vector2i(i, j);
					tr =pos.length_2();
					if(tr<=R2)
					{
						if(j<0)
						FaceDetector_up.Add(posi);
						else
						{
							if(i<=0)
							FaceDetector_down_L.Add(posi);
							if (i >= 0)
							FaceDetector_down_R.Add(posi);
						}
						if (tr <= er2)
					{
						eyes.Add(new Vector2i(i, j));
					}

                      
					}
				}
			}
			Vector2i Lv = new Vector2i(-offset,0);
			Vector2i Rv= new Vector2i(offset, 0);
			Vector2i Nv = new Vector2i(0, offset2);
			foreach (Vector2i v in eyes)
			{
				EyeDetector_L.Add(v + Lv);
				EyeDetector_R.Add(v + Rv);
				if (v.X <= 0)
				{
					NoseDetector_L.Add(v + Nv);
				}
				else if(v.X >= 0)
				{
					NoseDetector_R.Add(v + Nv);
				}
			}
		}
		public HeadHunter(bitmap Source)
		{
			this.Source = Source;
			int rr = Math.Min(Source.Width, Source.Height);
			rr /= 2;
			setR(rr);
			width = Source.Width;
			height = Source.Height;
		}
		public AreaMap getNoseMap()
		{
			AreaMap map = new AreaMap(width, height);
			bitmap e = new bitmap(width, height)
			; int[,] data = e.getData();
			int[,] data0 = Source.getData();
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (colorLib.isNoseColor(data0[x, y]))
					{
						map.add_XY(x, y);
					}
				}
			}

			return map;
		}
		public AreaMap getEyesMap()
		{
			AreaMap map = new AreaMap(width, height);
			bitmap e = new bitmap(width, height)
			;int[,] data = e.getData();
			int[,] data0 = Source.getData();
			for (int x = 0; x < width; x ++)
			{
				for (int y = 0; y < height; y++)
				{
					if(colorLib.isEyeColor(data0[x,y]) )
					{
						map.add_XY(x, y);
					}
				}
			}
			
			return map;
				}
		public Vector3? search()
		{
			AreaMap e = getEyesMap();
			AreaMap n = getNoseMap();
			int stride = (int)(r * 0.05);
			double rate;
			stride = stride > 0 ? stride : 1;
			Vector2i pos;
			Vector2 vL, vR;
			Vector2 eyePos_L;
			Vector2 eyePos_R;
			Area area;
			AreaSet NoseEye ;
			double fl,el ;
			double fr,er;
			int x_max = width - (int)(r / 2.2);
			int y_max = height - (int)(r / 2.2);
			for (int x=(int)(r/2.5);x<x_max;x+=stride)
			{
				for (int y = 0; y < y_max; y+=stride)
				{
					eyePos_L = new Vector2(0, 0);
					eyePos_R = new Vector2(0, 0);
                    double num = 0;
					double num2=0;
					NoseEye = new AreaSet();
					el =er= 0;
					foreach (Vector2i v in EyeDetector_L)
					{
						pos = v.add(x, y);
						area = e.getArea(pos.X, pos.Y);
						if (area != null)
						{
							NoseEye.add(area);
							eyePos_L += pos.Vector2;
							el++;
						num++;
						}
						if (isSkin(Source.getSafeValue(pos.X, pos.Y)))
						{
							num2++;
						}
					}
					rate= num / EyeDetector_L.Count;
					if(rate>0.06&&rate<0.35&&num2/EyeDetector_L.Count>0.6)
					{
						eyePos_L /= num;
num = 0;
						num2 = 0;
					foreach (Vector2i v in EyeDetector_R)
					{
						pos = v.add(x, y);
							area = e.getArea(pos.X, pos.Y);
						if ( area!= null)
						{
								NoseEye.add(area);
								num++;
								er++;
								eyePos_R += pos.Vector2;
						}
							if (isSkin(Source.getSafeValue(pos.X, pos.Y)))
							{
								num2++;
							}
						}
						rate = num / EyeDetector_L.Count;
						if (rate > 0.06 && rate < 0.35 && num2 / EyeDetector_L.Count > 0.6&MathHelper.STDEV_ONE(er,el)<0.35)
					{
							eyePos_R /= num;
                              num = 0;

							double dev = MathHelper.STDEV_ONE((eyePos_R.reduce(x,y)).length_2(),( eyePos_L.reduce(x,y)).length_2());
							if (dev > 0.25)
								continue;



							vL = new Vector2(0, 0);
							foreach (Vector2i v in NoseDetector_L)
							{
								pos = v.add(x, y);
								area = n.getArea(pos.X, pos.Y);
								if ( area!= null)
								{
									num++; NoseEye.add(area);
									vL += new Vector2(pos.X, pos.Y);
								}
							}
							rate = num / NoseDetector_L.Count;
							if(rate>0&&rate<0.3)
							{
								vL /= num;
								num = 0;
								fl = fr = 0;
								vR = new Vector2(0, 0);
								foreach (Vector2i v in NoseDetector_R)
								{
									pos = v.add(x, y);
									area = n.getArea(pos.X, pos.Y);
									if (area != null)
									{
										num++; NoseEye.add(area);
										vR += new Vector2(pos.X, pos.Y);
									}
								}
								rate = num / NoseDetector_R.Count;
								if (rate > 0 && rate < 0.3)
								{
									vR /= num;
									if(vR.distance(vL)<=r/4)
									{
		                           foreach (Vector2i v in FaceDetector_down_L)
									{
										pos = v.add(x, y);
										if (isSkin(Source.getSafeValue(pos.X, pos.Y)))
										{
											num++;
												fl++;
										}
									}
										foreach (Vector2i v in FaceDetector_down_R)
										{
											pos = v.add(x, y);
											if (isSkin(Source.getSafeValue(pos.X, pos.Y)))
											{
												num++;
												fr++;
											}
										}
										if (num / (FaceDetector_down_L.Count + FaceDetector_down_R.Count) > 0.75&&MathHelper.STDEV_ONE(fl,fr)<0.15)
									{

										foreach (Vector2i v in FaceDetector_up)
										{
											pos = v.add(x, y);
											if (isSkin(Source.getSafeValue(pos.X, pos.Y)))
											{
												num++;
											}
										}
										if (num / (FaceDetector_down_L.Count+FaceDetector_down_R.Count + FaceDetector_up.Count) > 0.6&&NoseEye.Count>1)
										{
											
											return new Vector3(x, y, r);
										}
									}


									}
							

								}
							}


							
					
					}
					}
					
					
					
					
				}
			}
			r *= 0.9;
			if(r>=minr)
			{
				setR(r);
				return search();
			}
			else
			{
				return null;
			}
		}
	}
	public static class colorLib
	{
	static	Vector3  SkinColor = new Vector3(151, 125, 112);
		static Vector3 SkinColor2 = new Vector3(136, 95, 75);
		static Vector3 SkinColor3 = new Vector3(151, 125, 112);
		static Vector3 SkinColor4 = new Vector3(115, 74, 70);
		static Vector3 SkinColor5 = new Vector3(168, 121, 115);
		static Vector3 SkinColor6 = new Vector3(131, 100, 80);
		static Vector3 SkinColor7 = new Vector3(112, 96, 71);

	static	public double minLength2 = 10000;
	static	public double cos = 0.999;
		static public double maxLength2 = 255 * 255 * 3;
		public static double Three05 = Math.Sqrt(3);

		static public bool isEyeColor(int color)
		{
			int r = (color >> 16) & 0xff;
			int g = (color >> 8) & 0xff;
			int b = (color) & 0xff;

			double v = Math.Sqrt(r * r + g * g + b * b) / Three05;
			return v < 90;
		}
		static public bool isNoseColor(int color)
		{
			int r = (color >> 16) & 0xff;
			int g = (color >> 8) & 0xff;
			int b = (color) & 0xff;

			double v = Math.Sqrt(r * r + g * g + b * b) / Three05;
			return v < 100;
		}
		public static bool isSkinColor(int color)
		{
			Vector3 cv = new Vector3(color);

			return cv.length_2() >= minLength2 && cv.length_2() <= maxLength2 && ((cv.cos(SkinColor) >= cos || cv.cos(SkinColor2) >= cos || cv.cos(SkinColor3) >= cos || cv.cos(SkinColor4) >= cos || cv.cos(SkinColor5) >= cos || cv.cos(SkinColor6) >= cos || cv.cos(SkinColor7) >= cos));
		}
	}
public	class FaceHelper
	{
		public static FaceHelper Helper = new FaceHelper(new bitmap(0, 0));
	public bitmap source { get; private set; }
		bitmap Skin;
		bitmap eyes;
		public AreaMap Area { get; private set; }
	
		public double minLength2 =10000;
		public double maxLength2 =255*255*3;
		public double[,] density;
		public double[,] delta;
		public int r = 2;
		public int eye_r = 15;
		int width;
		int height;
		public FaceHelper(bitmap source)
		{
			this.source = source;
			width = source.Width;
			height = source.Height;
		}
		public void setColor(int color)
		{
		
			updateSkin();
			delta = null;
			density = null;
		}
		
		public bool isSkinColor(int color)
		{
			
			return colorLib.isSkinColor(color);
		}
		
		public void updateSkin()
		{
			Skin = getSkin();
		}

		public bitmap searchEyes()
		{
			if (density == null)
				updateDensity();

			bitmap b = new bitmap(width, height);
			b.Foreach((int x, int y, int[,] data) => {
				double d = density[x, y];
				if (d > 0.3 && d < 0.7)
				{ data[x, y] = 0x00ffffff; };
			});
			return b;
		}
		public bitmap[] getDetails()
		{
			if (Area == null)
				upadteArea();
			
			
			Area[] areas = Area.getAreas;
			bitmap[] bitmaps = new bitmap[areas.Length];
			int n = 0;
			foreach (Area area in areas)
			{
				if(area.Direction_X==null)
				{
					area.updateDirection();
				}
				int w = area.maxx - area.minx + 1;
				int h = area.maxy - area.miny + 1;
				int offsetx = area.minx;
				int offsety = area.miny;
				bitmap b = new bitmap(w, h);
				int[,] c = b.getData();
				int[,] data = source.getData();
				for (int i = 0; i < w; i++)
				{
					for (int j = 0; j < h; j++)
					{
						c[i, j] = data[i + offsetx, j + offsety];
					}
				}
				foreach(Vector2i v in area.positions)
				{
					c[v.X - offsetx, v.Y - offsety] = 0x0000ff00;

				}
				foreach(Vector2 v in area.PosList_X)
				{
					c[(int)v.X , (int)v.Y ] = 0x00ff0000;
				}
				foreach (Vector2 v in area.PosList_Y)
				{
					c[(int)v.X, (int)v.Y ] = 0x000000ff;
				}
				b.name = "x_" +((Vector2)area.Direction_X).angle()+"y_"+area.Direction_Y.angle()+"Cos_"+area.Direction_X.Value.cos(area.Direction_Y);
				bitmaps[n] = b;
				n++;
			}
			return bitmaps;
		}
		public bitmap[] getAreas()
		{
			if (Area == null)
				upadteArea();
			Area[] areas = Area.getAreas;
			bitmap[] bitmaps = new bitmap[areas.Length];
			int n = 0;
			foreach (Area area in areas)
			{

				int w = area.maxx - area.minx + 1;
				int h = area.maxy - area.miny + 1;
				int offsetx = area.minx;
				int offsety = area.miny;
				bitmap b = new bitmap(w, h);
				int[,] c = b.getData();
				int[,] data = source.getData();
				for(int i=0;i<w;i++)
				{
					for (int j = 0; j <h; j++)
					{
						c[i , j ] = data[i+offsetx, j+offsety];
					}
				}
				bitmaps[n] = b;
				n++;
			}
			return bitmaps;
		}
		public bitmap getEyes()
		{
			bitmap bitmap = source;
			bitmap bitmap1 = new bitmap(bitmap.Width, bitmap.Height);
			int[,] v1 = bitmap1.Data;
			int color;
			bitmap.Foreach((int x, int y, int[,] data) => {
				color = data[x, y];
				
			});

			return bitmap1;

		}
		public void upadteArea()
		{
			if(density==null)
			{
				updateDensity();
			}
			if(delta==null)
			{
				updateDelta();
			}
			Area = new AreaMap(width,height);
			for(int i=0;i<width;i++)
			{
				for (int j = 0; j <height; j++)
				{
		
					if(density[i,j]>0.8&&delta[i,j]<0.5)
					{
						Area.add_XY(i, j);
					}
				}
			}
		}
		public void updateDelta()
		{
			delta = new double[width, height];
			bitmap d = source.applyFiltering(SobelFilter.sobelFilter);
			int[,] data = d.getData();
			int x, y,r,g,b,v;
			double k = 255 * 3;
			for (x = 0; x < width; x++)
			{
				for (y = 0; y < height; y++)
				{
					v = data[x, y];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v ) & 0xff;
					delta[x, y] = (r + g + b) / k;
				}
			}
		}
		public void updateDensity()
		{
			density = new double[width, height];
			int x; int y; int i, j; double total, color; int w =width, h = height;
			if(Skin==null)
			{
				updateSkin();
			}
			for (x = 0; x < w; x++)
			{
				for (y = 0; y < h; y++)
				{
					total = 0;
					color = 0;
					for (i = x - r; i <= x + r; i++)
					{
						for (j = y - r; j <= y + r; j++)
						{
							total++;
							if (Skin.getSafeValue(i, j) != 0)
							{
								color++;
							}
						}
					}
					double vv = color / total;
					density[x, y] = vv;
				}
			}
		}

		public bitmap getDensity(bitmap skin,double min)
		{
			int x; int y; int i, j; double total, color;int w = skin.Width,h=skin.Height;
			bitmap map = new bitmap(w, h);
			
			for (x = 0; x < w; x++)
			{
				for (y = 0; y < h; y++)
				{
					total = 0;
					color = 0;
					for (i = x - r; i <= x+r; i++)
					{
						for (j = y - r; j <=y+ r; j++)
						{
							total++;
							if (skin.getSafeValue(i, j) != 0)
							{
								color++;
							}
						}
					}
					double vv = color / total;
					vv = vv >= min ? vv : 0;
					int v = (int)( vv* 255);
					
					map.Data[x, y] = v | (v << 8) | (v << 16);
				}
			}
			return map;
		}
		public bitmap scanArea(bitmap scan)
		{
			if (Skin == null)
				updateSkin();

			bitmap b = source.Clone();
			int lastlength = 0;
			int x, y;for (y = 0; y < height; y++)
				{
				int px = width;
				int l = 0;
				
			for (x = 0; x < width; x++)
			
				{
				if(scan.Data[x,y]!=0)
					{
						if(px==width)
						{
							px = x;
						}
						l = x - px + 1;
					}


				}
			if(l>0&&l>lastlength)
				{
					int lx = l + px;
					for (x = 0; x <l; x++)

					{
						b.Data[x, y] = 0x00ff0000;
						
					


					}
					lastlength = l;
				}
			}
			return b;
				}
		public bitmap getDensity(double min)
		{ 
			if(Skin==null)
			{
				updateSkin();
			}
			return getDensity(Skin, min);
		}
		public bitmap getSkin()
		{
			bitmap bitmap = source;
			bitmap bitmap1 = new bitmap(bitmap.Width, bitmap.Height);
			int[,] v0 = bitmap.Data;
			int[,] v1 = bitmap1.Data;
			int color;
			bitmap.Foreach((int x, int y,int [,] data) => {
				color = data[x, y];
			if(isSkinColor(color))
				{
					v1[x, y] = color;
				}
			});
			Skin = bitmap1;
			return bitmap1;

		}
	}
}
