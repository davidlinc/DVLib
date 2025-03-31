using DVOSLib;
using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ZeminHelper
{
	public static class ZBFFileHelper
	{
	public	static readonly int headBytes = 4 * 9 + 20 * 8;
	public	static readonly double pixelSize = 5.3e-3;

		public unsafe static ComplexMap readCsv(string filePath)
		{
			using (FileStream f = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader sr = new StreamReader(f))
				{


					List<string[]> ss = new List<string[]>();
					var v = sr.ReadLine();
					var vs = v.Split(',');
					int countPerLine = vs.Length;
					ss.Add(vs);
					Complex[] cmp = new Complex[countPerLine];
					while (!sr.EndOfStream)
					{
						v = sr.ReadLine();
						vs = v.Split(',');
						ss.Add(vs);
					}

					ComplexMap complex = new ComplexMap(countPerLine, ss.Count);
					for (int i = 0; i < countPerLine; i++)
					{
						for (int j = 0; j < ss.Count; j++)
						{
							double vr = double.Parse(ss[j][i]);
							complex[i, j] =vr ;
							
						}
					}
					return complex;
				}
			}
		}
		public unsafe static ZBFData readData(string filePath)
		{	ZBFData data = new ZBFData();
		using(FileStream f=new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{

			
				Span<byte> buffer =MemoryMarshal.Cast<ZBFHeader,byte>( MemoryMarshal.CreateSpan(ref data.header, 1)).Slice(0,headBytes);
				f.Read(buffer);
				data.complexMap = new ComplexMap(data.header.nx, data.header.ny);
			
				fixed (Complex* v= data.complexMap.Data)
				{
					Span<byte> b = MemoryMarshal.Cast<Complex, byte>(new Span<Complex>(v, data.header.nx * data.header.ny));
					f.Read(b);
				}
			   
			}
		return data;
		}
	}

	public class ZBFData
	{
		public ZBFHeader header;
		public ComplexMap complexMap;
		public ZBFData()
		{
			header.inti();
		}
		public void Save(string path,bool inverse=true)
		{
			using (FileStream file = new FileStream(path, FileMode.Create))
			{
				file.Write(header.getSpan());

				if(inverse)
				{
					for(int i = 0;i<complexMap.Width;i++)
					{
						for (int j = 0; j < complexMap.Height; j++)
						{
							complexMap[i, j].imaginaryPart *= -1;
						}
					}
				}

				file.Write(MemoryMarshal.Cast<Complex,byte>( complexMap.getSpan()));
			}
		}
	}


	[StructLayout(LayoutKind.Explicit)]
	public struct ZBFHeader
	{
		[FieldOffset(0)]
		public int version;
		[FieldOffset(4)]
		public int nx;
		[FieldOffset(8)]
		public int ny;
		[FieldOffset(12)]
		public int isPolarized;
		[FieldOffset(16)]
		public int unit;
		[FieldOffset(20)]
		public int any1;
		[FieldOffset(24)]
		public int any2;
		[FieldOffset(28)]
		public int any3;
		[FieldOffset(32)]
		public int any4;
		[FieldOffset(36)]
		public double dx;
		[FieldOffset(44)]
		public double dy;
		[FieldOffset(52)]
		public double pilotXz;
		[FieldOffset(60)]
		public double pilotXRayleigh;
		[FieldOffset(68)]
		public double politXWaist;
		[FieldOffset(76)]
		public double pilotYz;
		[FieldOffset(84)]
		public double pilotYRayleigh;
		[FieldOffset(92)]
		public double politYWaist;
		[FieldOffset(100)]
		public double wavelength;
		[FieldOffset(108)]
		public double index;
		[FieldOffset(116)]
		public double couplingEfficiency;
		[FieldOffset(124)]
		public double systemEfficiency;
		[FieldOffset(132)]
		public double anyA;
		[FieldOffset(140)]
		public double anyB;
		[FieldOffset(148)]
		public double anyC;
		[FieldOffset(156)]
		public double anyD;
		[FieldOffset(164)]
		public double anyE;
		[FieldOffset(172)]
		public double anyF;
		[FieldOffset(180)]
		public double anyG;
		[FieldOffset(188)]
		public double anyH;

		public static readonly int headBytes = 4 * 9 + 20 * 8;

		public void setBaseInfo(double dx,double dy,int nx,int ny,double waveLength,double index)
		{
			this.dx = dx;
			this.dy = dy;
			this.nx = nx;
			this.ny = ny;
			this.index = index;
			wavelength = waveLength;
		}

		public void inti()
		{
			version = 1;
		}
		public Span<byte> getSpan()
		{
			return MemoryMarshal.Cast<ZBFHeader, byte>(MemoryMarshal.CreateSpan(ref this, 1)).Slice(0,headBytes);
		}
		static Type Type=typeof(ZBFHeader);
		public override string ToString()
		{

			string s = "";
			foreach(var v in Type.GetFields())
			{
				if(!v.IsStatic)
				s += v.Name+":"+v.GetValue(this)+"\n";
			}

			return s;
		}
	}


}
