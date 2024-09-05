using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using NewPhysics;
using DVOSLib;

namespace Optic3D
{

	public static class Optic3DHelper
	{
		public static Vector3? onSurface(Vector3 pos,Vector3 normal,Vector3 dir,double n1,double n2)
		{
			double cos = dir.cos(normal);
			double sin = 1-Math.Sqrt(cos*cos);
			if(sin==0)
			{
				return dir;
			}
			Vector3 h = (dir - normal * cos)/sin;
			sin = sin*n1/n2;
			if(sin<1) 
			{
				 cos=1-Math.Sqrt(sin*sin);
				return sin * h + cos * normal;
			}
			return null;
		}
	}
	public class Ray
	{
		public Ray(Vector3 pos,Vector3 dir)
		{
			this.position = pos;
			this.direction = dir;
		}
		public Vector3 position { get; private set; }
		public Vector3 direction { get; private set; }
	}

	public class SphericalSurface : ISurface
	{
		public Vector3 position { get; private set; }
		public Vector3 center { get; private set; }
		public double cr { get; private set; }
		public double hr { get; private set; }
		public double D{ get; private set; }
	double r_;
		public double ni{get; private set;}
		public double no { get; private set; }

		public SphericalSurface(Vector3 pos,Vector3 center,double diameter,double ni,double no)
		{
			position = pos;
			this.center = center;
			D = diameter;
			this.ni = ni;
			this.no = no;
			update();
		}
		public void update()
		{
			 hr = D/ 2;
			cr = center.distanceTo(position);
			double k1 = hr * hr;
			double k3 = cr * cr;
			double k2 = Math.Sqrt(k3 - k1);
			r_ = cr - k2 ;
			r_=r_ * r_ + k1;
		}
		 public Ray getNextRay(Ray ray)
		{
			double d = 0;
			Vector3? vector3 = getRayTrace(ray.position, ray.direction,ref d);
		
			if(vector3!=null)
			{
				Vector3 pos = vector3.Value;
				

				Vector3 n = (pos - center).nolrmalized();
				double v = n.dot(ray.direction);
			
				if (v>0)
				{
					Vector3? nr = Optic3DHelper.onSurface(pos, n, ray.direction, ni, no);
			
					if (nr!=null)
					{
						return new Ray(pos, nr.Value);
					}
				}
				else if(v<0)
				{
					Vector3? nr = Optic3DHelper.onSurface(pos, -n, ray.direction, no, ni);
					if (nr != null)
					{
						return new Ray(pos, nr.Value);
					}
				}
				else
				{
					return new Ray(pos, ray.direction);
				}
			}
			return null;

		}

		public Vector3? getRayTrace(Vector3 p, Vector3 d,ref double dis ,double max=Double.PositiveInfinity)
		{
			
				Vector3 dp = p.reduce(center);
				double a = d.x * d.x + d.y * d.y + d.z * d.z;
				double b = 2 * d.x * dp.x + 2 * d.y * dp.y + 2 * d.z * dp.z;
				double c = dp.x * dp.x + dp.y * dp.y + dp.z * dp.z;
				DistanceEqution de = new DistanceEqution(a, b, c);
				double[] s = de.getSolutions(cr * cr);
				if (s!=null)
				{


					Vector3 pos = p.add(d.scale(s[0]));
				if(s[0]>0&&pos.distance_2To(position)<r_)
				{
					dis = s[0];
                     return pos;
				}
				else if (s[1]>0)
				{
					pos = p.add(d.scale(s[1]));
					if (pos.distance_2To(position) < r_)
					{
						dis =s[1];
						return pos;
					}
				}
					
				}
				return null;
			
		}
	}
	public class POP
	{

		public double size { get; private set; }
	   public int count { get; private set; }
       public double df { get; private set; }
		double f ;
		double n = 1;
		Map<double> dmap;
		Map<Vector2> posmap;
		public double k { get; private set; }
		public double lambda { get; private set; }
		public double dk { get; private set; }
		public Vector2 center { get; private set; }
		Dictionary<double, ComplexMap> H = new Dictionary<double, ComplexMap>();



		public POP(double size,int count,double lambda_nm)
		{
			this.size = size;
			this.count = count;
			df= 1 / size;
			f = count * df;
			dk = size / count;
			dmap = new Map<double>(count,count);
			posmap = new Map<Vector2>(count, count);
			lambda = lambda_nm* 0.000001;
			k = Math.PI * 2 / (lambda_nm * 0.000001);
			center = new Vector2(count - 1, count - 1) / 2;
			double dx, dy;
			dmap.Foreach((x, y, d) => {

				dx =  x-center.X ;
				dy = y- center.Y  ;
				d[x, y] = dx * dx + dy * dy;
				posmap[x,y]=new Vector2 (dx, dy);

			});
		}

		public ComplexMap getH(double n)
		{
			if (H.ContainsKey(n))
			{
				return H[n];
			}
			else
			{
				double k2 = k * k * n * n;
				double fpi2 = 4 * Math.PI * Math.PI * df * df;
				ComplexMap H0 = new ComplexMap(count, count);
				H0.Foreach((x, y, d) => {
					d[x, y] = Math.Sqrt(k2 - fpi2 * (dmap[x, y]));
				});
				H.Add(n, H0);
				return H0;
			}
		}
	


		public double getFP(double tracePos=-9999,double rayHeight=0.01)
		{
			Ray ray = new Ray(new Vector3(rayHeight, 0, tracePos), new Vector3(0, 0, 1));
			foreach(SphericalSurface s in lens)
			{
				if(ray!=null)
				{
                  ray=s.getNextRay(ray);
				}
				
			}

			if(ray!=null&&(ray.direction.x!=0))
			{
				double k = ray.position.x / ray.direction.x;
				Vector3 pos = ray.position - ray.direction * k;
				return pos.z;
			}
			return tracePos;
		}
		public double getIP(double tracePos , double rayHeight = 0.002)
		{
			Ray ray = new Ray(new Vector3(0, 0, tracePos), new Vector3(rayHeight, 0, Math.Sqrt(1-rayHeight*rayHeight)));
			foreach (SphericalSurface s in lens)
			{
				if (ray != null)
				{
					ray = s.getNextRay(ray);
				}

			}

			if (ray != null && (ray.direction.x != 0))
			{
				double k = ray.position.x / ray.direction.x;
				Vector3 pos = ray.position - ray.direction * k;
				return pos.z;
			}
			return tracePos;
		}
		public double getRate(double tracePos, double rayHeight = 0.002)
		{
			double f=getIP(tracePos, rayHeight);
			Ray ray = new Ray(new Vector3(rayHeight, 0, tracePos), new Vector3(rayHeight, 0, Math.Sqrt(1 - rayHeight * rayHeight)));
			foreach (SphericalSurface s in lens)
			{
				if (ray != null)
				{
					ray = s.getNextRay(ray);
				}

			}

			if (ray != null && (ray.direction.x != 0))
			{
				double k = (f-ray.position.z) / ray.direction.z;
				Vector3 pos = ray.position + ray.direction * k;
				return pos.x/rayHeight;
			}
			return tracePos;
		}
		public List<SphericalSurface> lens = new List<SphericalSurface>();

		public POP addLens(SphericalSurface surface)
		{
			lens.Add(surface);
			update();
			return this;
		}

		public POP addLens(double r1, double d1, double r2, double d2, double n, double pos, double t)
		{
			bool pr = r1 > 0;
			lens.Add(new SphericalSurface(new Vector3(0, 0, pos), new Vector3(0, 0, pos + r1), d1, pr ? n : this.n, pr ? this.n : n));
		    pr= r2 > 0;
			lens.Add(new SphericalSurface(new Vector3(0, 0, pos+t), new Vector3(0, 0, pos+t + r2), d2, pr ? this.n : n, pr ? n : this.n));
			update(); 
			return this;
		}
		public POP insertLens(double r1, double d1, double r2, double d2, double n, double distance, double t)
		{

		return	addLens(r1, d1, r2, d2, n, getLastSurfacePosition() + distance, t);

		}


		public ComplexMap createPan  (double phySizeD,double E)
		{
			double r = phySizeD / 2/size*count;
			r *= r;

			ComplexMap map = new ComplexMap(count, count);
			map.Foreach((x, y, d) => {
				if (r > dmap[x,y])
				{
					d[x, y] = E;
				}
			});
			return map;
		}
		public ComplexMap createObject(double phySizeD, double E)
		{
			double r = phySizeD / 2 / size * count;
			r *= r;
			double k = size / count;
			
			ComplexMap map = new ComplexMap(count, count);
			map.Foreach((x, y, d) => {
				if (r > dmap[x, y])
				{
					d[x, y] = E * Complex.Exp(-Complex.I * this.k * dmap[x,y]);
				}
			});
			return map;
		}
		static Random random = new Random();
		public ComplexMap createRandomObject(double phySizeD, double E)
		{
			double r = phySizeD / 2 / size * count;
			r *= r;
			double k = size / count;

			ComplexMap map = new ComplexMap(count, count);
			map.Foreach((x, y, d) => {
				if (r > dmap[x, y])
				{
					d[x, y] = E * Complex.Exp(-Complex.I * random.NextDouble()*2*Math.PI);
				}
			});
			return map;
		}
		public double getLastSurfacePosition()
		{
			double pos = Double.NegativeInfinity;
			foreach(SphericalSurface spherical in lens)
			{
				if(pos<spherical.position.z)
				{
					pos = spherical.position.z;
				}
			}
			return pos;
		}
		public ComplexMap createPan(double phySizeD, double E, Vector3 direction)
		{
			direction = direction.nolrmalized();
			double r = phySizeD / 2 / size * count;
			r *= r;

			ComplexMap map = new ComplexMap(count, count);
			Vector2 v;
			Complex k0 = Complex.I * k * dk;
			map.Foreach((x, y, d) => {
				if (r > dmap[x, y])
				{
					v = posmap[x, y];
					d[x, y] = E*Complex.Exp(k0*(direction.x*v.X+direction.y*v.Y));
				}
			});
			return map;
		}
		public double getMaxAngle()
		{
			return Math.Asin(lambda * count * 0.5 / size) * 180 / Math.PI;
		}
		public Vector3 getPos(Vector3 pos,Vector3 dir,double fpos)
		{
			double cPos = pos.z;

			Ray ray = new Ray(pos, dir.nolrmalized());

			foreach(SphericalSurface s in lens)
			{
				Ray next = s.getNextRay(ray);
				if(next==null)
				{
					break;
				}
				else
				if( next.position.z>fpos)
				{
					break;
				}
				else
				{
					ray = next;
				}

				
			}

			return (fpos - ray.position.z) / ray.direction.z * ray.direction + ray.position;


		}
		public Vector2 toIMpos(Vector3 v)
		{
			return new Vector2(v.x/dk+center.X, v.y/dk+center.Y);
		}
		public ComplexMap getOutput(ComplexMap input, double inputPos, double ouputPos)
		{
			double currentPos = inputPos;
			ComplexMap current = input.Clone().FFT().fftShift();

			foreach (SphericalSurface s in lens)
			{
				if (s.position.z > currentPos && s.position.z < ouputPos)
				{
					current = goDistance(current, s.position.z - currentPos, s.center.z > s.position.z ? s.no : s.ni);
					currentPos = s.position.z;
					current = goLens(current, s);
				}
				else
				{
					break;
				}
			}
			if (currentPos <= ouputPos)
			{
				current = goDistance(current, ouputPos - currentPos, 1);
				return current.ifftShift().iFFT();
			}

			return null;
		}
		public ComplexMap[] getOutput(ComplexMap input, double inputPos, params double[] doubles)
		{
			double currentPos = inputPos;
			ComplexMap current = input.Clone().FFT().fftShift();
			ComplexMap[] map = new ComplexMap[doubles.Length];
			List<SphericalSurface> list = lens.ToList();
			for (int i = 0; i < doubles.Length; i++)
			{
				current = getOutputInF(current, list, currentPos, doubles[i],out list);
				if(current != null)
				{
					map[i] = current.ifftShift().iFFT();
					currentPos = doubles[i];
				}
				else
				{
					break;
				}
			}
			return map;
		}

		public ComplexMap getOutputInF(ComplexMap input, List<SphericalSurface> sphericals,double inputPos, double ouputPos,out List<SphericalSurface> sphericals_)
		{
			double currentPos = inputPos;
			ComplexMap current = input;
			int i = 0;

			foreach (SphericalSurface s in lens)
			{
				if (s.position.z > currentPos && s.position.z < ouputPos)
				{
					current = goDistance(current, s.position.z - currentPos, s.center.z > s.position.z ? s.no : s.ni);
					currentPos = s.position.z;
					current = goLens(current, s);
					i++;
				}
				else
				{
					break;
				}
			}
			sphericals_ = sphericals.GetRange(i, sphericals.Count - i);
			if (currentPos <= ouputPos)
			{
				current = goDistance(current, ouputPos - currentPos, 1);
			
				return current;
			}

			return null;
		}

		Dictionary<SphericalSurface,ComplexMap> map = new Dictionary<SphericalSurface, ComplexMap>();


	

		public double getSpotSize(ComplexMap map,int smooth=1)
		{
			double[] ds = map.scanRR();
			ds = ds.smooth(smooth).derivation().smooth(smooth);

			double min = Double.PositiveInfinity;
			double size = -1;
			
			for(int i=0;i<ds.Length;i++)

			{
				if (ds[i]<min)
				{
					min = ds[i];
					size = i * dk*2;
				}

			}
			return size;
		}

		public ComplexMap goLens(ComplexMap map, SphericalSurface lens)
		{
			double f;
			//f = (lens.ni - lens.no) / lens.cr;
			Ray ray = new Ray(new Vector3(0.01 * lens.hr, 0, lens.position.z - 10), new Vector3(0, 0, 1));
			ray = lens.getNextRay(ray);

			Vector3 pos = ray.position - (ray.position.x / ray.direction.x) * ray.direction;
			f = pos.z - lens.position.z;

			double r = lens.hr / size * count;
			r *= r;

			double n = lens.center.z > lens.position.z ? lens.ni : lens.no;

			ComplexMap m = map.Clone().ifftShift().iFFT();

			Complex k0 = -0.5 * k * n * dk * dk / f * Complex.I;
			ComplexMap mm = getLens(lens);
			m.Foreach((x, y, d) => {

				if (r > dmap[x, y])
				{
					//d[x, y] *= Complex.exp(k0 * dmap[x, y]);
					d[x, y] *= mm[x, y];
				}
				else
				{
					d[x, y] = 0;
				}

			});
			return m.FFT().fftShift();
		}
		ComplexMap getLens(SphericalSurface lens)
		{
			if (map.ContainsKey(lens))
			{
				return map[lens];
			}
			else
			{
				ComplexMap m

					= new ComplexMap(count, count);



				double r = lens.hr / size * count;
				r *= r;

				double n = lens.center.z > lens.position.z ? lens.ni : lens.no;
				Complex k0 = -0.5 * k * n * dk * dk  * Complex.I;
				double r2,f;
				m.Foreach((x, y, d) => {

					r2 = dmap[x, y];
					if (r > r2)
					{
					
						Ray ray = new Ray(new Vector3(Math.Sqrt(r2)*dk, 0, lens.position.z - 10000), new Vector3(0, 0, 1));
						ray = lens.getNextRay(ray);
						Vector3 pos = ray.position - (ray.position.x / ray.direction.x) * ray.direction;
						f = pos.z - lens.position.z;
						d[x, y] = Complex.exp(k0 * dmap[x, y]/f);

					}
					else
					{
						d[x, y] = 0;
					}

				});
				map.Add(lens, m);
				return m;
			}
		}
		ComplexMap getLens_o(SphericalSurface lens)
		{
			if(map.ContainsKey(lens))
			{
				return map[lens];
			}
			else
			{
				ComplexMap m
					
					= new ComplexMap(count,count);
				double r = lens.hr / size * count;
				r *= r;
				double crr = lens.cr * lens.cr;
				Complex ki =Complex.I* k * lens.ni;
				Complex ko = Complex.I * k * lens.no;
				double cr = lens.cr;
				double d0=cr-Math.Sqrt(crr-lens.hr*lens.hr);
				double r2,dO,dI;
				double kk = size / count;
				kk *= kk;
				m.Foreach((x, y, d) => {

					r2 = dmap[x, y];
					if (r > r2)
					{
						dO =cr- Math.Sqrt(crr - r2*kk);
						dI = d0 - dO;
						d[x, y] = Complex.exp(dO*ko+dI*ki);
					}
					else
					{
						d[x, y] = 0;
					}

				});
				map.Add(lens, m);
				return m;
			}
		}

		public ComplexMap goDistance(ComplexMap input, double distance, double n)
		{
			ComplexMap map = input.Clone();
			map.Foreach((x, y, data) => {

				data[x, y] = data[x, y] * Complex.exp(Complex.I * distance * getH(n)[x, y]);
			});
			return map;
		}

		public void update()
		{
			lens = (from SphericalSurface s in lens orderby s.position.z ascending select s).ToList();
		}


	}
	public interface ISurface
	{
		Vector3? getRayTrace(Vector3 pos, Vector3 direction,ref double d, double max = Double.PositiveInfinity);
		Ray getNextRay(Ray ray);
		
	}
}
