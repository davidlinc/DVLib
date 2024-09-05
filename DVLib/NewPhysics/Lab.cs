using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathBase;
using System.Threading.Tasks;

namespace NewPhysics
{
	class Lab
	{
	}
public	class ComplexVector3D
	{
public	readonly	Complex x;
		
public readonly Complex y;
		
public readonly Complex z;
		public ComplexVector3D(Complex x,Complex y,Complex z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public ComplexVector3D conjugate()
		{
			return new ComplexVector3D(x.conjugate(), y.conjugate(), z.conjugate());
			
		}

		
	 public	double length_2()
		{
			return x.length_2()+y.length_2()+z.length_2();
		}
		public double realPart()
		{
			return Math.Sqrt(x.realPart*x.realPart + y.realPart*y.realPart + z.realPart*z.realPart);
		}
		public double length()
		{
			return Math.Sqrt( x.length_2() + y.length_2() + z.length_2());
		}
		public static ComplexVector3D operator +(ComplexVector3D f1, ComplexVector3D f2)
		{
			return new ComplexVector3D(f1.x + f2.x, f1.y + f2.y, f1.z + f2.z);
		}
		public static ComplexVector3D operator -(ComplexVector3D f1, ComplexVector3D f2)
		{
			return new ComplexVector3D(f1.x - f2.x, f1.y - f2.y, f1.z - f2.z);
		}
		public static ComplexVector3D operator *(ComplexVector3D f1, Complex complex)
		{
			return new ComplexVector3D(f1.x*complex, f1.y * complex, f1.z * complex);
		}
		
		public static ComplexVector3D operator /(ComplexVector3D f1, Complex complex)
		{
			return new ComplexVector3D(f1.x / complex, f1.y * complex, f1.z /complex);
		}
		public static ComplexVector3D operator *( Complex complex,ComplexVector3D f1)
		{
			return new ComplexVector3D(f1.x * complex, f1.y * complex, f1.z * complex);
		}

		public static  implicit operator ComplexVector3D(Vector3 v)
		{
			return new ComplexVector3D(v.x, v.y, v.z);
		}
		}

	public delegate ComplexVector3D LightField(double x,double y,double z)
		;

	public delegate ComplexVector3D TimeLightField(double x, double y, double z, double t)
		;
	public class SphericalWave:PhysicsLightField
	{
		Vector3 position;
		double A;
		double K;
		public SphericalWave(Vector3 position,double k,double a)
		{
			this.position = position;

			this.K = k;
			A = a;
			U = (double x, double y, double z) =>
			  {
				  Vector3 p = new Vector3(x, y, z);
				  double r = p.distanceTo(this.position);
				  Vector3 dir = (p - position).nolrmalized();
				  Vector3 kr = dir * K;

				  return A / r * (e ^ (i * (kr.x * x + kr.y * y + kr.z * z))) * (ComplexVector3D)dir;

			  };
		}

	}
	public class PlaneWave:PhysicsLightField
	{
		Vector3 K;
		double A;
		ComplexVector3D direction;
		public PlaneWave(Vector3 k,double a)
		{
			this.A = a;
			K = k;
			direction = k.nolrmalized();
			U = (double x, double y, double z) => {
				return this.A *( e ^ (i * (K.x * x + K.y * y + K.z * z))) * direction;
			};
		}
		public PlaneWave(Vector3 dir,double k_, double A)
		{
			this.A = A;
			direction = dir.nolrmalized();
			K = dir.nolrmalized() * k_;

			
			U = (double x, double y, double z) => {
				return this.A * (e ^ (i * (K.x * x + K.y * y + K.z * z))) * direction;
			};
		}
	}
	public class PhysicsLightField
	{
		public static double C = 299792458;
		public static Complex i = Complex.I;
		public static Complex e = Complex.E;
	     internal	LightField U;
	    public double phase=0;

		
		public PhysicsLightField()
		{

		}
		public PhysicsLightField(LightField field)
		{
			U = field;
		}

		public static double nmLambdaToK(double lambda)
		{
			lambda = lambda / 1000000000;
			return Math.PI * 2 / lambda;
		}


		public static PhysicsLightField operator +(PhysicsLightField f1,PhysicsLightField f2)
		{
			return new PhysicsLightField((double x, double y, double z) => { return f1.getValue(x,y,z) + f2.getValue(x,y,z); });
		}
		public static PhysicsLightField operator -(PhysicsLightField f1, PhysicsLightField f2)
		{
			return new PhysicsLightField((double x, double y, double z) => { return f1.getValue(x, y, z) - f2.getValue(x, y, z); });
		}
		public static PhysicsLightField operator *(PhysicsLightField f1,Complex complex)
		{
			return new PhysicsLightField((double x, double y, double z) => { return f1.getValue(x, y, z)*complex; });
		}
		public static PhysicsLightField operator *(Complex complex,PhysicsLightField f1 )
		{
			return new PhysicsLightField((double x, double y, double z) => { return f1.getValue(x, y, z) * complex; });
		}
		public static PhysicsLightField operator /(PhysicsLightField f1, Complex complex)
		{
			return new PhysicsLightField((double x, double y, double z) => { return f1.getValue(x, y, z) / complex; });
		}
		public ComplexVector3D getValue(Vector3 r)
		{
			return U(r.x, r.y, r.z)*(e^(i*phase));
		}
	public	ComplexVector3D getValue(double x, double y, double z)
		{
			return U(x,y, z) * (e ^ (i * phase));
		}

	}
}
