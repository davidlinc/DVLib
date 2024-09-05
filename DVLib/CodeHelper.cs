using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Images;
using MathBase;

namespace DVOSLib
{
public class FFTCoder
	{
		public byte[] append(byte[] a, byte[]b)
		{
			byte[] newlist = new byte[a.Length +b.Length];
			a.CopyTo(newlist, 0);
			b.CopyTo(newlist, a.Length);
			return newlist;
		}

	
		public ComplexMap toComplexMap(byte[] data)
		{
			long size=data.LongLength;
			data = append(BitConverter.GetBytes(size), data);
			size = data.LongLength;
			if(size%8==0)
			{
				size = size / 8;
			}
			else
			{
				
				data = append(data, new byte[8-size%8] );
				size = size / 8 + 1;
			}
			double f = (Math.Log(Math.Sqrt(size)) / Math.Log(2));
			int a = (int)f;

			if(f!=a)
			{
				a++;
			}
			a = (int)Math.Pow(2, a);
			ComplexMap bitmap = new ComplexMap(a,a);
			int i = 0;
			bitmap.Foreach((x, y, data_) =>
			{
				
	            if (i < data.Length)
				{
                 data_[x, y] =new Complex((data[i] << 24) | (data[i+1] << 16) | (data[i+2]<<8)|data[i+3],

					 (data[i+4] << 24) | (data[i+5] << 16) | (data[i+6] << 8) | data[i + 7])  ;
				i+=8;
					
				}
			
			
			}
			);
			
			
			bitmap = bitmap.FFT().fftShift();
			return bitmap;

		}

		public int[] getInts(double r,double i)
		{
			byte[] r_ = BitConverter.GetBytes(r);
			byte[] i_ = BitConverter.GetBytes(i);

			return new int[] { (r_[0], r_[1], r_[2],r_[3]).ARGB2Int(), ( r_[4], r_[5],r_[6], r_[7]).ARGB2Int(), ( i_[0],i_[1], i_[2], i_[3]).ARGB2Int(), (i_[4], i_[5], i_[6],i_[7]).ARGB2Int() };
		}

		public Complex GetComplex(int[] ints)
		{

			var c1 = ints[0].Int2ARGB();
			var c2 = ints[1].Int2ARGB();
			var c3 = ints[2].Int2ARGB();
			var c4 = ints[3].Int2ARGB();
			double r = BitConverter.ToDouble(new byte[] { (byte)c1.a, (byte)c1.r, (byte)c1.g, (byte)c1.b, (byte)c2.a, (byte)c2.r, (byte)c2.g, (byte)c2.b },0);
			double i = BitConverter.ToDouble(new byte[] { (byte)c3.a, (byte)c3.r, (byte)c3.g, (byte)c3.b, (byte)c4.a, (byte)c4.r, (byte)c4.g, (byte)c4.b }, 0);
			return new Complex(r, i);
		}


		public bitmap toBitmap(ComplexMap map)
		{
			bitmap bitmap = new bitmap(map.Width*2
				, map.Height * 2);
			map.Foreach((x, y, data) => {
				Complex c = data[x, y];
				int[] iss = getInts(c.realPart, c.imaginaryPart);
				int xx = x * 2;
				int yy = y * 2;
				int n = 0;
				for(int i=0; i<2;i++)
				{
					for(int j=0; j<2;j++)
					{
                      bitmap[xx+i,yy+ j] = iss[n];
						n++;
					}
					
				}
			});

			return bitmap;
		}
		public string toString(ComplexMap map,byte fake)
		{
			byte[] bytes = map.getBytes();
			byte[] temp = new byte[2];
			temp[1] = fake;
			char[] c = new char[bytes.Length];
			for (int i = 0, j = 0; j < c.Length; i++, j++)
			{
				 temp[0] = bytes[j];
				c[j] = BitConverter.ToChar(temp, 0);
			}
			return new string(c);
		}
		public ComplexMap backToComplexMap(string bitmap)
		{

			
			char[] c = bitmap.ToCharArray();
            byte[] bytes =new byte[c.Length];
			byte[] temp;
			for (int i = 0, j = 0; j < c.Length; i ++ , j++)
			{
				temp = BitConverter.GetBytes(c[j]);
				bytes[i]=temp[0];
			}
			return ComplexMap.getSqureFromBytes( bytes).fftShift().iFFT();
		}
		public bitmap encode(byte[] bytes)
		{
			return toBitmap(toComplexMap(bytes));
		}
		public byte[] decode(bitmap bitmap)
		{
			return backToData(backToComplexMap(bitmap));
		}
		public string encodeString(byte[] bytes,byte fake)
		{
			return toString(toComplexMap(bytes),fake);
		}

		public byte[] codeText(string text)
		{
			return Encoding.Default.GetBytes(text);
		}

		public string getText(byte[] bytes)
		{
			return Encoding.Default.GetString(bytes);
		}
		public byte[] decodeString(string bitmap)
		{
			return backToData(backToComplexMap(bitmap));
		}
		public ComplexMap  backToComplexMap(bitmap bitmap)
		{
			
			ComplexMap complex = new ComplexMap(bitmap.Width/2, bitmap.Height / 2);
			complex.Foreach((x, y, data) => {
				int yy = y * 2;
				int xx = x * 2;
				data[x, y] = GetComplex(new int[] { bitmap[xx,yy], bitmap[xx, yy+1], bitmap[xx+1, yy], bitmap[xx+1, yy+1]});
			});
			complex = complex.fftShift().iFFT();
			
			return complex;
		}

		public byte[] backToData(ComplexMap bitmap)
		{
			byte[] size_=new byte[8];
			int i = 0;long j = 0;
			byte[] data=new byte[0];
			bool flag = true;
			long dl=0;
			int a, b;
			for (int x=0;x<bitmap.Width&&flag; x++)
			{
				for(int y=0;y<bitmap.Height&&flag; y++)
				{
					if(i<8)
					{

						a = (int)Math.Round(bitmap[x, y].realPart);
						b = (int)Math.Round(bitmap[x, y].imaginaryPart);
						size_[i] = (byte)( a>>24);
						size_[i+1] = (byte)((a>>16)&0xff);
						size_[i+2] = (byte)((a >> 8) & 0xff);
						size_[i+3] = (byte)((a ) & 0xff);
						size_[i+4] = (byte)((b >>24) & 0xff);
						size_[i + 5] = (byte)((b >>16) & 0xff);
						size_[i + 6] = (byte)((b >> 8) & 0xff);
						size_[i + 7] = (byte)(b & 0xff);
						dl = BitConverter.ToInt64(size_, 0);
						data =new byte[dl];
						if (dl % 8 == 0)
						{
					
						}
						else
						{
							data = append(data, new byte[8 -dl % 8]);
						}
						i +=8;
                        
					}
					else if(j<dl)
					{
						a = (int)Math.Round(bitmap[x, y].realPart);
						b = (int)Math.Round(bitmap[x, y].imaginaryPart);
						data[j] = (byte)(a >> 24);
						data[j + 1] = (byte)((a >> 16) & 0xff);
						data[j + 2] = (byte)((a >> 8) & 0xff);
						data[j + 3] = (byte)((a) & 0xff);
						data[j + 4] = (byte)((b >> 24) & 0xff);
						data[j + 5] = (byte)((b >> 16) & 0xff);
						data[j + 6] = (byte)((b >> 8) & 0xff);
						data[j + 7] = (byte)(b & 0xff);
						j += 8; 
					}
					else
					{
						flag = false;
					}
					
				}
			}

			data = data.Take((int)dl).ToArray();
			return data;
		}


	}
}
