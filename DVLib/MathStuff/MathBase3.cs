using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVOSLib;
namespace MathBase
{
	public class DvRandom
	{

		public string seed{get;private set;}
		int[] bitPool;
		int[] numPool;
		public int size { get; private set; }
		public int current { get; private set; }
		public bool Loaded { get; private set; }

		public DvRandom(string seed = "", int size = 50000)
		{
			Loaded = false;
			this.size = size;

			if (seed == null)
			{
				seed = "";
			}
			seed += "seed";
			this.seed = seed;
			bitPool = getBit(seed.ToArray());
			numPool = getNumPool();
			while (numPool.Length < size)
			{
				excuteLoop();
			}
			Loaded= true;
		}
		void excuteLoop()
		{
			List<int> i = new List<int>();
			int n = 0;
			foreach (int c in numPool)
			{
				foreach (char ch in c.ToString())
				{
					getBit(ch, i);
				}
				i.Add(bitPool[n]);
				n++;
			}
			bitPool = i.ToArray();
			numPool = getNumPool();

		}

		public int Next(int min, int max)
		{
			return Next(max - min) + min;
		}
		List<int> getBit(char char1, List<int> list = null)
		{
			if (list == null)
			{
				list = new List<int>();
			}

			int c = char1;
			if (c <= 0)
			{
				c = -c;
			}
			do
			{
				list.Add(c & 1);
				c = (c >> 1);
			}
			while (c > 0);
			return list;

		}

		internal int NextColor()
		{
			return (Next(255), Next(255), Next(255)).RGB2Int();
		}

		internal float Nextfloat(float v1, float v2)
		{
			return (float)(v1 + (v2 - v1) * NextDouble());
		}

		int[] getBit(char[] chars)
		{
			List<int> l = new List<int>();
			foreach (char c in chars)
			{
				getBit(c, l);
			}

			return l.ToArray();
		}
		public double NextDouble()
		{
			return Next(5000000) / 5000000.0;
		}

		int[] getNumPool()
		{

			int n = 0;
			int num = 0;
			int number;
			int[] pool = new int[bitPool.Length];
			while (num < bitPool.Length)
			{
				number = bitPool[n];
				n++;
				if (n >= bitPool.Length)
				{
					n = 0;
				}
				for (int i = 0; i < 30; i++)
				{
					number = (number << 1) | bitPool[n];
					n++;
					if (n >= bitPool.Length)
					{
						n = 0;
					}

				}
				pool[num] = number;
				num++;
			}

			return pool;
		}

		public int Next(int max)
		{

			int c = numPool[current];
			current++;
			if (current >= numPool.Length)
			{
				current = 0;
			}
			return c % max;

		}
		public int Range(int min,int max)
		{
			return Next(min, max+1);
		}




	}
	public class dvRandom
	{
		public static string text;
		public string seed { get; private set; }
		int i = 0;
		int d = 0;
		public int length { get; private set; }
		char[] numberpool;
		double[] doublepool = new double[24000];
		public dvRandom(string code, int length)
		{
			I = 0; seed = code;
			this.length = length;
			createnumberpool_start();

		}
		public int NextColor()
		{
			return (Next(0,255),Next(0,255),Next(0,255)).RGB2Int();
		}
		public dvRandom()
		{
			I = 0;
			seed = "林大为";
			length = 36000;
			createnumberpool_start();

		}
		public static string gets_code(string code)
		{
			int s = 0;
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return (s % 100).ToString();
		}
		public static string gets_code2(string code)
		{
			int s = 0;
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return (s % 5).ToString();
		}
		public static string getansi(string code)
		{
			string s = "";
			for (int i = 0; i < code.Length; i++)
			{
				s += ((int)code[i]);
			}
			return s;
		}
		public string getnumber(string code, int length)
		{

			code = gets_code(code) + code.Length + gets_code2(code) + getansi(code);
			while (code.Length < length)
			{
				code = code + getansi(code);
			}
			while (code.Length > length)
			{
				code = code.Substring(0, code.Length - 1);
			}
			return code;
		}
		public int I { get; private set; }
		char[] temp = new char[40000];
		public bool finished { get { return numberpool.Length > length; } }
		public void createnumberpool(int index)
		{
			if ((I + index) * 20 > temp.Length)
			{
				char[] temptemp = new char[I * 20 + index * 20];
				temp.CopyTo(temptemp, 0);
				temp = temptemp;
			}
			char[] temp0;
			int II = I + index;
			for (; I < II; I++)
			{


				temp0 = CreatePoolbase(seed);
				for (int j = I * 20; j < I * 20 + 20; j++)
				{
					temp[j] = temp0[j - I * 20];
				}


				if ((I + 1) % 40 == 0)

				{
					createdoublepool();
					numberpool = new char[I * 20 + 20];

					for (int k = 0; k < numberpool.Length; k++)
					{
						numberpool[k] = temp[k];
					}
					for (int j = 0; j < 100; j++)
						temp = exchange(temp, Next(0, numberpool.Length - 1), Next(0, numberpool.Length - 1));



				}
			}
			createdoublepool();
		}
		void createnumberpool_start()
		{
			numberpool = getnumber(seed, 666).ToCharArray();










		}
		char readpool()
		{
			i++;
			if (i >= numberpool.Length)
				i = 0;
			return numberpool[i];
		}
		public int Range(int a, int b)
		{
			return Next(a, b);
		}
		public float Range(float a, float b)
		{
			return Nextfloat(a, b);
		}
		string getnumber(int length)
		{
			string result = "";
			int temp = i;
			for (int j = 0; j < length; j++)
			{
				result += readpool();

			}
			i = temp + 1;
			if (i == numberpool.Length)
				i = 0;
			return result;
		}
		public int Next_01()
		{
			return int.Parse(getnumber(1));
		}
		public int Next_09(int n)
		{
			return int.Parse(getnumber(1)) % n;
		}
		public int Next(int min, int max)
		{
			return (int)(min + (max - min + 1) * NextDouble());
		}

		public int Next0(int min, int max)
		{
			int lenth;
			int result;
			if (max < min)
			{
				int temp = max;
				max = min;
				min = temp;
			}
			max++;
			if (max > 0)
			{
				lenth = (max - min).ToString().Length + 1;
			}
			else
			{
				lenth = 2;
			}
			result = int.Parse(getnumber(lenth));
			result = min + result % (max - min);

			return result;
		}
		void createdoublepool()
		{
			i = 0;
			for (int j = 0; j < doublepool.Length; j++)
			{
				doublepool[j] = nextDouble();
			}
		}

		public double nextDouble()
		{

			return double.Parse("0." + getnumber(10));
		}
		public double NextDouble()
		{
			d++;
			if (d >= doublepool.Length)
				d = 0;
			return doublepool[d];
		}
		public float Nextfloat(float min, float max)
		{
			return (float)((max - min) * NextDouble() + min);
		}
		public double NextfDouble(double min, double max)
		{
			return ((max - min) * NextDouble() + min);
		}
		public string exchange(string s, int i1, int i2)
		{

			char[] vs = s.ToCharArray();
			if (i1 < vs.Length && i2 < vs.Length)
			{
				char temp = vs[i1];
				vs[i1] = vs[i2];
				vs[i2] = temp;
				return new string(vs);
			}
			return s;
		}
		public char[] exchange(char[] s, int i1, int i2)
		{


			if (i1 < s.Length && i2 < s.Length)
			{
				char temp = s[i1];
				s[i1] = s[i2];
				s[i2] = temp;

			}
			return s;
		}
		char[] poolbase = { '0', '9', '1', '8', '2', '7', '3', '6', '4', '5', '5', '4', '6', '3', '7', '2', '8', '1', '9', '0' };
		public char[] CreatePoolbase(string code)
		{

			for (int i = 0; i < Next0(100, 200); i++)
			{
				poolbase = exchange(poolbase, Next0(0, 19), Next0(0, 19));
			}
			return poolbase;
		}
	}
}
