using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVOSLib;
using MathBase;
using Images;

namespace NewPhysics
{
    public class Gravity : IWorldPart
    {

        World world;
        Vector3 g;

    public Gravity(World world, Vector3 g)
    {
        this.world = world;
        this.g = g;
    }

 
    public World getOwner()
    {
        return world;
    }

    public void setOwner(World owner)
    {
        this.world = owner;
    }


    public void interact(PhysicsObject Object, double dt)
    {

        if (Object.isFreeze())
            return;
        Object.velocity = Object.velocity.add(g.scale(dt));
    }
}
public class AirForce : IPart
    {
        PhysicsObject o;
    double factor;

    public AirForce(PhysicsObject Object, double k)
    {
        o = Object;
        factor = k;
    }


    public PhysicsObject getOwner()
    {
        return o;
    }


    public void setOwner(PhysicsObject owner)
    {
        o = owner;
    }


    public void tick(double dt)
    {
        o.setVelocity(o.getVelocity().scale(factor));
    }


    public void onTickEnd(double dt)
    {

    }
}
    public class Range : IWorldPart
    {

        World world;
        Vector3 max;
        Vector3 min;

    public Range(World world, Vector3 m, Vector3 M)
    {
        this.world = world;
        max = M;
        min = m;
    }


    public World getOwner()
    {
        return world;
    }


    public void setOwner(World owner)
    {
        this.world = owner;
    }


    public void interact(PhysicsObject aobject, double dt)
    {
        if (aobject.isFreeze())
        {
            return;
        }
        double x = aobject.position.x;
        double y = aobject.position.y;
        double z = aobject.position.z;
        Vector3 v = aobject.getVelocity();
        if (x > max.x)
        {
            aobject.setPosition(new Vector3(max.x, y, z));
            x = max.x;
           aobject.setVelocity(new Vector3(-v.x, v.y, v.z));
        }
        else if (x < min.x)
        {
            aobject.setPosition(new Vector3(min.x, y, z));
            x = min.x;

            aobject.setVelocity(new Vector3(-v.x, v.y, v.z));
        }

        if (z > max.z)
        {
            aobject.setPosition(new Vector3(x, y, max.z));
            z = max.z;
            aobject.setVelocity(new Vector3(v.x, v.y, -v.z));
        }
        else if (z < min.z)
        {
            aobject.setPosition(new Vector3(x, y, min.z));
            aobject.setVelocity(new Vector3(v.x, v.y, -v.z));
            z = min.z;
        }

        if (y > max.y)
        {
            aobject.setPosition(new Vector3(x, max.y, z));
            aobject.setVelocity(new Vector3(v.x, -v.y, v.z));
            y = max.y;
        }
        else if (y < min.y)
        {
            aobject.setPosition(new Vector3(x, min.y, z));
            aobject.setVelocity(new Vector3(v.x, -v.y, v.z));
            y = min.y;
        }
    }
}

public class Background : IRenderObject, ICopyObject<Background>
    {

        public readonly int color;

        public Background(int color)
        {
            this.color = color;
        }





        public void render(Camera camera, Zmap bitmap)
        {
            for(int x=0;x<bitmap.Width;x++)
			{
                for(int y=0;y<bitmap.Height;y++)
				{
					if (bitmap[x,y]<double.PositiveInfinity)
					{

					}
                    else
					{
                        bitmap.getSource()[x, y] = color;
					}
				}
			}
        }


        public Background copy()
        {
            return new Background(color);
        }
    }
}
