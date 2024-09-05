using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using Images;
namespace physics
{
	namespace  Optics
	{
		public static class OpticHelper
		{
			
				static public bool normal(this double v)
				{
					return !double.IsNaN(v) && !double.IsInfinity(v);
				}
				static public bool normal(this Vector2 v)
				{
					return v.X.normal() && v.Y.normal();
				}
			
		}
		public class Ray:OpticsObject
		{

			void onPositionChanged(Vector2 pos)
			{
				direction = handle.Position - Position;
			}
			public class rayHandle:OpticsObject
			{
				
			public	Ray father { get; private set; }
			
				void onPositionChanged(Vector2 pos)
				{
					father.direction = pos - father.Position;
				}
				public rayHandle(Vector2 position,Ray ray):base("",position)
				{
					father = ray;
					PositionChanged += onPositionChanged;
				}
			}
			public class subRay
			{
				public bool isEnd { get; private set; }
				public Vector2 position { get; private set; }
				public Vector2 direction { get; private set; }
				public subRay(Vector2 p,Vector2 d)
				{
					position = p;
					direction = d.nolrmalized();
					isEnd = false;
				}
				public subRay(Vector2 p)
				{
					position = p;
					direction = (0,0);
					isEnd = true;
				}
			}
			Vector2 direction;
		public rayHandle handle { get; private set; }
			
			OpticsSystem system;
			public List<subRay> rays {
				get
				{
					double u, h, l;
					subRay last;
					List<subRay> subRays = new List<subRay>();
					last = new subRay(Position, direction);
					subRays.Add(last);
					int i = 0;
					foreach(Refractingsurface face in from f in system.surfaces where f.Position.X>Position.X select f)
					{
						if (!face.enabled) continue;
							u = (Vector2.Xaxis.angle() - last.direction.angle()) / 180 * Math.PI;
							if (last.direction.Y == 0)
							{
								l = double.NegativeInfinity;
							}
							else
							{
								l = (new Line2d(last.position, last.position + last.direction).crosspoint(Line2d.Xaxis)).X;
							}
							h = u * (l - face.Position.X);

							if (last.direction.Y == 0)
							{
								h = last.position.Y;
							}
							if (h > face.size || h < -face.size)
							{
								u = face.u1(l - face.Position.X, h);
								l = face.l1(l - face.Position.X);
								h = u * l;
								l += face.Position.X;


								subRays.Add(new subRay(new Vector2(face.Position.X, (new Line2d(last.position, last.position + last.direction).crosspoint(new Line2d(face.Position, face.Position + new Vector2(0, 1)))).Y)));
								break;
							}
						if (face.n0 != face.n1)
						{
							u = face.u1(l - face.Position.X, h);
							l = face.l1(l - face.Position.X);
							h = u * l;
							l += face.Position.X;

							Vector2 temp = new Vector2(face.Position.X, (new Line2d(last.position, last.position + last.direction).crosspoint(new Line2d(face.Position, face.Position + new Vector2(0, 1)))).Y);

							double dd = new Vector2((new Vector2(l, 0) - temp).X * face.n0 * face.n1, 0).nolrmalized().X;
							last = new subRay(temp, dd * (new Vector2(l, 0) - temp));

							subRays.Add(last);
							i++;
						}
						
					}
					return subRays;
				}
			}
			public Ray(string name,Vector2 position,Vector2 direction):base(name,position)
			{
				this.direction = direction.nolrmalized();
				handle = new rayHandle(position + this.direction*5,this);
				PositionChanged += onPositionChanged;
			}

			public void addToSystem(OpticsSystem system)
			{
				this.system = system;
				system.add(handle);
			}
		}
		public class Refractingsurface: OpticsObject//单个折射面
		{
			public double n0;//折射率n
			public double n1;//折射率n'
			public double r;//曲率半径
			public double size;
			Refractingsurface(string name ,double n0,double n1,double r,double d,Vector2 position):base(name,position)
			{
				size = d / 2;
				this.n0 = n0;
				this.n1 = n1;
				this.r = r;
			}
			public Refractingsurface(string name,double n0, double n1, double r, double d, double position):base(name ,new Vector2(position,0))
			{
				size = d / 2;
				this.n0 = n0;
				this.n1 = n1;
				this.r = r; 
			}
			public Refractingsurface(double n0, double n1, double r, double d, double position) : base("球面", new Vector2(position, 0))
			{
				size = d / 2;
				this.n0 = n0;
				this.n1 = n1;
				this.r = r; 
			}
			Vector2 C { get { return new Vector2(r, 0); } }//球心的相对坐标
			public Vector2 c { get
				{
					return C + Position;
				}
			}
			 
			public double l1(double l0)//计算像方截距
			{

				double l1
				= n1 / ((n1 - n0) / r + n0 / l0);
				return l1;
			}
			public double l0(double l1)//计算物方截距（逆过程）
			{

			
				double l0
				= -n0 / ((n1 - n0) / r - n1 / l1);
				return l0;
			}
			public double b(double l0)//计算垂轴放大倍率
			{
				return n0 * l1(l0) / n1 / l0;
			}
			public double a(double l0)//计算轴向放大倍率
			{
				double b_=b(l0) ;
				return n1 / n0 * b_ * b_;
			}
			public double v(double l0)//计算角放大倍率
			{
				return l0 / l1(l0);
			}
			public Vector2 Image(Vector2 Object)//计算像点位置
			{
				Object = Object - Position;//计算物点相对折射面（O点）的坐标
				Vector2 image= C + (C - Object) / (r - Object.X) * l1(Object.X);//像点相对折射面（O点）的坐标
				return image + Position;//像点在世界坐标系的坐标

			}
			public Vector2 Object(Vector2 Image)//计算像点位置(逆过程)
			{
				Image = Image - Position;//计算物点相对折射面（O点）的坐标
				Vector2 image = C + (C - Image) / (r - Image.X) * l1(Image.X);//像点相对折射面（O点）的坐标
				return image + Position;//像点在世界坐标系的坐标

			}

			public double u1(double l0,double h0)//计算像方孔径角
			{
				
				double u0 = h0 / l0;
				double i0 =( h0-u0*r) / r;
				double i1 = n0 / n1 * i0;
				return u0 + i0 - i1;
			}
			public double u1_(double l0, double u0)//计算像方孔径角
			{

				
				double i0 = (u0*l0 - u0 * r) / r;
				double i1 = n0 / n1 * i0;
				return u0 + i0 - i1;
			}
			public double u0(double l1, double h1)//计算物方孔径角
			{

				double u1 = h1 / l1;
				double i1 = (h1 - r * u1) / r;
				double i0 = i1 * n1 / n0;
				return u1-i0 + i1;
			}
			public double u0_(double l1, double u1)//计算物方孔径角
			{

				
				double i1 = (u1*l1 - r * u1) / r;
				double i0 = i1 * n1 / n0;
				return u1 - i0 + i1;
			}


		}
		public class OpticsSystem : OpticsObject//光学系统
		{
		
			
			public OpticsSystem(string name) : base(name, new Vector2(0, 0))
			{
				cleanEvent();
				PositionChanged+=onPositionChanged;
			} 
			List<OpticsObject> objects = new List<OpticsObject>();//折射面的数组
			public void reverse()
			{
				if(Eabledsurfaces.Count>0)
				{
					Vector2 op = firstOne.Position;foreach(Refractingsurface surface in Eabledsurfaces)
				{
					double temp = surface.n0;
					surface.n0 = surface.n1;
					surface.n1 = temp;
					surface.Position = -	surface.Position;
					surface.r = -surface.r;
				}
					op = op - firstOne.Position;
					foreach (Refractingsurface surface in Eabledsurfaces)
					{

						surface.Position += op;
					}
				}
				
				
			}//倒置所有折射面
			public Refractingsurface firstOne//获取第一个折射面
			{
				get
				{
					List<Refractingsurface> refractingsurfaces = Eabledsurfaces;
					if (Eabledsurfaces.Count > 0)
						return Eabledsurfaces[0];
					return null;
				}
			}
			public Refractingsurface lastOne//获取最后一个一个折射面
			{
				get
				{
					List<Refractingsurface> refractingsurfaces = Eabledsurfaces;
					if (Eabledsurfaces.Count > 0)
						return Eabledsurfaces[Eabledsurfaces.Count-1];
					return null;
				}
			}
			public IEnumerable<Ray> Rays {get
				{
					return from OpticsObject in objects where OpticsObject.isRay select (Ray)OpticsObject;
				} }
		
			public double f1 { get {
					return 100 / u1(double.NegativeInfinity, 100);
				} }//计算像方焦距
			public double f0
			{
				get
				{
					
					return 100 / u0(double.PositiveInfinity, 100);
				}
			}//计算物方焦距
			public double lf1 { get
				{
				
					return l1(double.NegativeInfinity);
				}
			}//计算像方焦面位置(绝对位置)
			public double lf1_
			{
				get
				{
					if (lastOne != null)
					{
						return l1(double.NegativeInfinity) - lastOne.Position.X;
					}
					return l1(double.NegativeInfinity);
				}
			}//计算像方焦面位置(相对最后一个折射面的位置)
			public double lf0
			{
				get
				{
					return l0(double.PositiveInfinity);
				}
			}//计算物方焦面位置(绝对位置)
			public double lf0_
			{
				get
				{
					if (firstOne != null)
					{
						return l0(double.NegativeInfinity) - firstOne.Position.X;
					}
					return l0(double.PositiveInfinity);
				}
			}//计算物方焦面位置(相对第一个折射面的位置)
			public double lh1
			{
				get
				{
					return lf1 - f1;
				}
			}//计算像方主面位置(绝对位置)
			public double lh1_
			{
				get
				{
					return lf1_ - f1;
				}
			}//计算像方主面位置(相对最后一个折射面)
			public double lh0
			{
				get
				{
					return lf0 - f0;
				}
			}//计算物方主面位置(绝对位置)
			public double lh0_
			{
				get
				{
					return lf0_ - f0;
				}
			}//计算物方主面位置(相对第一个折射面)
			public	List<Refractingsurface> surfaces { get{return(from dgzsm in objects where dgzsm.isRefractingsurface
										  orderby dgzsm.Refractingsurface.Position.X ascending
										  select dgzsm.Refractingsurface).ToList();
				}}//折射面从左到右排序;
			public List<Refractingsurface> Eabledsurfaces
			{
				get
				{
					return (from dgzsm in objects
							where dgzsm.isRefractingsurface&&dgzsm.Refractingsurface.n1!= dgzsm.Refractingsurface.n0 && dgzsm.Refractingsurface.enabled
							orderby dgzsm.Refractingsurface.Position.X ascending
							select dgzsm.Refractingsurface).ToList();
				}
			}//折射面从左到右排序;
			public List<OpticsObject> all { get { return (from oo in objects select oo).ToList(); } }
			void onPositionChanged(Vector2 pos)
			{
				Vector2 dr = pos - Position;
				setpos(pos);
				foreach(OpticsObject opticsObject in all)
				{
					if(opticsObject.isRefractingsurface)
					{
						opticsObject.Position += new Vector2(dr.X,0);
					}
					else
					opticsObject.Position += dr;
				}

			}
			public void setMain()
			{
				isMain = true;
			}
			bool isMain = false;
			public void add(OpticsObject Object)//添加折射面
			{
				if(!objects.Contains(Object))
				objects.Add(Object);

				if(Object.isRay&&isMain)
				{
					((Ray)Object).addToSystem(this);
				}
			}
			public void remove(OpticsObject Object)
			{
				objects.Remove(Object);
			}
			public Vector2 Image(Vector2 Object)//计算像点坐标
			{
				Vector2 result=Object;
				foreach(Refractingsurface surface in Eabledsurfaces)
				{
					result = surface.Image(result);
				}
				return result;
			}
			public Vector2 Object(Vector2 Image)//计算物点坐标
			{
				List<Refractingsurface> list1 = Eabledsurfaces;
				list1.Reverse();
				Vector2 result = Image;
				foreach (Refractingsurface surface in list1)
				{
					result = surface.Object(result);
				}
				return result;
			}
			public double l1(double l0)//计算像距
			{
				double result = l0;
				foreach (Refractingsurface surface in Eabledsurfaces)
				{
					result = result - surface.Position.X;
					result = surface.l1(result);
					result = result + surface.Position.X;
				}
				return result;
			}
			public double l0(double l1)//计算物距
			{
				List<Refractingsurface> list = Eabledsurfaces;
				list.Reverse();
				double result = l1;
				foreach (Refractingsurface surface in list)
				{
					result = result - surface.Position.X;
					result = surface.l0(result);
					result = result + surface.Position.X;
				}
				return result;
			}
			public double u1(double l0,double h0)//计算像方孔径角
			{
				double h = h0;
				double u=h0/l0;
				double l = l0;
				int i = 0;
				foreach (Refractingsurface surface in Eabledsurfaces)
				{
					l =l- surface.Position.X;
					
					if(i==0)
					{
                    u = surface.u1(l,h);
					}
					else
					{
					u = surface.u1_(l, u);
					}
					
					l = surface.l1(l);
					
					
					l+=surface.Position.X;
					i++;
				}
				return u;
			}
			public double u0(double l1, double h1)//计算物方孔径角
			{
				double h = h1;
				double u = h1 / l1;
				double l = l1;
				List<Refractingsurface> list = Eabledsurfaces;
				list.Reverse();
				int i = 0;
				foreach (Refractingsurface surface in list)
				{
					l = l - surface.Position.X;
				
					if(i==0)
					{
                    u = surface.u0(l, h);
					}else
					{
						u = surface.u0_(l, u);
					}
					
					l = surface.l0(l);
					
				 l += surface.Position.X;
					i++;
				}
				return u;
			}

		}

		public class OpticsObject
		{
			public bool enabled = true;
			public	delegate void pf(Vector2 p);
		public	event pf PositionChanged;
			public override string ToString()
			{
				if (isRay)
					return "光线 " + name;
				if (isRefractingsurface)
					return "折射面 " + name;
				return name;
			}
			public Ray Ray { get { return (Ray)this; } }
			public Refractingsurface  Refractingsurface { get { return (Refractingsurface)this; } }
			public Ray.rayHandle RayHandle { get { return (Ray.rayHandle)this; } }
			public bool isRay { get { return this is Ray; } }
			public bool isSystem { get { return this is OpticsSystem; } }
			public OpticsSystem  opticsSystem{ get { return (OpticsSystem)this; } }

			public bool isRefractingsurface
			{
				get{ return this is Refractingsurface; }
			}
			public bool isRayHandle
			{
				get { return this is Ray.rayHandle; }
			}
			public void setpos(Vector2 p)
			{
				pos = p;
			}
			public string name;
			Vector2 pos;
	
		public	Vector2 Position { get { return pos; }set { PositionChanged(value) ; } }
			public OpticsObject
				(string name,Vector2 position)
			{
				PositionChanged = delegate(Vector2 v) { pos = v; };
				this.name = name;
				this.Position = position;
			}public void cleanEvent()
		{
			PositionChanged = delegate (Vector2 v) { };
		}
		}

		
	
		public class OpticsWorld
		{
			public OpticsObject selected;
			public int width;
			public int height;
			public double zoom=10;
			public Vector2 pos;
			public int arch=20;
			Line2d rightLine
			{
				get
				{
					return new Line2d(toWorldPosition(new Vector2(width, 0)), toWorldPosition(new Vector2(width, height)));
				}
			}
			Line2d leftLine
			{
				get
				{
					return new Line2d(toWorldPosition(new Vector2(0, 0)), toWorldPosition(new Vector2(0, height)));
				}
			}
			public OpticsSystem mainSystem { get; private set; }
			public List<OpticsSystem> Gruops = new List<OpticsSystem>();
			public double radius=6;
			public OpticsWorld(int w,int h)
			{
				width = w;
				height = h;
				pos = new Vector2(20, 0);
				mainSystem = new OpticsSystem("");
			}
			public void setSystem(OpticsSystem sys)
			{
				sys.setMain();
				mainSystem = sys;
			}
			public void addGroup()
			{
				OpticsSystem temp = new OpticsSystem("光组" + (Gruops.Count + 1));
				Gruops.Add(temp);
				mainSystem.add(temp);
			}
			public void removeGroup(OpticsSystem system)
			{
				Gruops.Remove(system);
				mainSystem.remove(system);
			}
			public void addGroup(string name,Vector2 p)
			{
				OpticsSystem temp = new OpticsSystem(name);
				temp.Position = p;
				Gruops.Add(temp);
				mainSystem.add(temp);
			}
			public Vector2 toScreenPosition(Vector2 worldPosition)
			{
				return (new Vector2(worldPosition.X,-worldPosition.Y) - new Vector2(pos.X,-pos.Y)) * zoom + new Vector2(width / 2, height / 2);
			}
			public Vector2 toWorldPosition(Vector2 ScreenPosition)
			{
				Vector2 result= (ScreenPosition - new Vector2(width / 2, height / 2)) / zoom + new Vector2(pos.X, -pos.Y);
				return new Vector2(result.X,-result.Y);
			}
			/*
			public	Bitmap GetImage()
			{
				Bitmap backgroung = new Bitmap(width, height);
				Graphics graphics = Graphics.FromImage(backgroung);
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				Brush brush = new SolidBrush(Color.SkyBlue);
				Pen pen = new Pen(Color.Red);
				graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
				Vector2 axis = toScreenPosition(new Vector2(0, 0));
				graphics.DrawLine(pen, 0, (float)axis.Y, width, (float)axis.Y);
				if(mainSystem.surfaces.Count>0)
				{
	            Vector2 lh0a= toScreenPosition(new Vector2(mainSystem.lh0,radius*1.5));
				Vector2 lh1a = toScreenPosition(new Vector2(mainSystem.lh1, radius * 1.5));
				Vector2 lh0b = toScreenPosition(new Vector2(mainSystem.lh0, -radius * 1.5));
				Vector2 lh1b = toScreenPosition(new Vector2(mainSystem.lh1, -radius * 1.5));
				Vector2 lf0 = toScreenPosition(new Vector2(mainSystem.lf0, 0));
				Vector2 lf1 = toScreenPosition(new Vector2(mainSystem.lf1, 0));
					if(mainSystem.lh1.normal())
					{
                pen = new Pen(Color.Blue,2);
				graphics.DrawLine(pen, (float)lh1a.X,(float) lh1a.Y, (float)lh1b.X, (float)lh1b.Y);
					}

					if (mainSystem.lh0.normal())
					{
						pen = new Pen(Color.Blue,2);
						graphics.DrawLine(pen, (float)lh0a.X, (float)lh0a.Y, (float)lh0b.X, (float)lh0b.Y);
					}
				brush = new SolidBrush(Color.Green);
				
				pen = new Pen(Color.Black);
					if(mainSystem.lf0.normal())
				graphics.FillEllipse(brush, (float)lf0.X-4, (float)lf0.Y-4, (float)8, (float)8);
					if (mainSystem.lf1.normal())
						graphics.FillEllipse(brush, (float)lf1.X-4, (float)lf1.Y-4, (float)8, (float)8);
		
					foreach (Refractingsurface surface in mainSystem.surfaces)
				{
						double radius = surface.size;
						pen = new Pen(Color.Green);
						pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
						pen.DashPattern = new float[] { 6f, 2f };
						Vector2 ha = toScreenPosition(new Vector2(surface.Position.X, radius * 1.2));
						Vector2 hb = toScreenPosition(new Vector2(surface.Position.X,- radius * 1.2));
						graphics.DrawLine(pen, (float)ha.X, 0, (float)hb.X, height);
						if ((OpticsObject)surface==selected)
					{
						pen = new Pen(Color.Gray,4);
					}
					else
					{
							if(surface.enabled)
						pen = new Pen(Color.Black,3);
							else
								pen = new Pen(Color.White);
						}
					if(surface.r.normal()&&surface.Position.normal())
						{
							
							if (radius > Math.Abs(surface.r))
								radius = Math.Abs(surface.r);
                    double angle = Math.Asin(radius / surface.r);
					double a1 = angle + Math.PI;
					double a2 = -angle + Math.PI;
					double da = (a1 - a2) / arch;
					List<Vector2> vectors = new List<Vector2>();
					vectors.Add(toScreenPosition(new Vector2(Math.Cos(a2), Math.Sin(a2)) * surface.r + surface.c));
					for(int i=0;i<arch;i++)
					{
						vectors.Add(toScreenPosition(new Vector2(Math.Cos(a2+da*(i+1)), Math.Sin(a2 + da * (i + 1))) * surface.r + surface.c));
					}
					
					for (int i = 0; i < arch; i++)
					{
						graphics.DrawLine(pen, (float)vectors[i].X, (float)vectors[i].Y, (float)vectors[i+1].X, (float)vectors[i+1].Y);
					}
						}
					else if(surface.Position.normal())
						{
							Vector2 l1 = toScreenPosition(new Vector2(surface.Position.X, radius));
							Vector2 l2= toScreenPosition(new Vector2(surface.Position.X, -radius));
							graphics.DrawLine(pen, (float)l1.X, (float)l1.Y, (float)l2.X, (float)l2.Y);

						}


					}	
				}		
				foreach(Ray ray in mainSystem.Rays)
					{

						SolidBrush brush1=new SolidBrush(Color.Blue);
						List<Ray.subRay> rays = ray.rays;Vector2 h = toScreenPosition(ray.Position);
						if(selected == (OpticsObject)ray||(selected !=null &&selected.isRayHandle&&selected.RayHandle.father==ray))
						{
                  brush = new SolidBrush(Color.Gray); pen = new Pen(Color.Pink,2); brush1 = new SolidBrush(Color.BlueViolet);
						}
						else
						{
                 brush = new SolidBrush(Color.White);
							pen = new Pen(Color.Gold); brush1= new SolidBrush(Color.Blue);
						}
						
						graphics.FillEllipse(brush, (float)h.X - 3, (float)h.Y - 3, (float)6, (float)6);
						Vector2 h1 = toScreenPosition(ray.handle.Position); 
						graphics.FillRectangle(brush1, (float)h1.X -3, (float)h1.Y - 3, (float)6, (float)6);
						Vector2 p1,p2,p3;
						for(int i=0;i<rays.Count-1;i++)
						{
							p2 = toScreenPosition(new Vector2(rays[i + 1].position.X, rays[i + 1].position.Y));
					
							{
                    p1 = toScreenPosition(new Vector2(rays[i].position.X, rays[i].position.Y));
							
						
							graphics.DrawLine(pen, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
							}
						}
						
					
						if (!rays[rays.Count - 1].isEnd)
						{		p1 = toScreenPosition(rays[rays.Count - 1].position);
							double d = rays[rays.Count - 1].direction.X > 0 ? 1 : -1;
							if (d > 0)
							{
								p2 = toScreenPosition(rightLine.crosspoint(new Line2d(rays[rays.Count - 1].position, rays[rays.Count - 1].position + rays[rays.Count - 1].direction)));

							}
							else
							{
								p2 = toScreenPosition(leftLine.crosspoint(new Line2d(rays[rays.Count - 1].position, rays[rays.Count - 1].position + rays[rays.Count - 1].direction)));

							}	
							graphics.DrawLine(pen, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
						
							pen = new Pen(Color.DarkGray);
							pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
							pen.DashPattern = new float[] { 15f, 3f };
							p1 = toScreenPosition(rays[rays.Count - 1].position);
							if (d > 0)
							{
								p2 = toScreenPosition(leftLine.crosspoint(new Line2d(rays[rays.Count - 1].position, rays[rays.Count - 1].position - rays[rays.Count - 1].direction)));

							}
							else
							{
								p2 = toScreenPosition(rightLine.crosspoint(new Line2d(rays[rays.Count - 1].position, rays[rays.Count - 1].position - rays[rays.Count - 1].direction)));

							}
							graphics.DrawLine(pen, (float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
						}
					}
				
				foreach (OpticsSystem System in Gruops)
				{	Vector2 p = toScreenPosition(System.Position);
					brush = new SolidBrush(Color.IndianRed);
					if(!System.enabled)
						brush = new SolidBrush(Color.Yellow);
					if (selected==System)
					brush = new SolidBrush(Color.ForestGreen);
					graphics.FillEllipse(brush, (float)p.X-5, (float)p.Y-5, (float)10, (float)10);

					if (System.Eabledsurfaces.Count > 0)
					{
					
						Vector2 lh0a = toScreenPosition(new Vector2(System.lh0, radius * 1.5));
						Vector2 lh1a = toScreenPosition(new Vector2(System.lh1, radius * 1.5));
						Vector2 lh0b = toScreenPosition(new Vector2(System.lh0, -radius * 1.5));
						Vector2 lh1b = toScreenPosition(new Vector2(System.lh1, -radius * 1.5));
						Vector2 lf0 = toScreenPosition(new Vector2(System.lf0, 0));
						Vector2 lf1 = toScreenPosition(new Vector2(System.lf1, 0));
						if (System.lh1.normal())
						{
							pen = new Pen(Color.Blue, 2);
							if (selected == System)
								pen = new Pen(Color.Red, 3);
							graphics.DrawLine(pen, (float)lh1a.X, (float)lh1a.Y, (float)lh1b.X, (float)lh1b.Y);
						}

						if (System.lh0.normal())
						{
							pen = new Pen(Color.Blue, 2);
							if (selected == System)
						pen = new Pen(Color.Red, 3);
							graphics.DrawLine(pen, (float)lh0a.X, (float)lh0a.Y, (float)lh0b.X, (float)lh0b.Y);
						}
						brush = new SolidBrush(Color.Green);
						if (selected == System)
							brush = new SolidBrush(Color.Red);
						pen = new Pen(Color.Black);
						if (selected == System)
							pen = new Pen(Color.Brown);
						if (System.lf0.normal())
							graphics.FillEllipse(brush, (float)lf0.X - 2, (float)lf0.Y - 2, (float)4, (float)4);
						if (System.lf1.normal())
							graphics.FillEllipse(brush, (float)lf1.X - 2, (float)lf1.Y - 2, (float)4, (float)4);
					}
				}

				return backgroung;
			}
			*/
		}
	}
}
