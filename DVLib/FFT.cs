using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVOSLib;
using MathBase;
using Images;
using MachineLearning;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace MathBase
{
	
	public interface IFFilter
	{
		void apply(ComplexMap map);

	}
	public delegate Complex TF(int u, int v,int w,int h);
	public class FFilter:IFFilter
	{
		public static FFilter BHTEST = new FFilter((int x,int y, int w, int h) => {

			double k= 1 / (1 + Math.Pow(new Vector2(x -w/2, y -h/2).value() / (w/10), 2));

			return k; });

		TF transferFunction;
		public FFilter(TF tF)
		{
			transferFunction = tF;
		}
		public void apply(ComplexMap map)
		{
			Complex[,] values = map.Data;
			int w = map.Width;
			int h = map.Height;
			for(int i=0;i<w;i++)
			{
				for (int j = 0; j < h; j++)
				{
					values[i, j] *= transferFunction(i, j,w,h);
				}
			}
		}
	}

	public static class FFTHelper
	{
		public static bool useMultiThreads = true;
		/// <summary>
		/// 二维快速傅里叶变换
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
		public static ComplexMap FFT(this ComplexMap map )
		{
			int w = map.Width;
			int h = map.Height;

			ComplexMap r=new ComplexMap(w,h);
			for(int i=0;i<w;i++)
			{
				r.SetColumn(i,map.GetColumn(i).FFT());
			}
			for (int i = 0; i < h; i++)
			{
				r.SetRow(i, r.GetRow(i).FFT());
			}

			return r;
		}

		public unsafe static double MultiplyM(double[] nums)
		{
			int vectorSize = Vector<double>.Count;
			var accVector = Vector256.Create(1.0,1,1,1);
			int i;
			var array = nums;
			double result = 1.0d;
			fixed (double* p = array)
			{
				for (i = 0; i <= array.Length - vectorSize; i += vectorSize)
				{
					//var v = new Vector<double>(array, i);
					var v = *(Vector256<double>*)(p + i);
					accVector =Avx.Multiply(accVector, v);
				}
			}
			var tempArray = new double[Vector<double>.Count];
			Span<Vector256<double>> t =MemoryMarshal.Cast<double,Vector256<double>>( tempArray.AsSpan());
			t[0] = accVector;
			for (int j = 0; j < tempArray.Length; j++)
			{
				result = result * tempArray[j];
			}

			for (; i < array.Length; i++)
			{
				result *= array[i];
			}

			return result;
		}
		public static double MultiplyN(double[] nums)
		{
			double result = 1.0d;

			for (int i = 0; i < nums.Length; i++)
			{
				result *= nums[i];
			}
			return result;
		}
		public static ComplexMap iFFT(this ComplexMap map)
		{
			int w = map.Width;
			int h = map.Height; ComplexMap r = new ComplexMap(w, h);
			for (int i = 0; i < w; i++)
			{
				r.SetColumn(i, map.GetColumn(i).iFFT());
			}
			for (int i = 0; i < h; i++)
			{
				r.SetRow(i, r.GetRow(i).iFFT());
			}
			return r;
		}

		public static Complex[] dft(this Complex[] data)
		{
			int Width = data.Length;
			double v1 = -2 * Math.PI / Width;
			double theta = 0;
			double theta2;
			Complex[] complices = new Complex[Width];
			for (int i = 0; i < Width; i++)
			{
				
				Complex sum1 = 0;
				theta2 = 0;
				for (int x = 0; x < Width; x++)
				{
					sum1 +=data[x] * Complex.Exp(theta2);
					theta2 += theta;
				}
				theta+=v1;

				complices[i] = sum1;
			}
			return complices;
		}
		public static Complex[] idft(this Complex[] data)
		{
			int Width = data.Length;
			double v1 = 2 * Math.PI / Width;

			double v11 = 0;
			Complex[] complices = new Complex[Width];
			for (int i = 0; i < Width; i++)
			{
				
				Complex sum1 = 0;
				double v2 = 0;
				for (int x = 0; x < Width; x++)
				{
					sum1 += data[x] * Complex.Exp(v2);
		
					v2 += v11;
				}
				v11 += v1;

				complices[i] = sum1;
			}
			
				double d = 1.0 / complices.Length;
				for (int i = 0; i < complices.Length; i++)
				{
					complices[i] *= d;
				}

			return complices;
		}
		/// <summary>
		/// 快速傅里叶反变换
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
	
		public static Complex[] FFT(this Complex[] map)
		{
			int l = map.Length;

			if(l.BitCount()==1)
			return fft_core(map,ref l, 1);

			return BluesteinFFT(map);
		}

		public static Complex[] fftShift(this Complex[]map)
		{
			int length = map.Length;
			int l=map
				.Length/2;
			int l_ = l + 1;
			Complex temp;
			Complex[] r = new Complex[map.Length];
			int max=map.Length-1;
			bool odd = max % 2 == 0;

			if(odd)
			{
				for(int i=0; i < length; i++)
				{
					if(i<l)
					{
						r[i] = map[i + l_];
					}
					else
					{
						r[i] = map[i-l];
					}
				}
			}
			else
			{
            for(int i=0;i<l;i++)
			{
			
				r[i]=map[l+i];
				r[l+i] = map[i];
			}
			}

			
			return r;
		}
		public static Complex[] ifftShift(this Complex[] map)
		{
			int length = map.Length;
			int l = map
				.Length / 2;
			int l_ = l + 1;
			Complex temp;
			Complex[] r = new Complex[map.Length];
			int max = map.Length - 1;
			bool odd = max % 2 == 0;

			if (odd)
			{
				for (int i = 0; i < length; i++)
				{
					if (i < l_)
					{
						r[i] = map[i + l];
					}
					else
					{
						r[i] = map[i - l_];
					}
				}
			}
			else
			{
				for (int i = 0; i < l; i++)
				{

					r[i] = map[l + i];
					r[l + i] = map[i];
				}
			}


			return r;
		}
		public static int BitCount(this byte b)
		{
			return (1 & b) + (1 & (b >> 1)) + (1 & (b >> 2)) + (1 & (b >> 3)) + (1 & (b >> 4)) + (1 & (b >> 5)) + (1 & (b >> 6)) + (1 & (b >> 7));
		}
		public unsafe static int BitCount(this int b)
		{
			var bp = (byte*)&b;
			return BitCount(bp[0])+ BitCount(bp[1])+ BitCount(bp[2]) + BitCount(bp[3]);
		}
		public static Complex[] iFFT(this Complex[] map)
		{
			
			int l = map.Length;
			
			if(l.BitCount()==1)
			return fft_core(map, ref l, -1);

			return BluesteinFFT(map,-1);
		}
		/// <summary>
		/// 二维快速傅里叶变换和反变换的核心函数通过两次一维fft实现
		/// </summary>
		/// <param name="src"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		/// 

		public static Complex[] FFT_test(this Complex[] c)
		{
			int n = c.Length;
			bool odd = n % 2 == 1;
			double k = -2 * Math.PI / n;
			double temp;
			Complex[] cout = new Complex[n];
			if (n == 1)
			{
				cout[0] = c[0];
				return cout;
			}
			else
			{
				int n_2 = n/2;
				Complex[] c1 = new Complex[odd?n_2+1:n_2];
				Complex[] c2 = new Complex[n_2];
		
				
				bool odd_ = false;
			    for(int i = 0, j=0; i < n; i++)
				{
					if(odd_)
					{
						c2[j]=c[i];
						j++;
					}
					else
					{
						c1[j] = c[i];
					}

					odd_ = !odd_;
				}
				Complex[] c1out = FFT_test(c1);
				Complex[] c2out = FFT_test(c2);
				temp = 0;
				if(odd)
				{
					cout[0] = c1out[0];
					temp += k;
					for (int i = 0; i < n_2; i++)
					{
						c2out[i] = c2out[i] * Complex.Exp(temp);
						cout[i+1] = c1out[i+1] + c2out[i];
						cout[i + n_2+1] = c1out[i+1] - c2out[i];
						temp += k;
					}
				}
				for (int i = 0; i < n_2; i++)
				{
					c2out[i] = c2out[i] * Complex.Exp(temp);
					cout[i] = c1out[i] + c2out[i];
					cout[i + n_2] = c1out[i] - c2out[i];
					temp += k;
				}

				return cout;
			}
		}

		public static FFTData getFFT(int width,int flag)
		{
			int Relog2N = ReLog2N(width);
			int ReWidth = 0x01 << Relog2N;
			Complex[] WN = getWN(ReWidth, flag);
			int[] Rag = getReArrange(ReWidth, Relog2N);
			return new FFTData(WN,Rag,Relog2N);
		}
		public static ComplexMap fft_2D_core(ComplexMap src, ref int width, ref int height, int flag)
		{
			//补零后长度
			int width_Log2N = ReLog2N(width);
			int height_Log2N = ReLog2N(height);
			int Relog2N = Math.Max(width_Log2N, height_Log2N);
			int ReWidth = 0x01 << Relog2N;
			int ReHeight = ReWidth;

			//重新复制数据，清零
			ComplexMap ReList2D_ = new ComplexMap(ReWidth, ReHeight);
			Complex[,] ReList2D = ReList2D_.Data;
			int width_temp = Math.Min(src.Width, width);
			int height_temp = Math.Min(src.Height, height);	
			
			for (int i = 0; i < width_temp; i++)
			{
				for (int j = 0; j < height_temp; j++)
				{
					ReList2D[i, j] = src[i, j];
				}
			}


			FFTData data = getFFTData(Relog2N, flag);
			int Lenght_temp;
			Complex[] WN = getWN(ReWidth, flag);
			int[] Rag = getReArrange(ReWidth, Relog2N);
			//第1遍fft

		
			if(useMultiThreads)
			{
				int[][] index = ReHeight.getRange().split(20);
	Parallel.ForEach(index, (int[] ind) =>
			{   Complex[] Xm1;
				Complex[] Xk1; 
				foreach (int i in ind)
				{
					Xm1 = new Complex[ReList2D_.Width];
					for (int u = 0; u < ReList2D_.Width; u++)
					{
						Xm1[u]= ReList2D_[u, i];
					}
					Lenght_temp = Xm1.Length;
					Xk1 = fft_core(Xm1, ref Lenght_temp, flag, data);
				
					for (int u = 0; u <ReList2D_.Width; u++)
					{
						ReList2D_[u,i] = (u < Xk1.Length) ? Xk1[u] : 0;
					}
				}
            });

				Parallel.ForEach(index, (int[] ind) =>
				{
					Complex[] Xk;
					Complex[] Xn;
					foreach (int i in ind)
					{
						Xn = new Complex[ReList2D_.Height];
						for (int u = 0; u < ReList2D_.Height; u++)
						{
							Xn[u] = ReList2D_[i, u];
						}
						Lenght_temp = Xn.Length;
						Xk = fft_core(Xn, ref Lenght_temp, flag,data);
					
						for (int u = 0; u < ReList2D_.Height; u++)
						{
							ReList2D_[i, u] = Xk[u];
						}
					}
				});
			}
			else
			{
			
					Complex[] Xm1;
					Complex[] Xk1;
					for (int i =0;i<ReHeight;i++)
					{
						Xm1 = new Complex[ReList2D_.Width];
						for (int u = 0; u < ReList2D_.Width; u++)
						{
							Xm1[u] = ReList2D_[u, i];
						}
						Lenght_temp = Xm1.Length;
						Xk1 = fft_core(Xm1, ref Lenght_temp, flag,data);
					
						for (int u = 0; u < ReList2D_.Width; u++)
						{
							ReList2D_[u, i] = (u < Xk1.Length) ? Xk1[u] : 0;
						}
					}

				
					Complex[] Xk;
					Complex[] Xn;
				for (int i = 0; i < ReHeight; i++)
				{
						Xn = new Complex[ReList2D_.Height];
						for (int u = 0; u < ReList2D_.Height; u++)
						{
							Xn[u] = ReList2D_[i, u];
						}
						Lenght_temp = Xn.Length;
						Xk = fft_core(Xn, ref Lenght_temp, flag,data);
					
						for (int u = 0; u < ReList2D_.Height; u++)
						{
							ReList2D_[i, u] = Xk[u];
						}
					}
			
			}
		

			


			//第2遍fft
		
			//赋值
			width = ReWidth;
			height = ReHeight;

			//清理内存

			//返回
			return ReList2D_;
		}

		/// <summary>
		/// 计算尺寸大小 （对数）fft要求长宽为2的n次方
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static int ReLog2N(int count)
		{
			int log2N = 0;
			uint mask = 0x80000000;
			for (int i = 0; i < 32; i++)
			{
				if (0 != ((mask >> i) & count))
				{
					if ((mask >> i) == count) log2N = 31 - i;
					else log2N = 31 - i + 1;
					break;
				}
			}
			return log2N;
		}
		/// <summary>
		/// 根据二进制长度重新计算序号
		/// </summary>
		/// <param name="dat"></param>
		/// <param name="bitlenght"></param>
		/// <returns></returns>
		/// 

		static int[] getReArrange(int N,int bitlenght)
		{
			int[] re= new int[N];
			int ret;
			for(int dat=0;dat<N;dat++)
			{
				 ret = 0;
				for (int i = 0; i < bitlenght; i++)
				{
					if (0 != (dat & (0x01 << i))) ret |= ((0x01 << (bitlenght - 1)) >> i);
				}
				re[dat] = ret;
			}
			return re;
		}
		private static int ReArrange(int dat, int bitlenght)
		{
			int ret = 0;
			for (int i = 0; i < bitlenght; i++)
			{
				if (0 != (dat & (0x01 << i))) ret |= ((0x01 << (bitlenght - 1)) >> i);
			}
			return ret;
		}
		/// <summary>
		/// 一维快速傅里叶变换
		/// </summary>
		/// <param name="src"></param>
		/// <param name="lenght"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		/// 



		public static int preLength = 0;
		public static FFTData[] listHelper
			=new FFTData[preLength];

		public static FFTData[] listHelperR
			= new FFTData[preLength];

		public static void setPreLength(int l)
		{
			preLength = l;
			listHelper=new FFTData[(int)l];
			listHelperR=new FFTData[(int)l];
			for (int i = 0; i < listHelper.Length; i++)
			{
				int N = 1 << i;
				listHelper[i] = getFFT(N, 1);
				listHelperR[i] = getFFT(N, -1);
			}
		}
		public static ComplexMap phaseMove(this ComplexMap map, double dx,double dy)
		{
			Complex[,] data = map.Data;
			Complex thetaX = -Math.PI * 2 * dx / map.Width*Complex.I;
			Complex thetaY = -Math.PI * 2 * dy / map.Height*Complex.I;
			Complex tx = 0;
			Complex ty ;

			ComplexMap map1 = new ComplexMap(map.Width, map.Height);
			Complex[,] data2 = map1.Data;
			for(int i=0;i<map.Width;i++)
			{
				
				ty = 0;
				for(int j = 0; j < map.Height; j++)
				{

					data2[i,j]=data[i,j]*Complex.Exp(tx+ty);
					ty += thetaY;
				}	
					tx += thetaX;
			}
			return map1;
		}
		public static ComplexMap phaseMove_(this ComplexMap map, double dx, double dy)
		{
			Complex[,] data = map.Data;
			Complex thetaX = -Math.PI * 2 * dx / map.Width * Complex.I;
			Complex thetaY = -Math.PI * 2 * dy / map.Height * Complex.I;
			Complex tx = 0;
			Complex ty;
			for (int i = 0; i < map.Width; i++)
			{

				ty = 0;
				for (int j = 0; j < map.Height; j++)
				{

					data[i, j] = data[i, j] * Complex.Exp(tx + ty);
					ty += thetaY;
				}
				tx += thetaX;
			}
			return map;
		}

		static FFTHelper()
		{
			for (int i = 0; i < preLength; i++)
			{
				int N = 1 << i;
				listHelper[i] = getFFT(N, 1);
				listHelperR[i] = getFFT(N, -11);
			}
		}
		public static FFTData getFFTData(int bitlength,int flag)
		{
			if(bitlength<preLength)
			{
				if(flag>0)
				{
					return listHelper[bitlength];
				}
				else
				{
					return listHelperR[bitlength];
				}
			}
			else
			{
				return getFFT(1<<bitlength, flag);
			}
		}

		static Complex[] Chirp(int N)
		{
			Complex[] complexes = new Complex[N];
			double n1 =-Math.PI / N;
			double angle;
			double angle0 = 0;
			for (int i = 0;i < N;i++)
			{
				angle = angle0 * i;
				complexes[i] = new Complex(Math.Cos(angle), Math.Sin(angle));
				angle0 += n1;
			}
			return complexes;
		}
		static Dictionary<int, Complex[]> Chirps = new();
		public static Complex[] getChirp(int n)
		{
			if(Chirps.TryGetValue(n, out Complex[] complexes))
			{
				return complexes;
			}
			else
			{
				var v=Chirp(n);
				Chirps.Add(n, v);
				return v;
			}
		}
		public static Complex[] BluesteinFFT(this Complex[] input,int dir=1)
		{
            int N=input.Length;
			int M =0x01<< ReLog2N(2 * N - 1);
			Complex[] a=new Complex[M];
			Complex[] b=new Complex[M]; 
			Complex[] result = new Complex[N];
			Complex[] chirp = getChirp(N);
			if(dir<0)
			{
				Complex[] temp= new Complex[N];
				var c = chirp.AsSpan();
				c.CopyTo(temp);
				var s=MemoryMarshal.Cast<Complex,double>( temp.AsSpan());
				int NN = N * 2;
				for(int i=1;i<NN;i+=2)
				{
					s[i] = -s[i];
				}
				chirp = temp;
			}
			int Nn=N-1;
			int Mn=M-1;
			// Chirp function: z_n = exp(πi * n^2 / N)
			for (int n = 0; n < N; n++)
			{
				a[n] = input[n] * chirp[n];
				
				b[n] = chirp[n].conjugate();
				if(n>0)
				b[M-n] = chirp[n].conjugate();
			}

			int l = M;
			// Compute FFT of a and b
			Complex[] A =fft_core(a,ref l,1);
			Complex[] B =fft_core(b, ref l, 1);

			// Convolution in frequency domain
			Complex[] C = new Complex[M];
			for (int i = 0; i < M; i++)
			{
				C[i] = A[i] * B[i];
			}

			// Inverse FFT of the product
			Complex[] convResult = fft_core(C,ref l,-1);

			// Multiply result by chirp function and extract relevant part
			for (int k = 0; k < N; k++)
			{
				result[k] = convResult[k] * chirp[k];
			}
			if(dir<0)
			{
				double d = 1.0 / N;
				for (int k = 0; k < N; k++)
				{
					result[k] *= d;
				}
			}
			return result;
		}
		public static Complex[] getWN(int N,int flag)
		{
					int n = N / 2;
			double a = 2 * Math.PI / N;
			Complex [ ]	WN = new Complex[n];
			double theta = 0;
			for (int i = 0; i < n; i++)
			{
				WN[i] = new Complex(Math.Cos(theta), -flag * Math.Sin(theta));
				theta += a;
			}
			return WN;
		}

		public unsafe static Complex[] fft_core(this Complex[] src, ref int lenght, int flag,FFTData data=null)
		{
            int relog2N = ReLog2N(lenght);
			if(data ==null)
			{
				data = getFFTData(relog2N, flag);
			}
			//按时间抽取FFT方法(DIT)

			int bitlength = data.bitlength;
			int[] Rag = data.Rag;
	
			Complex[] WN = data.WN;
			
			int N = 0x01 << bitlength;
			//补零后长度

			Complex[] dataSource = new Complex[N];
			src.CopyTo(dataSource, 0);

			//重新复制数据，同时进行
			//    逆序排放，并补零
			int index;
			Complex[] Register = new Complex[N];
		
			for (int i = 0; i < N; i++)
			{
				
			
				Register[i] = dataSource[Rag[i]] ;
			}

			//蝶形运算
			int Index0, Index1;
			Complex temp;
			int tempi,tempi2;
			int tp1, tp2,tp3;
			for (int steplenght = 2; steplenght <= N; steplenght<<=1)
			{
				tempi = N / steplenght;
				tempi2 = steplenght >> 1;
				tp1 = 0;
				for (int step = 0; step < tempi; step++)
				{
					
					tp2 = tp1 + tempi2;
					tp3 = 0;
					for (int i = 0; i < tempi2; i++)
					{
						Index0 = tp1 + i;
						Index1 = tp2 + i;

						temp = Register[Index1] * WN[tp3];
						Register[Index1] = Register[Index0] - temp;
						Register[Index0] = Register[Index0] + temp;
						tp3 += tempi;
					}
					tp1 += steplenght;
				}
			}

			//若为idft
	

			//赋值
			lenght = N;

			if(flag==-1)
			{
	        double d = 1.0 / Register.Length;
			for(int i=0;i<Register.Length;i++)
			{
				Register[i] *=d;
			}

			}
		
			//返回
			return Register;
		}
	}

	public class FFTData  
	{
		public Complex[] WN { get; private set; }
		 public int[] Rag { get; private set; }

		public int bitlength { get; private set; }

		public FFTData(Complex[] wN, int[] rag, int bitlength)
		{
			WN = wN;
			Rag = rag;
			this.bitlength = bitlength;
		}
	}
}
