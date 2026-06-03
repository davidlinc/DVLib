using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVOSLib;
using NewPhysics;
using Images;
using System.Xml;
using System.Runtime.Intrinsics;
using System.Reflection;
using System.IO;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace MathBase
{
    public struct Complex : IEquatable<Complex>, IByteArrayObject<Complex>,IStreamObject<Complex>
    {


        public double realPart;
        public double imaginaryPart;
        public static FieldInfo r;
        public static FieldInfo i;
        public static double PI = Math.PI;
        public static Complex I = new Complex(0, 1);
        public static Complex ONE = new Complex(1, 0);
        public static Complex ZERO = new Complex(0, 0);
        public static Complex E = new Complex(Math.E, 0);

        static Complex()
            {
			FieldInfo[] fieldInfos = typeof(Complex).GetFields();
            r = fieldInfos[0];
            i = fieldInfos[1];
            }

        public Complex
        (double r, double i)
        {
            this.realPart = r;
            this.imaginaryPart = i;
        }
		public Complex
		   (Complex raw)
		{
			this.realPart = raw.realPart;
			this.imaginaryPart = raw.imaginaryPart;
		}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.realPart + b.realPart, a.imaginaryPart + b.imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator ++(Complex a)
        {
            return new Complex(a.realPart + 1, a.imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator --(Complex a)
        {
            return new Complex(a.realPart - 1, a.imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.realPart - b.realPart, a.imaginaryPart - b.imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex add(Complex b)
        {
            return new Complex(realPart + b.realPart, imaginaryPart + b.imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex reduce(Complex b)
        {
            return new Complex(realPart - b.realPart, imaginaryPart - b.imaginaryPart);
        }
        public static double p = Math.PI / 180;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double cosAngle(double angle)
		{
            return Math.Cos(p* angle);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double sinAngle(double angle)
        {
            return Math.Sin(p * angle);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex mul(Complex b)
        {
            return new Complex(realPart * b.realPart - imaginaryPart * b.imaginaryPart, realPart * b.imaginaryPart + imaginaryPart * b.realPart)
       ;
        }

		static Vector256<double>  mask = Vector256.Create(0.0, -0.0, 0.0, -0.0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Complex mul2(Complex b)
        {
			var M1 = Vector256.Create(realPart,imaginaryPart,b.realPart,b.imaginaryPart);
			var M2 = Avx2.Permute4x64(M1, 0b11101110);
		    M1 = Avx2.Permute4x64(M1, 0b01000100);
			var M3 = Avx.Multiply(Avx.Xor(M1, mask), M2);
			var M4 = Avx.Multiply(M1, Avx2.Permute4x64(M2, 0b10110001));
            M1 = Avx.HorizontalAdd(M3, M4);
			return Unsafe.Read<Complex>(&M1);
		}

        public static double dot(Span<double> a, Span<double> b)
        {
            double r = 0;
            for (var i = 0; i < a.Length; i++) { 
            
            r+= a[i] * b[i];
            }
            return r;
        }

		public static double dot2(Span<double> a, Span<double> b )
        {
            var M1 = Vector256<double>.Zero;

            int left = a.Length % 4;
            double result = 0;
            var sa = MemoryMarshal.Cast<double, Vector256<double>>(a);
			var sb = MemoryMarshal.Cast<double, Vector256<double>>(b);
            
			for (int i=0; i<sa.Length; i++)
            {
                M1 =Avx.Add( Avx.Multiply(sa[i], sb[i]),M1);
            }
            result = M1.GetElement(0)+M1.GetElement(1)+M1.GetElement(2)+M1.GetElement(3);
            if(left>0)
            {
                for (int i=0,ind=a.Length-1;i<left;i++,ind--)
                {

                    result += a[ind] * b[ind];
                }
            }

            return result;
        }
		public unsafe static Complex[] mul2(Complex[] a, Complex[] b)
        {
            Complex[] re = new Complex[a.Length];
			int li = a.Length - 1;
			
                var M1 = new Vector256<double>();
                var M2 = new Vector256<double>();

            ReadOnlySpan<Vector256<double>> a_=MemoryMarshal.Cast<Complex,Vector256<double>>(a);
			ReadOnlySpan<Vector256<double>> b_ = MemoryMarshal.Cast<Complex, Vector256<double>>(b);
            Span<Vector256<double>> span = MemoryMarshal.Cast<Complex, Vector256<double>>(re);
			var mask= Vector256.Create(0.0,-0.0,0.0,-0.0);
                int t = a.Length>>1;
                int left = a.Length - (t<<1 );
				for (int i = 0; i < t; i++)
					{
				span[i] = Avx.HorizontalAdd(Avx.Multiply(Avx.Xor(a_[i], mask), b_[i]), Avx.Multiply(a_[i], Avx2.Permute4x64(b_[i], 0b10110001)));
				}
                if(left>0)
                {
                    re[li] = a[li] * b[li];
                }
            return re;
        }
        public static Complex test;
		public static Vector256<double> test2;
		public unsafe static Complex[] mul(Complex[] a, Complex[] b)
		{
			Complex[] re = new Complex[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                re[i]= a[i]*b[i];
            }

            return re;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator *(Complex a, Complex b)
        {
          return new Complex(a.realPart * b.realPart - a.imaginaryPart * b.imaginaryPart, a.realPart * b.imaginaryPart + a.imaginaryPart * b.realPart);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex scale(double f)
        {
            return new Complex(realPart * f, imaginaryPart * f);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Complex(double s)
        {
            return new Complex(s, 0);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex div(Complex b)
        {
            return this.mul(b.conjugate()).scale(1.0 / b.length_2());
        }

        public static Complex operator /(Complex a, Complex b)
        {
            double d = 1 / (b.realPart * b.realPart + b.imaginaryPart * b.imaginaryPart);
            return new Complex((a.realPart * b.realPart + a.imaginaryPart * b.imaginaryPart) * d,
                (a.imaginaryPart * b.realPart - a.realPart * b.imaginaryPart) * d
                );
        }
        public static Complex operator -(Complex a)
        {
            return new Complex(-a.realPart, -a.imaginaryPart);
        }
        public static Complex exp(Complex complex)
        {
            return new Complex(Math.Cos(complex.imaginaryPart), Math.Sin(complex.imaginaryPart)).scale(Math.Exp(complex.realPart));
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex conjugate()
        {
            return new Complex(realPart, -imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double length()
        {

            return Math.Sqrt(realPart * realPart + imaginaryPart * imaginaryPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double length_2()
        {

            return realPart * realPart + imaginaryPart * imaginaryPart;
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double length_4()
        {
            double v = realPart * realPart + imaginaryPart * imaginaryPart;
            return v*v ;
        }
        public override string ToString()
        {
            
            return "("+realPart+","+imaginaryPart+")";
        }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double mu()
        {
            return Math.Atan2(imaginaryPart, realPart);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex log()
        {
            return new Complex(Math.Log(length()), mu());
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex log(Complex a, Complex b)
        {
            return exp(b) / exp(a);

        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Complex Pow(double comp)

        {

            double ther = length();

            double sita = mu();

            double R = Math.Pow(ther, comp) * Math.Cos(comp * sita);

            double I = Math.Pow(ther, comp) * Math.Sin(comp * sita);

            return new Complex(R, I);

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Complex other)
        {
            return realPart == other.realPart && imaginaryPart == other.imaginaryPart;
        }



		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Complex complex, Complex complex1)
        {
            return complex.realPart == complex1.realPart && complex.imaginaryPart == complex1.imaginaryPart;
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Complex complex, Complex complex1)
        {
            return complex.realPart != complex1.realPart || complex.imaginaryPart != complex1.imaginaryPart;
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex operator ^(Complex complex, Complex power)
        {
            if (complex == E)
            {
                return exp(power);
            }

            return exp(complex.log() * power);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]

		public static Complex Exp(Complex c)
        {
            double amplitude = Math.Exp(c.realPart);
            double cr = amplitude * Math.Cos(c.imaginaryPart);
            double ci = amplitude * Math.Sin(c.imaginaryPart);
            return new Complex(cr, ci);//保留四位小数输出
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Complex Exp(double im)
        {

            double cr = Math.Cos(im);
            double ci = Math.Sin(im);
            return new Complex(cr, ci);
        }

		public byte[] getBytes(byte[] outArray)
		{
            byte[] b = BitConverter.GetBytes(realPart);
                b.CopyTo(outArray,0);
            b = BitConverter.GetBytes(imaginaryPart);
            Array.Copy(b, 0, outArray, 8, 8);
            return outArray;
                ;
		}

		public byte[] getBytes()
		{
            byte[] outArray = new byte[16];
            byte[] b = BitConverter.GetBytes(realPart);
            b.CopyTo(outArray, 0);
            b = BitConverter.GetBytes(imaginaryPart);
            Array.Copy(b, 0, outArray, 8, 8);
            return outArray;
        }

		public  Complex  readBytes(byte[] bytes, int offset = 0)
		{

            r.SetValue(this, BitConverter.ToDouble(bytes, offset));
            i.SetValue(this, BitConverter.ToDouble(bytes, offset + 8));
            return this;
		}
        public static Complex fromBytes(byte[] bytes, int offset = 0)
        {
            return new Complex(BitConverter.ToDouble(bytes, offset), BitConverter.ToDouble(bytes, offset + 8));
         
        }

		public void writeStream(Stream stream)
		{
            stream.Write(getBytes(), 0, 16);
		}

		public void writeStream(Stream stream, byte[] buffer)
		{
            buffer = getBytes(buffer);
            stream.Write(buffer, 0, buffer.Length);
		}

		public Complex readStream(Stream stream)
		{
            byte[] bytes = new byte[16];
            stream.Read(bytes, 0, 16);
            return readBytes(bytes);
        }

		public Complex readStream(Stream stream, byte[] buffer)
		{
            stream.Read(buffer, 0, 16);
            return readBytes(buffer);
		}

        public static Complex fromStream(Stream stream)
		{
            byte[] bytes = new byte[16];
            stream.Read(bytes, 0, 16);
            Complex result = new Complex(BitConverter.ToDouble(bytes,0), BitConverter.ToDouble(bytes,  8));
            return result;
		}
        public static Complex fromStream(Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, 16);
            Complex result = new Complex(BitConverter.ToDouble(buffer, 0), BitConverter.ToDouble(buffer, 8));
            return result;
        }
    }
    public struct Vector3i : ICopyObject<Vector3i>
    {

        public readonly int x;
        public readonly int y;
        public readonly int z;
        public Vector3i(double x, double y, double z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3i(int color)
        {
            x = (color >> 16)
                    & 0xff;
            y = (color >> 8)
         & 0xff;
            z = (color)
        & 0xff;
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3i operator *(double a, Vector3i b)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }
        public static Vector3i operator *(int a, Vector3i b)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }
        public static Vector3i operator *(Vector3i b, double a)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }

        public double[] split(Vector3i x, Vector3i y)
        {

            if ((x.x == 0 && y.x == 0))
            {
                double x1 = x.z, x2 = y.z, x0 = this.z;
                double y1 = x.y, y2 = y.y, y0 = this.y;

                double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
                double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
                return new double[] { a, b };
            }
            if ((x.y == 0 && y.y == 0))
            {
                double x1 = x.z, x2 = y.z, x0 = this.z;
                double y1 = x.x, y2 = y.x, y0 = this.x;

                double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
                double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
                return new double[] { a, b };
            }
            else
            {
                double x1 = x.x, x2 = y.x, x0 = this.x;
                double y1 = x.y, y2 = y.y, y0 = this.y;

                double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
                double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
                return new double[] { a, b };
            }

        }
        public static Vector3i operator /(Vector3i b, double a)
        {
            return new Vector3i(b.x / a, b.y / a, b.z / a);
        }
        public static Vector3i operator *(Vector3i b, int a)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }
        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3i operator -(Vector3i a)
        {
            return new Vector3i(-a.x, -a.y, -a.z);
        }
        public static implicit operator Vector3i(Color32ARGB color)
        {
            return new Vector3i(color.r, color.g, color.b);
        }
        public static implicit operator Color32ARGB(Vector3i color)
        {
            return new Color32ARGB((int)color.x, (int)color.y, (int)color.z);
        }
        public double length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double length_2()
        {
            return x * x + y * y + z * z;
        }

        public Vector3i scale(double d)
        {
            return new Vector3i(x * d, y * d, z * d);
        }

        public Vector3i add(Vector3i other)
        {
            return new Vector3i(x + other.x, y + other.y, z + other.z);
        }
        public Vector3i add(int x, int y, int z)
        {
            return new Vector3i(x + this.x, y + this.y, z + this.z);
        }

        public Vector3i reduce(Vector3i other)
        {
            return new Vector3i(x - other.x, y - other.y, z - other.z);
        }

        public double dot(Vector3i other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public double distanceTo(Vector3i v)
        {
            return this.reduce(v).length();
        }
        public double cos(Vector3i v)
        {
            return dot(v) / (length() * v.length());
        }
        public double distance_2To(Vector3i v)
        {
            return reduce(v).length_2();
        }
        public Vector3i cross(Vector3i other)
        {
            return new Vector3i(y * other.z - z * other.y, z * other.x - x * other.z, x * other.y - y * other.x);
        }


        public Vector3i normalized()
        {
            if (x == 0 && y == 0 && z == 0)
            {
                return new Vector3i(0, 0, 0);
            }
            double l = length();
            return new Vector3i(x / l, y / l, z / l);
        }

        public Vector3i copy()
        {
            return new Vector3i(x, y, z);
        }
    }
    public struct Vector3 : ICopyObject<Vector3>, IxmlObject<Vector3>
    {
  public double x;
  public double y;
  public double z;
     
        public  bool isZore{get{return  x == 0 && z == 0 && y != 0;} }

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public bool isOnY { get { return x == 0 && z == 0 && y != 0; } }
        public bool isOnX { get { return x != 0 && z == 0 && y == 0; } }
        public bool isOnZ { get { return x == 0 && z != 0 && y == 0; } }
      

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3(int color)
		{
            x = (color >> 16)
                    & 0xff;
           y = (color >> 8)
        & 0xff;
            z = (color)
        & 0xff;
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(double a, Vector3 b)
        {
            return new Vector3(a * b.x, a * b.y, a * b.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(int a, Vector3 b)
        {
            return new Vector3(a * b.x, a * b.y, a * b.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(Vector3 b, double a)
        {
            return new Vector3(a * b.x, a * b.y, a * b.z);
        }

        public Matrix GetMatrix()
		{
            Matrix m = new Matrix(1, 3);
            m.SetData(new double[] { x, y, z });
            return m;
		}

        public double[] split_M(Vector3 x, Vector3 y)
		{
            Vector3 v = x.cross(y);
            double[] d = new double[] { x.x, x.y, x.z, y.x, y.y, y.z, v.x, v.y, v.z };
            Matrix m = new Matrix(3);
            m.SetData(d);

            Matrix n = GetMatrix();

            if(m.InvertGaussJordan())
			{
                return n * m;
			}
            return new double[] {0,0};

        }
        public double[] split(Vector3 x,Vector3 y)
		{
            




           if((x.x==0&&y.x==0))
			{
                double x1 = x.z, x2 = y.z, x0 = this.z;
                double y1 = x.y, y2 = y.y, y0 = this.y;

                double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
                double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
                return new double[] { a, b };
            }
            if ((x.y == 0 && y.y == 0))
            {
                double x1 = x.z, x2 = y.z, x0 = this.z;
                double y1 = x.x, y2 = y.x, y0 = this.x;

                double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
                double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
                return new double[] { a, b };
            }
            else
			{
            double x1 = x.x, x2 = y.x, x0 = this.x;
            double y1 = x.y, y2 = y.y, y0 = this.y;
           
            double a = (x0 * y2 - y0 * x2) / (y2 * x1 - x2 * y1);
            double b = (x0 * y1 - x1 * y0) / (x2 * y1 - x1 * y2);
			return new double[]{ a,b};
			}

		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator /(Vector3 b, double a)
        {
            return new Vector3(b.x / a, b.y / a, b.z / a);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator *(Vector3 b, int a)
        {
            return new Vector3(a * b.x, a * b.y, a * b.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector3(Color32ARGB color)
        {
            return new Vector3(color.r, color.g, color.b);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector3((double ,double,double) vec)
		{
            return new Vector3(vec.Item1, vec.Item2, vec.Item3);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Color32ARGB(Vector3 color)
        {
            return new  Color32ARGB ((int)color.x, (int)color.y, (int)color.z);
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double length()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double length_2()
        {
            return x * x + y * y + z * z;
        }

        public Vector3 scale(double d)
        {
            return new Vector3(x * d, y * d, z * d);
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 add(Vector3 other)
        {
            return new Vector3(x + other.x, y + other.y, z + other.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 add(double x,double y,double z)
        {
            return new Vector3(x + this.x, y + this.y, z +this.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 reduce(Vector3 other)
        {
            return new Vector3(x - other.x, y - other.y, z - other.z);
        }

		public override string ToString()
		{
			return "("+x+","+y+","+z+")";
		}
		public double dot(Vector3 other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public double distanceTo(Vector3 v)
        {
            return this.reduce(v).length();
        }
        public double cos(Vector3 v)
		{
            return dot(v) / (length() * v.length());
		}
        public double distance_2To(Vector3 v)
        {
            return reduce(v).length_2();
        }
        public double size()
		{
            return Math.Abs(x * y * z);
		}
        public double simpleSize()
        {
            return Math.Abs(x)+ Math.Abs(y)+ Math.Abs(z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector3 cross(Vector3 other)
        {
            return new Vector3(y * other.z - z * other.y, z * other.x - x * other.z, x * other.y - y * other.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 normalized()
        {
           
            double l = Math.Sqrt(x*x+y*y+z*z);
            return new Vector3(x / l, y / l, z / l);
        }

        public Vector3 getPosRHT23D()
		{
            return new Vector3(Math.Cos(z) * x, Math.Sin(z) * x, y);
		}
        public Vector3 copy()
        {
            return new Vector3(x, y, z);
        }

		public XmlElement writeXml(XmlElement element)
		{
            element.SetAttribute("X", x.ToString());
            element.SetAttribute("Y", y.ToString());
            element.SetAttribute("Z", z.ToString());
            return element;
		}

		public Vector3 readXml(XmlElement element)
		{
            return new Vector3(double.Parse(element.GetAttribute("X")), double.Parse(element.GetAttribute("Y")), double.Parse(element.GetAttribute("Z")));
		}
	}
    public struct Vector2i
    {
        public readonly int X;
        public readonly int Y;
        public Vector2 Vector2{ get { return new Vector2(X, Y); } }
        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2i operator *(double a, Vector2i b)
        {
            return new Vector2i(a * b.X, a * b.Y);
        }
        public static Vector2i operator *(int a, Vector2i b)
        {
            return new Vector2i(a * b.X, a * b.Y);
        }
        public static Vector2i operator *(Vector2i b, double a)
        {
            return new Vector2i(a * b.X, a * b.Y);
        }
        public static Vector2i operator /(Vector2i b, double a)
        {
            return new Vector2i(b.X / a, b.Y / a);
        }
        public static Vector2i operator *(Vector2i b, int a)
        {
            return new Vector2i(a * b.X, a * b.Y);
        }
        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2i operator -(Vector2i a)
        {
            return new Vector2i(-a.X, -a.Y);
        }
        //构造函数
        public Vector2i(double x, double y)
        {
            this.X = (int)x;
            this.Y = (int)y;
        }
        public Vector2i(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        //运算

        public double angle()
        {
            Vector2i vector2 = this;
            if (vector2.X == 0)
            {
                if (vector2.Y == 0)
                {
                    return 0;
                }
                else if (vector2.Y > 0)
                {
                    return 90;
                }
                else
                {
                    return 270;
                }
            }
            else if (vector2.Y == 0)
            {
                if (vector2.X > 0)
                {
                    return 0;
                }
                else
                {
                    return 180;
                }
            }
            else
            {


                return Math.Atan2(vector2.Y, vector2.X) / Math.PI * 180;
            }
        }

        public Vector2i row(double angle)
        {
            Vector2i point = this;
            return new Vector2i(point.length() * Math.Cos((angle + point.angle()) / 180 * Math.PI), point.length() * Math.Sin((angle + point.angle()) / 180 * Math.PI));
        }

        //取值
        public double distance(Vector2i v2)
        {
            return new Vector2i(X - v2.X, Y - v2.Y).length();
        }

        public double product(Vector2i v2)
        {
            return v2.X * X + v2.Y * Y;
        }

        public double cos(Vector2i v2)
        {
            return product(v2) / length() / v2.length();
        }

        public double length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double length_2()
        {
            return X * X + Y * Y;
        }

        public Vector2i normalized()//返回单位向量
        {
            if (length() != 0)
                return new Vector2i(X / length(), Y / length());
            else
                return new Vector2i(0, 0);


        }
        //转化

        public Vector2i scale(double d)
        {
            return new Vector2i(X * d, Y * d);
        }

        public Vector2i add(Vector2i other)
        {
            return new Vector2i(X + other.X, Y + other.Y);
        }
        public Vector2i add(int x,int y)
        {
            return new Vector2i(X + x, Y +y);
        }
        public Vector2i reduce(Vector2i other)
        {
            return new Vector2i(X - other.X, Y - other.Y);
        }

        public double dot(Vector2i other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2i? cross(Vector2i other)
        {
            return null;
        }

        public String pos()
        {
            return "(" + X + "," + Y + ")";
        }

    }
    public struct Vector2:IxmlObject<Vector2>
    {
        public double X;
        public double Y;
        public static readonly  Vector2 Xaxis  = new Vector2(1, 0);
        public static readonly Vector2 Yaxis  = new Vector2(0, 1);

		public static readonly Vector2 One = new Vector2(1,1);
		public static readonly Vector2 Zero  = new Vector2(0, 0);
		public Vector2i Vector2i { get { return new Vector2i((int)X, (int)Y); } } 
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }
        public static implicit operator Vector2((double, double) vec)
        {
            return new Vector2(vec.Item1, vec.Item2);
        }
        public static Vector2 operator *(double a, Vector2 b)
        {
            return new Vector2(a * b.X, a * b.Y);
        }
        public static Vector2 operator *(int a, Vector2 b)
        {
            return new Vector2(a * b.X, a * b.Y);
        }
        /// <summary>
        /// 标量乘法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X * b.X, a.Y * b.Y);
		}
		public static Vector2 operator *(Vector2 b, double a)
        {
            return new Vector2(a * b.X, a * b.Y);
        }
        public static Vector2 operator /(Vector2 b, double a)
        {
            return new Vector2(b.X / a, b.Y / a);
        }
		public Vector2 scale(double x, double y)
		{
			return new Vector2(this.X * x, this.Y *
				y);
		}
		public static Vector2 operator *(Vector2 b, int a)
        {
            return new Vector2(a * b.X, a * b.Y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.X, -a.Y);
        }
        //构造函数
        public Vector2(double x=0, double y=0)
        {
            this.X = x;
            this.Y = y;
        }
        
        //运算

        public double angle()
        {
            Vector2 vector2 = this;
            if (vector2.X == 0)
            {
                if (vector2.Y == 0)
                {
                    return 0;
                }
                else if (vector2.Y > 0)
                {
                    return 90;
                }
                else
                {
                    return 270;
                }
            }
            else if (vector2.Y == 0)
            {
                if (vector2.X > 0)
                {
                    return 0;
                }
                else
                {
                    return 180;
                }
            }
            else
            {


                return Math.Atan2(vector2.Y, vector2.X)  * 180/ Math.PI;
            }
        }

        public Vector2 row(double angle)
        {
            Vector2 point = this;
            double v = point.value();
            return new Vector2( v* Math.Cos((angle + point.angle()) * Math.PI/ 180 ),v * Math.Sin((angle + point.angle())* Math.PI / 180 ));
        }
        public static Vector2 fromAngle(double length,double angle)
		{
            return new Vector2(length * Math.Cos((angle )), length * Math.Sin((angle ) ));

        }

        public static Vector2 fromAngleDegree(double length, double angle)
        {
            return new Vector2(length * Math.Cos((angle) * MathHelper.degree2radian), length * Math.Sin((angle) * MathHelper.degree2radian));

        }

        public Vector2i getClost()
		{
            return new Vector2i(Math.Round(X), Math.Round(Y));
		}
        public Vector2i getVector2i()
        {
            return new Vector2i(Math.Round(X), Math.Round(Y));
        }
        //取值
        public double distance(Vector2 v2)
        {
            return new Vector2(X - v2.X, Y - v2.Y).value();
        }

        public double product(Vector2 v2)
        {
            return v2.X * X + v2.Y * Y;
        }

        public double cos(Vector2 v2)
        {
            return product(v2) / value() / v2.value();
        }

        public double value()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double length_2()
        {
            return X * X + Y * Y;
        }

        public Vector2 normalized()//返回单位向量
        {
            if (value() != 0)
                return new Vector2(X / value(), Y / value());
            else
                return new Vector2(0, 0);


        }
        //转化
        public static implicit operator Vector2(int i)
        {
            return new Vector2(i, i);
        }
        public static implicit operator Vector2(Vector2i v)
        {
            return new Vector2(v.X, v.Y);
        }
        public Vector2 scale(double d)
        {
            return new Vector2(X * d, Y * d);
        }

        public Vector2 add(Vector2 other)
        {
            return new Vector2(X + other.X, Y + other.Y);
        }
        public Vector2 add(double x, double y)
        {
            return new Vector2(X + x, Y + y);
        }
        public Vector2 reduce(Vector2 other)
        {
            return new Vector2(X - other.X, Y - other.Y);
        }
        public Vector2 reduce(double x, double y)
        {
            return new Vector2(X - x, Y - y);
        }
        public double dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        public Vector2? cross(Vector2 other)
        {
            return null;
        }
        public override string ToString()
        {
            return pos();
        }
        public String pos()
        {
            return "(" + X + "," + Y + ")";
        }

        public XmlElement writeXml(XmlElement element)
        {
            element.SetAttribute("X", X.ToString());
            element.SetAttribute("Y",Y.ToString());
            return element;
        }

        public Vector2 readXml(XmlElement element)
        {
            return new Vector2(double.Parse(element.GetAttribute("X")), double.Parse(element.GetAttribute("Y")));
        }

		internal double CrossProduct(Vector2 pB)
		{
            return X * pB.Y - Y * pB.X;
		}
	}
    public struct Triangle2D:ICopyObject<Triangle2D>
    {
        public static readonly double ONETHREE = 1.0 / 3;
        public Vector2 p1 { get; private set; } = new Vector2(Double.NaN);
        public Vector2 p2{ get; private set; }
        public Vector2 p3 { get; private set; }
    Vector2 position;
        public Triangle2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            position = p1.add(p2.add(p3)).scale(ONETHREE);
        }

        
	public	Triangle2D copy()
		{
            Triangle2D d2 = new Triangle2D(p1, p2,p3);
            d2.position = position;
            return d2;
		}
        public bool isEmpty()
        {
            return double.IsNaN(p1.X);
        }
     public   bool IsIn( Vector2 pointP)
    {
        Vector2 PA = p1-(pointP);
        Vector2 PB =p2-(pointP);
        Vector2 PC =p3-(pointP);
        double t1 = PA.CrossProduct(PB);
        double t2 = PB.CrossProduct(PC);
        double t3 = PC.CrossProduct(PA);
        return t1* t2 >= 0 && t1* t3 >= 0;
    }

    }
    public struct DistanceEqution
    {
        double a;
        double b;
        double c;

        public DistanceEqution(double k1, double k2, double k3)
        {
            a = k1;
            b = k2;
            c = k3;
        }

        public double Value(double x)
        {
            return a * x * x + b * x + c;
        }

        public double[] max(double min, double max)
        {
            if (max < min)
            {
                double temp = max;
                max = min;
                min = temp;
            }
            if (Ultra() > min && Ultra() < max)
            {
                double[] m = { min, Value(min) }, M = { max, Value(max) }, U = { Ultra(), Value(Ultra()) };
                if (m[1] < M[1])
                {
                    if (M[1] < U[1])
                    {
                        return U;
                    }
                    else
                    {
                        return M;
                    }
                }
                else
                {
                    if (m[1] < U[1])
                    {
                        return U;
                    }
                    else
                    {
                        return m;
                    }
                }

            }
            else
            {
                double[] m = { min, Value(min) }, M = { max, Value(max) };
                if (m[1] > M[1])
                {
                    return m;
                }
                else
                {
                    return M;
                }
            }
        }


        public double minSolution(double value)
        {
            double a = this.a;
            double b = this.b;
            double c = this.c - value;
            double derta = b * b - 4 * a * c;
            if (derta >= 0)
            {
                if (a > 0)
                    return ((-b - Math.Sqrt(derta)) * 0.5 / a);
                else if (a < 0) return ((-b + Math.Sqrt(derta)) * 0.5 / a);
                else if (b != 0)
                {
                    return c / -b;
                }
            }
            return Double.NaN;
        }
        public double[] getSolutions(double value)
		{
            double a = this.a;
            double b = this.b;
            double c = this.c - value;
            double derta = b * b - 4 * a * c;
            double k1, k2, v;
            if (derta >= 0)
            {
                if (a > 0)
                {
                    k1 = 0.5 / a;
                     k2 = Math.Sqrt(derta);

                    return new double[] { ((-b - k2) * k1), ((-b + k2) * k1) };
                }

                else if (a < 0)
                {k1 = 0.5 / a;
                    k2 = Math.Sqrt(derta);

                    return new double[] { ((-b + k2) * k1), ((-b - k2) * k1) };

                }
                else if (b != 0)
                {
                    v = c / -b;
                    return new double[] {v,v};
                }
            }
            return null;
        }
        public double MaxSolution(double value)
        {
            double a = this.a;
            double b = this.b;
            double c = this.c - value;
            double derta = b * b - 4 * a * c;
            if (derta >= 0)
            {
                if (a > 0)
                    return ((-b+ Math.Sqrt(derta)) * 0.5 / a);
                else if (a < 0) return ((-b - Math.Sqrt(derta)) * 0.5 / a);
                else if (b != 0)
                {
                    return c / -b;
                }
            }
            return Double.NaN;
        }
        public double[] min(double min, double max)
        {


            if (max < min)
            {
                double temp = max;
                max = min;
                min = temp;
            }
            if (Ultra() > min && Ultra() < max)
            {
                double[] m = { min, Value(min) }, M = { max, Value(max) }, U = { Ultra(), Value(Ultra()) };
                if (m[1] > M[1])
                {
                    if (M[1] > U[1])
                    {
                        return U;
                    }
                    else
                    {
                        return M;
                    }
                }
                else
                {
                    if (m[1] > U[1])
                    {
                        return U;
                    }
                    else
                    {
                        return m;
                    }
                }

            }
            else
            {
                double[] m = { min, Value(min) }, M = { max, Value(max) };
                if (m[1] < M[1])
                {
                    return m;
                }
                else
                {
                    return M;
                }
            }
        }

        public double Ultra()
        {
            return -(b / 2 / a);
        }

    }
    public class Circular:Plane,IxmlObject<Circular>
	{
        public static Circular Empty = new Circular(Vector3.Zero, Vector3.Zero, 0);
        public double r { get; internal set; }
        public Circular(Vector3 pos, Vector3 dir,double r):base(pos,dir)
		{
            this.r = r;
		}
        new  public XmlElement writeXml(XmlElement element)
        {
            element.SetAttribute("R", r.ToString());
            XmlElement pos = element.OwnerDocument.CreateElement("Position");
            position.writeXml(pos);
            XmlElement dir = element.OwnerDocument.CreateElement("Dirction");
            direction.writeXml(dir);
            element.AppendChild(pos);
            element.AppendChild(dir);
            return element;
        }
        new public Circular readXml(XmlElement element)
		{
            Plane p = base.readXml(element);
            double r = double.Parse(element.GetAttribute("R"));
            return new Circular(p.position, p.direction, r);
		}
	}
    public class  Plane:IxmlObject<Plane>
	{
        public Vector3 position { get; internal set; }
        public Vector3 direction { get; internal set; }

        double dl2_=-1;

        double dl2
        {
            get
            {
                if (dl2_ == -1)
                {
                    dl2_ = direction.length_2();
                }
                return dl2_;
            }
        }


        public (Vector2 RH,Vector3 direction) getRH(Vector3 vector3)
        {
            double xp = position.x;
            double yp = position.y;
            double zp = position.z;

            double xd = direction.x;
            double yd = direction.y;
            double zd = direction.z;

            double x = vector3.x;
            double y = vector3.y;
            double z = vector3.z;

            double h = (x * xd + y * yd + z * zd - xd * xp - yd * yp - zd * zp) / direction.length_2();
            Vector3 pos = position + h * direction;
            pos = vector3 - pos;
            double r = pos.length();
            return (new Vector2(r, h),pos.normalized());
        }
    

 
        public Vector3 getRHTheta(Vector3 vector,double theta)
		{
            Vector2 RH = getRH(vector).RH;
            return new Vector3(RH.X, RH.Y, theta);
		}

        public static Plane Empty = new Plane(Vector3.Zero, Vector3.Zero);
        public Plane(Vector3 pos,Vector3 dir)
		{
            position = pos;
            direction = dir.normalized();
		}

        public Vector3? getCrossPoint(Vector3 position, Vector3 direction, ref double distance)
        {
            double l2 = direction.dot(this.direction);
            if (l2 == 0)
            {
                return null;
            }
            Vector3 dp = this.position.reduce(position);
            double l1 = dp.dot(this.direction);

            double n = l1 / l2;
            distance = n;

            Vector3 result = position.add(direction.scale(n));


       

                return result;
            
        }

		public XmlElement writeXml(XmlElement element)
		{
            XmlElement pos = element.OwnerDocument.CreateElement("Position");
            position.writeXml(pos);
            XmlElement dir = element.OwnerDocument.CreateElement("Dirction");
            direction.writeXml(dir);
            element.AppendChild(pos);
            element.AppendChild(dir);
            return element;
		}

		public Plane readXml(XmlElement element)
		{
            Vector3 dir=Vector3.Zero;
            Vector3 pos=Vector3.Zero;
            foreach(XmlElement element1 in element.ChildNodes)
			{
                if(element1.Name.Equals("Position"))
				{
                    pos = Vector3.Zero.readXml(element1);
				}
                else if(element1.Name.Equals("Dirction"))
				{
                    dir= Vector3.Zero.readXml(element1); 
                }
			}
            return new Plane(pos, dir);
		}
	}

    public class Triangle : ICopyObject<Triangle>{

        public static readonly double ONETHREE = 1.0 / 3;
      internal  Vector3 p1;
        internal Vector3 p2;
        internal Vector3 p3;

        public Vector3 position { get; internal set; }
        public Vector3 direction { get; internal set; }
        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        position = p1.add(p2).add(p3).scale(ONETHREE);
        direction = p1.reduce(p2).cross(p2.reduce(p3)).normalized();

    }
        public Plane GetPlane()
		{
            return new Plane(position, direction);
		}
        public Circular GetCircular()
		{
            Ray3d r = new Ray3d((p1 + p2) / 2, (p1 - p2).cross(direction));
            Ray3d r2 = new Ray3d((p1 + p3) / 2, (p1 - p3).cross(direction));
            Vector3 pos = r.getTarget(r2);
            double rudis = pos.distanceTo(p1);
            return new Circular(pos, direction, rudis);
        }
        public virtual void  setPos(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            position = p1.add(p2).add(p3).scale(ONETHREE);
            direction = p1.reduce(p2).cross(p2.reduce(p3)).normalized();
       
        }
        public Vector3 getP1()
    {
        return p1;
    }

    public Vector3 getP2()
    {
        return p2;
    }

    public Vector3 getP3()
    {
        return p3;
    }

    public void setPosition(Vector3 position)
    {
        Vector3 dp = position.reduce(this.position);
        p1 = p1.add(dp);
        p2 = p2.add(dp);
        p3 = p3.add(dp);
        this.position = position;
    }
    public void move(Vector3 dp)
    {

        p1 = p1.add(dp);
        p2 = p2.add(dp);
        p3 = p3.add(dp);
        this.position = position.add(dp);
    }
    public bool cross(Vector3 position, Vector3 direction)
    {
            double d=0;
        return getCrossPoint(position, direction,ref d) != null;
    }


    public Vector3? getCrossPointOfSurface(Vector3 position, Vector3 direction)
    {
        double l2 = direction.dot(this.direction);
        if (l2 == 0)
        {
            return null;
        }
        Vector3 dp = this.position.reduce(position);
        double l1 = dp.dot(this.direction);

        double n = l1 / l2;
        if (n < 0)
        {
            return null;
        }

        Vector3 result = position.add(direction.scale(n));
        return result;


    }
        public bool isIn(Vector3 pos)
		{

            Vector3 v1 = p1.reduce(pos);
            Vector3 v2 = p2.reduce(pos);
            Vector3 v3 = p3.reduce(pos);

            double d1 = v1.cross(v2).dot(this.direction);
            double d2 = v2.cross(v3).dot(this.direction);
            double d3 = v3.cross(v1).dot(this.direction);
            if ((d1 > 0 && d2 > 0 && d3 > 0) || (d1 < 0 && d2 < 0 && d3 < 0))
            {
                return true ;
            }
            return false;
        }
    public Vector3? getCrossPoint(Vector3 position, Vector3 direction,ref double distance)
    {

     
        double l2 = direction.dot(this.direction);
        if (l2 == 0)
        {
            return null;
        }
        Vector3 dp = this.position.reduce(position);
        double l1 = dp.dot(this.direction);

        double n = l1 / l2;
            distance = n;
        if (n < 0)
        {
            return null;
        }

        Vector3 result = position.add(direction.scale(n));


        Vector3 v1 = p1.reduce(result);
        Vector3 v2 = p2.reduce(result);
        Vector3 v3 = p3.reduce(result);

        double d1 = v1.cross(v2).dot(this.direction);
        double d2 = v2.cross(v3).dot(this.direction);
        double d3 = v3.cross(v1).dot(this.direction);
        if ((d1 > 0 && d2 > 0 && d3 > 0) || (d1 < 0 && d2 < 0 && d3 < 0))
        {
            
            return result;
        }
        return null;
    }

    public virtual Triangle copy()
    {
        return new Triangle(p1.copy(), p2.copy(), p3.copy());
    }
}

}
