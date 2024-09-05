using Images;
using MathBase;
using System;
using DVOSLib;
using System.Collections.Generic;
using Optic3D;

namespace NewPhysics
{

    public class PaperCard:Paper
	{
        
        public PaperCard(World world,bitmap texture):base(world,texture)
		{
            setDirection((0, 1, 0));
        }
        Vector3 x;
        Vector3 y;
        double size = 1;
        double r = 0.5;

        public void setSize(double size)
		{
            this.size = size;
            r = size / 2;
		}
		public override void setTheta(double theta)
		{
			base.setTheta(theta);
            setDirection(getDirection());
		}
		public override void setDirection(Vector3 vector)
		{
			base.setDirection(vector);
            if(direction.x==0&&direction.z==0)
			{
                if(direction.y==0)
				{
                    direction = new Vector3(0, 1, 0);
				}
                x = new Vector3(1, 0, 0);
                y = new Vector3(0, 0, 1);
                
			}
            else
			{
                Vector2 v2= new Vector2(direction.x, direction.z).row(90).nolrmalized();
                x = new Vector3(v2.X, 0, v2.Y);
                y = x.cross(direction);
                
			}
            x = Math.Cos(theta) * x + Math.Sin(theta) * y;
            y = x.cross(direction);
            setPos(position + r * x + r * y, position + r * x - r * y, position - r * x + r * y, position - r * x - r * y);
		}

	}
	public class Paper : PhysicsObject, IRayTraceSet
    {

       internal TextureTriangle t1;
        internal TextureTriangle t2;
        bitmap texture;
        Vector3 p1; Vector3 p2; Vector3 p3; Vector3 p4;

        public void setFixTexture(bool f)
		{
            t1.fixTexture = t2.fixTexture = f;
		}
        public Paper(World world, bitmap texture) : base(world)
        {
            this.p1 =(0,0,0); this.p2 = (1, 0, 0); this.p3 = (0, 0, 1); this.p4 = (1, 0, 1);
           
            this.texture = texture;
            t1 = new TextureTriangle(p1, p2, p3, texture, world, true);
            t2 = new TextureTriangle(p2, p3, p4, texture, world, false);
            position = (p1 + p2 + p3 + p4) * 0.25;
        }
        public Paper(World world,Vector3 p1,Vector3 p2,Vector3 p3,Vector3 p4,bitmap texture) : base(world)
        {
            this.p1 = p1;this.p2 = p2;this.p3 = p3;this.p4 = p4;
            this.texture = texture;
            t1 = new TextureTriangle(p1, p2, p3, texture, world, true);
            t2 = new TextureTriangle(p2, p3, p4, texture, world, false);
            position = (p1 + p2 + p3 + p4) * 0.25;
        }
        public void setColor(int c)
		{
            t1.color = c;
            t2.color = c;
		}
        public void setTexture(bitmap texture)
        {
            t1.setTexture(texture);
            t2.setTexture(texture);
        }
        public void setPos(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
		{
          
            t1.setPos(p1, p2, p3);
            t2.setPos(p2, p3, p4);
            position = (p1 + p2 + p3 + p4) * 0.25;
        }


        public override void tick(double dt)
        {
            Vector3 dp = velocity.scale(dt);
            position = position.add(dp);
            t1.move(dp);
            t2.move(dp);
        }


        public void move(Vector3 dp)
		{
            t1.move(dp);
            t2.move(dp);
		}
        public override void setPosition(Vector3 position)
        {
            Vector3 dp = position.reduce(this.position);
            t1.move(dp);
            t2.move(dp);
            this.position = position;

        }
        public Paper withCenter(Vector3 p)
        {
            position = p;
            return this;
        }










        public override PhysicsObject copy()
        {

            Paper paper = new Paper(world, p1, p2, p3, p4, texture);
            return paper;
        }


        public IRayTraceObject[] getObjects()
        {
            return new IRayTraceObject []{t1,t2 };
        }
    }

	public class StructureRay : ILightSourse
	{
        Vector3 position;
        Vector3 castPos1;
        Vector3 castPos2;
        Triangle Triangle;
        double maxsin = 0.001;
        float factor ;
        public StructureRay(Vector3 position, Vector3 castPos1, Vector3 castPos2,float factor=1,float r=1,float g=0,float b=0):base(r,g,b)
		{
			this.position = position;
			this.castPos1 = castPos1;
			this.castPos2 = castPos2;
            this.factor = factor;
            Triangle = new Triangle(position, castPos1, castPos2);
		}
        public void setMaxSin(double max)
		{
            maxsin = max;
		}
		public override ILightSourse copy()
		{
            return new StructureRay(position, castPos1, castPos2,r,g,b);
		}  
        public List<IRayTraceObject> getIRayTrace(World world)
            {
                List<IRayTraceObject> iRayTraceObjects = world.iRayTraceObjects;
                return iRayTraceObjects;
            }


		public override float getIntensity(IRayTraceObject aobject, Vector3 vector3d,Vector3 dir, bool shadow)
		{
         
            Vector3 d = vector3d - position;
                d = d.nolrmalized();

            if (dir.dot(d) < 0)
                return 0;

            bool flag = Triangle.isIn(vector3d);
            if (flag)
            {

                if (Math.Abs((d).cos(Triangle.direction)) < maxsin)
                {
                    float k = 1;
                  
                    RayTraceResult self = aobject.rayTrace(position, d); 
                    if (self != null)
                        k = (float)Math.Abs(self.direction.dot(d));
                    if (shadow)
                    {
                        if (self == null || self.point.distance_2To(vector3d) > 0.01)
                        {
                            return 0;
                        }
                        foreach (IRayTraceObject object1 in getIRayTrace(aobject.getWorld()))
                        {

                            if (object1 != aobject)
                            {
                                RayTraceResult result = object1.rayTrace(position, d);



                                if (result != null)
                                {
                                    if ((self.getPoint().distance_2To(position) > result.getPoint().distance_2To(position)))
                                    {
                                        return 0; ;
                                    }

                                }

                            }



                        }
                    }
                    Vector3 vector3d1 = position.reduce(vector3d);

                    return factor*k;

                }
                else return 0;
            }
            else
			{
                return 0;
			}

              
        }

		public override Light getIntensity(Vector3 vector3d)
		{
            Light light = new Light();
            bool flag = Triangle.isIn(vector3d);
            if(flag)
			{
                Vector3 d = vector3d - position;
                d = d.nolrmalized();
                if( Math.Abs((d).cos(Triangle.direction)) < maxsin)
				{
                    light.r = d * r*factor;
                    light.g = d * g * factor;
                    light.b = d * b * factor;
				}
			}
            return light;
		}
	}
	public class DotLight : ILightSourse
    {
        Vector3 position;
    float factor;

    public DotLight(Vector3 p, float f,float r,float g,float b):base(r,g,b)
    {
        this.position = p;
        this.factor = f;
    }

    public List<IRayTraceObject> getIRayTrace(World world)
    {
            List<IRayTraceObject> iRayTraceObjects = world.iRayTraceObjects;
return iRayTraceObjects;
    }


        public override float getIntensity(IRayTraceObject aobject, Vector3 vector3d,Vector3 dir, bool shadow)
        {

            float f = 1;
            Vector3 d = vector3d.reduce(position).nolrmalized();
            double v = d.dot(dir);
            if(v<0)
			{
                return 0;
			}

            float k=1 ; 
            RayTraceResult self = aobject.rayTrace(position, d);
            if(self!=null)
            k = (float)v;
            if(shadow){ 
            if (self == null || (self.point-(vector3d)).simpleSize() > 0.001)
            {
                return 0;
            }
            foreach (IRayTraceObject object1 in getIRayTrace(aobject.getWorld())) {

                if (object1 != aobject)
                {
                    RayTraceResult result = object1.rayTrace(position, d);



                    if (result != null)
                    {
                        if ((self.getPoint().distance_2To(position) > result.getPoint().distance_2To(position)))
                        {
                            f *= 0.02f;
                        }

                    }

                }



            }
        }
Vector3 vector3d1 = position.reduce(vector3d);

return f * factor / (float)(vector3d1.dot(vector3d1))*k;
    }
        public override Light getIntensity(Vector3 vector3d)
        {
            Light l = new Light();
            float f = 1;
            Vector3 d = vector3d.reduce(position).nolrmalized();
            Vector3 vector3d1 = position.reduce(vector3d);
            float I = f * factor / (float)(vector3d1.dot(vector3d1));
            l.r = d * I * r;
            l.g = d * I * g;
            l.b = d * I * b;
            return l;
        }



        public override ILightSourse copy()
{
    return new DotLight(position, factor,r,g,b);
}
}

    public class Mark : IRenderObject
    {
        public Mark(Vector3 pos,int color,double r)
		{
            this.pos = pos;
            this.color = color;
            this.r = r;
		}
        public void setPosition(Vector3 pos)
		{
            this.pos = pos;
		}
        Vector3 pos;
        int color;
        double r;
        public void render(Camera camera, Zmap bitmap)
        {
            Vector2 p = camera.pointInScreen(pos);
			if (p.X >= 0 && p.Y >= 0 && p.X < camera.width && p.Y < camera.height )
			{
                bitmap.getSource()[(int)p.X, (int)p.Y] = color;
                bitmap[(int)p.X, (int)p.Y] = 0;
			}
        }

		internal Vector3 getPosition()
		{
            return pos;
		}
	}

    public class TextureCube:PhysicsObject,IRayTraceSet
	{
		public override void setPosition(Vector3 position)
		{
            Vector3 dr = position - this.position;
            p1.move(dr);

            p2.move(dr);
            p3.move(dr);
            p4.move(dr);
            p5.move(dr);
            p6.move(dr);
            this.position = position;
		}
		public void setTexture(bitmap texture)
		{
            p1.setTexture(texture);
            p2.setTexture(texture);
            p3.setTexture(texture);
            p4.setTexture(texture);
            p5.setTexture(texture);
            p6.setTexture(texture);
        }
        public void setTexture(bitmap texture, bitmap texture2, bitmap texture3, bitmap texture4, bitmap texture5,bitmap texture6)
        {
            p1.setTexture(texture);
            p2.setTexture(texture2);
            p3.setTexture(texture3);
            p4.setTexture(texture4);
            p5.setTexture(texture5);
            p6.setTexture(texture6);
        }
        public TextureCube(World w, double a) : base(w)
        {
           inti(a, a, a);
        }
            public TextureCube (World w, double a, double b, double c) :base(w)
		{
            inti(a, b, c);
        }

        void inti(double a, double b, double c)
		{
this.a = a;
            this.b = b;
            this.c = c;

            double r = a / 2;
            double r2 = b / 2;
            double r3 = c / 2;

            Vector3 v1 = position + new Vector3(-r, -r2, -r3);
            Vector3 v2 = position + new Vector3(r, -r2, -r3);
            Vector3 v3 = position + new Vector3(-r, r2, -r3);
            Vector3 v4 = position + new Vector3(r, r2, -r3);
            Vector3 v5 = position + new Vector3(-r, -r2, r3);
            Vector3 v6 = position + new Vector3(r, -r2, r3);
            Vector3 v7= position + new Vector3(-r, r2, r3);
            Vector3 v8 = position + new Vector3(r, r2, r3);
            p1 = new Paper(world, v1, v2, v3, v4, null);
            p2 = new Paper(world, v6, v5, v8, v7, null);
            p3 = new Paper(world, v1, v2, v5, v6, null);
            p4 = new Paper(world,  v7, v8,v3, v4, null);
            p5 = new Paper(world, v3, v1, v7, v5, null);
            p6 = new Paper(world, v8, v6,v4, v2,  null);
            set = new
                IRayTraceObject[] { p1.t1, p2.t1, p3.t1, p4.t1, p5.t1, p6.t1, p1.t2, p2.t2, p3.t2, p4.t2, p5.t2, p6.t2 };
		}

        double a;
        double b;
        double c;

       public Paper p1;
       public Paper p2;
       public Paper p3;
       public Paper p4;
       public Paper p5;
       public Paper p6;
        IRayTraceObject[] set;

	public	IRayTraceObject[] getObjects()
		{
            return set;
		}
	}
    public class EdgesDisplayer : IRenderObject
	{
        int color;

        bool enabled = true;

        public void enable(bool enabled)
		{
            this.enabled = enabled;
		}
        public EdgesDisplayer(int color)
		{
            this.color = color;
		}
        public void render(Camera camera, Zmap zmap)
        {
            bitmap bitmap = zmap.getSource();
            if(enabled)
			{
List<IRayTraceObject> renderObjects = camera.GetWorld().iRayTrace();
            foreach(IRayTraceObject rayTrace in renderObjects)
			{
                if(rayTrace is Triangle)
				{
                    Triangle triangle = (Triangle)rayTrace;
                    Vector2 p1 = camera.pointInScreen(triangle.p1);
                    Vector2 p2= camera.pointInScreen(triangle.p2);
                    Vector2 p3 = camera.pointInScreen(triangle.p3);
                        bool b1 = (triangle.p1 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                        bool b2 = (triangle.p2 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                        bool b3 = (triangle.p3 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                        if(b1&&b2)
                        bitmap.drawline(p1, p2, color);
                        if(b2&&b3)
                    bitmap.drawline(p2, p3, color);
                        if(b1&&b3)
                    bitmap.drawline(p3, p1, color);
                }
                else if(rayTrace is Ball)
					{
                        Ball ball = (Ball)rayTrace;
                        ball.renderEdge(bitmap, camera, color, 200);
					}
			}
			}
            
        }
    }
    public class Lens : PhysicsObject, IRayTraceObject
    {
        Optic3D.SphericalSurface surface;
        int count;
        
        public Lens(SphericalSurface surface,World world,int c,int count=10):base(world)
        {
            color = c;
            this.count = count;
            this.surface = surface;
          
            update();
            
        }
        int color;
        Vector3 dir;
        Vector3 x;
        Vector3 y;
        List<List<Vector3>> list=new List<List<Vector3>>();
        
        public double getPos(double r)
		{
            
            return surface.cr-Math.Sqrt( surface.cr*surface.cr-r*r);
		}
        public void update()
		{
            list.Clear();
            dir = surface.position - surface.center;
            dir = -dir.nolrmalized();
            if(dir.y==0&&dir.z==0)
			{
                x = (dir + new Vector3(0, 1, 0)).nolrmalized().cross(dir).nolrmalized();
                y = x.cross(dir);
			}
            else
			{
                x = (dir + new Vector3(1, 0, 0)).nolrmalized().cross(dir).nolrmalized();
                y = x.cross(dir);
            }


            double dt = Math.PI * 2 / count;
            double tpi = Math.PI * 2;
            double t = 0;
            double r0 = surface.hr;
            double dr = r0 / count;
            double r00 = r0; ;
            Vector3? fe=null;
           
            List<double> rs = new List<double>();
            List<double> ds = new List<double>();
            List<Vector3> edge=new List<Vector3>();
            list.Add(edge);

            List<Vector3> templ;
            Vector3 temp; double sin ;
                double cos;
                double l;
            for(int i = 0; i < count; i++)
			{
                rs.Add(getPos(r00));
                ds.Add(r00);
                    r00 -= dr;
			}
            for(;t<tpi;t+=dt)
			{
                Vector3? first_= null;
              sin = Math.Sin(t);
               cos = Math.Cos(t);
                templ = new List<Vector3>();
                for (int i=0;i<count;i++)
				{
                    l = ds[i];
                    Vector3 v = dir * rs[i]+sin*x*l+cos*y*l+surface.position;
                    if (first_==null)
					{
                        first_ = v;
                        edge.Add(v);
                        if(fe==null)
						{
                            fe = first_;
						}
					}
                    templ.Add(v);

				}
                list.Add(templ);
			}
            if(fe!=null)
			{
                edge.Add(fe.Value);
			}
		}
        public void renderLines(Camera camera, Zmap bitmap,ICollection<Vector3> Vectors)
        {
            Vector2? last = null;
            Vector3 lv = Vector3.Zero;
            Vector2 v2;



            foreach (Vector3 v in Vectors)
            {
                if (last == null)
                {
                    last = camera.pointInScreen(v);
                    lv = v;
                }
                else
                {
                    v2 = camera.pointInScreen(v);
                    bool b1 = (lv - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                    bool b2 = (v - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                    if (b1 && b2)
                    {
                        bitmap.getSource().drawline(v2, last.Value, color);
                    }

                    last = v2;
                    lv = v;
                }
            }
        }

        public void render(Camera camera, Zmap bitmap)
        {
            
                foreach(List<Vector3> list in list)
			{
                renderLines(camera, bitmap, list);
			}
            
        }

	

        int minx = 0;
        int maxx = 0;
        int miny = 0;
        int maxy = 0;

        public void prepareForCamera(Camera raySource)
        {
            double r = this.surface.cr;
            Vector3[] surface = new Vector3[]
                    {
                       this.surface.center .add(new Vector3(r,r,r)),
                       this.surface.center .add(new Vector3(-r,-r,-r)),
                      this.surface.center .add(new Vector3(r,-r,r)),
                       this.surface.center.add(new Vector3(r,-r,-r)),
                         this.surface.center .add(new Vector3(r,r,-r)),
                         this.surface.center .add(new Vector3(-r,r,r)),
                         this.surface.center .add(new Vector3(-r,r,-r)),
                        this.surface.center .add(new Vector3(-r,-r,r)),
                    };
            for (int i = 0; i < surface.Length; i++)
            {
                Vector2 p = raySource.pointInScreen(surface[i]);
                if (i == 0)
                {
                    maxx = (int)p.X;
                    maxy = (int)p.Y;
                    minx = (int)p.X;
                    miny = (int)p.Y;
                }
                else
                {
                    if (p.X > maxx)
                        maxx = (int)p.X;
                    if (p.Y > maxy)
                        maxy = (int)p.Y;
                    if (p.X < minx)
                        minx = (int)p.X;
                    if (p.Y < miny)
                        miny = (int)p.Y;
                }
            }
        }

        public RayTraceResult rayTrace(Vector3 position, Vector3 direction)
		{
            double d=0;
            Vector3? v=surface.getRayTrace(position, direction,ref d);
            if(v!=null)
			{ Vector3 dir = (v.Value - surface.center).nolrmalized();
                if(dir.dot
                   (direction)<0)
				{
                    dir = -dir;
				}
                return new RayTraceResult(this, v.Value,dir, color, d);
			}
			return null;
		}

		public Vector3 getPosition()
		{
			return surface.position;
		}

        public PhysicsObject getObject()
        {
            return null;
        }

        public RayTraceResult rayTrace(Vector3 position, Vector3[,] dirs, int x, int y)
        {
            if (x >= minx && x <= maxx && y >= miny && y <= maxy)
            {
                return rayTrace(position, dirs[x, y]);
            }
            return null;
        }

        public void render(Zmap bitmap, Camera camera)
		{
			this.render(camera,bitmap);
		}

		public IRayTraceObject Copy()
		{
			return new Lens(surface,world,color,count);
		}
	}
    public class Lines : IRenderObject
	{
        public Lines(int c)
		{
            color = c;
		}
        int color;
        List<Vector3> Vectors = new List<Vector3>();

        public void add(Vector3 vector3)
		{
            Vectors.Add(vector3);
		}
		public void render(Camera camera, Zmap bitmap)
		{
            Vector2? last=null;
            Vector3 lv=Vector3.Zero;
            Vector2 v2;
			foreach(Vector3 v in Vectors)
			{
                if(last==null)
				{
                    last = camera.pointInScreen(v);
                    lv = v;
				}
                else
				{
                    v2=camera.pointInScreen(v);
                    bool b1 = (lv - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                    bool b2 = (v - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                    if(b1&& b2)
					{
                bitmap.getSource().drawline(v2,last.Value,color);
					}
                    
                    last =v2;
                    lv = v;
                }
			}
		}
	}

	public class Marks : IRenderObject
	{
        List<Mark> marks=new List<Mark>();

        public void clean()
		{
            marks.Clear();
		}
        public void add(Mark mark)
		{
            marks.Add(mark);
		}
        public void add(Vector3 pos, int color=0xffffff,double r=1)
		{
            marks.Add(new Mark(pos,color,r));
		}
		public void render(Camera camera, Zmap bitmap)
		{
			foreach(Mark mark in marks)
			{
                mark.render(camera, bitmap);
			}
		}
        public Vector3[] GetVector3s()
		{
            Vector3[] vs = new Vector3[marks.Count];
            int i = 0;
            foreach(var v in marks)
			{
                vs[i] = v.getPosition();
                i++;
			}
            return vs;

		}
	}
	public class BackgroundLight : ILightSourse
    {
        Vector3 dir;

        float factor;
        Light light;

        public void setFactor(float factor)
		{
            this.factor = factor;

            light = new Light();
            light.r = r * dir * factor;
            light.g = g * dir * factor;
            light.b = b * dir * factor;
        }
    public BackgroundLight(float f,float r,float g,float b,Vector3 dir):base(r,g,b)
    {
            this.dir = dir.nolrmalized();
        this.factor = f;
          light = new Light(); 
            light.r = r * dir*factor;
            light.g = g * dir*factor;
            light.b=  b * dir*factor;
    }


    public override float  getIntensity(IRayTraceObject aobject, Vector3 vector3d,Vector3 dir,bool shadow)
        {
            RayTraceResult self = aobject.rayTrace(vector3d-dir, dir);
            float k = 1;
            if(self!=null)
			{
             k = (float)Math.Abs(self.direction.dot(dir));
			}
       
            return factor*k;
    }
        public override Light getIntensity( Vector3 vector3d)
        {
            return light;
        }

        public override ILightSourse copy()
    {
        BackgroundLight backgroundLight = new BackgroundLight(factor,r,g,b,dir);
        return backgroundLight;
    }
}
	public class RenderObjects : PhysicsObject, IRayTraceSet
	{
        List<IRayTraceObject> objects;
        public void add(IRayTraceObject o
      )
		{
            objects.Add(o);
		}
        public RenderObjects(World world):base(world)
		{

		}
		IRayTraceObject[] IRayTraceSet.getObjects()
		{
			return objects.ToArray();
		}
	}
	public class TextureTriangle : Triangle, IRayTraceObject
    {
        PhysicsObject Object;

        World world;
        bitmap texture;
        bool part1 = true;
        Triangle2D triangle2D;
        Vector3 x_axis;
        double width;
        double height;
        Vector3 y_axis;
        Vector2 x_2d;
        Vector2 y_2d;
        double w_2d;
        double h_2d;

        public PhysicsObject getObject()
		{
            return Object;
		}
        public void setObject(PhysicsObject physics)
		{
            Object=physics;
		}
        public void setTexture(bitmap b)
		{
            texture = b;
		}

        public TextureTriangle(Vector3 p1, Vector3 p2, Vector3 p3, bitmap texture, World world,bool part1) : base(p1, p2, p3)
        {
            this.world = world;
            this.part1 = part1;
            this.texture = texture;
            if(part1)
			{
                x_axis = (p2 - p1);
                width = x_axis.length();
                x_axis= x_axis.nolrmalized();
                y_axis = (p3 - p1);
                height = y_axis.length();
                y_axis = y_axis.nolrmalized();
            }
            else
			{
                x_axis = (p3-p2);
                width = x_axis.length();
                x_axis = x_axis.nolrmalized();
                y_axis = (p3 - p1);
                height = y_axis.length();
                y_axis = y_axis.nolrmalized();
            }

        }

		public override void setPos(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			base.setPos(p1, p2, p3);
            if (part1)
            {
                x_axis = (p2 - p1);
                width = x_axis.length();
                x_axis = x_axis.nolrmalized();
                y_axis = (p3 - p1);
                height = y_axis.length();
                y_axis = y_axis.nolrmalized();
            }
            else
            {
                x_axis = (p3 - p2);
                width = x_axis.length();
                x_axis = x_axis.nolrmalized();
                y_axis = (p3 - p1);
                height = y_axis.length();
                y_axis = y_axis.nolrmalized();
            }
        }
		public World getWorld()
        {
            return world;
        }
        public int minx = 0;
        public int maxx = 0;
        public int miny = 0;
        public int maxy = 0;
        public int color = 0xffffff;
        public void prepareForCamera(Camera camera)
        {
            Vector2 P1 = camera.pointInScreen(getP1());
            Vector2 P2 = camera.pointInScreen(getP2());
            Vector2 P3 = camera.pointInScreen(getP3());
            triangle2D = new Triangle2D(P1, P2, P3);
            minx = (int)(Math.Min(Math.Min(P1.X, P2.X), P3.X));
            miny = (int)(Math.Min(Math.Min(P1.Y, P2.Y), P3.Y));
            maxx = (int)(Math.Max(Math.Max(P1.X, P2.X), P3.X));
            maxy = (int)(Math.Max(Math.Max(P1.Y, P2.Y), P3.Y));
           if(!camera.isInFov(getP1())|| !camera.isInFov(getP2())||! camera.isInFov(getP2()))
			{
                minx = miny = 0;
                maxx = camera.width;
                maxy = camera.height;
			}
            if (part1)
            {
                x_2d = (P2 - P1);
                w_2d = x_2d.value();
                x_2d = x_2d.nolrmalized();
                y_2d = (P3 - P1);
                h_2d = y_2d.value();
                y_2d = y_2d.nolrmalized();
            }
            else
            {
                x_2d = (P3 - P2);
                w_2d= x_2d.value();
                x_2d = x_2d.nolrmalized();
                y_2d = (P3 - P1);
                h_2d = y_2d.value();
                y_2d = y_2d.nolrmalized();
            }
        }
        public bool fixTexture = false;

        
        public int getColor(Vector3 vector3)
		{
            if(texture==null)
			{
                return color;
			}
            double x, y;
           if (part1)
			{
                Vector3 pos = vector3 - p1;double[] vs;
                if(fixTexture)
				{
                    vs=pos.split_M(x_axis,y_axis);
				}
                else
				{
                     vs= pos.split(x_axis, y_axis);
				}
                 
              x = vs[0]/width;
              y = vs[1]/height;

			}
           else
			{
                Vector3 pos = vector3 - p3;
                double[] vs;
                if (fixTexture)
                {
                    vs = pos.split_M(x_axis, y_axis);
                }
                else
                {
                    vs = pos.split(x_axis, y_axis);
                }
                x = vs[0] / width+1;
                y = vs[1] / height+1;
            }
           if(x>=0&&x<1&&y>=0&&y<1)
			{
                return texture.GetColor((int)(texture.Width * x), (int)(texture.Height * y));
			}

            return 0;

		}
        public RayTraceResult rayTrace(Vector3 position, Vector3 direction)
        {
            RayTraceResult result = null;
            double s=0;
            Vector3? p = getCrossPoint(position, direction,ref s);
            if (p != null)
            {
                Vector3 p_ = (Vector3)p;
                int c = getColor(p_);
                if ((c >> 24 & 255) == 255)
				{
                    Vector3 di = this.direction;
                    if(di.dot(direction)<0)
					{
                        di = -di;
					}
                result = new RayTraceResult(this,p_, di,c, s * direction.length()); 
				}
                    
            }
            return result;
        }


        public Vector3 getPosition()
        {
            return position;
        }
       
        public IRayTraceObject Copy()
        {

            return (IRayTraceObject)copy();
        }
        public override Triangle copy()
        {
            TextureTriangle t = new TextureTriangle(getP1(), getP2(), getP3(), texture, world,part1);
            t.maxx = maxx;
            t.maxy = maxy;
            t.minx = minx;
            t.miny = miny;
            t.position = position;
            t.direction = direction;
            if (triangle2D != null)
            {
                t.triangle2D = triangle2D.copy();
            }
            return t;
        }

        public static int times = 0;
        public RayTraceResult rayTrace(Vector3 position, Vector3[,] dirs, int x, int y)
        {
            if (x >= minx && x <= maxx && y >= miny && y <= maxy)
            {
                RayTraceResult r=rayTrace(position, dirs[x, y]);
                if(r!=null)
                times++;
                return r;
            }
            return null;
        }

        public static double renderConst = 8;
        public void render(Zmap zmap, Camera camera)
        {
          prepareForCamera(camera);
            bitmap bitmap = zmap.getSource();
            
            if (w_2d * h_2d *renderConst>= camera.width * camera.height &&texture!=null)
            {
              
                return;
			}
                
            if(texture==null&&!camera.renderNoTexture)
			{
                Light light = camera.world.getLightIntensity(getPosition());
              

                bool b1 = (p1 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                bool b2 = (p2 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;
                bool b3 = (p3 - camera.getPosition()).nolrmalized().dot(camera.getDirection()) > 0;

                int c_ = Helper.mixColor(color, new Vector3(Math.Abs(light.r.dot(direction)), Math.Abs(light.g.dot(direction)), Math.Abs(light.b.dot(direction))));


                if (b1 && b2)
                    bitmap.drawline(triangle2D. p1, triangle2D.p2,c_,camera.maxLineLength);
                if (b2 && b3)
                    bitmap.drawline(triangle2D.p2, triangle2D.p3,c_, camera.maxLineLength);
                if (b1 && b3)
                    bitmap.drawline(triangle2D.p3, triangle2D.p1, c_, camera.maxLineLength);
                return;
                
			}
           
            
           
            

            if ((position - camera.getPosition()).nolrmalized().dot(camera.getDirection()) <= Math.Cos(camera.getFov()/2))
            {
                
                return;
			}
                bool noTexture = texture == null;
            if(noTexture)
			{
            texture = new bitmap(1, 1);
                texture.paint(color);
			}

            
            int q0 = camera.qualityReduce;
            int qt = 0;
                double maxX = w_2d;
                double maxY = h_2d;
                double dmy = h_2d/ w_2d;
            int c0=-1;
          Light l;
            int c=0;
			Vector3 p0_3d = p1.copy();
                Vector3 p_3d;
            bool hasColor = false;
                Vector3 dx = width / w_2d * x_axis;
                Vector3 dy= height / h_2d * y_axis;
                Vector2 p0 = new Vector2(triangle2D.p1.X,triangle2D.p1.Y); 
            if(!part1)
			{
                p0_3d = p3.copy();
                p0 = new Vector2(triangle2D.p3.X, triangle2D.p3.Y);
            }
                Vector2 p;
                double z=0;
                double dx_i =  texture.Width/(double)w_2d*0.999;
                double dy_i= texture.Height/(double)h_2d*0.999;
                double tx = 0;
                double ty;
            if(!part1)
			{
                dx_i *= -1;
                dy_i *= -1;
                tx = texture.Width - 1;
			}

         

                for(int x=0;x<maxX;x++)
				{
                    p = p0;
                    p_3d = p0_3d;
                    ty = 0;
                if(!part1)
				{
                    ty = texture.Height - 1;
				}
                    for(int y=0;y<maxY;y++)
					{
                    if(qt>0)
					{
                        qt--;
					}
                    bool update = qt == 0;


                    int X = (int)Math.Round(p.X);
                    int Y = (int)Math.Round(p.Y);
                    if(X>=0&&Y>=0&&X<camera.width&&Y<camera.height)
					{
                     if(update)
						{
                        z = camera.position.distance_2To(p_3d);
						}
                   
						if (zmap[X,Y]>z)
						{
                            
                    if(update)
							{
                                
                                
                                 c0 = texture[(int)tx, (int)ty];
								
                 
                                hasColor = (c0 >>24&255)==255;              
                    if(hasColor)
					{
                    l  = camera.world.getLightIntensity(p_3d);
                    c = Helper.mixColor(c0,new Vector3(Math.Abs(l.r.dot(direction)), Math.Abs(l.g.dot(direction)), Math.Abs(l.b.dot(direction)) ) );  
					
					}

                   		}
                    if(hasColor)
							{
                     zmap[X, Y] = z;
                    zmap.getSource()[X,Y] = c;
							}
                    
                           
						}

                    
                    
					}


                   
					
                         if(part1)
					{
                        p += y_2d;
                        p_3d += dy;
					}
                    else
					{
                        p -= y_2d;
                        p_3d -= dy;
                    }
                        ty += dy_i;
                    if(update)
					{
                        qt = q0;
					}
					}
                    maxY -= dmy;
                if(part1)
				{
                    p0 += x_2d;
                    p0_3d += dx;
				}
                else
				{
                    p0 -= x_2d;
                    p0_3d -=dx;
                }
                   
                    tx += dx_i;
                    
				}
			if(noTexture)
			{
                texture = null;
			}

        }
    }

public class Ball : PhysicsObject,IRayTraceObject
    {

    double r;

    int color;

    public Ball(World world, double r, int color):base(world)
    {
    
        this.r = r;
        this.color = color;
        addPart(new BallCollider(this, r));
    }

    public int getColor()
    {
        return color;
    }

    public void setColor(int color)
    {
        this.color = color;
    }

    public double getR()
    {
        return r;
    }

    public void setR(double r)
    {
        parts.Remove(getCollider());
        this.r = r;
        addPart(new BallCollider(this, r));
    }


    public override void setPosition(Vector3 position)
    {
        Vector3 dp = position.reduce(this.position);
        this.position = position;
    }

    public override void  tick(double dt)
    {
        setPosition(getPosition().add(getVelocity().scale(dt)));
    }

    int minx = 0;
    int maxx = 0;
    int miny = 0;
    int maxy = 0;

    public void prepareForCamera(Camera raySource)
    {
        Vector3[] surface = new Vector3[]
                {
                        position.add(new Vector3(r,r,r)),
                        position.add(new Vector3(-r,-r,-r)),
                        position.add(new Vector3(r,-r,r)),
                        position.add(new Vector3(r,-r,-r)),
                        position.add(new Vector3(r,r,-r)),
                        position.add(new Vector3(-r,r,r)),
                        position.add(new Vector3(-r,r,-r)),
                        position.add(new Vector3(-r,-r,r)),
                };
        for (int i = 0; i < surface.Length; i++)
        {
            Vector2 p = raySource.pointInScreen(surface[i]);
            if (i == 0)
            {
                maxx = (int)p.X;
                maxy = (int)p.Y;
                minx = (int)p.X;
                miny = (int)p.Y;
            }
            else
            {
                if (p.X > maxx)
                    maxx = (int)p.X;
                if (p.Y > maxy)
                    maxy = (int)p.Y;
                if (p.X < minx)
                    minx = (int)p.X;
                if (p.Y < miny)
                    miny = (int)p.Y;
            }
        }
    }






    public RayTraceResult rayTrace(Vector3 p, Vector3 d)
    {
        Vector3 dp = p.reduce(position);
        double a = d.x * d.x + d.y * d.y + d.z * d.z;
        double b = 2 * d.x * dp.x + 2 * d.y * dp.y + 2 * d.z * dp.z;
        double c = dp.x * dp.x + dp.y * dp.y + dp.z * dp.z;
        DistanceEqution de = new DistanceEqution(a, b, c);
        double s = de.minSolution(r * r);
        if (s >= 0)
        {
                Vector3 pos = p.add(d.scale(s));
                Vector3 dir = (pos - position).nolrmalized();
                if(d.dot(dir)<0)
				{
                    dir = -dir;
				}
                return new RayTraceResult(this, pos, dir, color, s * d.length()) ;
        }
        return null;
    }


    public RayTraceResult rayTrace(Vector3 position,Vector3[,] dirs, int x, int y)
    {
        if (x >= minx && x <= maxx && y >= miny && y <= maxy)
        {
            return rayTrace(position, dirs[x,y]);
        }
        return null;
    }

        public void renderEdge(bitmap bitmap, Camera camera,int color,int nums)
        {
            Vector3 dir = (position - camera.getPosition()).nolrmalized();
            Vector3 x_ = dir.cross(dir + new Vector3(1, 0, 0)).nolrmalized();
            Vector3 y_ = dir.cross(x_);
            double dt = 2*Math.PI/nums;
            int x;
            int y;
            for (double t=0;t<Math.PI*2;t+=dt)
			{
                Vector3 pos = position.add(x_ * Math.Cos(t) * r + y_ * Math.Sin(t) * r);
                Vector2 p = camera.pointInScreen(pos);
               x = (int)p.X;
                y = (int)p.Y;
                if (x >= 0 && x <=bitmap.Width && y >= 0 && y <= bitmap.Height)
                {
                    bitmap.SetColor(x, y, color);
                }
                    
			}



        }
        public void render(Zmap bitmap, Camera camera)
    {

    }

        public IRayTraceObject Copy()
		{
            return (IRayTraceObject)copy();
		}

    public override PhysicsObject copy()
    {
        Ball c = new Ball(world, r, color);
        c.maxy = maxy;
        c.miny = miny;
        c.maxx = maxx;
        c.minx = minx;
        c.position = position.copy();
        c.velocity = velocity.copy();
        return c;
    }

		public PhysicsObject getObject()
		{
			return this;
		}
	}

}
