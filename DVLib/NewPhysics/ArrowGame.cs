using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVOSLib;
using MathBase;
using Images;
using NewPhysics;


namespace NewPhysics
{
	public static class ArrowGameHelper
	{
		public static void creatTarget_X(World world,Vector3 pos,bitmap targetTexture, bitmap targetTexture2, bitmap stickTexture)
		{

			TextureCube target = new TextureCube(world, 0.3, 1, 1);
			target.setPosition(pos + new Vector3(0, -2, 0));
			target.setTexture(targetTexture2, targetTexture2, targetTexture2, targetTexture2, targetTexture,  targetTexture2);

			TextureCube stick = new TextureCube(world, 0.1, 1.5, 0.1);
			stick.setPosition(pos + new Vector3(0, -0.75, 0));
			stick.setTexture(stickTexture);
			world.addToWorld(stick);
			world.addToWorld(target);
		}

	}

	public class ArrowMark:IRayTraceObject
	{
		ArrowObject arrow;
		public ArrowMark(ArrowObject arrow)
		{
			this.arrow = arrow;
		}
		public IRayTraceObject Copy()
		{
			return this;
		}

		public PhysicsObject getObject()
		{
			return arrow;
		}

		public Vector3 getPosition()
		{
			return new Vector3(double.PositiveInfinity,double.PositiveInfinity,double.PositiveInfinity);
		}

		public World getWorld()
		{
			return arrow.world;
		}

		public void prepareForCamera(Camera raySource)
		{
			
		}

		public RayTraceResult rayTrace(Vector3 position, Vector3 direction)
		{
			return null;
		}

		public RayTraceResult rayTrace(Vector3 position, Vector3[,] dirs, int x, int y)
		{
			return null;
		}

	

		public void render(Zmap bitmap, Camera camera)
		{
			if(arrow.isOnground)
			{
Vector2 pos = camera.pointInScreen(arrow.getPosition());
			int x =(int) pos.X;
			int y=(int) pos.Y;
			if(x>=0&&x<camera.width&&y>=0&&y<camera.height)
			{
				for(int i=-1;i<=1;i++)
					{
						for (int j = -1; j <= 1; j++)
						{
							bitmap[i + x, y +j]=0;
							bitmap.getSource()[i + x, y + j] =arrow.groundColor;
						}
					}
			}
			}
			
		}
	}
	public class BowString : IRayTraceObject
	{
		BowObject bow;
		public BowString (BowObject bow)
		{
			this.bow =bow;
		}
		public IRayTraceObject Copy()
		{
			throw new NotImplementedException();
		}

		public PhysicsObject getObject()
		{
			return bow;
		}

		public Vector3 getPosition()
		{
			return bow.getPosition();
		}

		public World getWorld()
		{
			return bow.world;
		}

		public void prepareForCamera(Camera raySource)
		{
		}

		public RayTraceResult rayTrace(Vector3 position, Vector3 direction)
		{
			return null;
		}

		public RayTraceResult rayTrace(Vector3 position, Vector3[,] dirs, int x, int y)
		{
			return null;
		}

		public void render(Zmap bitmap, Camera camera)
		{
			Vector2 p1 = camera.pointInScreen(bow.upNode2);
			Vector2 p2 = camera.pointInScreen(bow.downNode2);
			Vector2 p12 = camera.pointInScreen(bow.arrowPos);
			bitmap.getSource().drawline(p1, p12, 0xffffff);
			bitmap.getSource().drawline(p12, p2, 0xffffff);
		}
	}
	public class BowObject: PhysicsObject, IRayTraceSet
	{
		static double sqrt3 = Math.Sqrt(3);
		static double maxPullFactor =0.32;
		double size;
		Paper handle;
		Paper up1;
		Paper up2;
		Paper down1;
		Paper down2;
		Paper upString;
		Paper downString;
		IRayTraceObject[] objects;
		Vector3 shootDirection;
		Vector3 gravityDirection;
		Vector3 sideDirection;
		double pull=0;
		double lhandle;
		double lp1;
		double lp2;
		Vector3 upNode1;
		internal Vector3 upNode2;
		Vector3 upNode3;
		Vector3 downNode1;
		 internal Vector3 downNode2;
		Vector3 downNode3;
		public double power = 50;
		internal Vector3 arrowPos;

		ArrowObject arrow;
		
		public BowObject(World world, double size, bitmap texture1, bitmap texture2, bitmap texture3):base(world)
		{
			handle = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), texture1);
			up1 = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), texture2);
			down1 = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), texture2);
			up2 = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), texture3);
			down2 = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), texture3);
			upString = new Paper(world, new Vector3(0, 0, 0),new Vector3(0,0,0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new bitmap(1, 1) );
			downString = new Paper(world, new Vector3(0, 0, 0), new Vector3(0, 0, 0),new Vector3(0, 0, 0), new Vector3(0, 0, 0), new bitmap(1, 1));
			objects = new IRayTraceObject[] {handle.t1,handle.t2,up1.t1,up1.t2, up2.t1, up2.t2,down1.t1,down1.t2, down2.t1, down2.t2,upString.t1,downString.t1,upString.t2,downString.t2 };
			this.size = size;
			lhandle = size * 0.5;
			lp1 = lhandle / sqrt3;
			lp2 = 0.1 * size;
			setDirection(new Vector3(1, 0, 0));
			
		}
		public override void setDirection(Vector3 vector)
		{
			base.setDirection(vector);
			shootDirection = getDirection();
			gravityDirection = new Vector3(0, 1, 0);
			sideDirection = gravityDirection.cross(shootDirection);
			gravityDirection = sideDirection.cross(shootDirection);
			if (gravityDirection.y < 0) 
			gravityDirection *= -1;
			updateDirection();

		}

		public void loadArrow(ArrowObject arrow)
		{
			this.arrow = arrow;
			arrow.onBow = true;
			arrow.isOnground = false;
		}
		public override void setPosition(Vector3 position)
		{
			Vector3 dp = position - this.position;
			handle.setPosition(handle.getPosition() + dp);
			up1.setPosition(up1.getPosition() + dp);
			down1.setPosition(down1.getPosition() + dp);
			up2.setPosition(up2.getPosition() + dp);
			down2.setPosition(down2.getPosition() + dp);
			upString.setPosition(upString.getPosition() + dp);
			downString.setPosition(downString.getPosition() + dp);
			if (arrow!=null)
			{
				arrow.setPosition(arrow.getPosition() + dp);
			}

			base.setPosition(position);

		}

		public void shootArrow(double speed)
		{
			arrow.onBow = false;
            arrow.setVelocity(speed*shootDirection);
		}
		public void pullBow(double p)
		{
			pull += p;
			if(pull>maxPullFactor)
			{
				pull = maxPullFactor;
			}
		}

		public bool isArrowLoaded()
		{
			return arrow != null;
		}
		public void release()
		{
			if(arrow!=null)
			{
             double p = Math.Sqrt(pull / maxPullFactor);
			shootArrow(p * power );
			}
			arrow = null;
			pull = 0;
		}
		public void  updateDirection()
		{
			Vector3 updirection1;
			Vector3 downDirection1;
			Vector3 updirection2;
			Vector3 downDirwction2;
			double a = Math.Sqrt(0.25 - pull * pull) - 0.25;
			double b = Math.Sqrt(0.5 / 3 - a * a);
			updirection1 = -a * gravityDirection - b * shootDirection;
			updirection1 = updirection1.nolrmalized();
			updirection2 = shootDirection-9*gravityDirection;
			updirection2 = updirection2.nolrmalized();

			downDirection1= a * gravityDirection - b * shootDirection;
			downDirection1 = downDirection1.nolrmalized();
			downDirwction2= shootDirection + 9 * gravityDirection;
			downDirwction2 = downDirwction2.nolrmalized();

			upNode1 = position - gravityDirection * 0.25 * size;
			upNode2 = upNode1 + updirection1 * lp1;
			upNode3 = upNode2 + updirection2 * lp2;

			downNode1 = position + gravityDirection * 0.25 * size;
			downNode2 = downNode1 + downDirection1 * lp1;
			downNode3 = downNode2 + downDirwction2 * lp2;
			Vector3 sideDir = sideDirection * 0.02;
			Vector3 sideDir3 = sideDirection * 0.005;
			Vector3 sideDir2 = sideDirection * 0.01;
			Vector3 sideDir4 = sideDirection * 0.001;
			handle.setPos(upNode1 + sideDir, upNode1 - sideDir, downNode1 + sideDir, downNode1 - sideDir);
			up1.setPos(upNode2 + sideDir2, upNode2 - sideDir2, upNode1 + sideDir, upNode1 - sideDir);
			up2.setPos(upNode3 + sideDir3, upNode3 - sideDir3, upNode2 + sideDir2, upNode2 - sideDir2);
			down1.setPos(downNode1 + sideDir, downNode1 - sideDir, downNode2 + sideDir2, downNode2 - sideDir2);
			down2.setPos(downNode2 + sideDir2, downNode2 - sideDir2, downNode3 + sideDir3, downNode3 - sideDir3);

			arrowPos = 0.5 * (upNode2 + downNode2) - pull * shootDirection * size;

			

			if(arrow!=null)
			{
				arrowPos += sideDirection * 0.04;
			}
            upString.setPos(arrowPos+sideDir4,arrowPos-sideDir4, upNode2 + sideDir4, upNode2 - sideDir4);
			downString.setPos(arrowPos + sideDir4, arrowPos - sideDir4, downNode2 + sideDir4, downNode2 - sideDir4);

			if(arrow!=null)
			{
            arrow.setDirection(shootDirection );
				arrow.setVelocity(arrow.getDirection());
			arrow.setPosition(arrowPos + shootDirection * arrow.length);
			}
			

		}
		public IRayTraceObject[] getObjects()
		{
			return objects;
		}
	}
	public class ArrowObject : PhysicsObject,IRayTraceSet
	{
		Paper arrowModle1;
		Paper arrowModle2;
		internal bool onBow = false;
		IRayTraceObject[] objects;
		internal double length;
		internal bool isOnground = false;
		IRayTraceObject Object;
		Vector3 stickDirection;
		double maxLife=30;
		double life = 0;
		ArrowMark mark;
		
		public int groundColor { get; private set; }

		public ArrowObject(World world,double length,bitmap texture) : base(world)
		{
			Vector3 side_offset = direction.isOnY?(1,0,0):direction.cross((0,1,0)).nolrmalized();
			Vector3 y_offset = side_offset.cross(direction).scale(0.05);
			side_offset *= 0.05;
			Vector3 d_offset = -direction*length;
			this.length = length;
			arrowModle1 = new Paper(world, position + side_offset, position - side_offset, position + side_offset + d_offset, position - side_offset + d_offset,texture);
			arrowModle1.setColor(0x00ffff);
			arrowModle2 = new Paper(world, position + y_offset, position - y_offset, position + y_offset + d_offset, position - y_offset + d_offset, texture);
			arrowModle2.setColor(0x00ffff);
			mark = new ArrowMark(this);
			objects = new IRayTraceObject[] { arrowModle1.t1,arrowModle1.t2, arrowModle2.t1, arrowModle2.t2 ,mark};
			
		}
		public override void setDirection(Vector3 vector3)
		{
			
			if(vector3.x==0&&vector3.y==0&&vector3.z==0)
			{
				vector3 = new Vector3(0, 1, 0);
			}
			else
			{
               direction = vector3.nolrmalized();
			}
			Vector3 side_offset = direction.isOnY ? (1, 0, 0) : direction.cross((0, 1, 0)).nolrmalized();
			Vector3 y_offset = side_offset.cross(direction).scale(0.05);
			side_offset *= 0.05;
			Vector3 d_offset = -direction*length;
			arrowModle1.setPos(position + side_offset, position - side_offset, position + side_offset + d_offset, position - side_offset + d_offset);
			arrowModle2 .setPos(position + y_offset, position - y_offset, position + y_offset + d_offset, position - y_offset + d_offset);


		}
		public override void tick(double dt)
		{
			if(onBow)
			{
				return;
			}
			life+=dt;
			if(life>=maxLife)
			{
				remove();
			}
			if(!isOnground)
			{
             RayTraceResult result=	world.rayTrace(position, velocity,velocity.length()*dt+0.001,objects);
			if(result!=null&&result.Object!=null)
			{
					
				    isOnground = true;
					Object = result.getObject();
					stickDirection = getVelocity().nolrmalized()*0.05;
					setPosition(result.point+stickDirection);
					stickDirection *= 2;
					setVelocity((0, 0, 0));
					groundColor = result.getColor();
				}
			}
		
			if(!isOnground)
			{
           setDirection(getVelocity());
			}
			else
			{
			RayTraceResult rayTrace=	Object.rayTrace(getPosition()-stickDirection, stickDirection);
				if(rayTrace==null)
				{
					isOnground = false;
				}
				setVelocity((0, 0, 0));
			}
			
			base.tick(dt);

		}
		public override void setPosition(Vector3 position)
		{    Vector3 dp = position - this.position;
			arrowModle1.setPosition(arrowModle1.getPosition() + dp);
			arrowModle2.setPosition(arrowModle2.getPosition() + dp);
			base.setPosition(position);
			
		}
		public IRayTraceObject[] getObjects()
		{
			return objects;
		}
	}
}
