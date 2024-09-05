using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using NewPhysics;
using Images;
namespace NewPhysics
{
    
    public interface IInteractPart : IPart
    {

    void interact(PhysicsObject Object, double dt);
    }
    public abstract  class AbstractCollider : IInteractPart
    {
        public AbstractCollider(PhysicsObject @object)
		{
            owner = @object;
		}
       internal PhysicsObject owner;
       internal List<PhysicsObject> objects = new List<PhysicsObject>();




   
    public  void onTickEnd(double dt)
    {
        objects.Clear();
    }

    public abstract CollideResult getCollide(PhysicsObject Object, double dt);

    public void interact(PhysicsObject Object, double dt)
    {

        if (!objects.Contains(Object))
        {
            objects.Add(Object);
            if (Object.getCollider() != null)
            {
                Object.getCollider().objects.Add(owner);
                CollideResult result = getCollide(Object, dt);
                if (result != null)
                {
                    owner.setPosition(result.p1);
                    owner.setVelocity(result.v1);
                    Object.setVelocity(result.v2);
                    Object.setPosition(result.p2);
                }
            }

        }

    }

  
    public PhysicsObject getOwner()
    {
        return owner;
    }

   
    public void setOwner(PhysicsObject owner)
    {
        this.owner = owner;
    }

   
    public void tick(double dt)
    {
    }

	

	
}



    public abstract class AbstractForce : IInteractPart
    {
        PhysicsObject owner;
        List<PhysicsObject> objects = new List<PhysicsObject>();

    public AbstractForce(PhysicsObject owner)
    {
        this.owner = owner;
    }

  
    public void onTickEnd(double dt)
    {
        objects.Clear();
    }

    public abstract Vector3 getForce(PhysicsObject aobject, double dt);


    public void interact(PhysicsObject aobject, double dt)
    {

        if (!objects.Contains(aobject))
        {
            objects.Add(aobject);
            if (aobject.getCollider() != null)
            {
                aobject.getCollider().objects.Add(owner);
                Vector3 f = getForce(aobject, dt);
               aobject.setVelocity(aobject.getVelocity().add(f.scale(dt / aobject.mass)));
                owner.setVelocity(owner.getVelocity().add(f.scale(-dt / owner.mass)));
            }

        }

    }

 
    public PhysicsObject getOwner()
    {
        return owner;
    }


    public void setOwner(PhysicsObject owner)
    {
        this.owner = owner;
    }


    public void tick(double dt)
    {
    }

  
}

public class CollideResult
    {
       public  Vector3 p1;
        public Vector3 v1;
        public Vector3 p2;
        public Vector3 v2;
    }
public abstract class ILightSourse : ICopyObject<ILightSourse> {

        internal  float r;
        internal float g;
        internal float b;

        public void setColor(float R,float G,float B)
		{
            r = R;
            g = G;
            b = B;
		}
        public ILightSourse(float R,float G,float B)
		{
            r = R;
            g = G;
            b = B;
		}
   abstract public float getIntensity(IRayTraceObject Object, Vector3 vector3d,Vector3 dir,bool shadow);
        abstract public Light getIntensity( Vector3 vector3d);
        abstract public ILightSourse copy();
}
public interface ICopyObject<T>
	{
		T copy();
	}
    public interface IWorldPart
    {
        World getOwner();

        void setOwner(World owner);

        void interact(PhysicsObject Object, double dt);
    }
    public interface IRenderObject
    {


        void render(Camera camera, Zmap bitmap);
    }
    public interface IRayTraceSet
    {
        IRayTraceObject[] getObjects();


	
    }
    public interface IPart
	{


		PhysicsObject getOwner();

		void setOwner(PhysicsObject owner);

		void tick(double dt);

		void onTickEnd(double dt);

	}
public  class RayTraceResult
        {
          public readonly  IRayTraceObject Object;
        public readonly Vector3 point;
        public readonly int color;
        public readonly Vector3 direction;
        public readonly double distance;

            public RayTraceResult(IRayTraceObject Object, Vector3 point, Vector3 direction, int color, double distance)
		{
			this.Object = Object;
			this.point = point;
			this.color = color;
			this.direction = direction;
			this.distance = distance;
		}


		public int getColor()
            {
                return color;
            }

            public IRayTraceObject getObject()
            {
                return Object;
            }


        public Vector3 geDirection()
        {
            return direction;
        }
        public Vector3 getPoint()
            {
                return point;
            }
        }
    public interface IRayTraceObject
    {
        World getWorld();


        void prepareForCamera(Camera raySource);
        RayTraceResult rayTrace(Vector3 position, Vector3 direction);
        Vector3 getPosition();
         PhysicsObject getObject();
        RayTraceResult rayTrace(Vector3 position,Vector3[,] dirs, int x, int y);
        void render(Zmap bitmap, Camera camera);


        IRayTraceObject Copy();

       
    }
}
