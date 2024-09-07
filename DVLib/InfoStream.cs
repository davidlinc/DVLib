using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MathBase;
using Images;
using IntMap = Images.bitmap;
using DVOSLib;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace DVOSLib
{

	public enum InfoType:byte
	{
		OBJECTS,BYTE, BOOL,INT,LONG, FLOAT,  DOUBLE,STRING, INTARRAY, STRINGARRAY, FILE,END,LISTOBJECTS, NULL,HEAD,FILESTREAM,MAP,COMMAND,
		INFINITE_MAP,VECTOR2,VECTOR3
	}
	public class Command
	{
		public readonly String name;
        public bool cancel = false;
        public readonly Object[] arguments;
		public Command(String name,params Object[]objects)
		{
			this.name = name;
			this.arguments = objects;
		}
	}


	public delegate void WriteFunction(InfoStream stream,object o);
	public delegate object ReadFunction(InfoStream stream);
	public class InfoStream
	{

		 static InfoStream()
		{
			register(InfoType.VECTOR2, (i, o) => { i.writeVector2((Vector2)o); }, (i) => { return i.readVector2(); },typeof( Vector2));
			register(InfoType.VECTOR3, (i, o) => { i.writeVector3((Vector3)o); }, (i) => { return i.readVector3(); }, typeof(Vector3));

		}

		internal	byte[] bytes = new byte[1];
		int pointer = 1;
		int Capcity=1;
		int Length = 1;
	    internal	List<FileStream> files=new List<FileStream>();
	    static	Dictionary<InfoType, ReadFunction> readFMap = new Dictionary<InfoType, ReadFunction>();
	    static	Dictionary<Type, WriteFunction> writeFMap = new Dictionary<Type, WriteFunction>();

		public static void register(InfoType type,WriteFunction wf,ReadFunction rf,params Type[] types)
		{
			readFMap.Add(type, rf);
			foreach(Type type1 in types)
			{
				writeFMap.Add(type1 as Type, wf);
			}
		}
	
		void grow()
		{
			Capcity *= 2;
			byte[] bytes = new byte[Capcity];
			Array.Copy(this.bytes, bytes, Length);
			this.bytes = bytes;
		}
		public InfoStream write(params object[] o)
		{
			foreach (object s in o)
			{
				if(s==null)
				{
					writeNull();
				}
				else if(s is string)
				{
					writeString((string)s);
				}
				else if(s is bool)
				{
					writeBool((bool)s);
				}
				else if (s is int)
				{
					writeInt((int)s);
				}
				else if (s is long)
				{
					writeLong((long)s);
				}
				else if (s is byte)
				{
					writeByte((byte)s);
				}
				else if (s is double)
				{
					writeDouble((double)s);
				}
				else if (s is float)
				{
					writeFloat((float)s);
				}
				else if (s is int[])
				{
					writeIntArray((int[])s);
				}
				else if (s is string[])
				{
					writeStringArray((string[])s);
				}
				else if (s is Map<int>)
				{
					writeMap<int>((Map<int>)s);
				}
				else if (s is Map<bool>)
				{
					writeMap<bool>((Map<bool>)s);
				}
				else if (s is Map<byte>)
				{
					writeMap<byte>((Map<byte>)s);
				}
				else if (s is Map<float>)
				{
					writeMap<float>((Map<float>)s);
				}
				else if (s is Map<long>)
				{
					writeMap<long>((Map<long>)s);
				}
				else if (s is Map<double>)
				{
					writeMap<double>((Map<double>)s);
				}
				else if (s is InfiniteMap)
				{
					writeInfiniteMap((InfiniteMap)s);
				}
				else
				{
					WriteFunction write;
					if(writeFMap.TryGetValue(s.GetType(),out write))
					{
						write(this,s);
					}
				}
			}
			return this;
		}
		public InfoStream(int cpacity=1024)
		{
			
			this.Capcity = cpacity;
			bytes = new byte[Capcity];
			setInfoType(InfoType.OBJECTS);
			Length = 1;
		
			
		}

		public void punchAss()
		{
			int e = 0;
			for(int i=bytes.Length-1;i>=0;i--)
			{
				if(bytes[i]==(byte)InfoType.END)
				{
					e = i;
				}
			}
			byte[] nb = new byte[e];
			for(int i=0;i<e;i++)
			{
				nb[i] = bytes[i];
			}
			bytes = nb;
		}

		public void setPointer(int p)
		{
			pointer = p;
		}
		public void behead()
		{
			if (bytes[0] == (byte)InfoType.HEAD) 
			{
				byte[] nb = new
				byte[bytes.Length - 5];
				Span<byte> bs = bytes;
				bs = bs.Slice(5, bytes.Length - 5);
				Span<byte> nbs = nb;
				bs.CopyTo(nbs);
			}
		}
		public InfoStream(byte[] bs)
		{
			bytes= bs;
			pointer = 5;
		}

		Span<byte> getSpan()
		{
			return ((Span<byte>)bytes).Slice(0, Length);
		}

		public void setInfoType(InfoType type)
		{
			bytes[0] = (byte)type;
		}


		public static byte[] appendArray(params byte[][] bytes)
		{
			long sum = 0, sum2 = 0;
			foreach (byte[] bs in bytes)
			{
				sum += bytes.LongLength;
			}
			byte[] r = new byte[sum];
			foreach (byte[] bs in bytes)
			{
				Array.Copy(bs, 0, r, sum2, bs.LongLength);
				sum2 += bs.LongLength;
			}
			return r;
		}

		public void appendValues(params byte[] bytes)
		{
			append(bytes);
		}

		public void append<T>(T value)where T: struct
		{
			append(getSpan(value));
		}

			public void append(Span<byte> bytes)
		{
			int nl = Length + bytes.Length;
			while(Capcity<nl)
			{
				grow();
			}
			Span<byte> raw = ((Span<byte>)this.bytes).Slice(Length,bytes.Length);
			bytes.CopyTo(raw);
			Length += bytes.Length;
		}
		InfoType getInfoType<T>(Map<T> map)
		{
			if(map is Map<int>)
			{
				return InfoType.INT;
			}
			else if(map is Map<long>)
			{
				return InfoType.LONG;
			}	
			else if(map is Map<bool>)
			{
				return InfoType.BOOL;
			}
			else if(map is Map<double>)
			{
				return InfoType.DOUBLE;
			}
			else if(map is Map<byte>)
			{
				return InfoType.BYTE;
			}
			else if(map is Map<float>)
			{
				return InfoType.FLOAT;
			}
			return InfoType.NULL;　
		}
		Span<byte> getByteArray<T> (T[] map)where T:struct
		{
			Span<T> span = map;
			return MemoryMarshal.Cast<T, byte>(span);
		}

		Span<byte> getSpan<T>(T value)where T:struct
		{
			unsafe
			{
                T* p= &value;
				Span<T> st = new Span<T>(p, 1);
				return MemoryMarshal.Cast<T,byte>(st);
			}
		}
		public void writeMap<T >(Map<T> map)where T:struct
		{
			appendValues((byte)InfoType.MAP, (byte)getInfoType(map));
			append(map.Width);
			append(map.Height);
			append(MemoryMarshal.Cast<T, byte>(map.getSpan()));
    		}
		public void writeNull()
		{
			appendValues((byte)InfoType.NULL);
		}
		public void writeByte(byte i)
		{
			appendValues((byte)InfoType.BYTE,i);
		}
		public byte readByte()
		{

			pointer += 2;

			return bytes[pointer - 1];
		}

		public byte[] getTosend()
		{
			var b = getSpan();
			byte[] bs = new byte[5 + Length];
			
			b.CopyTo(((Span<byte>)bs).Slice(5));
			bs[0] = (byte)InfoType.HEAD;
			int i = bs.Length;
			bs[1] = (byte)(i >> 24);
			bs[2] = (byte)(i >> 16);
			bs[3] = (byte)(i >> 8);
			bs[4] = (byte)(i);
			return bs;
		}
		public void writeLong(long i)
		{
			append((byte)InfoType.LONG);
			append(i);
		}

		public long readLong()
		{
			int p = pointer+1;
			pointer += 9;
			return BitConverter.ToInt64(bytes,p);
		}
		public void writeFileStream(FileStream file,string path)
		{
			append(InfoType.FILESTREAM);
			writeString(path);
			files.Add(file);
		}
		public FileStream readFileStream()
		{
			pointer++;
			string path = readString();

			DirectoryInfo info = new DirectoryInfo(path);
			if(!Directory.Exists(info.Parent.FullName))
			{
				Directory.CreateDirectory(info.Parent.FullName);
			}
			FileStream f = new FileStream(path, FileMode.Create);
			return f;
		}
		public void writeInt(int i)
		{
		    append(InfoType.INT);
			append(i);
		}
	public object readNull()
		{
			pointer++;
			return null;
		}
		public int readInt()
		{
			pointer++;
			var v = BitConverter.ToInt32(bytes, pointer);
			pointer += 4;
			return  v;
		}
		public double readDouble()
		{
			pointer++;
			var v = BitConverter.ToDouble(bytes, pointer);
			pointer += 8;
			return v ;
		}
		public float readFloat()
		{
			pointer++;
			var v = BitConverter.ToSingle(bytes, pointer);
			pointer += 4;
			return pointer;
		}
		public void writeFloat(float i)
		{ 
			append(InfoType.FLOAT);
			append(i);
		}
		public string readString()
		{
			int l = readInt();
			byte[] str = new byte[l];
			for(int i=0;i<l;i++)
			{
				str[i] = bytes[pointer];
				pointer++;
			}
			return System.Text.Encoding.UTF8.GetString(str);
		}
		public void writeDouble(double i)
		{

			append(InfoType.DOUBLE);
			append(i);
		}
		public InfoType type()
		{
			return( InfoType)bytes[pointer];
		}
	InfoStream(params InfoStream[] infos)
		{
			bytes = new byte[5];
			int l = infos.Length;
			bytes[0] = (byte)InfoType.LISTOBJECTS;
			bytes[1] = (byte)((l >> 24) & 0xff);
			bytes[2] = (byte)((l >> 16) & 0xff);
			bytes[3] = (byte)((l >> 8) & 0xff);
			bytes[4] = (byte)((l) & 0xff);

			foreach (InfoStream s in infos)
			{
				append(s.getSpan());
				appendValues((byte)InfoType.END);
				foreach(FileStream f in s.files)
				{
					files.Add
						(f);
				}
			}

		}
		public List<object> readList()
		{
			List<object> objects = new List<Object>();
			InfoType current = currentType();
			ReadFunction rf;
		

			while (current != InfoType.END)
			{
				if (current == InfoType.DOUBLE)
				{
					objects.Add(readDouble());
				}
				else if (current == InfoType.INT)
				{
					objects.Add(readInt());
				}
				else if (current == InfoType.STRING)
				{
					objects.Add(readString());
				}
				else if (current == InfoType.FLOAT)
				{
					objects.Add(readFloat());
				}
				else if (current == InfoType.INTARRAY)
				{
					objects.Add(readIntArray());
				}
				else if (current == InfoType.LONG)
				{
					objects.Add(readLong());
				}
				else if (current == InfoType.BYTE)
				{
					objects.Add(readByte());
				}
				else if (current == InfoType.BOOL)
				{
					objects.Add(readBool());
				}
				else if (current == InfoType.FILESTREAM)
				{
					objects.Add(readFileStream());
				}
				else if (current == InfoType.STRINGARRAY)
				{
					objects.Add(readStringArray());
				}
				else if (current == InfoType.NULL)
				{
					objects.Add(readNull());
				}
				else if (current == InfoType.MAP)
				{
					objects.Add(readMap());
				}
				else if (current == InfoType.INFINITE_MAP)
				{
					
					objects.Add(readInfiniteMap());
				}
				else if(readFMap.TryGetValue(current,out rf))
				{
					objects.Add(rf(this));
				}
				else
				{
					pointer++;
				}
				current = currentType();
			}

			return objects;
		}
		

		public Object readMap()
		{

			if(currentType()==InfoType.NULL)
			{
				pointer++;
				return null;
			}

			pointer++;
			InfoType type=(InfoType)currentType();
			int x = readInt();
			pointer--;
			int y = readInt();
			if (type == InfoType.BOOL)
			{
				Map<bool> map = new Map<bool>(x,y);
				bool[,] data = map.Data;
				for(int i=0;i<x;i++)
				{
					for(int j=0;j<y;j++)
					{
						pointer--;
						data[i,j] = readBool();
					}
				}
				return map;
			}
			else if (type == InfoType.BYTE)
			{
				Map<byte> map = new Map<byte>(x,y);
				byte[,] data = map.Data;
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						pointer--;
						data[i, j] = readByte();
					}
				}
				return map;
			}
			else if (type == InfoType.INT)
			{
				Map<int> map = new Map<int>(x,y);
				int[,] data = map.Data;
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						pointer--;
						data[i, j] = readInt();
					}
				}
				return map;
			}
			else if (type == InfoType.FLOAT)
			{
				Map<float> map = new Map<float>(x,y);
				float[,] data = map.Data;
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						pointer--;
						data[i, j] =readFloat();
					}
				}
				return map;
			}
			else if(type == InfoType.LONG)
			{
				Map <long> map = new Map<long>(x,y);
				long[,] data = map.Data;
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						pointer--;
						data[i, j] = readLong();
					}
				}
				return map;
			}
			else if(type==InfoType.DOUBLE)
			{
				Map<double> map = new Map<double>(x,y);
				double[,] data = map.Data;
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < y; j++)
					{
						pointer--;
						data[i, j] = readDouble();
					}
				}
				return map;
			}
			return new Map<object>(x, y);
		}
		public object readInfo()
		{
			InfoType type_ = type();
			if(type_==InfoType.HEAD)
			{
				readInt();
			}
			type_ = type();
			if (type_ == InfoType.OBJECTS)
			{
				List<object> list = readList();
				if(list.Count==1&&!(list[0] is FileStream))
				{
					return list[0];
				}
				else
				{
					return list;
				}
			}
			else if (type_ == InfoType.COMMAND)
			{
				List<object> list = readList();

				return new Command((string)list[0], list.Skip(1).ToArray());
			}
			else if(type_==InfoType.LISTOBJECTS)
			{
				int n = readInt();
				List<object>[] lists = new List<object>[n];
				for(int i=0;i<n;i++)
				{
					lists[i] = readList();
					pointer++;
				}
				return lists;
			}

			return null;
		}

		public Vector3 readVector3()
		{
			double x = readDouble();
			pointer--;
			double y = readDouble();
			pointer--;
			double z = readDouble();
			return new Vector3(x, y, z);
		}
		public void writeVector2(Vector2 v)
		{
			append(InfoType.VECTOR2);
			append(v.X);
			append(v.Y);	
		}
		public Vector2 readVector2()
		{
			double x = readDouble();
			pointer--;
			double y = readDouble();
			return new Vector2(x, y);
		}
		public void writeVector3(Vector3 v)
		{
			append(InfoType.VECTOR3);
			append(v.x);
			append(v.y);
			append(v.z);
		}
		public InfoType currentType()
		{
			if(pointer>=bytes.Length)
			{
				return InfoType.END;
			}
			byte b = bytes[pointer];

			

			if (b >= System.Enum.GetValues(InfoType.DOUBLE.GetType()).Length)
			{
				return InfoType.END;
			}
			else
			{
				return (InfoType)b;
			}
		}
		public static InfoStream create(params object[] objs)
		{
			return new InfoStream().write(objs);
		}
		public static InfoStream createList(params InfoStream[] objs)
		{
			return new InfoStream(objs);
		}
		public void writeBool(bool b)
		{
			append(InfoType.BOOL);
			append(b ? (byte)1 : (byte)0);
		}
		public static byte[] intToBytes(int i)
		{
			byte[] meg2 = new byte[4];
			meg2[0] = (byte)((i >> 24));
			meg2[1] = (byte)((i >> 16));
			meg2[2] = (byte)((i >> 8));
			meg2[3] = (byte)((i));
			return meg2;
		}
		public void write1_4(List<List<IntMap>> data)
		{
			append(data.Count);
			foreach (List<IntMap> bitmaps in data)
			{
				append(bitmaps.Count);
				foreach (IntMap bitmap in bitmaps)
				{
					write(bitmap);
				}
			}
		}
		public List<List<IntMap>> read1_4()
		{
			pointer--;
			int l = readInt();
			int k;
			List<List<IntMap>> list = new List<List<IntMap>>(l);
			List<IntMap> b;
			for (int i = 0; i < l; i++)
			{
				pointer--;
				k = readInt();
				b = new List<IntMap>(k);
				for (int j = 0; j < k; j++)
				{
					Object obj = readMap();
					if(obj != null)
					{
                        b.Add(new bitmap( (Map<int>)obj));
					}
					else
					{
						b.Add(null);
					}

					
				}
				list.Add(b);
			}
			return list;
		}
		public InfiniteMap readInfiniteMap()
		{
			InfiniteMap worldBitmap = new InfiniteMap();
			pointer++;
			worldBitmap.setPP(read1_4());
			worldBitmap.setPN(read1_4());
			worldBitmap.setNN(read1_4());
			worldBitmap.setNP(read1_4());
			return worldBitmap;
		}
		public void writeInfiniteMap(InfiniteMap worldBitmap)
		{
			append(new byte[] { (byte)InfoType.INFINITE_MAP });
			write1_4(worldBitmap.getPP());
			write1_4(worldBitmap.getPN());
			write1_4(worldBitmap.getNN());
			write1_4(worldBitmap.getNP());
		}
		public bool readBool()
		{
			pointer += 2;
			return bytes[pointer - 1] == 0 ? false : true;
		}
		public InfoStream asCommand()
		{
			return withType(InfoType.COMMAND);
		}
		public InfoStream withType(InfoType type)
		{
			setInfoType(type);
			return this;
		}
		public void writeIntArray(int[] ints)
		{
			append(InfoType.INTARRAY);
			append(ints.Length);
			append(MemoryMarshal.Cast<int,byte>( ints) );

		}
		public void writeStringArray(string[] ints)
		{
			append(InfoType.STRINGARRAY);
			append(ints.Length);
		foreach(string s in ints)
			{
				writeString(s);
			}
		}
		public string[] readStringArray()
		{
			int l = readInt();
			string[] ints = new string[l];
			for (int i = 0; i < l; i++)
			{
				ints[i] = readString();
			}
			return ints;
		}
		public int[] readIntArray()
		{
			int l = readInt();
			int[] ints = new int[l];
			for (int i = 0; i <l;i++)
			{
				pointer--;
				ints[i] = readInt();
			}
			return ints;
		}

		public void writeString(string s)
		{
			byte[] msg = Encoding.UTF8.GetBytes(s);
			append(InfoType.STRING);
			append(msg.Length);
			append(msg);
		}
	}
}
