using DVOSLib;
using MathBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.ZeminHelper
{
	public static class SpotHelper
	{

		static Dictionary<int, ComplexMap> GaussMap = new Dictionary<int, ComplexMap>();
		static Dictionary <int,ComplexMap>Gauss_FFT=new Dictionary<int,ComplexMap>();
		public static double factor = 5;
		static double edge = 0.1;

		static ComplexMap getGauss(int size)
		{
			if(GaussMap.TryGetValue(size,out var map))
			{
				return map;
			}

			ComplexMap gauss = new ComplexMap(size, size);
			Vector2 center= (size/2,size/2);
			Vector2 temp;
			for (int i = 0; i < size; i++) 
			{
			var v=gauss.getColumnSpan(i);
				for (int j = 0; j < size; j++)
				{
					temp = (i, j);
					double dis = (temp - center).length_2();
					v[j] = Math.Exp(-dis / size * factor);
				}
			}
			GaussMap.Add(size, gauss);
			return gauss;
		}
		static ComplexMap getGauss_FFT(int size)
		{
		if(Gauss_FFT.TryGetValue(size,out var map))
			{
				return map;
			}

		var v=getGauss(size).FFT();
			Gauss_FFT.Add(size, v);
			return v;

		}

		public static (double[] p, double[] A ,double[] Amp ,double[] mu) getInputAndTarget(ComplexMap map,double radius,(double x,double y)pos)
		{	Vector2 c = pos;
			c+=(map.Width/2,map.Height/2);
			int startX = Math.Max(0, (int)(c.X - 1 - radius));
			int startY= Math.Max(0, (int)(c.Y- 1 - radius));
			int size = (int)(radius * 2 + 2);
		
			List<double> ps= new List<double>(size*size*314/400);
			List<double> As= new List<double>(size*size * 314 / 400);
			List<double> mus = new List<double>(size * size * 314 / 400);
			List<double> Amps = new List<double>(size * size * 314 / 400);
			for (int i = startX; i < startX + size&&i<map.Width; i++) {
				for (int j = startY; j < startY + size&&j<map.Height; j++)
				{
					Vector2 v = (i, j);
					Vector2 dv = v - c;
					double r = (v - c).value();
					if(r<=radius)
					{
						ps.Add(r/radius);
						As.Add(Math.Atan2(dv.Y, dv.X));
						mus.Add(map[i, j].mu());
						Amps.Add(map[i,j].length());
					}
				}
			}

			return (ps.ToArray(), As.ToArray(),Amps.ToArray(),mus.ToArray());
		}

		public static ComplexMap reconstruct(int width,int height,double radius, (double x, double y) pos, double[] p, double[] A, Span<double> Amp, double[] mu=null)
		{
			if(mu==null)
			{
				mu = new double[Amp.Length];
				mu.AsSpan().Fill(0);
			}
			Vector2 c=(width/2,height/2);
			ComplexMap complexMap=new ComplexMap(width,height);
			complexMap.getSpan().Fill(0);
			
				for (int i = 0; i <p. Length; i++)
            {

					Vector2 dir = (Math.Cos(A[i]), Math.Sin(A[i]));
					dir *= p[i] * radius;
					dir += c;
					complexMap[(int)Math.Round(dir.X), (int)Math.Round(dir.Y)] = Amp[i] * new Complex(Math.Cos(mu[i]), Math.Sin(mu[i]));

				
		       
            }
				

			return complexMap;
        }

		public static void Decay(this ComplexMap map,double startDistance,double zeroDistance, Vector2? center=null)
		{
			if(center==null)
			{
				center=(map.Width/2,map.Height/2);
			}

			double f = 20 / (zeroDistance - startDistance);
            for (int i = 0; i <map.Width; i++)
            {
				for (int j = 0; j < map.Height; j++)
				{
					Vector2 pos = (i, j);
					double d = (pos - center.Value).value()-startDistance;
					if(d>0)
					{
						map[i, j] *= Math.Exp(-(d*d)*f);
					}
				}
			}
        }
		public static (double radius, (double x, double y) pos) findCircle(ComplexMap map, out ComplexMap map_fft, int sample = 16, double step = 2,double min=20)
		{
			if(map.Width!=map.Height)
			{
				throw new Exception("图像的宽高不一致!");
			}
			map=map.Clone();

			var c = map.getSpan();
			for(int i = 0; i < c.Length;i++)
			{
				if (c[i].realPart<min)
				{
					c[i]=0;
				}
			}

			map_fft = map.FFT();
			var gs = map_fft * getGauss_FFT(map_fft.Width);
			gs = gs.iFFT().limtedMax_(1,out int maxIndex);

			double max=-1;
			double temp;

			var v = gs.getSpan();
			(int x,int y)pos=(maxIndex/map.Height,maxIndex%map.Height);
			double dtheta =2* Math.PI / sample;
			double theta = 0;	
			double maxR = -1;
			Vector2 p = pos; 
			p += (map.Width / 2.0, map.Height / 2.0);
			double maxL = Math.Min(Math.Min(map.Width - p.X,p.X), Math.Min(map.Height - p.Y, p.Y))-1;

			for (int i = 0; i < sample; i++) {

				Vector2 dir = (Math.Cos(theta), Math.Sin(theta));
				dir *= step;
				double l = 0;
				Vector2 pos_ = p;
			
				while(l+step<maxL)
				{

					l += step;
					int x = (int)pos_.X;
					int y=(int)pos_.Y;

					if(x<map.Width&&y<map.Height&&x>=0&&y>=0)
					{
						double value = map[x,y].length();
						if(value<edge)
						{
							break;
						}
					}
					else
					{
						break;
					}
					pos_ += dir;
				}

				if(l>maxR)
				{
					maxR = l;
				}
				

			}

			return (maxR,pos);
		}

	}
}
