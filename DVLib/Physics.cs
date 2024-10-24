using System;
using MathBase;
using vector2 = MathBase.Vector2;
using Images;
using System.Drawing;
using System.IO;
namespace Physics
{
	public enum fieldtype : int
	{
		none = 0,
		恒加速度向心 = 1,
		恒大向心 = 2,
		类电场力 = 3





	}
	public enum stuff : int
	{
		场轨迹 = 0,
		场 = 1,
		物体轨迹 = 2,
		物体 = 3


	}
	


	public class Time
	{
		Physics.Physics2.world world;
		System.Timers.Timer timer;
		string unit = "s";
		double t;
		int dt;
		public void setworld(Physics.Physics2.world world)
		{
			this.world = world;
		}
		public Time(int t_min)
		{
			timer = new System.Timers.Timer();
			dt = t_min;
			timer.Interval = dt;
			t = 0;
		}
		public string time()
		{
			return t + unit;
		}
		public void start()
		{
			timer.Start();
			timer.Elapsed += timerun;
		}
		public System.Timers.Timer gettimer()
		{
			return timer;
		}
		public void stop()
		{
			timer.Stop();
		}
		public void timerun(object sender, EventArgs e)
		{
			if (t > Double.MaxValue - 1) t = 0;
		}
		public void tadd(double dT)
		{
			t += dT;
		}
		public void tset(double T)
		{
			t = T;
		}
		public double gettime()
		{
			return t;
		}

		public void attachtoworld(Physics2.world w)
		{
			timer.Elapsed += w.timerun;
		}
	}

	namespace Physics2
	{

	
		public class staticobject
		{
			vector2 pos;
			vector2[] points;
			public staticline[] lines;
			bool paint;
			double rotation;
			double size;
			public int color;
			public staticobject(vector2 cpos, vector2[] points, bool paint = false, double rotation = 0, double size = 1)
			{
				this.pos = cpos;
				this.points = points;
				this.paint = paint;
				this.rotation = rotation;
				this.size = size;
				this.color = Colors.Black;
				lines = real();
			}
			public void setpaint(bool flag)
			{
				paint = flag;
			}
			public void setrotation(double angle)
			{
				rotation = angle;
				lines = real();
			}
			public void setsize(double size)
			{
				this.size = size;
				lines = real();
			}
			public void setcolor(int color)
			{
				this.color = color;
			}
			public void setpaint(double size)
			{
				this.size = size;
			}
			public staticline[] real()
			{
				if (points != null)
				{
					vector2[] result = new vector2[points.Length];
					for (int i = 0; i < points.Length; i++)
					{
						result[i] = pos + points[i].row(rotation) * size;
					}
					staticline[] staticlines = new staticline[points.Length];
					for (int i = 0; i < points.Length - 1; i++)
					{
						staticlines[i] = new staticline(result[i], result[i + 1]);
					}
					staticlines[staticlines.Length - 1] = new staticline(result[0], result[result.Length - 1]);
					return staticlines;
				}
				return null;

			}
			public staticline clost(vector2 vector)
			{

				return null;
			}
			public void collision(mp_object2 Object2)
			{
				foreach (staticline staticline in lines)
				{
					staticline.collision(Object2);
				}
			}



		}
		public class staticline
		{
			public vector2 pos1 { get; }
			public vector2 pos2 { get; }
			public Line2d line { get; }
			public staticline(vector2 p1, vector2 p2)
			{
				pos1 = p1;
				pos2 = p2;
				line = new Line2d(p1, p2);
			}
			public void collision(mp_object2 object2)
			{
				vector2 v = object2.getvelocity2();
				vector2 pos = object2.getdisplacement();
				double r = object2.getcollision();
				bool flag = line.distancetopoint(pos + v * object2.getRdt()) < r;
				int n = 2;
				if (line.distancetopoint(pos) < r)
					n = 1;
				bool flag3;
				if (!flag)
				{
					vector2 p1 = pos + v * object2.getRdt();
					vector2 cp = line.crosspoint(new Line2d(pos, pos + v * object2.getRdt()));
					flag = flag || (p1 - pos).dot(cp - pos) * (pos - p1).dot(cp - p1) > 0;
				}

				vector2 direction = (pos1 - pos2).row(90).nolrmalized();
				bool flag2 = (pos - pos1).dot(pos2 - pos1) * (pos - pos2).dot(pos1 - pos2) > 0;
				if (flag && flag2)
				{

					object2.setvelocity2(v - n * direction.dot(v) * direction);
				}
				else
				{

				}
			}



		}
		public class mp_object2
		{
			Plot2D jt = new Plot2D(1920, 1080);
			Plot2D vt = new Plot2D(1920, 1080);
			Plot2D at = new Plot2D(1920, 1080);
			Plot2D jxt = new Plot2D(1920, 1080);
			Plot2D vxt = new Plot2D(1920, 1080);
			Plot2D axt = new Plot2D(1920, 1080);
			Plot2D jyt = new Plot2D(1920, 1080);
			Plot2D vyt = new Plot2D(1920, 1080);
			Plot2D ayt = new Plot2D(1920, 1080);
			Plot2D mt = new Plot2D(1920, 1080);
			vector2 f = new vector2();
			world father;
			vector2 P;
			double strength = 1;
			string name;
			string tag;
			vector2 F;
			double Q = 0;
			vector2 v0;
			vector2 v = 0;
			vector2 A = 0;
			vector2 jerk = 0;
			int collisions = 0;
			vector2 d;
			double m = 1;
			double c;
			//get,set
			public double getRdt()
			{
				return father.getRdt();
			}
			public void Collisions()
			{
				collisions++;
			}
			public void setmother(world m)
			{
				father = m;
			}
			public world getmother()
			{
				return father;
			}
			public double getstrength()
			{
				return strength;
			}
			public void setstrength(double s)
			{
				strength = s;
			}
			public double dynamicquality()
			{

				if (v.value() <= father.getc())
					return m / Math.Sqrt(1 - v.value() * v.value() / father.getc() / father.getc());
				else return double.PositiveInfinity;
			}
			public vector2 dv(double dt)
			{
				double C = father.getc();
				f = F; vector2 v1; vector2 a_now;
				if (father.usingrt)
				{
					P += dt * F;
					v1 = (P * C) / Math.Sqrt(C * C * m * m + P.dot(P));
					a_now = (v1 - v) / dt;
				}
				else 
				{
					v1 = v + F / m * dt;
					a_now = F / m;
					updateP();
				}


				jerk = (a_now - A) / dt;
				A = a_now;
				return v1;
			}
			public double kineticenergy()
			{
				return v.value() * v.value() / 2 * m;
			}
			public mp_object2()
			{
				v0 = new vector2();
				name = "null";
				F = new vector2();
				v = new vector2(0,0);
				d = new vector2();
				m =1;
				c = 1;
				P = m * v;
			}
			public mp_object2(vector2 f2, vector2 v2, vector2 d2, double m2, double c2)
			{
				v0 = new vector2();
				name = "null";
				F = f2;
				v = v2;
				d = d2;
				m = m2;
				c = c2;
				P = m * v;
			}
			public vector2 U()
			{
				vector2 V = father.V;
				Double C = father.getc();
				vector2 U = (V + v) / (1 + V.dot(v) / C / C);
				return U;
			}
			public mp_object2(vector2 d2, double m2,double c2)
			{
				v0 = new vector2();
				name = "null";
				F = new vector2();
				v = new vector2();
				d = d2;
				m = m2;
				c = c2;
				P = m * v;
			}
			public void setcharge(double q)
			{
				Q = q;
			}
			public void setname(string n)
			{
				name = n;
			}
			public string getname()
			{
				return name;
			}
			public vector2 getP()
			{
				return P;
			}
			public void addforce(vector2 f)
			{
				F += f;
			}
			public vector2 getF()
			{
				return F;
			}
			public void addvector2(vector2 dv)
			{
				v += dv;
			}
			public void adddisplacement(vector2 dd)
			{
				d += dd;
			}
			public void setforce(vector2 f)
			{
				F = f;
			}
			public void setforce(int f)
			{
				F = new vector2(f, f);
			}
			public void setmass(double m0)
			{
				m = m0;


				if (father != null)
					P = dynamicquality() * v;
			}

			public void updateP()
			{

				if (father != null)
				{
					if (father.usingrt) P = dynamicquality() * v;
					else
						P = m * v;
				}

			}
			public void settag(string s)
			{
				tag = s;
			}
			public mp_object2 clone()
			{
				mp_object2 temp = new mp_object2(F, v, d, m, c);
				return temp;


			}

			public string gettag()
			{
				return tag;
			}

			public void setcollision(double c0)
			{
				c = c0;
			}
			public void setvelocity2(vector2 dv)
			{
				v = dv;
				if (father != null)
					P = dynamicquality() * v;
			}
			public DistanceEqution distance(mp_object2 o2)
			{
				vector2 d1 = d, d2 = o2.d, v1 = v, v2 = o2.v;
				double a, b, c;
				a = (v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y);
				b = 2 * (v1.X - v2.X) * (d1.X - d2.X) + 2 * (v1.Y - v2.Y) * (d1.Y - d2.Y);
				c = (d1.X - d2.X) * (d1.X - d2.X) + (d1.Y - d2.Y) * (d1.Y - d2.Y);
				return new DistanceEqution(a, b, c);

			}
			public void setdisplacement(vector2 dd)
			{
				d = dd;
			}
			public bool willcollision(mp_object2 o2)
			{
				double dt = father.getdt();
				bool result = false;
				if (distance(o2).min(0, dt * father.gett() * father.gettimespeed())[1] < (o2.c + c) * (o2.c + c))
				{
					result = true;
				}


				return result;
			}
			public vector2 getforce()
			{

				return f;
			}
			public double getmass()
			{
				return m;
			}
			public double GetCharge()
			{
				return Q;
			}
			public double getcollision()
			{
				return c;
			}
			public vector2 getvelocity2()
			{
				return v;
			}
			public vector2 getdisplacement()
			{
				return d;
			}
			public void collision(mp_object2 o2)
			{

				if (c >= 0 && o2.c >= 0)
				{
					vector2 direction = (d - o2.d).nolrmalized(); double v1 = v.dot(direction), v2 = o2.getvelocity2().dot(direction), m1 = m, m2 = o2.m;
					double v21 = (2 * m1 * v1 + m2 * v2 - m1 * v2) / (m1 + m2), v11 = (2 * m2 * v2 + m1 * v1 - m2 * v1) / (m1 + m2);
					if (m == double.PositiveInfinity)
					{
						v21 = -v2;
						v11 = 0;
						if (m2 == double.PositiveInfinity)
						{
							double temp = v2;
							v2 = v1;
							v1 = temp;
						}
					}
					else if (m2 == double.PositiveInfinity)
					{
						v11 = -v1;
						v21 = 0;

					}

					v11 = v11 - direction.dot(v);
					v21 = v21 - direction.dot(o2.v);

					v += v11 * direction;
					o2.v += v21 * direction;
					updateP();
					o2.updateP();
					Collisions();
					o2.Collisions();

					/*旧算法
					{
						double x1 = v.X, y1 = v.Y, x2 = o2.v.X, y2 = o2.v.Y, x0 = (d - o2.d).X, y0 = (d - o2.d).Y, m1 = m, m2 = o2.m;
						double n = (2.0 * ((x0 * x2 + y0 * y2) / m2 - (x0 * x1 + y0 * y1) / m1))
							/ ((x0 * x0 + y0 * y0) / (m1 * m1) + (x0 * x0 + y0 * y0) / (m2 * m2));

						v += (d - o2.d) * n / m1;
						o2.v -= (d - o2.d) * n / m2;
					}*/



				}

			}
			public void gravity(mp_object2 o2)
			{

				double g = father.getG();
				double r = (d - o2.d).value();
				if (r == 0)
				{
					return;
				}

				if (o2.m != double.PositiveInfinity && m != double.PositiveInfinity)
					addforce((o2.d - d).nolrmalized() * (g * m * o2.m / r / r));




			}

			public void Coulombforce(mp_object2 o2)
			{

				double k = father.getK();
				double r = (d - o2.d).value();
				if (r == 0)
				{
					return;
				}

				if (o2.m != double.PositiveInfinity && m != double.PositiveInfinity)
					addforce((-o2.d + d).nolrmalized() * (k * Q * o2.Q / r / r));




			}

			public void elasticforce(mp_object2 o2)

			{
				double r = getcollision() + o2.getcollision() - (d - o2.d).value();
				if ((getdisplacement() - o2.getdisplacement()).value() < getcollision() + o2.getcollision())
				{

					if (o2.m != double.PositiveInfinity && m != double.PositiveInfinity)
					{
						vector2 af = -(o2.d - d).nolrmalized() * (50 * Math.Min(m, o2.m) * r * r);
						addforce(af);
						o2.addforce(-af);
					}


				}
			}
			public void clean()
			{
				mt.clean();
				vt.clean();

				at.clean();
				jt.clean();
				vxt.clean();

				axt.clean();
				jxt.clean();
				vyt.clean();

				ayt.clean();
				jyt.clean();
				updatemap();
			}
			public void savemap()
			{
				if (father.usingdata)
				{
					vt.setstring("时间/s", "速度/ m/s");
					at.setstring("时间/s", "加速度/ m/s^2");
					jt.setstring("时间/s", "急动度/ m/s^3");
					vt.setn(20, 20);
					at.setn(20, 20);
					jt.setn(20, 20);
					vxt.setstring("时间/s", "X速度/ m/s");
					axt.setstring("时间/s", "X加速度/ m/s^2");
					jxt.setstring("时间/s", "X急动度/ m/s^3");
					vxt.setn(20, 20);
					axt.setn(20, 20);
					jxt.setn(20, 20);
					mt.setstring("时间/s", "动质量/ kg");
					mt.setn(20, 20);
					vyt.setstring("时间/s", "Y速度/ m/s");
					ayt.setstring("时间/s", "Y加速度/ m/s^2");
					jyt.setstring("时间/s", "Y急动度/ m/s^3");
					vyt.setn(20, 20);
					ayt.setn(20, 20);
					jyt.setn(20, 20);
					Directory.CreateDirectory(name);
					/*
					vxt.get().save(name + "\\vx-t.jpg");
					axt.get().save(name + "\\ax-t.jpg");
					jxt.get().save(name + "\\jx-t.jpg");
					vyt.get().save(name + "\\vy-t.jpg");
					ayt.get().save(name + "\\ay-t.jpg");
					jyt.get().save(name + "\\jy-t.jpg");
					vt.get().save(name + "\\v-t.jpg");
					at.get().save(name + "\\a-t.jpg");
					jt.get().save(name + "\\j-t.jpg");
					mt.get().save(name + "\\m-t.jpg");*/

				}
			}
			public void updatemap()
			{
				vt.add(new vector2(father.gettime().gettime(), v.value()));
				at.add(new vector2(father.gettime().gettime(), A.value()));
				jxt.add(new vector2(father.gettime().gettime(), jerk.value()));
				vxt.add(new vector2(father.gettime().gettime(), v.X));
				axt.add(new vector2(father.gettime().gettime(), A.X));
				jyt.add(new vector2(father.gettime().gettime(), jerk.X));
				vyt.add(new vector2(father.gettime().gettime(), v.Y));
				ayt.add(new vector2(father.gettime().gettime(), A.Y));
				jt.add(new vector2(father.gettime().gettime(), jerk.Y));
				mt.add(new vector2(father.gettime().gettime(), dynamicquality()));
			}
			public string imformation()
			{
				string result;
				result = "名称：" + name + "\n属性：" + tag + " 属性强度：" + strength + "\n电荷：" + GetCharge() + "\n受力：" + getforce() + "\n加速度：" + A + "\n急动度：" + jerk.value() + "m/s^3" + "\n速度：" +v + "\n位置：" + d + "\n质量：" +m + "\n半径：" + c + "\n动质量：" + dynamicquality() + "\n碰撞次数：" + collisions + "\n动量：" + P.value() + "kg*m/s";
				return result;
			}
			public void update(double dt)
			{

				if (double.IsInfinity(m))
				{
					d += v * dt;
				}
				else
				{
					double C = father.getc();
				
						v0 = v;
					
					if (dt > 0)
					{
						v = dv(dt);
						d += v * dt;
					}
					else
					{
						d += v * dt;
						v = dv(dt);


					}
					if (v.value() >= C)

					{
						v = v.nolrmalized() * (father.getc());
					}
				}

			}


		}
		public class field2 : mp_object2
		{

			mp_object2 father;
			double r_field;
			fieldtype type;
			Func<mp_object2, vector2> force;
			vector2 field_type_0(mp_object2 o)
			{
				return new vector2(0, 0);
			}
			vector2 field_type_1(mp_object2 o)//大小恒定方向指向场源
			{
				return -(o.getdisplacement() - getdisplacement()).nolrmalized() * getstrength();
			}
			vector2 field_type_2(mp_object2 o)//加速度大小恒定方向指向场源
			{
				if (getstrength() * o.getmass() != double.PositiveInfinity)
				{
					return -(o.getdisplacement() - getdisplacement()).nolrmalized() * getstrength() * o.getmass();
				}
				return 0;
			}
			vector2 field_type_3(mp_object2 o)//类电场
			{
				double r = (o.getdisplacement() - getdisplacement()).value(); double n;
				if (r != 0)
					n = o.getstrength() * getstrength() / r / r;
				else
					n = 0;
				return (o.getdisplacement() - getdisplacement()).nolrmalized() * n;


			}
			public fieldtype gettype()
			{
				return type;
			}
			//构造函数
			public field2() : base(0, 1, 0)
			{

				r_field = 0;
				type = 0;
				force = field_type_0;
				setstrength(1);
			}
			public field2(fieldtype t, double r_f, double s, vector2 pos, double m, double r) : base(pos, m, r)
			{
				r_field = r_f;
				setstrength(s);
				type = t;

				switch (type)
				{
					case fieldtype.恒大向心:
						force = field_type_2;
						break;
					case fieldtype.恒加速度向心:
						force = field_type_1;
						break;
					case fieldtype.类电场力:
						force = field_type_3;
						break;
				}
			}
			public double getr_f()
			{
				return r_field;
			}
			public void setr_f(double R)
			{
				r_field = R;
			}
			public void createforce(mp_object2 o)
			{
				if (hastag(o))
				{
					if (father == null)
					{
						this.addforce(0 - force(o));
					}
					else
					{
						father.addforce(0 - force(o));
					}

					o.addforce(force(o));
				}
			}

			public void bond(mp_object2 o)
			{
				if (o == null)
				{
					return;
				}
				father = o;
				setmass(father.getmass());

			}
			new public void update(double dt)
			{
				if (father != null)
				{
					father.addforce(getF());
					this.setdisplacement(father.getdisplacement());
					this.setvelocity2(father.getvelocity2());
				}
				else
				{
					base.update(dt);
				}

			}
			public void settype(fieldtype type)
			{
				this.type = type;
				switch (type)
				{
					case fieldtype.恒大向心:
						force = field_type_1;
						break;
					case fieldtype.恒加速度向心:
						force = field_type_2;
						break;
					case fieldtype.类电场力:
						force = field_type_3;
						break;
				}
			}
			public void settags(string tag)
			{
				base.settag(tag);
			}
			public mp_object2 getfather()
			{
				return father;
			}
			new public string imformation()
			{
				string result;
				if (father != null)
					result = "绑定物体：" + father.getname() + "\n作用半径：" + r_field + "\n场类型：" + type + "\n" + base.imformation();
				else
					result = "\n作用半径：" + r_field + "\n场类型：" + type + "\n" + base.imformation();
				return result;
			}
			public bool hastag(mp_object2 o)
			{
				if (base.gettag() == null)
				{
					return true;
				}
				else
				{
					if (o.gettag() == null)
					{
						return false;
					}

					if (base.gettag() == o.gettag())
					{
						return true;
					}

					return false;
				}
			}
		}

		public class world
		{
			staticobject[] staticobjects = new staticobject[0];
			public vector2 V = 0;
			public vector2 R = 0;
			double c = 299792458;
			public bool usingdata = true;
			public bool usingrt = true;
			public bool usingcr = false;
			public bool usingfieldforce = true;
			public bool usinggravity = true;
			public bool usingelasticforce = true;
			public bool usingcollision = true;
			public bool usingCoulombforce = true;
			public double dt = (double)1 / 1000000;//单位时间
			double G = 6.67 / 100000000000;
			double K = 8.987551 * 1000000000;
			double timespeed;
			Time world_time;
			mp_object2[] objects;
			field2[] fields;
			int t;
			public double getc()
			{
				return c;
			}
			public void setc(Double speed)
			{
				c = speed;
			}
			public void setG(double g)
			{
				G = g;
			}
			public world()
			{
				t = 100;
				timespeed = 1;
				world_time = new Time(t);
				world_time.setworld(this);
				objects = new mp_object2[0];
				fields = new field2[0];
				world_time.attachtoworld(this);
			}

			public world(int t0)
			{
				timespeed = 1;
				t = t0;
				world_time = new Time(t);
				world_time.setworld(this);
				objects = new mp_object2[0];
				fields = new field2[0];
				world_time.attachtoworld(this);
			}

			public void starttime()
			{
				world_time.start();
			}
			public double getdt()
			{
				return dt * timespeed;
			}
			public void gravity()
			{
				for (int i = 0; i < objects.Length; i++)
				{
					for (int j = i + 1; j < objects.Length; j++)
					{

						objects[i].gravity(objects[j]);
						objects[j].gravity(objects[i]);
					}
				}
			}
			public void Coulombforce()
			{
				for (int i = 0; i < objects.Length; i++)
				{
					for (int j = i + 1; j < objects.Length; j++)
					{

						objects[i].Coulombforce(objects[j]);
						objects[j].Coulombforce(objects[i]);
					}
				}
			}

			public void elasticforce()
			{
				for (int i = 0; i < objects.Length; i++)
				{
					for (int j = i + 1; j < objects.Length; j++)
					{

						objects[i].elasticforce(objects[j]);
					}
				}
			}
			bool crs = true;
			public void collision()
			{

				for (int i = 0; i < objects.Length; i++)
				{

					for (int j = i + 1; j < objects.Length; j++)
					{

						if (objects[i].willcollision(objects[j]))
						{
							mp_object2 o1 = objects[i], o2 = objects[j];
							if ((o1.getvelocity2() - o2.getvelocity2()).dot(o1.getdisplacement() - o2.getdisplacement()) * t < 0)
							{
								objects[i].collision(objects[j]);
								if (usingcr)
								{
									if (crs)
									{
										reversetime();
									}
									crs = !crs;
								}

							}
						}

					}
				}


			}
			public void collision(mp_object2 o)
			{
				for (int j = 0; j < objects.Length; j++)
				{
					if (o != objects[j])
					{
						if (o.willcollision(objects[j]))
						{
							mp_object2 o1 = o, o2 = objects[j];
							if ((o1.getvelocity2() - o2.getvelocity2()).dot(o1.getdisplacement() - o2.getdisplacement()) * t < 0)
							{
								o.collision(objects[j]);

							}
						}
					}
				}

			}
			public bool willcollision()
			{
				bool result = false;
				for (int i = 0; i < objects.Length; i++)
				{
					for (int j = i + 1; j < objects.Length; j++)
					{

						if (objects[i].willcollision(objects[j]))
						{
							mp_object2 o1 = objects[i], o2 = objects[j];
							if ((o1.getvelocity2()- o2.getvelocity2()).dot(o1.getdisplacement() - o2.getdisplacement()) * t < 0)
							{
								return true;

							}
						}

					}
				}
				return result;

			}
			public void stoptime()
			{
				world_time.stop();
			}


			public void reversetime()
			{
				t = -t;
			}
			public void addobject(mp_object2 o)
			{
				mp_object2[] a = new mp_object2[objects.Length + 1];
				objects.CopyTo(a, 0);
				objects = a;
				objects[objects.Length - 1] = o;
				o.setmother(this);
				o.updateP();
			}
			public void addstaticobject(staticobject o)
			{
				staticobject[] a = new staticobject[staticobjects.Length + 1];
				staticobjects.CopyTo(a, 0);
				staticobjects = a;
				staticobjects[staticobjects.Length - 1] = o;
			}
			public staticobject[] getstaticobjects()
			{
				return staticobjects;
			}
			public void addfield(field2 o)
			{
				field2[] a = new field2[fields.Length + 1];
				fields.CopyTo(a, 0);
				fields = a;
				fields[fields.Length - 1] = o;
				o.setmother(this);
			}
			public void settimespeed(double ts)
			{
				timespeed = ts;
			}
			public void setdt(double Dt)
			{
				dt = Dt;
			}
			public void timepassunit()
			{


				for (int j = 0; j < objects.Length; j++)
				{
					objects[j].setforce(0);

				}
				for (int j = 0; j < fields.Length; j++)
				{

					fields[j].setforce(0);
				}
				if (usingfieldforce)
				{
					for (int j = 0; j < objects.Length; j++)
					{
						for (int i = 0; i < fields.Length; i++)
						{
							if ((fields[i].getdisplacement() - objects[j].getdisplacement()).value() < fields[i].getr_f())
								fields[i].createforce(objects[j]);
						}

					}
				}
				if (usingcollision)
				{
					while (willcollision())
					{
						collision();
					}
					for (int i = 0; i < objects.Length; i++)
					{
						foreach (staticobject staticobject in staticobjects)
						{
							staticobject.collision(objects[i]);
						}
					}
				}

				if (usinggravity)
					gravity();
				if (usingelasticforce)
					elasticforce();
				if (usingCoulombforce)
					Coulombforce();

				for (int j = 0; j < objects.Length; j++)
				{
					objects[j].update(dt * Math.Abs(t) / t * timespeed);
				}
				for (int i = 0; i < fields.Length; i++)
				{
					fields[i].update(dt * Math.Abs(t) / t * timespeed);

				}



			}
			public double getRdt()
			{
				return dt * Math.Abs(t) / t * timespeed;
			}
			public void timepassms(double ms)
			{

				Double n = 1.0 / dt / 1000;
				for (int i = 0; i < ms * n; i++)
				{
					timepassunit();

					gettime().tadd(dt * timespeed);
				}
			}
			public void timerun(object sender, EventArgs e)
			{

				if (usingdata)
				{
					foreach (mp_object2 o in objects)
					{

						o.updatemap();
					}
					foreach (field2 o in fields)
					{
						o.updatemap();
					}
				}
				timepassms(gettime().gettimer().Interval);
			}
			public field2 getfield(int i)
			{
				if (i < fields.Length)
				{
					return fields[i];
				}
				else return new field2();
			}
			public int getindex(mp_object2 o)
			{
				for (int i = 0; i < objects.Length; i++)
				{
					if (objects[i] == o)
					{
						return i;
					}
				}
				return -1;
			}
			public int getindex(field2 o)
			{
				for (int i = 0; i < fields.Length; i++)
				{
					if (fields[i] == o)
					{
						return i;
					}
				}
				return -1;
			}
			public void removeobject(int i)
			{
				mp_object2[] temp = new mp_object2[objects.Length - 1];
				for (int j = 0, n = 0; j < objects.Length; j++)
				{
					if (j != i)
					{
						temp[n] = objects[j];
						n++;
					}
					else
					{
						objects[j].clean();
					}
				}
				objects = temp;
			}
			public void removefield(int i)
			{
				field2[] temp = new field2[fields.Length - 1];
				for (int j = 0, n = 0; j < fields.Length; j++)
				{
					if (j != i)
					{
						temp[n] = fields[j];
						n++;
					}
					else
					{
						fields[j].clean();
					}
				}
				fields = temp;
			}
			public field2[] getfields(string name)
			{
				field2[] result = new field2[0];
				foreach (field2 f in fields)
				{
					if (f.getname() == name)
					{
						field2[] temp = new field2[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = f;
						result = temp;
					}
				}
				return result;
			}
			public field2[] getfields_tag(string tag)
			{
				field2[] result = new field2[0];
				foreach (field2 f in fields)
				{
					if (f.gettag() == tag)
					{
						field2[] temp = new field2[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = f;
						result = temp;
					}
				}
				return result;
			}
			public mp_object2[] getobjects(string name)
			{
				mp_object2[] result = new mp_object2[0];
				foreach (mp_object2 f in objects)
				{
					if (f.getname() == name)
					{
						mp_object2[] temp = new mp_object2[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = f;
						result = temp;
					}
				}
				return result;
			}
			public mp_object2[] getobjects()
			{

				return objects;
			}
			public field2[] getfields()
			{

				return fields;
			}
			public mp_object2[] getobjects_tag(string tag)
			{
				mp_object2[] result = new mp_object2[0];
				foreach (mp_object2 f in objects)
				{
					if (f.gettag() == tag)
					{
						mp_object2[] temp = new mp_object2[result.Length + 1];
						result.CopyTo(temp, 0);
						temp[result.Length] = f;
						result = temp;
					}
				}
				return result;
			}
			public double gettimespeed()
			{
				return timespeed;
			}
			public int gett()
			{
				return t;
			}
			public Time gettime()
			{
				return world_time;
			}
			public double getG()
			{
				return G;
			}
			public double getK()
			{ return K; }
			public void setK(Double k)
			{
				K = k;
			}
			public void sett(int time)
			{
				t = time;
			}
			public void clean()
			{
				objects = new mp_object2[0];
				fields = new field2[0];

			}
			public mp_object2 getobject(int i)
			{
				if (i < objects.Length)
				{
					return objects[i];
				}
				else return new mp_object2();
			}



		}
	}
	namespace Physics3
	{
	}
}
