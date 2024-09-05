using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathBase;
using Images;
using System.IO;
using DVOSLib;
using System.Xml;
using MachineLearning;
namespace Images
{





	public delegate double[][] HoffFunction(int x, int y);

	public class HoffCircle
	{
		int MinR;
		int MaxR;
		int Width;
		int Height;

		List<Vector3i> points;

		int[,,] Count;

		public int this[int x, int y, int r]
		{
			get
			{

				return Count[x, y, r - MinR];

			}
			set
			{

				Count[x, y, r - MinR] = value;

			}
		}
		public HoffCircle(int minR, int maxR, int width, int height)
		{
			MinR = minR;
			MaxR = maxR;
			Width = width;
			Height = height;

			int s = (int)(Math.PI * ((MaxR + 1) * (MaxR + 1) - MinR * MinR));
			points = new List<Vector3i>(s);
			for (int x = -MaxR; x < MaxR; x++)
			{
				for (int y = -MaxR; y < maxR; y++)
				{
					int r = (int)new Vector2i(x, y).length();
					if (r >= minR && r < maxR)
					{
						points.Add(new Vector3i(x, y, r));
					}
				}
			}


		}

		public void fill(Map<int> map)
		{
			Count = new int[Width, Height, MaxR - MinR];
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{



					Vector3i v;
					if (map[x, y] != 0)
					{
						foreach (Vector3i v3 in points)
						{
							v = v3.add(x,y,0);
							if (v.x >= 0 && v.y >= 0 && v.x < Width && v.y < Height)
							{
								this[v.x, v.y, v.z]++;
							}

						}
					}
				}
			}
		}
	}
	public class Space<T>
	{
		public int Dimenssion { get { return dim; } }
		public int[] Sizes { get { return Width.ToArray(); } }
		int dim;
		int maxLevel;
		object[] space;
		int[] Width;

public	T this[params int[] index] { get {
				int x;
				int w;
				object[] array=space;
				for(int i=maxLevel;i>0;i--)
				{
					x = index[i];
					w = Width[i];
					if(x>=0&&x<w)
					{
						array =(object[]) array[x];
					}
					else
					{
						return default(T);
					}
				}
				return (T)array[index[0]];
			
			}
			set
			{
				int x;
				int w;
				object[] array = space;
				for (int i = maxLevel; i > 0; i--)
				{
					x = index[i];
					w = Width[i];
					if (x >= 0 && x < w)
					{
						array = (object[])array[x];
					}
					else
					{
						return ;
					}
				}
			array[index[0]]=value;

			}

		}
		public Space(params int[] sizes)
        {
			dim = sizes.Length;
			maxLevel = dim - 1;
			space = getArray(dim - 1, sizes);
			Width = sizes;
        }	
		 object[] getArray(int level,params int[] length)
	{
		if(level==0)
		{
			return new object[length[level]];
		}
		else
		{
			int c = length[level];
			object[] objs = new object[c];
			for(int i=0;i<c;i++)
			{
				objs[i] = getArray(level - 1, length);
			}
			return objs;
		}
		
	}
	}


	
	public class HaarWorkingList:IxmlObject<HaarWorkingList>// 用于快速计算Haar特征值的序列
	{
		class HPLIB//帮助建立 HaarWorkingList的类
		{
		public	List<HaarOperator> operators = new List<HaarOperator>();
			public int findIndex(HaarOperator.HaarType type,int w,int h)//获取指定类型的Haar模板在列表中的位置，没有则创建
			{
				int n = 0;
				foreach(HaarOperator o in operators)
				{
					if(o.Type==type&&o.width==w&&o.height==h)
					{
						return n;
					}
					n++;
				}
				operators.Add(HaarOperator.getFromType(type, w, h));
				return n;
			}

		}
		public HaarWorkingList(string path)//从文件创建实例
		{
			loadFile(path);
		}
		public HaarWorkingList()// 创建空实例
		{
		}
		public HaarWorkingList(List<HaarInfo> infos)//从动态数组中Haarinfo序列创建实例
		{
			this.infos = new HaarWorkinginfo[infos.Count];
			int n = 0;
			HPLIB hPLIB = new HPLIB();
			int index;
			HaarWorkinginfo haar;
			foreach (HaarInfo info in infos)//根据Haarinfo信息创建HaarWorkinginfo信息并创建Haar模板
			{
				index = hPLIB.findIndex(info.Type, info.haarWidth, info.haarHeight);
				haar = new HaarWorkinginfo();
				haar.index = index;
				haar.Operator = hPLIB.operators[index];
				haar.x = info.x;
				haar.y = info.y;
				haar.Channel = info.Channel;
				this.infos[n] = haar;
				n++;
			}
			this.operators = hPLIB.operators.ToArray();
			GC.Collect();
		}
		public HaarWorkingList(ICollection<HaarInfo> infos)//从有序的Haarinfo序列创建实例
		{
			this.infos = new HaarWorkinginfo[infos.Count];
			int n = 0;
			HPLIB hPLIB = new HPLIB();
			int index;
			HaarWorkinginfo haar;
			foreach(HaarInfo info in infos)//根据Haarinfo信息创建HaarWorkinginfo信息并创建Haar模板
			{
				index = hPLIB.findIndex(info.Type, info.haarWidth, info.haarHeight);
				haar = new HaarWorkinginfo();
				haar.index = index;
				haar.Operator = hPLIB.operators[index];
				haar.x = info.x;
				haar.y = info.y;
				haar.Channel = info.Channel;
				this.infos[n] = haar;
				n++;
			}
			this.operators = hPLIB.operators.ToArray();
			GC.Collect();
		}
		public HaarWorkingList(IReadOnlyCollection<HaarInfo> infos)//从只读的有序Haarinfo序列创建实例
		{
			this.infos = new HaarWorkinginfo[infos.Count];
			int n = 0;
			HPLIB hPLIB = new HPLIB();
			int index;
			HaarWorkinginfo haar;
			foreach (HaarInfo info in infos)//根据Haarinfo信息创建HaarWorkinginfo信息并创建Haar模板
			{
				index = hPLIB.findIndex(info.Type, info.haarWidth, info.haarHeight);
				haar = new HaarWorkinginfo();
				haar.index = index;
				haar.Operator = hPLIB.operators[index];
				haar.x = info.x;
				haar.y = info.y;
				haar.Channel = info.Channel;
				this.infos[n] = haar;
				n++;
			}
			this.operators = hPLIB.operators.ToArray();
			GC.Collect();
		}
		public  HaarWorkingList selectRange(int minInclude, int maxExclude)// 输出指定信息维度范围的序列
		{

			HaarWorkinginfo[] fdata = new HaarWorkinginfo[maxExclude - minInclude];
			Array.Copy(infos, minInclude, fdata, 0, maxExclude - minInclude);
			HaarWorkingList l = new HaarWorkingList();
			l.infos = fdata;
			l.operators = operators;
			return l;
		}
		public HaarWorkingList selectRange(ICollection<int> range)// 输出指定信息维度范围的序列
		{

			HaarWorkinginfo[] fdata = new HaarWorkinginfo[range.Count];
			for(int i=0;i<range.Count;i++)
			{
				fdata[i] = infos[ range.ElementAt(i)];
			}
			HaarWorkingList l = new HaarWorkingList();
			l.infos = fdata;
			l.operators = operators;
			return l;
		}
		public class HaarWorkinginfo:IxmlObject<HaarWorkinginfo>// 储存有Haar特征值信息的类 与HaarInfo类的区别在于包含一个Haar模板可以直接计算,HaarInfo中是模板的信息，需要先创建模板再计算

		{
			/// <summary>
			///颜色通道
			/// </summary>
			public Channel Channel;
			/// <summary>
			/// Haar模板在HaarWorkinginfoList中的位置
			/// </summary>
			public int index;
			/// <summary>
			/// Haar模板
			/// </summary>
			public HaarOperator Operator;
			/// <summary>
			/// 坐标
			/// </summary>
			public int x;
			/// <summary>
			/// 
			/// </summary>
			public int y;
			//以下是关于储存读取文件的函数，有两种储存模式，xml文件和直接用二进制储存
			public HaarWorkinginfo readXml(XmlElement element)
			{
				index = int.Parse(element.GetAttribute("Index"));
				x = int.Parse(element.GetAttribute("X"));
				y = int.Parse(element.GetAttribute("Y"));
				Channel = (Channel)int.Parse(element.GetAttribute("Channel"));
				return this;
			}
	
			public	void write(Stream s)
		{
			    s.Write(BitConverter.GetBytes(index), 0, 4);
				s.Write(BitConverter.GetBytes(x), 0, 4);
				s.Write(BitConverter.GetBytes(y), 0, 4);
				s.Write(BitConverter.GetBytes((int)Channel), 0, 4);
			}

			public XmlElement writeXml(XmlElement element)
			{
			   
				element.SetAttribute("Index", index.ToString());
				element.SetAttribute("X", x.ToString());
				element.SetAttribute("Y", y.ToString());
				element.SetAttribute("Channel", ((int)Channel).ToString());
				return element;
			}
		}

		

		HaarOperator[] operators;
		HaarWorkinginfo[] infos;

		public HaarWorkingList loadFile(string path)
		{
			if(File.Exists(path))
			{
				FileStream stream = new FileStream(path, FileMode.Open);
				 read(stream);
				stream.Close();
				stream.Dispose();
				
			}return this;
		}

		public HaarWorkingList loadXMLFile(string path)
		{
			if (File.Exists(path))
			{
				XmlDocument x = new XmlDocument()
					  ;
				x.Load(path);
				readXml(x.DocumentElement);

			}
			return this;
		}
		public void saveXmlFile(string path)
		{

			XmlDocument f = new XmlDocument();
			XmlElement e = f.CreateElement("List");
			f.AppendChild(e);
			writeXml(e);
			f.Save(path);


		}
		public void saveFile(string path)
		{
			
				FileStream stream = new FileStream(path, FileMode.Create);
				write(stream);
			stream.Flush();
			stream.Close();
				stream.Dispose();

			
		
		}
	
		public HaarWorkingList read(Stream s)
		{
			byte[] data = new byte[4];
			s.Read(data, 0, 4);
			int count = BitConverter.ToInt32(data,0);
			int w, h;
			HaarOperator.HaarType type;
			operators = new HaarOperator[count];
			for(int i=0;i<count;i++)
			{
				s.Read(data, 0, 4);
				type = (HaarOperator.HaarType) BitConverter.ToInt32(data, 0);
				s.Read(data, 0, 4);
				w = BitConverter.ToInt32(data, 0);
				s.Read(data, 0, 4);
				h = BitConverter.ToInt32(data, 0);
				operators[i] = HaarOperator.getFromType(type, w, h);
			}
			s.Read(data, 0, 4);
			count = BitConverter.ToInt32(data, 0);
			infos = new HaarWorkinginfo[count];
			for (int i = 0; i < count; i++)
			{
				HaarWorkinginfo info = new HaarWorkinginfo();

				s.Read(data, 0, 4);
				info.index = BitConverter.ToInt32(data, 0);
				s.Read(data, 0, 4);
				info.x = BitConverter.ToInt32(data, 0);
				s.Read(data, 0, 4);
				info.y = BitConverter.ToInt32(data, 0);
				s.Read(data, 0, 4);
				info.Channel = (Channel)BitConverter.ToInt32(data, 0);
				info.Operator = operators[info.index];
				infos[i] = info;
			}
				return this;
		}
		public void write(Stream s)
		{
			s.Write(BitConverter.GetBytes(operators.Length), 0, 4);
			foreach(HaarOperator haar in operators)
			{
				s.Write(BitConverter.GetBytes((int)haar.Type), 0, 4);
				s.Write(BitConverter.GetBytes(haar.width), 0, 4);
				s.Write(BitConverter.GetBytes(haar.height), 0, 4);
			}
			s.Write(BitConverter.GetBytes(infos.Length), 0, 4);
			foreach (HaarWorkinginfo info in infos)
			{
				info.write(s);
			}
		}
		public DataVector<double, int> getADAData(IntegralMap map, IntegralMap sq, int label,double factor)//计算带标签的特征值
		{
			double[] d = getData(map, sq,factor);

			DataVector<double, int> data = new DataVector<double, int>(d.Length);
			data.Label = label;
			data.Data = d;
			return data;


		}
		public  DataVector<double,int> getADAData(IntegralMap map,IntegralMap sq,int label)//计算带标签的特征值
		{
			double[] d = getData(map,sq);

			DataVector<double, int> data = new DataVector<double, int>(d.Length);
			data.Label = label;
			data.Data = d;
			return data;


		}
		public DataVector<double, int> getADAData(bitmap map, int label)//计算带标签的特征值
		{
			double[] d = getData(map);

			DataVector<double, int> data = new DataVector<double, int>(d.Length);
			data.Label = label;
			data.Data = d;
			return data;


		}
		public HaarWorkingList getInfoFromFullTrainedADA( AdaBoost ai)//从强分类器中生成对应计算序列
		{
			List<HaarWorkinginfo> l = new List<HaarWorkinginfo>();
			foreach (DecisionStump s in ai.m_WeakClassifiers)
			{
				l.Add(infos[s.Dimension]);
			}
			HaarWorkingList list = new HaarWorkingList();
			list.operators = operators;
			list.infos = l.ToArray();
			return list;
		}
		public double[] getData(bitmap bitmap)//计算特征值

		{
			IntegralMap[] maps = IntegralMap.GetRGBGraySQ(bitmap);
			IntegralMap r = maps[0];
			IntegralMap g = maps[1];
			IntegralMap b = maps[2];
			IntegralMap gray = maps[3];
			IntegralMap rsq = maps[4];
			IntegralMap gsq = maps[5];
			IntegralMap bsq = maps[6];
			IntegralMap graysq = maps[7];
			double[] data = new double[infos.Length];
			int n = 0;
		
			foreach (HaarWorkinginfo info in infos)
			{
				switch(info.Channel)
				{

					case Channel.Red:info.Operator.setImage(r,rsq,bitmap.Width,bitmap.Height);break;
					case Channel.Green: info.Operator.setImage(g,gsq, bitmap.Width, bitmap.Height); break;
					case Channel.Blue: info.Operator.setImage(b,bsq, bitmap.Width, bitmap.Height); break;
					default: info.Operator.setImage(gray,graysq, bitmap.Width, bitmap.Height); break;
				}
				data[n] = info.Operator[info.x, info.y];
				n++;
			}
			return data;
		}
		public double[] getData(IntegralMap map, IntegralMap sq,double factor)//计算特征值
		{
			double[] data = new double[infos.Length];
			int n = 0;
			foreach (HaarOperator haar in operators)
			{
				haar.setImage(map, sq, factor);
			}
			foreach (HaarWorkinginfo info in infos)
			{
				data[n] = info.Operator[info.x, info.y];
				n++;
			}
			return data;
		}
		public double[] getData(IntegralMap map,IntegralMap sq)//计算特征值
		{
			double[] data = new double[infos.Length];
			int n = 0;
			foreach(HaarOperator haar in operators)
			{
				haar.setImage(map,sq,map.virtualWidth,map.virtualHeight);
			}
			foreach(HaarWorkinginfo info in infos)
			{
			data[n]=info.Operator[info.x, info.y];
				n++;
			}
			return data;
		}

		public XmlElement writeXml(XmlElement element)//写mxl文件
		{
			element.SetAttribute("InfoCount", infos.Length.ToString());
			element.SetAttribute("HaarCount", operators.Length.ToString());
		XmlElement e1=	element.OwnerDocument.CreateElement("Operators");
			XmlElement e2 = element.OwnerDocument.CreateElement("Infos");
			XmlElement e;
			element.AppendChild(e2);
			element.AppendChild(e1);
			int n = 0;
			foreach(HaarWorkinginfo haar in infos)
			{
				e = element.OwnerDocument.CreateElement("Info");
				e = haar.writeXml(e);
				e.SetAttribute("Pos", n.ToString());
				e2.AppendChild(e);
				n++;
			}
			n = 0;
			foreach (HaarOperator haar in operators)
			{
				e = element.OwnerDocument.CreateElement("Operator");
				e = haar.writeXml(e);
				e.SetAttribute("Pos", n.ToString());
				e1.AppendChild(e);
				n++;
			}
			return element;
		}

		public HaarWorkingList readXml(XmlElement element)//读mxl文件
		{
			int ic = int.Parse(element.GetAttribute("InfoCount"));
			int hc = int.Parse(element.GetAttribute("HaarCount"));
			operators = new HaarOperator[hc];
			infos = new HaarWorkinginfo[ic];
			int index;
			HaarOperator op;
			HaarWorkinginfo info;
			int pos;
			foreach (XmlElement e in element.ChildNodes)
			{
				if(e.Name.Equals("Operators"))
				{
					foreach (XmlElement e1 in e)
					{
						if (e1.Name.Equals("Operator"))
						{
							index = e1.GetAttributeInt("Pos");
							op = new HaarOperator("");
							op.readXml(e1);
							operators[index] = op;
						}
					}
				}
			}
			foreach (XmlElement e in element.ChildNodes)
			{
				if (e.Name.Equals("Infos"))
				{
					foreach (XmlElement e1 in e)
					{
						if (e1.Name.Equals("Info"))
						{
								
							info =new HaarWorkinginfo();
							info.readXml(e1);
							info.Operator = operators[info.index];
						    pos = e1.GetAttributeInt("Pos");
							infos[pos] = info;
						}
					}
				}
			}
			return this;
		}
	}
	public class HaarInfo:IxmlObject<HaarInfo> //储存特征值计算用的信息如坐标和相应Haar模板的信息
	{
	
public HaarInfo()
		{
		}
			public HaarInfo(string type,int index ,int w,int h,int hw,int hh,int x,int y,HaarOperator.HaarType Type,Channel channel)
		{
			this.index = index;
			this.Name = type;
			width = w;
			this.Type = Type;
			height = h;
			 haarWidth=hw ;
			  haarHeight=hh;
			this.x = x;
			this.y = y;
			this.Channel = channel;
		}
		
		public int index { get; internal set; }
		public int width { get; internal set; }
		public int height { get; internal set; }
		public int x{ get; internal set; }
		public int y { get; internal set; }
		public int haarWidth{ get; internal set; }
		public int haarHeight { get; internal set; }
		public Channel Channel { get; internal set; }
		public string Name { get; internal set; }

		public HaarOperator createNew()//根据信息生成Haar模板
		{
			HaarOperator haar;
			switch(Type)
			{
				case HaarOperator.HaarType.A:
					haar = HaarOperator.getTypeA(width,height);

					break;
				case HaarOperator.HaarType.B:
					haar = HaarOperator.getTypeB(width, height);

					break;
				case HaarOperator.HaarType.C:
					haar = HaarOperator.getTypeC(width, height);break;

				case HaarOperator.HaarType.D:
					haar = HaarOperator.getTypeD(width, height);break;

				case HaarOperator.HaarType.E:
					haar = HaarOperator.getTypeE(width, height);

					
					break;
				default: haar = null;break;
			}
			return haar;
		}


		public HaarOperator.HaarType Type { get; internal set; }


		//储存为xml文件的函数
		public HaarInfo readXml(XmlElement element)
		{
			index = int.Parse(element.GetAttribute("Index"));
			width= int.Parse(element.GetAttribute("Width"));
			height = int.Parse(element.GetAttribute("Height"));
			haarHeight = int.Parse(element.GetAttribute("HaarHeight"));
			haarWidth = int.Parse(element.GetAttribute("HaarWidth"));
			Channel = (Channel)int.Parse(element.GetAttribute("Channel"));
			x = int.Parse(element.GetAttribute("X"));
			y = int.Parse(element.GetAttribute("Y"));
			Type =(HaarOperator.HaarType) int.Parse(element.GetAttribute("Type"));
			Name = element.GetAttribute("Name");
			return this;
		}

		public XmlElement writeXml(XmlElement element)
		{
		    element.SetAttribute("Index", index.ToString());
			element.SetAttribute("Width", width.ToString());
			element.SetAttribute("Height", height.ToString());
			element.SetAttribute("HaarHeight", haarHeight.ToString());
			element.SetAttribute("HaarWidth", haarWidth.ToString());
			element.SetAttribute("X", x.ToString());
			element.SetAttribute("Channel", ((int)Channel).ToString());
			element.SetAttribute("Y", y.ToString());
			element.SetAttribute("Type", ((int)Type).ToString());
			element.SetAttribute("Name", Name.ToString());
			return element;
		}
	}

	//表征矩形框的类
	public class Rectanglei:IxmlObject<Rectanglei>
	{
		//右下角点坐标
		public int maxx { get; private set; }
		public int maxy { get; private set; }
	 //左上角点坐标
		public int miny { get; private set; }
		public int minx { get; private set; }

		public int X { get { return minx; }  }
		public int Y { get { return miny; } }

		public int CX { get { return (int)Math.Round( ( maxx+minx)/2.0); } }
		public int CY { get { return (int)Math.Round((maxy + miny) / 2.0); } }
		public int Width { get { return maxx - minx + 1; } }
		public int Height { get { return maxy - miny + 1; } }

		public int width { get { return maxx - minx ; } }
		public int height { get { return maxy - miny ; } }
		public bool isIn(Vector2i v)
		{
			return v.X >= minx && v.X <= maxx && v.Y >= miny && v.Y <= maxy;
		}

		//写xml文件
		public XmlElement writeXml(XmlElement element)
		{
			element.SetAttribute("MinX", minx.ToString());
			element.SetAttribute("MinY", miny.ToString());
			element.SetAttribute("MaxX", maxx.ToString());
			element.SetAttribute("MaxY", maxy.ToString());
			return element;
		}

		// 
		/// <summary>
		/// 合并矩形框
		/// </summary>
		/// <param name="rectangleis">矩形框序列</param>
		/// <param name="quick">是否采用快速方法，不使用则添加矩形时遍历之前所有矩形，使用则遇到可以合并的就停止遍历
		/// （默认若a可以与b合并，那么不能与a合并的也不能与b合并，所以不会存在多个可以与新添加矩形合并的矩形）</param>
		/// <returns></returns>
		public static Rectanglei[] Merge(ICollection<Rectanglei> rectangleis, bool quick = true)
		{
			List<Rectanglei> result = new List<Rectanglei>(rectangleis.Count);
			int c;
			Rectanglei temp;
			List<Rectanglei> temps;
			Rectanglei t;
			foreach (Rectanglei r in rectangleis)
			{
				c = result.Count;
				bool flag = false;
				t = r;
				for(int i=0;i<c;i++)
				{
					temp = result[i];
					temp = r.tryMerge(temp);
					if(temp!=null)
					{
						t = temp;
						if(quick)
						{
                         flag = true;
						result[i] = temp;
						break;
						}
						else
						{
							result.RemoveAt(i);
							i--;
							c--;
						}
						
					}
				}
				if(!flag||!quick)
				{
					result.Add(t);
				}
			}
			return result.ToArray();
		}
			public static Rectanglei[] Merge_map(ICollection<Rectanglei> rectangleis,int width,int height)
			//使用二维数据结构合并矩形，由于感觉计算量过大所以没有写完而采用了上面那种算法
		{
		
			Map<Rectanglei> map = new Map<Rectanglei>(width, height);
			List<Rectanglei> result = new List<Rectanglei>(rectangleis.Count);
			Map<int> index = new Map<int>(width, height);
			int rx, ry;
			Rectanglei current,temp;
			List<Rectanglei> temps = new List<Rectanglei>(100);
			foreach(Rectanglei r in rectangleis)
			{
				current = r;
				if(result.Count==0)
				{
					map[r.minx, r.miny] = r;
					index[r.minx, r.miny] = 0;
					result.Add(r);

				}
				else
				{
					temps.Clear();
					rx = r.Width >> 3;
					ry = r.Height >> 3;
					for(int i=-rx;i<=rx;i++)
					{
						for (int j = -ry; j<= ry; j++)
						{
							temp = map[i+r.minx, j+r.miny];
							if(temp!=null)
							{
								temps.Add(temp);
							}
						}
					}

					foreach(Rectanglei rn in temps )
					{

					}

				}
			}
			return result.ToArray();
		}
		public Rectanglei tryMerge( Rectanglei other)//判断是否可以合并，同时判断保留哪个(保留面积小的)
		{
			int dx, dy,dx2,dy2,l=this.Width+this.Height;

			//判断是否完全包含
			if(minx<=other.minx&&miny<=other.miny&&maxx>=other.maxx&&maxy>=other.maxy)
			{
				return other;
			}
			if (minx >= other.minx && miny >= other.miny && maxx <= other.maxx && maxy <= other.maxy)
			{
				return this;
			}

			 
			dx = minx - other.minx;
			dx=dx < 0 ? -dx : dx;
			dy = miny - other.miny;
			dy = dy < 0 ? -dy : dy;
			dx2 = maxx - other.maxx;
			dx2 = dx2 < 0 ? -dx2 : dx2;
			dy2 = maxy - other.maxy;
			dy2 = dy2 < 0 ? -dy2 : dy2;

			//判断两角点坐标是否非常接近
			if(((dx+dy)<<2)/l==0&&((dx2+dy2)<<1)/l==0)
			{
				if(Width*Height<other.Width*Height)
				{
					return this;
				}
				else
				{
					return other;
				}
			}
			return null;
		}

		public Rectanglei readXml(XmlElement element)// 读xml文件
		{
			minx = int.Parse(element.GetAttribute("MinX"));
			miny = int.Parse(element.GetAttribute("MinY"));
			maxx = int.Parse(element.GetAttribute("MaxX"));
			maxy = int.Parse(element.GetAttribute("MaxY"));
			return this;
		}
		public Rectanglei()//  空实例
		{

		}
		public Rectanglei(int x,int y, int width,int height)// 创建实例
		{
			minx = x;
			miny = y;
			maxx = x + width;
			maxy=y+height;
		}
		public Rectanglei(Vector2i p1,Vector2i p2)// 创建实例
		{
			maxx = Math.Max(p1.X, p2.X);
			maxy = Math.Max(p1.Y, p2.Y);
			minx = Math.Min(p1.X, p2.X);
			miny = Math.Min(p1.Y, p2.Y);
		}

	}
	public class HaarOperator:IxmlObject<HaarOperator>
	{
	public	enum HaarType:int//Haar模板的类型，我只定义了5基础
							 //的，
		{
			A,B,C,D,E,Other
		}
		// 用于计算的积分图
		IntegralMap image;
		IntegralMap Sqimage;

		public string name { get; internal set; }

		public HaarOperator(string type)
		{
			this.name = type;
		Type = HaarType.Other;
			broken = false;
		}

		// 设置作用的积分图 自动标准化因子
		public void setImage(IntegralMap image,IntegralMap sq,int w,int h)
		{
			this.image = image;
			Sqimage = sq;
		    double	sN1 = 1.0 / (w * h);
			double mean = image[ 1,1,w-1,h-1] * sN1;
			double sqmean = Sqimage[1,1,w-1,h-1] * sN1;
			varNormFactor =1.0/ Math.Sqrt(sqmean - mean * mean);
			
		}
		// 设置作用的积分图 手动传入标准化因子
		public void setImage(IntegralMap image, IntegralMap sq, double varNormFactor)
		{
			this.image = image;
			Sqimage = sq;
		this.	varNormFactor = varNormFactor;

		}
		double varNormFactor;

		//计算能够生产的所有数据数量
		public int Count { get
			{
				if(broken)
				{
					return 0;
				}
				if(image!=null)
				{
					return (image.width - width + 1)*(image.height - height + 1);
				}
				return 0;
			} }

		public bool broken { get; internal set; }//结构是否合理
		public HaarType Type { get; internal set; }//类型
		public int width{ get; internal set; }
		public int height{ get; internal set; }
		public	Rectanglei [] White { get; internal set; }//白色的矩形
		public Rectanglei[]Black { get; internal set; }//黑色的矩形



		public double this[int x, int y, IntegralMap integralImage, IntegralMap sq]//设置积分图并计算数据
		{
			get { setImage(integralImage,sq,integralImage.virtualWidth,integralImage.virtualHeight);return this[x, y]; }
		}
		public double this[int x, int y]// 计算数据
		{
			get {
				int sum = 0;
				
	foreach (Rectanglei rectanglei in White)
				{
                         sum += image[rectanglei.minx+x, rectanglei.miny + y, rectanglei.maxx + x, rectanglei.maxy + y];
			
					
					
				}
				foreach (Rectanglei rectanglei in Black)
				{
				

						sum -= image[rectanglei.minx + x, rectanglei.miny + y, rectanglei.maxx + x, rectanglei.maxy + y];
			


				}
				
			


			
			
				return sum * varNormFactor;
					; }
		}

		internal int getList(List<HaarInfo> list,int iwidth,int iheight,Channel channel)//计算自己能够计算的所有Haar特征值信息（不同的位置）
		{
			if (broken)
			{
				return 0;
			}

			int maxw = 0;
			int maxh = 0;

			maxw = iwidth - width + 1;
			maxh = iheight - height + 1;
			if(list!=null)
			{
	for (int i = 0; i < maxw; i++)
			{
				for (int j = 0; j < maxh; j++)
				{

					list.Add(new HaarInfo(name, list.Count, iwidth, iheight, width, height,i,j,this.Type,channel));
				}
			}
			}
		
			return maxh * maxw;
		}
	
		public double [] getData()// 获取所有能计算的特征值（位置不同)
		{
			
			if(broken)
			{
				return new double[0];
			}
			int maxw=0;
			int maxh=0;
			
	         maxw = image.width - width +1;
			 maxh = image.height - height+1 ;
			int n = 0;
			double[] data = new double[maxw * maxh];
			for (int i = 0; i < maxw; i++)
			{
				for (int j = 0; j < maxh; j++)
				{
					data[n] = this[i, j];
					n++;
				}
			}
			return data;
		
		

		}
		public static HaarOperator getFromType(HaarType type,int width ,int height)//根据类型尺寸创建模板
		{
			HaarOperator haar;
			switch (type)
			{
				case HaarType.A:
					haar = HaarOperator.getTypeA(width, height);

					break;
				case HaarOperator.HaarType.B:
					haar = HaarOperator.getTypeB(width, height);

					break;
				case HaarOperator.HaarType.C:
					haar = HaarOperator.getTypeC(width, height); break;

				case HaarOperator.HaarType.D:
					haar = HaarOperator.getTypeD(width, height); break;

				case HaarOperator.HaarType.E:
					haar = HaarOperator.getTypeE(width, height);


					break;
				default: haar = null; break;
			}
			return haar;
		}
		public static HaarOperator getTypeA(int width,int height)//创建模板A
		{
			if (width < 2 || height < 1 || width % 2 != 0)
			{
				HaarOperator haa = new HaarOperator("A_Broken");
			
				haa.broken = true;
				return haa;
			}

			
			int w1 = width / 2;
			HaarOperator haar = new HaarOperator("A");
			haar.Type = HaarType.A;
			haar.width = width;
			haar.height = height;
			haar.White = new Rectanglei[] {new Rectanglei(new Vector2i(0,0),new Vector2i(w1-1,height-1)) };
			haar.Black = new Rectanglei[] { new Rectanglei(new Vector2i(w1, 0), new Vector2i(width-1, height-1)) };
			return haar;
		}
		public static HaarOperator getTypeB(int width, int height)//
		{
	
			if (width < 1|| height < 2||height%2!=0)
			{
				HaarOperator haa = new HaarOperator("B_Broken");
				haa.broken = true;
				return haa;
			}
			int h1 = height / 2;
			HaarOperator haar = new HaarOperator("B");
			haar.Type = HaarType.B;
			haar.width = width;
			haar.height = height;
			haar.White = new Rectanglei[] { new Rectanglei(new Vector2i(0, 0), new Vector2i(width-1, h1-1)) };
			haar.Black = new Rectanglei[] { new Rectanglei(new Vector2i(0, h1), new Vector2i(width - 1, height - 1)) };
			return haar;
		}
		public static HaarOperator getTypeC(int width, int height)
		{
			if (width < 3 || height < 1||width%3!=0)
			{
				HaarOperator haa = new HaarOperator("C_Broken");
				haa.broken = true;
				return haa;
			}
			int w1 = width / 3;
			HaarOperator haar = new HaarOperator("C");
			haar.Type = HaarType.C;
			haar.width = width;
			haar.height = height;
			haar.White = new Rectanglei[] { new Rectanglei(new Vector2i(0, 0), new Vector2i(w1 - 1, height - 1)) ,
                                            new Rectanglei(new Vector2i(w1, 0), new Vector2i(w1*2 - 1, height - 1))
			};
			haar.Black = new Rectanglei[] { new Rectanglei(new Vector2i(w1*2, 0), new Vector2i(width - 1, height - 1)) };
			return haar;
		}
		public static HaarOperator getTypeD(int width, int height)
		{
			if (width < 1 || height < 3||height%3!=0)
			{
				HaarOperator haa = new HaarOperator("D_Broken");
				haa.broken = true;
				return haa;
			}
			int h1 = height / 3;
			HaarOperator haar = new HaarOperator("D");
			haar.Type = HaarType.D;
			haar.width = width;
			haar.height = height;
			haar.White = new Rectanglei[] { new Rectanglei(new Vector2i(0, 0), new Vector2i(width - 1, h1 - 1)),
			new Rectanglei(new Vector2i(0, h1), new Vector2i(width - 1, h1*2 - 1))};

			haar.Black = new Rectanglei[] { new Rectanglei(new Vector2i(0, h1*2), new Vector2i(width - 1, height - 1)) };
			return haar;
		}
		public static HaarOperator getTypeE(int width, int height)
		{
			if (width < 2 || height < 2 || height % 2 != 0 || width % 2 != 0)
			{
				HaarOperator haa = new HaarOperator("E_Broken");
				haa.broken = true;
				return haa;
			}
			
			int h1 = height / 2;
			int w1 = width / 2;
			HaarOperator haar = new HaarOperator("E");
			haar.Type = HaarType.E;
			haar.width = width;
			haar.height = height;
			haar.White = new Rectanglei[] { new Rectanglei(new Vector2i(0, 0), new Vector2i(w1 - 1, h1 - 1)),
			new Rectanglei(new Vector2i(w1, h1), new Vector2i(width - 1, height - 1))};

			haar.Black = new Rectanglei[] { new Rectanglei(new Vector2i(w1, 0), new Vector2i(width-1 , h1 - 1)),
			new Rectanglei(new Vector2i(0, h1), new Vector2i(w1-1 ,height-1))};
			return haar;
		}
		// 读写文件
		public XmlElement writeXml(XmlElement element)
		{
			XmlElement e;
			element.SetAttribute("Width", width.ToString());
			element.SetAttribute("Heihgt", height.ToString());
			element.SetAttribute("Broken", broken.ToString());
			element.SetAttribute("Name", name);
			element.SetAttribute("Type", ((int)Type).ToString());
			foreach(Rectanglei r in White)
			{
				e = element.OwnerDocument.CreateElement("White");
				element.AppendChild(e);
				r.writeXml(e);
			}
			foreach (Rectanglei r in Black)
			{
				e = element.OwnerDocument.CreateElement("Black");
				element.AppendChild(e);
				r.writeXml(e);
			}
			return element;
		}

		public HaarOperator readXml(XmlElement element)
		{
			int w = int.Parse(element.GetAttribute("Width"));
			int h=int.Parse( element.GetAttribute("Heihgt"));
			bool b = bool.Parse(element.GetAttribute("Broken"));
			name = element.GetAttribute("Name");
			List<Rectanglei> w_ = new List<Rectanglei>();
			List<Rectanglei> b_ = new List<Rectanglei>();
			Type = (HaarType)element.GetAttributeInt("Type");
			foreach (XmlElement xml in element.ChildNodes)
			{
				if(xml.Name.Equals("White"))
				{
					w_.Add(new Rectanglei().readXml(xml));
				}
				else if(xml.Name.Equals("Black"))
				{
					b_.Add(new Rectanglei().readXml(xml));
				}
			}
	
			width = w;
			height = h;
			White = w_.ToArray();
			Black = b_.ToArray();
			broken = b;
			return this;
		}
	}
	public static class Haar//整合了计算不同尺寸Haar值信息模板，计算特征值、查询数据维度功能的静态类
	{
		static List<IReadOnlyCollection<HaarInfo>> libRGB = new List<IReadOnlyCollection<HaarInfo>>();
		static int[,] libIndexRGB = new int[65, 65];
		static List<IReadOnlyCollection<HaarInfo>> libSingle = new List<IReadOnlyCollection<HaarInfo>>();
		static int[,,] libIndexSingle = new int[4, 65, 65];
		static Haar()
		{
			for(int c=0;c<4;c++)
			for(int i=0;i<65;i++)
			{
				for (int j = 0; j < 65; j++)
				{
					libIndexRGB[i, j] = -1;
					libIndexSingle[c,i, j] = -1 ;
				}
			}
		}
		public static IReadOnlyCollection<HaarInfo> SingleRules(Channel c,int w, int h)
		{
			if (w > 64 || h > 64||(int)c>3)
			{
				throw (new Exception("尺寸不得大于64"));
			}
			int i = libIndexSingle[(int)c,w, h];
			if (i < 0)
			{
				i = libSingle.Count;
				List<HaarInfo> infos = new List<HaarInfo>();
				getIndex(infos, w, h,c, "Single");
				libSingle.Add(infos);
				libIndexSingle[(int)c,w, h] = i;
			}
			return libSingle[i];
		}
		public static IReadOnlyCollection<HaarInfo> RGBRules( int w,int h )
		{
			if(w>64||h>64)
			{
				throw (new Exception("尺寸不得大于64"));
			}
			int i = libIndexRGB[w, h];
			if(i<0)
			{
				i = libRGB.Count;
				List<HaarInfo> infos = new List<HaarInfo>();
				getIndex(infos, w, h,Channel.Red, "R");
				getIndex(infos, w, h, Channel.Green, "G");
				getIndex(infos, w, h, Channel.Blue, "B");
				libRGB.Add(infos);
				libIndexRGB[w, h] = i;
			}
			return libRGB[i];
		}
		public static double[] getRGBData(bitmap bitmap)
		{
			IntegralMap[] map = IntegralMap.GetRGBGray(bitmap);
			double[] R = getData(map[0],map[4]);
			double[] G = getData(map[1],map[5]);
			double[] B = getData(map[2],map[6]);
			double[] d = new double[R.Length + G.Length + B.Length];
			R.CopyTo(d,0);
			G.CopyTo(d, R.Length);
			B.CopyTo(d, R.Length + G.Length);
			return d;

		}
		public static DataVector<double, int> getRGBADA(bitmap bitmap,int label)
		{
			double[] d = getRGBData(bitmap);
			DataVector<double, int> data = new DataVector<double, int>(d.Length);
			data.Data = d;
			data.Label = label;
			return data;

		}
	
	

		public static double[] getData(ICollection<HaarInfo> infos, IntegralMap map, IntegralMap sq,double factor)
		{
			double[] vs = new HaarWorkingList(infos).getData(map, sq,factor);
			GC.Collect();
			return vs;
		}
		public static double[] getData(ICollection<HaarInfo> infos, IntegralMap map,IntegralMap sq)
		{
			double[] vs = new HaarWorkingList(infos).getData(map,sq);
			GC.Collect();
			return vs;
		}
	
		    public static List<HaarInfo> getRGBIndex(int width, int height)
		{
			List<HaarInfo> infos = new List<HaarInfo>();
			getIndex(infos, width, height, Channel.Red, "Red");
			getIndex(infos, width, height, Channel.Green, "Green");
			getIndex(infos, width, height, Channel.Blue, "Blue");
			return infos;

		}
			public static int getIndex(List<HaarInfo> list ,int width,int height,Channel channel,string name="")
		{
			HaarOperator a, b, c, d, e;
			int count = 0;
			for (int i = 1; i <= width; i++)
			{
				for (int j = 1; j <= height; j++)
				{
					a = HaarOperator.getTypeA(i, j);
					b = HaarOperator.getTypeB(i, j);
					c = HaarOperator.getTypeC(i, j);
					d = HaarOperator.getTypeD(i, j);
					e = HaarOperator.getTypeE(i, j);
					a.name += name;
					b.name += name;
					c.name += name;
					d.name += name;
					e.name += name;
					count +=
					a.getList(list, width, height, channel);
					count += b.getList(list, width, height, channel);
					count += c.getList(list, width, height, channel);
					count += d.getList(list, width, height, channel);
					count += e.getList(list, width, height, channel);
				}
			}
		
			return count;
		}
			public static double[] getData(IntegralMap map,IntegralMap sq)
		{
			HaarOperator a, b, c, d, e;
			List<double[]> data = new List<double[]>();
			int count=0;
			int pos = 0;
			int cc ;
			for(int i=1;i<=map.width;i++)
			{
				for(int j=1;j<= map.height;j++)
				{
					a = HaarOperator.getTypeA(i, j);
					b = HaarOperator.getTypeB(i, j);
					c = HaarOperator.getTypeC(i, j);
					d = HaarOperator.getTypeD(i, j);
					e = HaarOperator.getTypeE(i, j);
					a.setImage(map,sq,map.virtualWidth,map.virtualHeight);
					b.setImage(map,sq, map.virtualWidth, map.virtualHeight);
					c.setImage(map,sq, map.virtualWidth, map.virtualHeight);
					d.setImage(map,sq, map.virtualWidth, map.virtualHeight);
					e.setImage(map,sq, map.virtualWidth, map.virtualHeight);

					count += a.Count + b.Count + c.Count + d.Count + e.Count;
					data.Add(a.getData());
					data.Add(b.getData());
					data.Add(c.getData());
					data.Add(d.getData());
					data.Add(e.getData());
				}
			}
			double[] Data = new double[count];
			foreach(double[] dd in data)
			{
				cc = dd.Length;
				Array.Copy(dd, 0, Data, pos, cc);
				pos += cc;
			}
			return Data;
		}
	}

	public class SearchBox//一个可以滑动的搜索框，输出图像尺寸固定，位置和在原始图像上的尺寸可以变化
	{
		public SearchBox(bitmap b,int w,int h)
		{
			O_width = w;
			O_height = h;
			width = w;
			height = h;
			output_ = new bitmap(w, h);
			Bitmap = b;
			setSize(w, h);
		}

		public void setSize(int w,int h)//设置对应于原图的尺寸
		{
			width = w;
			height = h;
			maxY= Bitmap.Height -height;
			maxX = Bitmap.Width - width;
			dx = width / (double)O_width;
			dy = height / (double)O_height;
		}
		int lx;
		int ly;

		public void cleanLast()//在原图中抹去上一个访问的搜索框，没大用
		{
			int[,] data = Bitmap.Data;
			double X = lx, Y;
			for (int i = 0; i < width; i++, X += 1)
			{
				Y =ly;
				for (int j = 0; j < height; j++, Y += 1)
				{
					data[(int)X, (int)Y]=0;
				}
			}
		}

	public	bitmap this[int x,int y]// 获取指定位置的图像，并缩放为固定尺寸后输出
		{
			get
			{
			
				int[,] data1 = output_.Data;
				int[,] data = Bitmap.Data;
				x = x < 0 ? 0 : x;	
				
				y = y <=maxY ? y : maxY;
				y = y < 0 ? 0 : y;
				x = x <= maxX ? x : maxX;

				lx = x;
				ly = y;
				double X = x, Y;
				for (int i=0;i<O_width;i++,X+=dx)
				{
					Y = y;
					for (int j = 0; j < O_height; j++,Y+=dy)
					{
						data1[i, j] = data[(int)X, (int)Y];
					}
				}


				return output_;
			}
		}

		bitmap Bitmap;
		int O_width;
		int O_height;
		int width;
		int height;
		int maxX;
		int maxY;
		double dx;
		double dy;

		bitmap output_;
		


	}

	public interface IFaceTeacher// "老师类"的接口判断是否有人脸
	{
		bool isWithFace(bitmap bitmap);

		 void createLib(ICollection<bitmap> bitmaps, int width, int height, int minWidth = 1, int minHeight = 1, int maxWidth = -1, int maxHeight = -1);

		bitmap[] createNegtiveTextBook(int count);
		bitmap[] createPositiveTextBook( int count);
	}


	public class HaarTeacher : IFaceTeacher
		//利用FaceScaner 对IFaceTeacher的一个实现，另外一个是利用OpenCv的分类器实现，由于这个工程没有安装Opencv所以不在这个文件里
	{
		FaceScaner face;
		int n = 1;
		public HaarTeacher(string path)

		{
			this.face = new FaceScaner(-1);
			face.load(path);
		}
		public HaarTeacher(FaceScaner teacher)

		{
			this.face = teacher;
		}
		Random ran = new Random((int)DateTime.Now.Ticks);


		int maxWidth;
		int minWidth;
		int minHeight;
		int maxHeight;
		bool hasLib = false;

		// 创建素材库
		public void createLib(ICollection<bitmap> bitmaps, int width, int height, int minWidth = 1, int minHeight = 1, int maxWidth = -1, int maxHeight = -1)
		{
			this.minHeight = minHeight;
			this.minWidth = minWidth;
			this.maxHeight = maxHeight;
			this.maxWidth = maxWidth;
			lib_N.Clear();
			lib_P.Clear();
			hasLib = true;
			this.width = width;
			this.height = height;
			bitmap b;
			foreach (bitmap bitmap in bitmaps)
			{
				b = bitmap.Clone();

				Rectanglei[] rects = new Rectanglei[0];

				rects = face.getFaces(bitmap, Math.Min(maxWidth,maxHeight), Math.Max(minWidth, minHeight));
				SearchBox box = new SearchBox(b, width, height);
				foreach (Rectanglei r in rects)
				{
					box.setSize(r.Width, r.Height);
					lib_P.Add(box[r.X, r.Y]);

				}
				IntegralMap image = new IntegralMap(b, Channel.Gray);
				int v = image[b.Width - 1, b.Height - 1] / b.Width / b.Height;
				v = v & 0xff;
				v = (v << 8) | v | (v << 16);
				int[,] data = b.getData();

				; lib_N.Add(bitmap);

			}


		}
		int width;
		int height;

		//随机获取负样本
		public bitmap[] createNegtiveTextBook(int count)

		{
			if (!hasLib)
			{
				return new bitmap[0];
			}
			List<bitmap> result = new List<bitmap>(count);

			while (result.Count < count && lib_N.Count > 0)
			{
				foreach (bitmap b in lib_N[ran.Next(0, lib_N.Count)].getRandomAreas(100, width, height, minWidth, minHeight, maxWidth, maxHeight))
				{
					if (!(countFace(b) > 0))
					{
						result.Add(b);
						if (result.Count >= count)
						{
							return result.ToArray();
						}
					}

				}

			}

			return result.ToArray();

		}
		List<bitmap> lib_P = new List<bitmap>();
		List<bitmap> lib_N = new List<bitmap>();

		//随机获取正样本
		public bitmap[] createPositiveTextBook(int count)
		{
			if (!hasLib)
			{
				return new bitmap[0];
			}
			List<bitmap> result = new List<bitmap>(count);


			while (result.Count < count && lib_P.Count > 0)
			{
				result.Add(lib_P[ran.Next(0, lib_P.Count)]);
			}

			return result.ToArray();

		}
		public int countFace(bitmap bitmap)
		{
			return face.getFaces(bitmap, Math.Min(maxWidth, maxHeight), Math.Max(minWidth, minHeight)).Length;

		}
		public bool isWithFace(bitmap bitmap)
		{
			return face.getFaces(bitmap, Math.Min(maxWidth, maxHeight), Math.Max(minWidth, minHeight)).Length > 0;

		}
	}
	//一个传递字符串的委托， 用于传递信息给其他类
	public delegate void Notice(string info);

/// <summary>
/// 人脸检测器的类
/// </summary>
	public class FaceScaner:IxmlObject<FaceScaner>
	{
		int maxSize;
		int minSize;
		int size;
		double nextSize=0.9;
		float dx;
		float dy;
		int width;
		int height;
		int ScanSize = 24;//训练时的尺寸
		int ScanSize2 = 24*24;
		public event  Notice onNotice;

		HaarWorkingList[] haars=new HaarWorkingList[0];//计算Haar特征的序列
		AdaBoost[] ais=new AdaBoost[0];//训练好的强分类器
		DateTime t0;
		DateTime last;
		DateTime current;

		public void TickNotice( string s)//传递通知
		{
			last = current;
			current = DateTime.Now;
			onNotice(s+"\n耗时:" + (current - last).ToString() + "\n总耗时:" + (current - t0).ToString())
			 ;
		}
		/// <summary>
		/// 快速训练
		/// </summary>
		/// <param name="papers">用于制作训练集的图像</param>
		/// <param name="teacher">训练集制作器</param>
		/// <param name="maxADA">最大强分类器数量</param>
		/// <param name="maxSampleP">最大正样本数量</param>
		/// <param name="maxSampleN">最大负样本数量</param>
		/// <param name="maxWeakClassifiers">每个强分类器包含的弱分类器数量</param>
		/// <param name="firstBook">提供的基础训练集，可以为空</param>
		public void QuickTrain( ICollection<bitmap> papers, IFaceTeacher teacher, int maxADA = 20, int maxSampleP = 500, int maxSampleN = 500, int maxWeakClassifiers = 500,ICollection<DataVector<double, int>> firstBook=null)
		{
			t0 = DateTime.Now;
			last = t0;
			current = t0;
			AdaBoost ada;
			List<DataVector<double, int>> Book =new List<DataVector<double, int>>();

			IReadOnlyCollection<HaarInfo> rules = Haar.SingleRules(Channel.Gray, ScanSize, ScanSize);
			HaarWorkingList list = new HaarWorkingList(rules);

			List<HaarWorkingList> lists = new List<HaarWorkingList>();
			List<AdaBoost> Ais = new List<AdaBoost>();
			List<bitmap> errors = new List<bitmap>();
			List<bitmap> faces = new List<bitmap>();
			IntegralMap[] images;
			bitmap temp;
			teacher.createLib(papers,ScanSize,ScanSize,minSize, minSize,maxSize, maxSize);
			while (ais.Length < maxADA)
			{
				if(firstBook!=null)
				{
					Book = firstBook.ToList();
				}
				foreach(bitmap bitmap1 in teacher.createPositiveTextBook(maxSampleP))
				{
					images = IntegralMap.GetBSQ(bitmap1.toGray());
					Book.Add(list.getADAData(images[0], images[1], 1, getFactor(images[0], images[1])));
				}
				foreach (bitmap bitmap1 in teacher.createNegtiveTextBook( maxSampleN))
				{
					images = IntegralMap.GetBSQ(bitmap1.toGray());
					Book.Add(list.getADAData(images[0], images[1], -1, getFactor(images[0], images[1])));
				}

				TickNotice("更新样本库!");
				ada = new AdaBoost();
				ada.Train(AdaBoost.trainMode.Balance, adaSumMap.getMaps(Book), maxWeakClassifiers);
				GC.Collect();
				HaarWorkingList S_list = new HaarWorkingList(ada.resetDimension(rules));
				Ais.Add(ada);
				lists.Add(S_list);
				ais = Ais.ToArray();
				this.haars = lists.ToArray();
				TickNotice("完成一次训练! 已训练分类器:" + ais.Length + " 目标分类器数量:" + maxADA);
				if (ais.Length >= maxADA)
				{
					TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
					GC.Collect();
					return;
				}

				errors.Clear();
				foreach (bitmap bitmap in papers)
				{
					getFaces_train(bitmap, teacher, 250, 50, errors, 0.9,1);
					if (errors.Count >= 1 )
					{
						break;
					}

				}
				TickNotice("检测结束");
				if (errors.Count == 0)
				{
					TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
					GC.Collect();
					return;
				}
				GC.Collect();
				Book.Clear();
				int n = 0;


				GC.Collect();
			}



			TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
		}

		/// <summary>
		///  训练
		/// </summary>
		/// <param name="firstBook">提供的基础训练集</param>
		/// <param name="papers">用于制作扩展训练集的图像</param>
		/// <param name="teacher">训练集制作器，可以判断正误</param>
		/// <param name="maxADA">最大强分类器数量</param>
		/// <param name="maxSampleN">最大负样本数量</param>
		/// <param name="maxWeakClassifiers">每个强分类器包含的弱分类器数量</param>
		/// <param name="trainMode">训练模式（保证正样本正确率、保证负样本正确率、平衡）</param>
		/// <param name="maxError">容许的最大错误率，非平衡模式才有用</param>
		/// <param name="useCreatedSample">使用随机教师制作的训练集</param>
		/// 
		public void Train(ICollection<DataVector<double,int>> firstBook,ICollection<bitmap> papers,IFaceTeacher teacher ,int maxADA=20,int maxSampleN=500,  int maxWeakClassifiers=500,AdaBoost.trainMode trainMode=AdaBoost.trainMode.Positive, double maxError = 0.15,bool useCreatedSample=true)
		{
		
			List<DataVector<double, int>> FirstBook =firstBook.ToList();
	        firstBook = FirstBook;
			t0 = DateTime.Now;
			last = t0;
			current = t0;
			AdaBoost ada;
			List < DataVector<double, int> > 
				
				Book= firstBook.ToList();
			IReadOnlyCollection<HaarInfo> rules = Haar.SingleRules(Channel.Gray, ScanSize, ScanSize);//获取标准的Haar数据序列
			HaarWorkingList list = new HaarWorkingList(rules);//转化为用于快速运算的序列

			IntegralMap[] images;
			bitmap[] Negtive=new bitmap[0];
			if(useCreatedSample)//如果使用随机教师制作的训练集
			{
				teacher.createLib(papers, ScanSize, ScanSize,minSize,minSize,maxSize,maxSize);//创建训练集库
				Negtive = teacher.createNegtiveTextBook(maxADA * maxSampleN*50);//随机生产负样本
				for(int i=0;i<Negtive.Length;i++)
				{
					Negtive[i] = Negtive[i].toGray();//转化为灰度图

					if(i<maxSampleN)
					{ images = IntegralMap.GetBSQ(Negtive[i]);
						Book.Add(list.getADAData(images[0], images[1], -1, getFactor(images[0], images[1])));
					} //获取单词训练使用的负样本
				}
				TickNotice("制作负样本! " + " 数量:" + Negtive.Length);
				
			}
			
			
			List<HaarWorkingList> lists = new List<HaarWorkingList>();
			List<AdaBoost> Ais = new List<AdaBoost>();
			List<bitmap> errors = new List<bitmap>();
			bitmap temp;
			while(ais.Length<maxADA)
			{
				
				ada = new AdaBoost();//创建空的强分类器
			ada.Train(trainMode,adaSumMap.getMaps(Book), maxWeakClassifiers,maxError);//进行训练
			GC.Collect();//清理内存
			HaarWorkingList S_list = new HaarWorkingList(ada.resetDimension(rules));//获取训练器需要的Haar特征计算序列
			Ais.Add(ada);
			lists.Add(S_list);
				ais = Ais.ToArray();
				this.haars = lists.ToArray();//储存分类器和对应Haar特征计算序列
				TickNotice("完成一次训练! 已训练分类器:"+ais.Length+" 目标分类器数量:"+maxADA);
				if(ais.Length>= maxADA)//达到数量退出循环
				{
					TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
					GC.Collect();
					return;
				}
				
				errors.Clear();//准备错误收集器

				if(!useCreatedSample) //不使用随机训练集时
				{
                foreach(bitmap bitmap  in papers)
				{ 
					getFaces_train(bitmap, teacher, 500, ScanSize,errors,0.9,maxSampleN );//在图中进行判断，利用老师收集错误判断的负样本
					if(errors.Count>=maxSampleN)
					{
						break;
					}

				}
				}
				else//使用随机训练集时
				{
					recongnize(Negtive, errors, maxSampleN);//在图中进行判断收集错误判断的负样本
				}
				
				
				TickNotice("收集新样本!"  + " 负样本:"+ errors.Count);
				if (errors.Count==0)
				{
					if(useCreatedSample&&ais.Length<maxADA)//使用随机训练集时生成新的训练集
					{
						Negtive = teacher.createNegtiveTextBook(maxADA * maxSampleN * 50);
					}
					else// 没有错误退出循环
					{
                    TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
					GC.Collect();
					return;
					}

					
				}
				GC.Collect();

			
				Book = firstBook.ToList();
				foreach(bitmap b in errors)
				{
					images = IntegralMap.GetBSQ(b.scale(ScanSize,ScanSize));

					Book.Add(list.getADAData(images[0], images[1], -1, getFactor(images[0], images[1])));//将判断错误的图像制作成用于训练的Haar数据
				}
			
				
				TickNotice("更新样本库!");
				GC.Collect();
			}



			TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
		}


		/// <summary>
		/// 训练，无需老师只需提供足量的样本
		/// </summary>
		/// <param name="positive">正样本</param>
		/// <param name="negtive">负样本</param>
		/// <param name="maxADA">最大分类器数量</param>
		/// <param name="maxSampleN">最大负样本数量</param>
		/// <param name="maxWeakClassifiers">每个强分类器包含的弱分类器数量</param>
		/// <param name="trainMode">训练模式（保证正样本正确率、保证负样本正确率、平衡）</param>
		/// <param name="maxError">容许的最大错误率，非平衡模式才有用</param>
		public void Train(ICollection<bitmap> positive,ICollection<bitmap> negtive, int maxADA = 20, int maxSampleN = 500, int maxWeakClassifiers = 500, AdaBoost.trainMode trainMode = AdaBoost.trainMode.Positive, double maxError = 0.15)
		{

			
			t0 = DateTime.Now;
			last = t0;
			current = t0;
			AdaBoost ada;
			List<DataVector<double, int>> Book;
			List<DataVector<double, int>> Pos = new List<DataVector<double, int>>();

			IReadOnlyCollection<HaarInfo> rules = Haar.SingleRules(Channel.Gray, ScanSize, ScanSize);//获取标准的Haar数据序列
			HaarWorkingList list = new HaarWorkingList(rules);//转化为用于快速运算的序列
			bitmap temp;
			IntegralMap[] images;
			foreach(bitmap p in positive)//获取正样本
			{
				temp = p.scale(ScanSize,ScanSize).toGray();
				images = IntegralMap.GetBSQ(p);
				Pos.Add(list.getADAData(images[0], images[1], 1, getFactor(images[0], images[1])));

			}
			Book = Pos.ToList();
			int count = 0;
              foreach(bitmap n in negtive)
			{
				if(count<maxSampleN)
				{
                 temp = n.toGray();
				images = IntegralMap.GetBSQ(n);
				 Book.Add(list.getADAData(images[0], images[1], -1, getFactor(images[0], images[1])));
				}
				else
				{
					break;
				}
				
				
			}


			List<HaarWorkingList> lists = new List<HaarWorkingList>();
			List<AdaBoost> Ais = new List<AdaBoost>();
			List<bitmap> errors = new List<bitmap>();
			while (ais.Length < maxADA)
			{

				ada = new AdaBoost();//创建空的强分类器
				ada.Train(trainMode, adaSumMap.getMaps(Book), maxWeakClassifiers, maxError);//进行训练
				GC.Collect();//清理内存
				HaarWorkingList S_list = new HaarWorkingList(ada.resetDimension(rules));//获取训练器需要的Haar特征计算序列
				Ais.Add(ada);
				lists.Add(S_list);
				ais = Ais.ToArray();
				this.haars = lists.ToArray();//储存分类器和对应Haar特征计算序列
				TickNotice("完成一次训练! 已训练分类器:" + ais.Length + " 目标分类器数量:" + maxADA);
				if (ais.Length >= maxADA)//达到数量退出循环
				{
					TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
					GC.Collect();
					return;
				}

				errors.Clear();//准备错误收集器

				
					recongnize(negtive, errors, maxSampleN);//在图中进行判断收集错误判断的负样本
				

				TickNotice("收集新样本!" + " 负样本:" + errors.Count);
				if (errors.Count == 0)
				{
					
						TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
						GC.Collect();
						return;
					


				}
				GC.Collect();


				Book = Pos.ToList();
				foreach (bitmap b in errors)
				{
					images = IntegralMap.GetBSQ(b.scale(ScanSize, ScanSize));

					Book.Add(list.getADAData(images[0], images[1], -1, getFactor(images[0], images[1])));//将判断错误的图像制作成用于训练的Haar数据
				}


				TickNotice("更新样本库!");
				GC.Collect();
			}



			TickNotice("训练结束! " + " 分类器数量:" + ais.Length);
		}


		public void setSizeRange(bitmap bitmap,int maxsize,int minsize,double nextsize =0.9)//设置搜素的尺寸范围
		{
            if (maxsize > bitmap.Width)
				maxsize = bitmap.Width;
			if (maxsize > bitmap.Height)
				maxsize = bitmap.Height;
			size = maxsize;
			maxSize = maxsize;
			minSize = minsize;
			this.nextSize = nextsize;
			width = maxsize;
			height = maxsize;
			dx = width / ScanSize/2;
			dx = dx > 0 ? dx : 1;
			dy = height / ScanSize/2;
			dy = dy > 0 ? dy : 1;
		}
		public FaceScaner(int Size)
		{
			setScanSize(Size);
		}

		//设置训练尺寸
		public void setScanSize(int size)
		{
			ScanSize = size;
			ScanSize2 = size * size;
			resetSize(this.size);
		}

		//设置搜索框对应原图的尺寸
		public void resetSize
			(int s)
		{
			size = s;
			width = s;
			height = s;
			dx = width / ScanSize ;
			dx = dx > 0 ? dx : 1;
			dy = height / ScanSize ;
			dy = dy > 0 ? dy : 1;
		}
		 //随机数产生器
		Random r = new Random();

		//计算Haar标准化系数
		double getFactor(IntegralMap I,IntegralMap II)
		{
			double sN1 = 1.0 / (ScanSize2);
			double mean = I[1, 1, ScanSize - 1, ScanSize - 1] * sN1;
			double sqmean = II[1, 1, ScanSize - 1, ScanSize - 1] * sN1;
			return 1.0 / Math.Sqrt(sqmean - mean * mean);
		}

		/// <summary>
		/// 设置分类器和计算序列
		/// </summary>
		/// <param name="ais"></param>
		/// <param name="lists"></param>
		public void set(AdaBoost [] ais,HaarWorkingList[] lists)
		{
			this.ais = ais;
			haars = lists;
		}

		//在训练过程中搜集错误样本，原理和getFaces函数一样只是不会输出判断结果而是搜集错误数据
		void getFaces_train(bitmap bitmap,IFaceTeacher teacher, int maxsize, int minsize,List<bitmap> bs, double nextsize = 0.9, int maxSampleN = 250)
		{
			setSizeRange(bitmap, maxsize, minsize, nextsize);
			int MaxX, MaxY;
			bitmap b, gray;
			AdaBoost a;
			HaarWorkingList ha;
			bool flag;
			int c = ais.Length;
			gray = bitmap.toGray();
			SearchBox originbox; 
			SearchBox box;
			resetSize(size);
			while (size >= minSize)
			{
				
				bitmap scaleBitmap = gray.scale(bitmap.Width * ScanSize / width, bitmap.Height * ScanSize / height);
				double fWidth = bitmap.Width / (double)scaleBitmap.Width;
				double fHeight = bitmap.Height / (double)scaleBitmap.Height;
				IntegralMap[] maps = IntegralMap.GetGSQ(scaleBitmap);
				IntegralMap I = maps[0];
				IntegralMap II = maps[1];

				I.virtualHeight = ScanSize;
				I.virtualWidth = ScanSize;
				II.virtualHeight = ScanSize;
				II.virtualWidth = ScanSize;
				int x, y;
				originbox = new SearchBox(gray, (int)(width),(int)( height));
				double offsetx = 0.2 * width;
				double offsety = 0.2 * height;
				box = new SearchBox(scaleBitmap, ScanSize, ScanSize);
				MaxX = scaleBitmap.Width - ScanSize;
				MaxY = scaleBitmap.Height - ScanSize;
				double factor;
				for (int i = 0; i < MaxX; i++)
				{
					for (int j = 0; j < MaxY; j++)
					{
						II.setoffset(i, j);
						I.setoffset(i, j);
						factor = getFactor(I, II);
						flag = true;
						for (int n = 0; n < c; n++)
						{
							a = ais[n];
							ha = haars[n];
							if (a.Classify(ha.getADAData(I, II, 0, factor)) != 1)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							/*
							b = originbox[(int)(i * fWidth), (int)(j * fHeight)];
							if(!teacher.isSingleFace(b))
							{
                                 bs.Add(b.clone());
							   if(bs.Count>=maxcount)
								{
									return ;
								}
							}
							else
							{
								faces.Add(b.clone());
							}
							*/
							
							x = (int)(i * fWidth);
							y = (int)(j * fHeight );
						
                                 b = originbox[x, y];
							
							
						
								if(!teacher.isWithFace(b))
								{
                                 if(bs.Count < maxSampleN)
								{
                                 bs.Add(box[i,j].Clone());
									if(bs.Count>=maxSampleN)
									{
										return;
									}
								}
								}
								
								
						
						}


					}
				}

				resetSize((int)(size * nextSize));
			}

		}

		//在训练过程中搜集错误样本，bitmaps 中的图片都是和训练尺寸相同的负样本，原理和getFaces类似
		public void recongnize(ICollection<bitmap> bitmaps,List<bitmap> errors,int maxCount)
		{
			IntegralMap[] im;
			AdaBoost a;
			HaarWorkingList ha;
			IntegralMap I,II;
			int c = ais.Length;
			double factor;
			bool flag;
			
			foreach (bitmap bitmap in bitmaps)
			{
				flag = true;
				im = IntegralMap.GetBSQ(bitmap);
				I = im[0];
				II = im[1];
				for (int n = 0; n < c; n++)
				{
					a = ais[n];
					ha = haars[n];
					factor = getFactor(I, II);
					if (a.Classify(ha.getADAData(I, II, 0, factor)) == -1)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					
				  errors.Add(bitmap.Clone());
					if (errors.Count >= maxCount)
					{
						return;
					}
				}
			}
		}

		/// <summary>
		/// 检测人脸
		/// </summary>
		/// <param name="bitmap">待测图像</param>
		/// <param name="maxsize">对应原图的最大搜索窗口尺寸</param>
		/// <param name="minsize">最小窗口尺寸</param>
		/// <param name="maxCount">最大检测数量</param>
		/// <param name="nextsize">窗口迭代缩小的比例</param>
		/// <returns></returns>
		public Rectanglei[] getFaces(bitmap bitmap, int maxsize, int minsize, int maxCount=1000,double nextsize = 0.9)
		{
			setSizeRange(bitmap,maxsize, minsize, nextsize);// 设置窗口伸缩范围
			List<Rectanglei> bs = new List<Rectanglei>();
			int MaxX, MaxY;
			bitmap b,gray;
			AdaBoost a;
			HaarWorkingList ha;
			bool flag;
			int c = ais.Length;
			gray = bitmap.toGray();//转化为灰度图
			SearchBox originbox;//原图上的裁剪窗口
			resetSize(size);//设置当前窗口尺寸
			while (size>=minSize)//当尺寸小于最小尺寸时退出循环
			{
				GC.Collect();//清理内存
				bitmap scaleBitmap = gray.scale(bitmap.Width * ScanSize / width, bitmap.Height * ScanSize / height);
				//根据搜素窗口大小缩放原图
				double fWidth = bitmap.Width /(double) scaleBitmap.Width;
				double fHeight = bitmap.Height / (double)scaleBitmap.Height;//缩小的比例
				IntegralMap[] maps = IntegralMap.GetGSQ(scaleBitmap);//计算缩小后图像的积分图
				IntegralMap I = maps[0];
				IntegralMap II = maps[1];

				I.virtualHeight = ScanSize;
				I.virtualWidth = ScanSize;
				II.virtualHeight = ScanSize;
				II.virtualWidth = ScanSize;

				originbox = new SearchBox(bitmap, width, height);//设置在原图上的裁剪器，因为最开始输出的不是矩形框而是图片，现在没有用到
		
				MaxX = scaleBitmap.Width- ScanSize;
				MaxY = scaleBitmap.Height - ScanSize;//计算窗口滑动的范围
				double factor;
				for(int i=0;i<MaxX;i++)
				{
					for (int j = 0; j < MaxY; j ++)//遍历整幅图
					{
						II.setoffset(i, j);
						I.setoffset(i, j);
						factor = getFactor(I, II);
						flag= true;
						//设置积分图的位置偏移量，计算标准化因子
						for(int n=0;n<c;n++)//遍历所有的强分类器
						{
							a = ais[n];
							ha = haars[n];
                       if(a.Classify(ha.getADAData(I,II,0,factor))!=1)//计算出相应的Haar特征值并进行分类，负样本则退出循环
						{
								flag = false;
								break;
						}
						}
						if(flag)//所有的分类器都认为正确
						{
							bs.Add(new Rectanglei((int)(i*fWidth),(int)(j*fHeight),width,height));//计算当前窗口对应原图的矩形位置尺寸，并记录
							if (bs.Count >= maxCount)//矩形框达到最大值输出
							{
								return Rectanglei.Merge(bs);
							}
						}

						
					}
				}
				
				resetSize((int)(size * nextSize));//继续缩小搜索窗口尺寸，进入下一次迭代
			}
			return Rectanglei.Merge(bs);//最终输出合并后的矩形框
			
		}

		public XmlElement writeXml(XmlElement element)//写入xml文件（内存）
		{
			element.writeAdaHaarSets(ais,haars);
			element.SetAttribute("Size", ScanSize.ToString());
			return element;
		}

		public FaceScaner readXml(XmlElement element)//读取xml文件（内存）
		{
			ScanSize = element.GetAttributeInt("Size");
			setScanSize(ScanSize);
		var items=	element.readAdaHaarSets();
			
			ais = items.Item1;
			haars = items.Item2;
			return this;
		}
		public bool load(string path)//读取xml文件（硬盘）
		{
			
				XmlDocument x = new XmlDocument()
					;
				x.Load(path);
				
					readXml(x.DocumentElement);
			
		
			return true;
		}
		public bool saveData(string path)//写入xml文件（硬盘）
		{
			XmlDocument xml = new XmlDocument();
			
			XmlElement e = xml.CreateElement("AdaHaar");xml.AppendChild(e);
			writeXml(e);
			try
			{
				xml.Save(path);
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
	/// <summary>
	/// 积分图
	/// </summary>
	public class IntegralMap
	{
		
		public int[,] Value{ get; private set; }//值的二维数组
		public int width { get; private set; }//真实尺寸 用于窗口移动
		public int height { get; private set; }

		//虚拟尺寸 用于窗口移动
		public int virtualWidth;
		public int virtualHeight;
		public int this[Vector2i p1, Vector2i p2]//获取区域的灰度和
		{
			get
			{
				int maxx = Math.Max(p1.X, p2.X);
				int maxy = Math.Max(p1.Y, p2.Y);
				int minx = Math.Min(p1.X, p2.X);
				int miny = Math.Min(p1.Y, p2.Y);
			return this[maxx, maxy] + this[minx - 1, miny - 1] - this[maxx, miny - 1] - this[minx - 1, maxy];
			}
		}
		public int this[int minx, int miny,int maxx,int maxy]//获取区域的灰度和
		{
			get
			{			
				return this[maxx, maxy] + this[minx - 1, miny - 1] - this[maxx, miny - 1] - this[minx - 1, maxy];
			}
		}
		//相对的坐标偏移 用于窗口移动
		int offsetx = 0;
		int offsety = 0;
		public int this[int x, int y]//获取区域的灰度和（起点为原点）
		{
			

			get
			{   x +=offsetx;
				y += offsety;
				if(x<0||y<0)
				{
					return 0;
				}
				 return Value[x, y];
				
			}
		}
	 IntegralMap(int width,int height)// 创建空实例
		{

			this.width = width;
			this.height = height;
			Value = new int[width, height];
			virtualHeight = height;
			virtualWidth = width;
		}
		/// <summary>
		/// 根据位图创建积分图
		/// </summary>
		/// <param name="bitmap">位图</param>
		/// <param name="channel">颜色通道</param>
		/// <param name="sq">是否为灰度的平方</param>
		public IntegralMap(bitmap bitmap,Channel channel,bool sq=false)
		{
		
			width = bitmap.Width;
			height = bitmap.Height;
			Value = new int[width, height];
			if(!sq)
			{
switch (channel)
			{
				case Channel.Red: updateR(bitmap);break;
				case Channel.Green: updateG(bitmap); break;
				case Channel.Blue: updateB(bitmap); break;
				default: updateGray(bitmap);break;
			}
			}
			else
			{
				switch (channel)
				{
					case Channel.Red: updateRSQ(bitmap); break;
					case Channel.Green: updateGSQ(bitmap); break;
					case Channel.Blue: updateBSQ(bitmap); break;
					default: updateGraySQ(bitmap); break;
				}
			}
			
		}

		/// <summary>
		/// 计算蓝色通道的积分图和平方积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetBSQ(bitmap bitmap)
		{
			IntegralMap R = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap RSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			int[,] Rmap = R.Value;
			int[,] RmapSQ = RSQ.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v ) & 0xff;
					if (i == 0)
					{
						if (j == 0)
						{
							Rmap[i, j] = r;
							RmapSQ[i, j] = r * r;
						}
						else
						{
							Rmap[i, j] = r + Rmap[i, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Rmap[i, j] = r + Rmap[i - 1, j];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j];
						}
						else
						{
							Rmap[i, j] = r + Rmap[i - 1, j] + Rmap[i , j- 1] - Rmap[i - 1, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j] + RmapSQ[i , j- 1] - RmapSQ[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { R, RSQ };
		}
		/// <summary>
		/// 计算绿色色通道的积分图和平方积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetGSQ(bitmap bitmap)
		{
			IntegralMap R = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap RSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			int[,] Rmap = R.Value;
			int[,] RmapSQ = RSQ.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 8) & 0xff;
					if (i == 0)
					{
						if (j == 0)
						{
							Rmap[i, j] = r;
							RmapSQ[i, j] = r * r;
						}
						else
						{
							Rmap[i, j] = r + Rmap[i, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Rmap[i, j] = r + Rmap[i - 1, j];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j];
						}
						else
						{
							Rmap[i, j] = r + Rmap[i - 1, j] + Rmap[i, j - 1] - Rmap[i - 1, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j] + RmapSQ[i, j - 1] - RmapSQ[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { R, RSQ };
		}
		/// <summary>
		/// 计算红色通道的积分图和平方积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetRSQ(bitmap bitmap)
		{
			IntegralMap R = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap RSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			int[,] Rmap = R.Value;
			int[,] RmapSQ = RSQ.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					if (i == 0)
					{
						if (j == 0)
						{
							Rmap[i, j] = r;
							RmapSQ[i, j] = r * r;
						}
						else
						{
							Rmap[i, j] = r + Rmap[i, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Rmap[i, j] = r + Rmap[i - 1, j];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j];
						}
						else
						{
							Rmap[i, j] = r + Rmap[i - 1, j] + Rmap[i , j- 1] - Rmap[i - 1, j - 1];
							RmapSQ[i, j] = r * r + RmapSQ[i - 1, j] + RmapSQ[i, j - 1] - RmapSQ[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { R,RSQ };
		}
		/// <summary>
		/// 计算灰度通道的积分图和平方积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetGraySQ(bitmap bitmap)
		{
	
			IntegralMap Gray = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap GraySQ = new IntegralMap(bitmap.Width, bitmap.Height);
		
			int[,] Graymap = Gray.Value;
			int[,] GraymapSQ = GraySQ.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v) & 0xff;
					gray = r * 299 + g * 587 + b * 144;
					gray /= 1000;
					if (i == 0)
					{
						if (j == 0)
						{
					
							Graymap[i, j] = gray;
					
							GraymapSQ[i, j] = gray * gray;
						}
						else
						{
				
							Graymap[i, j] = gray + Graymap[i, j - 1];
							GraymapSQ[i, j] = gray * gray + GraymapSQ[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
					
							Graymap[i, j] = gray + Graymap[i-1, j];
							GraymapSQ[i, j] = gray * gray + GraymapSQ[i-1, j];
						}
						else
						{
						
							Graymap[i, j] = gray + Graymap[i- 1, j ] + Graymap[i , j- 1] - Graymap[i - 1, j - 1];
							GraymapSQ[i, j] = gray * gray + GraymapSQ[i-1, j] + GraymapSQ[i , j- 1] - GraymapSQ[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { Gray,  GraySQ };
		}

		/// <summary>
		/// 计算所有通道的积分图和平方积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetRGBGraySQ(bitmap bitmap)
		{
			IntegralMap R = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap G = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap B = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap Gray = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap RSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap GSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap BSQ = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap GraySQ = new IntegralMap(bitmap.Width, bitmap.Height);
			int[,] Rmap = R.Value;
			int[,] Gmap = G.Value;
			int[,] Bmap = B.Value;
			int[,] Graymap = Gray.Value;
			int[,] RmapSQ = RSQ.Value;
			int[,] GmapSQ = GSQ.Value;
			int[,] BmapSQ = BSQ.Value;
			int[,] GraymapSQ = GraySQ.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v) & 0xff;
					gray = r * 299 + g * 587 + b * 144;
					gray /= 1000;
					if (i == 0)
					{
						if (j == 0)
						{
							Rmap[i, j] = r;
							Gmap[i, j] = g;
							Bmap[i, j] = b;
							Graymap[i, j] = gray;
							RmapSQ[i, j] =r*r ;
							GmapSQ[i, j] =g*g;
							BmapSQ[i, j] = b*b;
							GraymapSQ[i, j] =gray*gray ;
						}
						else
						{
							Rmap[i, j] = r + Rmap[i, j - 1];
							Gmap[i, j] = g + Gmap[i, j - 1];
							Bmap[i, j] = b + Bmap[i, j - 1];
							Graymap[i, j] = gray + Graymap[i, j - 1];
							RmapSQ[i, j] = r*r + RmapSQ[i, j - 1];
							GmapSQ[i, j] = g*g + GmapSQ[i, j - 1];
							BmapSQ[i, j] = b*b + BmapSQ[i, j - 1];
							GraymapSQ[i, j] = gray*gray + GraymapSQ[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Rmap[i, j] = r + Rmap[i - 1, j];
							Gmap[i, j] = g + Gmap[i - 1, j];
							Bmap[i, j] = b + Bmap[i - 1, j];
							Graymap[i, j] = gray + Graymap[i-1, j];
							RmapSQ[i, j] = r*r + RmapSQ[i - 1, j];
							GmapSQ[i, j] = g*g + GmapSQ[i - 1, j];
							BmapSQ[i, j] = b*b + BmapSQ[i - 1, j];
							GraymapSQ[i, j] = gray*gray + GraymapSQ[i-1, j];
						}
						else
						{
							Rmap[i, j] = r + Rmap[i - 1, j] + Rmap[i , j- 1] - Rmap[i - 1, j - 1];
							Gmap[i, j] = g + Gmap[i - 1, j] + Gmap[i , j- 1] - Gmap[i - 1, j - 1];
							Bmap[i, j] = b + Bmap[i - 1, j] + Bmap[i, j - 1] - Bmap[i - 1, j - 1];
							Graymap[i, j] = gray + Graymap[i, j-1] + Graymap[i - 1, j] - Graymap[i - 1, j - 1];
							RmapSQ[i, j] = r*r + RmapSQ[i - 1, j] + RmapSQ[i , j- 1] - RmapSQ[i - 1, j - 1];
							GmapSQ[i, j] = g*g + GmapSQ[i - 1, j] + GmapSQ[i , j- 1] - GmapSQ[i - 1, j - 1];
							BmapSQ[i, j] = b*b + BmapSQ[i - 1, j] + BmapSQ[i , j- 1] - BmapSQ[i - 1, j - 1];
							GraymapSQ[i, j] = gray*gray + GraymapSQ[i, j-1] + GraymapSQ[i - 1, j] - GraymapSQ[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { R, G, B, Gray,RSQ,GSQ,BSQ,GraySQ };
		}

		/// <summary>
		/// 计算所有通道的积分图
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static IntegralMap[] GetRGBGray(bitmap bitmap)
		{
			IntegralMap R = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap G = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap B = new IntegralMap(bitmap.Width, bitmap.Height);
			IntegralMap Gray = new IntegralMap(bitmap.Width, bitmap.Height);
			int[,] Rmap = R.Value;
			int[,] Gmap = G.Value; 
			int[,] Bmap = B.Value; 
			int[,] Graymap = Gray.Value;
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			int width = bitmap.Width;
			int height = bitmap.Height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v) & 0xff;
					gray = r * 299 + g * 587 + b * 144;
					gray /= 1000;
					if (i == 0)
					{
						if (j == 0)
						{
							Rmap[i, j] = r;
							Gmap[i, j] = g;
							Bmap[i, j] = b;
							Graymap[i, j] = gray;
						}
						else
						{
							Rmap[i, j] = r + Rmap[i, j - 1];
							Gmap[i, j] = g + Gmap[i, j - 1];
							Bmap[i, j] = b + Bmap[i, j - 1];
							Graymap[i, j] = gray + Graymap[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Rmap[i, j] = r + Rmap[i - 1, j];
							Gmap[i, j] = g + Gmap[i - 1, j];
							Bmap[i, j] = b + Bmap[i - 1, j];
							Graymap[i, j] = gray + Graymap[i-1, j];
						}
						else
						{
							Rmap[i, j] = r + Rmap[i - 1, j] + Rmap[i, j - 1] - Rmap[i - 1, j - 1];
							Gmap[i, j] = g + Gmap[i - 1, j] + Gmap[i , j- 1] - Gmap[i - 1, j - 1];
							Bmap[i, j] = b + Bmap[i - 1, j] + Bmap[i , j- 1] - Bmap[i - 1, j - 1];
							Graymap[i, j] = gray + Graymap[i, j-1] + Graymap[i - 1, j] - Graymap[i - 1, j - 1];
						}
					}
				}
			}

			return new IntegralMap[] { R, G,B, Gray };
		}

		//将该实例更新为蓝色通道积分图
			public void updateBSQ(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v,  b;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					b = (v) & 0xff;
					b *= b;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = b;
						}
						else
						{
							Value[i, j] = b + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = b + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = b + Value[i - 1, j] + Value[i , j- 1] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}
		//将该实例更新为B通道积分图
		public void updateB(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v,  b;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					b = (v) & 0xff;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = b;
						}
						else
						{
							Value[i, j] = b + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = b + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = b + Value[i - 1, j] + Value[i, j - 1] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}//将该实例更新为Gray通道平方积分图
		public void updateGraySQ(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v) & 0xff;
					gray = r * 299 + g * 587 + b * 144;
					gray /= 1000;
					gray *= gray;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = gray;
						}
						else
						{
							Value[i, j] = gray + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = gray + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = gray + Value[i - 1, j] + Value[i , j- 1] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}//将该实例更新为灰度通道积分图
		public void updateGray(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v, r, g, b, gray;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					g = (v >> 8) & 0xff;
					b = (v) & 0xff;
					gray = r * 299 + g * 587 + b * 144;
					gray /= 1000;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = gray;
						}
						else
						{
							Value[i, j] = gray + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = gray + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = gray + Value[i , j- 1] + Value[i - 1, j] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}
		//将该实例更新为绿色通道平方积分图
		public void updateGSQ(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v, g;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					g = (v >> 8) & 0xff;
					g *= g;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = g;
						}
						else
						{
							Value[i, j] = g + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = g + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = g + Value[i , j- 1] + Value[i - 1, j] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}
		//将该实例更新为绿色通道积分图
		public void updateG(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v,  g;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					g = (v >> 8) & 0xff;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = g;
						}
						else
						{
							Value[i, j] = g + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = g + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = g + Value[i , j- 1] + Value[i - 1, j] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}
		//将该实例更新为红色通道平方积分图
		public void updateRSQ(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v, r;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					v = data[i, j];
					r = (v >> 16) & 0xff;
					r *= r;
					if (i == 0)
					{
						if (j == 0)
						{
							Value[i, j] = r;
						}
						else
						{
							Value[i, j] = r + Value[i, j - 1];
						}
					}
					else
					{
						if (j == 0)
						{
							Value[i, j] = r + Value[i - 1, j];
						}
						else
						{
							Value[i, j] = r + Value[i , j- 1] + Value[i - 1, j] - Value[i - 1, j - 1];
						}
					}
				}
			}
		}
		//设置坐标偏移
		public void setoffset(int x,int y)
		{
			offsetx = x;
			offsety = y;
		}
		//将该实例更新为红色通道积分图
		public void updateR(bitmap bitmap)
		{
			Value = new int[width, height];
			int[,] data = bitmap.getData();
			int v, r;
			for(int i=0;i<width;i++)
			{
				for(int j=0;j<height;j++)
				{
					 v = data[i, j];
					r = (v >> 16) & 0xff;
					if (i==0)
					{
						if(j==0)
						{
							Value[i, j] = r;
						}
						else
						{
							Value[i, j] = r+ Value[i, j-1];
						}
					}
					else
					{
						if(j==0)
						{
							Value[i, j] = r + Value[i-1, j];
						}
						else
						{
							Value[i, j] = r + Value[i , j- 1] + Value[i - 1, j]- Value[i - 1, j-1];
						}
					}
				}
			}
		}
	}
}
