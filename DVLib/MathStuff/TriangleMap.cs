using Images;
using MathBase;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathBase
{
	public enum TriangleType : byte
	{
		DOWN, RIGHT
	}

	public class TrangleInfos<T>
	{
		internal List<TriangleInfo<T>> list = new List<TriangleInfo<T>>();
		public TrangleInfos()
		{
		}


		public TrangleInfos(ICollection<TriangleInfo<T>> l)
		{
			list.EnsureCapacity(l.Count);
			foreach(var v in l)
			{
					list.Add(v);
			}
		}

		public void render(Map<T> map,Vector2 offset,Vector2 scale)
		{
			foreach(var v in list)
			{
				v.render(map, offset, scale);
			}
		}

		

	}
	public struct TriangleInfo<T>
	{
		 public readonly Triangle2D Triangle2D;
		 public  readonly T value;
		public TriangleInfo(Triangle2D Triangle, T value)
		{
			Triangle2D= Triangle;
			this.value= value;
		}

		public void render(Map<T> map,Vector2 offset,Vector2 scale)
		{
			map.drawTriangleNew(value, Triangle2D.p1*scale+offset, Triangle2D.p2*scale+offset, Triangle2D.p3 * scale + offset);
		}
	}
	public class TriangleValue<T>
	{

		public int x { get;internal set; }
		public int y { get; internal set; }
		public int size { get; internal set; }
		public T value { get; internal set; }

		public TriangleType type { get; internal set; }

		public void Draw(Map<T> map)
		{
			if(type==TriangleType.RIGHT)
			{
	        for(int i=0; i<size; i++)
			{
				for(int j=0; j<=i; j++)
				{
					map[x+i,y+j] = value;
				}
			}
			}
			else
			{
			for (int j = 0; j <size; j++)	
				{
					for (int i = 0; i <=j; i++)
					{
						map[x + i, y + j] = value;
					}
				}
			}
		
		}
		public TriangleInfo<T> TriangleInfo 
		{ 
			get 
			{ 
				
				if(type == TriangleType.DOWN)
				{
					Triangle2D t = new Triangle2D(new Vector2(x-0.5,y-0.5),new Vector2(x+size-0.5,y+size-0.5),new Vector2(x-0.5,y+size-0.5));
			       return new TriangleInfo<T>(t, value);
				}
				else
				{
					Triangle2D t = new Triangle2D(new Vector2(x - 0.5, y - 0.5), new Vector2(x + size - 0.5, y + size - 0.5), new Vector2(x+ size - 0.5, y  - 0.5));
					return new TriangleInfo<T>(t, value);
				}
			} 
		}

		
}

	class NotMatchException : Exception
	{
		public override string Message => "参考矩阵的尺寸应与原尺寸保持一致";
	}


	public class TriangleMap<T>
	{
		 List<TriangleValue<T>> triangles = new List<TriangleValue<T>>();	
		public int size { get { return triangles.Count; } }

		public void setMinLevel(int l)
		{
            for (int i = 0; i < triangles.Count; i++)
            {
				if (triangles[i].size<l)
				{
					triangles.RemoveAt(i);
					i--;
				}
            }
        }
		public TrangleInfos<T> GetTrangleInfos()
		{
			var t=new TrangleInfos<T>();	
			foreach (var tri in triangles)
			{
				t.list.Add(tri.TriangleInfo);
			}
			return t;
		}
		public TriangleValue<T>[] values {  get { return triangles.ToArray(); } }
	
		public static TriangleMap<T> getTriangleMap(Map<T> map, Map<bool> checker = null)
		{


			if (checker == null)
			{
				checker = new Map<bool>(map.Width,map.Height);
			}
			else if(map.Width!=checker.Width||map.Height!=checker.Height) 
			{
			
					throw new NotMatchException();
			}

			TriangleMap<T> map1 = new TriangleMap<T>();
			

	        for (int i = 0; i < map.Width; i++)
			{
				for(int j = 0; j < map.Height; j++)
				{
					if (!checker[i, j])
					{
						
						map1.triangles.Add( growStruct(i,j,map,checker));
					}
				}
			}
			return map1;
		}
	
		public static bool Eq(T a, T b,bool isv)

		{

			if(isv)
			{
				return EqV(a,b);
			}
			else
			{
				return EqC(a,b);	
			}

		}

		public void Draw(Map<T> map)
		{
			foreach(var v in triangles)
			{
				v.Draw(map);
			}
		}
		public unsafe static bool EqV(T a, T b)
		{
			try
			{
            return ((IEquatable<T>)a).Equals(b);
			}
			catch (Exception e)
			{
				int s = sizeof(T);
				byte* ap = (byte*)&a, bp = (byte*)&b;
				for (int i = 0; i < s; i++)
				{
					if (ap[i] != bp[i])
					{
						return false;
					}
				}
				return true;
			}
				
		}
		public unsafe static bool EqC(T a, T b)
		{
			if(a==null || b == null)
			{
				return a ==null&&b==null;
			}
			return &a == &b;
		}
public static Map<bool> getCheckerByBackground(Map<T> map,params T[] backgrounds)
		{
			HashSet<T> set = backgrounds.ToHashSet();
			Map<bool> map1=new Map<bool>(map.Width, map.Height);
			for (int i = 0; i < map.Width; i++)
			{
				for (int j = 0; j < map.Height; j++)
				{
					if (set.Contains(map[i,j]))
					{
						map1[i,j] = true;
					}
				}
			}
			return map1;
		}
static TriangleValue<T> growStruct(int x, int y, Map<T> map, Map<bool> checker = null)
{
			var down = growStructDown(x, y, map, checker);
			var right=growStructRight(x, y, map,checker);
	
			if(down.size>right.size)
			{
				right = down;

				for(int j=0;j<down.size; j++)
				{

				for(int i=0;i<=j;i++)
					{
						checker[i+x, j+y] = true;
					}
				}

			}
			else
			{
				for (int i = 0; i <right.size; i++)
				{
	for (int j = 0; j <=i; j++)
				{

					
						checker[i + x, j+y] = true;
					
				}
				}
			
			}

			return right;
}
static TriangleValue<T> growStructDown(int x, int y, Map<T> map, Map<bool> checker = null) 
		{

			var v = map[x, y];
			int triangleLevel = 1;
			bool flag = true;
			int xp = x; 
			int yp = y;
			int max;
			bool isv=typeof(T).IsValueType;
			int dy, dx;
			while (flag)
			{
				
					yp++;
					max = triangleLevel + 1 + xp;
					if (max <= map.Width && yp < map.Height)
					{
						for (int i = xp; i < max; i++)
						{
							if (!checker[i, yp] && Eq(map[i, yp],v,isv))
							{

							}
							else
							{
								flag = false;
								break;
							}
						}
					}
					else
					{
						flag = false;
					}
					if(flag)
				{
					triangleLevel++;
				}
					/*
					if (flag)
					{
						triangleLevel++;
						for (int i = xp; i < max; i++)
						{
							checker[i, yp] = true;
							((Map<int>)(object)map)[i, yp] =c;
						}
					}
				*/
			}

			return new TriangleValue<T> { x = x, y=y,size=triangleLevel,value=v,type=TriangleType.DOWN};

		}
static TriangleValue<T> growStructRight(int x, int y, Map<T> map, Map<bool> checker = null)
{

	var v = map[x, y];
	int triangleLevel = 1;
	bool flag = true;
	int xp = x; int yp = y;
	int max;
	bool isv = typeof(T).IsValueType;
	int dy, dx;
	while (flag)
	{

				
					xp++;
					max = triangleLevel + 1 + yp;
					if (max <= map.Height && xp < map.Width)
					{
						for (int i = yp; i < max; i++)
						{
							if (!checker[xp, i] && Eq(map[xp, i], v, isv))
							{

							}
							else
							{
								flag = false;
								break;
							}

						}
					}
					else
					{
						flag = false;
					}
				if (flag)
				{
					triangleLevel++;
				}
				/*
				if (flag)
				{
					triangleLevel++;
					for (int i = yp; i < max; i++)
					{
						checker[xp, i] = true;

						((Map<int>)(object)map)[xp, i] = c;
					}
				}*/
				/*
				if (flag)
				{
					triangleLevel++;
					for (int i = xp; i < max; i++)
					{
						checker[i, yp] = true;
						((Map<int>)(object)map)[i, yp] =c;
					}
				}
			*/
			}

	return new TriangleValue<T> { x = x, y = y, size = triangleLevel, value = v,type= TriangleType.RIGHT };

}
	}
}

