using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using DVOSLib;
using Images;
using NewPhysics;

namespace NewPhysics
{
    public class BallCollider : AbstractCollider
    {

   internal double r;

    public BallCollider(PhysicsObject owner, double r):base(owner)
    {
       
        this.r = r;
    }


    public DistanceEqution getDistanceEqution(PhysicsObject o2)
    {
        Vector3 d1 = owner.getPosition(), d2 = o2.getPosition(), v1 = owner.getVelocity(), v2 = o2.getVelocity();
        double a, b, c;
        a = (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
        b = 2 * (v1.x - v2.x) * (d1.x - d2.x) + 2 * (v1.y - v2.y) * (d1.y - d2.y) + 2 * (v1.z - v2.z) * (d1.z - d2.z);
        c = (d1.x - d2.x) * (d1.x - d2.x) + (d1.y - d2.y) * (d1.y - d2.y) + (d1.z - d2.z) * (d1.z - d2.z);
        return new DistanceEqution(a, b, c);

    }


    public override CollideResult getCollide(PhysicsObject aobject, double dt)
    {

        if (aobject.collider is BallCollider) {
            BallCollider b2 = (BallCollider)aobject.collider;
            double[] DAT = getDistanceEqution(aobject).min(0, dt);
            if (DAT[1] < (r + b2.r) * (r + b2.r) && (owner.velocity.reduce(aobject.velocity)).dot(owner.position.reduce(aobject.position)) < 0)
            {
                CollideResult result = new CollideResult();
                Vector3 direction = (owner.position.reduce(aobject.position)).nolrmalized();
                double v1 = owner.velocity.dot(direction), v2 = aobject.getVelocity().dot(direction),
                        m1 = owner.mass,
                        m2 = aobject.mass;
                double v21 = (2 * m1 * v1 + m2 * v2 - m1 * v2) / (m1 + m2), v11 = (2 * m2 * v2 + m1 * v1 - m2 * v1) / (m1 + m2);

                v11 = v11 - direction.dot(owner.velocity);
                v21 = v21 - direction.dot(aobject.velocity);

                result.p1 = owner.position;
                result.v1 = owner.getVelocity().add(direction.scale(v11));
                result.p2 = aobject.position;
                result.v2 = aobject.getVelocity().add(direction.scale(v21));
                return result;
            }

        }

        return null;
    }
}
public class Camera : PhysicsObject,ICopyObject<Camera>{

  public bool renderNoTexture = false;
  public readonly int width;
    public readonly int height;
    double fov;
    double focalLength;
        public int qualityReduce = 0;
    Vector3 direction;
   public Vector3 x_axis { get; private set; }
        public Vector3 y_axis { get; private set; }
        Vector3 x_axis_normal;
    Vector3 y_axis_normal;
    Vector3 cmosCenter;
    double pixelDensity;
    bitmap last;
    Vector3[,] dirs;
        double Theta=0;
        PhysicsObject target;
        bool rayTrace_ = true;
        public double maxLineLength=100000;
        public void enableRayTrace(bool rayTrace)
		{
            this.rayTrace_ = rayTrace;
		}
        public static bool forceStopUptateDirs = false;

        public Vector3 getDirection()
		{
            return direction;
		}
    public Camera(World world, int width, int height, double fov, double focalLength, Vector3 position, Vector3 direction):base(world)
    {
      
        this.width = width;
        this.height = height;
        this.fov = fov*Math.PI/180;
        this.focalLength = focalLength;
        this.position = position;
        this.direction = direction.nolrmalized();
        dirs = new Vector3[width,height];
            maxLineLength = Math.Sqrt(width * width + height * height) * 5;
        updateDirs();
            setFreeze(true);
    }
        public double getFov()
		{
            return fov;
		}
        public void setTarget(PhysicsObject physicsObject)
		{
            this.target = physicsObject;
		}
		public override void tick(double dt)
		{
			base.tick(dt);
            if(target!=null)
			{
                setDirection(target.getPosition() - position);
			}
		}
		public void updateDirs()
    {
            
            

        Vector2 vector2 = new Vector2(direction.x, direction.z).row(90).nolrmalized();
       Vector3 x_axis_ = new Vector3(vector2.X, 0, vector2.Y);
       Vector3 y_axis_ = x_axis_.cross(direction);
            double sin = Math.Sin(Theta);
            double cos = Math.Cos(Theta);
            x_axis = x_axis_ *cos + y_axis_ *sin;
            y_axis = x_axis.cross(direction);
            pixelDensity = Math.Sqrt(width * width + height * height) / Math.Tan(fov / 2) / focalLength / 2;
        x_axis_normal = x_axis.scale(1 / pixelDensity);
        y_axis_normal = y_axis.scale(1 / pixelDensity);
        cmosCenter = this.position.reduce(direction.scale(focalLength));
if (forceStopUptateDirs)
                return;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dirs[x,y] = getScreenDirection(x, y);
            }
        }
    }

    public bool isInFov(Vector3 p)
    {
        Vector3 d = p.reduce(position);
        double a = d.dot(direction) / p.length();
        return a >= 0 && a < Math.Cos(fov / 2);
    }

        public override void  setDirection(Vector3 direction)
		{
            this.direction = direction.nolrmalized();
            updateDirs();
        }
    public void setDirection(Vector3 direction,double theta)
    {
        this.direction = direction.nolrmalized();
            this.Theta = theta;
        updateDirs();
    }

    bool drawShadow = true;

    public void rayTrace(bitmap bitmap,ref int n_)
    {
      
        List<IRayTraceObject> objects = world.iRayTrace();
        foreach (IRayTraceObject Object in objects)
        {
            Object.prepareForCamera(this);
        }
              for (int y = 0; y < height; y++)
                {
            for (int x = 0; x < width; x++)
            {

              
                    double d = double.PositiveInfinity;
                    RayTraceResult final = null;
                    RayTraceResult[] results = new RayTraceResult[(objects.Count)];
                    int n = objects.Count;
                    for (int i = 0; i < n; i++)
                    {
                        results[i] = objects[i].rayTrace(position, dirs, x, y);
                    }



                    foreach (RayTraceResult result in results)
                    {

                        if (result != null)
                        {
                            double dd = result.point.distance_2To(position);
                            if (final == null || dd <= d)
                            {
                                final = result;
                                d = dd;

                            }
                        }

                    }

                    if (final != null)
                    {
                        Vector3 c = world.getLightIntensity(final.getObject(), final.getPoint(),final.geDirection(), drawShadow);
                        bitmap.SetColor(x, y, Helper.mixColor(final.getColor(), c));

                    }
                    n_++;
                }
            
            }
                

   
    }

    public Vector3[,] getDirs()
    {
        return dirs;
    }

    public void setDrawShadow(bool drawShadow)
    {
        this.drawShadow = drawShadow;
    }

  
    public Camera copy()
    {
        Camera camera = new Camera(world, width, height, fov, focalLength, position.copy(), direction.copy());
        return camera;
    }
     

        public bitmap render_RayTrace(ref int  n)
		{
            return render_RayTrace(new Zmap(width, height),ref n);
		}

        public bitmap render()
		{
            Zmap zmap = new Zmap(width, height);
            last = zmap.getSource();
            if (world.background != null)
            {
                world.background.render(this, zmap);
            }


            world.updateIRayTrace(position,true);
            IRayTraceObject[] objects = world.iRayTrace().ToArray();
           
            int c = objects.Length;
            int[][] indexs = c.getRange().split(8);

            Parallel.ForEach(indexs, (int[] index) => { 
            foreach(int ind in index)
				{
					objects[ind].render(zmap, this);
				}
            });
            foreach (IRenderObject render in world.RenderObjects())
            {
                render.render(this,zmap);
            }
            return zmap.getSource();
		}
    public bitmap render_RayTrace(Zmap b,ref int n)
    {
            n = 0;
        last = b.getSource();
     
            if(world.background!=null)
			{
                world.background.render(this, b);
			}
        world.updateIRayTrace(position,false);
        if(rayTrace_)
			{
               rayTrace(b.getSource(),ref n);
			}
       

            foreach(IRenderObject render in world.RenderObjects())
			{
                render.render(this, b);
			}
        return b.getSource();
    }
        public World GetWorld()
		{
            return world;
		}
    public Vector3 getPosition()
    {
        return position;
    }

    
    public void setPosition(Vector3 position)
    {
        this.position = position;
        updateDirs();
    }

    public Vector3 getX_axis()
    {
        return x_axis;
    }

    public Vector3 getY_axis()
    {
        return y_axis;
    }

    public Vector3 getScreenDirection(double x, double y)
    {
        x = x - width / 2 + 0.5;
        y = y - height / 2 + 0.5;
        return (position-(x_axis_normal*x+(y_axis_normal*-y)+(cmosCenter))).nolrmalized();
    }

    public Vector3 getCmosPoint(double x, double y)
    {
        x = x - width / 2 + 0.5;
        y = y - height / 2 + 0.5;

        return x_axis_normal.scale(x).add(y_axis_normal.scale(-y)).add(cmosCenter);
    }

    public Vector3 positionInCmos(Vector3 position)
    {


        Vector3 v0 = position.reduce(this.position);
        double theta = Math.Acos(v0.dot(direction) / v0.length());
        Vector3 v = v0.add(direction.scale(-position.reduce(this.position).dot(direction)));

        double cos = v.dot(x_axis) / v.length();
        double angle = Math.Acos(cos);
        if (v.dot(y_axis) < 0)
        {
            angle *= -1;
        }
        double r = Math.Tan(theta) * focalLength;
        return cmosCenter.add(x_axis.scale(cos * r)).add(y_axis.scale(-Math.Sin(angle) * r));
    }

    public RayTraceResult getWorldPosition(double x, double y, double distance)
    {
        if (x > 0 && y > 0 && x < width && y < height)
        {
            return world.rayTrace(position, dirs[(int)x,(int)y], distance);
        }
        return world.rayTrace(position, getScreenDirection(x, y));

    }

    public Vector2 pointInScreen(Vector3 position)
    {


        Vector3 v0 = position.reduce(this.position);
        double theta = Math.Acos(v0.dot(direction) / v0.length());
        Vector3 v = v0.add(direction.scale(-v0.dot(direction)));

        double cos = v.dot(x_axis) / v.length();
        double angle = Math.Acos(cos);
        if (v.dot(y_axis) < 0)
        {
            angle *= -1;
        }
        double r = Math.Tan(theta) * focalLength;

        return new Vector2(-Math.Cos(angle), Math.Sin(angle)).scale(r * pixelDensity).add(new Vector2(width / 2, height / 2));

    }


}

    public class Light
	{
        Vector3 r_ = new Vector3(0, 0, 0);
        Vector3 g_ = new Vector3(0, 0, 0);
        Vector3 b_ = new Vector3(0, 0, 0);
       public Vector3 r{ get { return r_; } set { r_ = new Vector3(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z)); } }
       public Vector3 g { get { return g_; } set { g_ = new Vector3(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z)); } }
        public Vector3 b { get { return b_; } set { b_ = new Vector3(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z)); } }
    }
public  class World : ICopyObject<World> {

    public Background background;
        internal List<IWorldPart> worldParts = new List<IWorldPart>();
        internal List<ILightSourse> lightSourses = new List<ILightSourse>();
      internal  List<PhysicsObject> objects = new List<PhysicsObject>();
        internal List<IRayTraceObject> iRayTraceObjects = new List<IRayTraceObject>();
        internal List<IRenderObject> renders = new List<IRenderObject>();

        public PhysicsObject[] physicsObjects {
			get { return objects.ToArray(); }
        }
PhysicsObject[] getAtIndex(int ind)
{
    if (ind < objects.Count) {
    PhysicsObject[] objects_ = new PhysicsObject[objects.Count - ind];
    for (int i = 0; i < objects_.Length; i++)
    {
        objects_[i] = objects[ind +i];
    }
    return objects_;
}
return new PhysicsObject[0];
    }

        public List<IRenderObject> RenderObjects()
		{
            return renders;
		}

    public void addToWorld(object physicsObject)
{
    if (physicsObject is PhysicsObject) {
    objects.Add((PhysicsObject)physicsObject);
} else if (physicsObject is IWorldPart) {
    worldParts.Add((IWorldPart)physicsObject);
} else if (physicsObject is Background) {
    background = (Background)physicsObject;
} else if (physicsObject is ILightSourse) {
    lightSourses.Add((ILightSourse)physicsObject);
            }
            else if (physicsObject is IRenderObject)
            {
                renders.Add((IRenderObject)physicsObject);
            }

        }

        public Light getLightIntensity(Vector3 v)
        {
            Light l = new Light();
            foreach (ILightSourse lightSourse in lightSourses)
            {
                Light light= lightSourse.getIntensity(v);
               l.r += light.r ;
               l.g += light.g;
               l.b += light.b;
            }
            return l;
        }
     
        public Vector3 getLightIntensity(IRayTraceObject Object, Vector3 v,Vector3 dir,bool shadow)
{
            float r=0, g=0, b=0;
  
    foreach (ILightSourse lightSourse in lightSourses) {
                float f = lightSourse.getIntensity(Object, v,dir,shadow);
                r += lightSourse.r * f ;
                g += lightSourse.g * f;
                b += lightSourse.b * f;
            }
return new Vector3(r,g,b);
    }
    public List<IRayTraceObject> getIRayTrace(Vector3 position,bool sort=true)
{
    List<IRayTraceObject> iRayTraceObjects = new List<IRayTraceObject>();

    foreach (PhysicsObject physicsObject in objects) {
    if (physicsObject is IRayTraceObject) {
        iRayTraceObjects.Add((IRayTraceObject)physicsObject);
    }
            else if (physicsObject is IRayTraceSet)
            {
        foreach (IRayTraceObject iRayTraceObject in((IRayTraceSet)physicsObject).getObjects())
        {
            iRayTraceObjects.Add(iRayTraceObject);
        }
    }
}
    if(sort)
Helper.sort(iRayTraceObjects,
     
             delegate  (IRayTraceObject a, IRayTraceObject b) {

    return a.getPosition().distance_2To(position) < b.getPosition().distance_2To(position);
        });
return iRayTraceObjects;
    }
    public void updateIRayTrace(Vector3 position,bool sort)
{
    iRayTraceObjects = getIRayTrace(position,sort);
}

public int getDefaultColor()
{
    if (background != null)
    {
        return background.color;
    }
    return unchecked((int)0xffffffff);
}
        public RayTraceResult rayTrace(Vector3 position, Vector3 direction, IRayTraceObject[] toIngnore = null)
        {
            List<IRayTraceObject> objects = getIRayTrace(position);
            foreach (IRayTraceObject Object in objects)
            {
                if (toIngnore != null && toIngnore.Contains(Object))
                {
                    continue;
                }
                RayTraceResult result = Object.rayTrace(position, direction);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        public RayTraceResult rayTrace(Vector3 position, Vector3 direction, double maxdistance,IRayTraceObject[] toIngnore=null)
{
    direction = direction.nolrmalized();
    List<IRayTraceObject> objects = getIRayTrace(position);
            
    foreach (IRayTraceObject Object in objects)
        {
                if(toIngnore!=null&& toIngnore.Contains(Object))
				{
                    continue;
				}
   RayTraceResult result = Object.rayTrace(position, direction);
    if (result != null&&result.distance<=maxdistance)
    {
        return result;
    }
}
return new RayTraceResult(null, position.add(direction.scale(maxdistance)),direction, getDefaultColor(),maxdistance);
    }
    public List<IRayTraceObject> iRayTrace()
{
    List<IRayTraceObject> iRenderObjects = new List<IRayTraceObject>(this.iRayTraceObjects.Count);
    foreach (IRayTraceObject iRenderObject in this.iRayTraceObjects) {
                iRenderObjects.Add(iRenderObject);
}


return iRenderObjects;
    }

    public void tick(double s)
{
            List<int> toRemove = new List<int>();

    int n = 0;
    foreach (PhysicsObject o in objects
        ) {
    foreach (IWorldPart part in worldParts)
    {
        part.interact(o, s);
    }
    foreach (IPart p in o.parts
    )
    {
        p.tick(s);
    }
    foreach (PhysicsObject oo in getAtIndex(n))
    {
        foreach (IPart iPart in o.parts)
        {
            if (iPart is IInteractPart) {
        ((IInteractPart)iPart).interact(oo, s);
    }
}
foreach (IPart iPart in oo.parts)
{
    if (iPart is IInteractPart) {
    ((IInteractPart)iPart).interact(o, s);
}
                }
            }
            o.tick(s);
                if(o.remove_)
				{
                    toRemove.Add(n);
				}

                n++;
            }

        foreach (PhysicsObject o in objects
        )
{
    foreach (IPart iPart in o.parts)
    {
        iPart.onTickEnd(s);
    }
}

        foreach(int o in toRemove)
			{
                objects.RemoveAt(o);
			}

    }


    public World copy()
{
    World world = new World();
    foreach (PhysicsObject o1 in objects)
        {
    PhysicsObject Object= o1.copy();
    Object.world = world;
    world.addToWorld(Object);
}
foreach (ILightSourse lightSourse in lightSourses)
{
    ILightSourse sourse = lightSourse.copy();
    world.addToWorld(sourse);
}
if (background != null)
    world.background = background.copy();
return world;
    }
}
    
    public class PhysicsObject : ICopyObject<PhysicsObject> {

       internal World world;
  internal  double mass = 1;
    internal List<IPart> parts = new List<IPart>();
   internal Vector3 velocity = new Vector3(0, 0, 0);
   internal Vector3 position = new Vector3(0, 0, 0);
   internal AbstractCollider collider = null;
  internal  bool  freeze = false;
        internal Vector3 direction = new Vector3(0, 1, 0);
     internal   bool remove_ = false;

        
        public void remove()
		{
            remove_ = true;
		}
    public PhysicsObject(World world)
    {
        this.world = world;

    }

    public bool isFreeze()
    {
        return freeze;
    }

    public void setFreeze(bool freeze)
    {
        this.freeze = freeze;
    }

    public double getMass()
    {
        return mass;
    }

    public World getWorld()
    {
        return world;
    }

    public void setMass(double mass)
    {
        this.mass = mass;
    }

    public AbstractCollider getCollider()
    {
        return collider;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public virtual void setPosition(Vector3 position)
    {
        this.position = position;
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
    }

    public virtual void tick(double dt)
    {
            setPosition(position.add(velocity.scale(dt)));
    }

    public void addPart(IPart part)
    {
        parts.Add(part);
        if (part is AbstractCollider) {
            collider = (AbstractCollider)part;
        }
    }
        internal double theta = 0;
        public virtual void setTheta(double theta)
		{
            this.theta = theta;
		}
        public virtual void setDirection(Vector3 vector)
		{
            direction = vector.nolrmalized();
		}

        public Vector3 getDirection()
		{
            return direction;
		}
    public virtual PhysicsObject copy()
    {
        PhysicsObject p = new PhysicsObject(world);
        p.position = position.copy();
        p.velocity = velocity.copy();
        return p;
    }
}
}
