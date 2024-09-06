using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using DVOSLib;
using AdaData = MachineLearning.DataVector<double, int>;
using DVOSLib;
using Images;
using MachineLearning;

namespace DVOSLib
{
    public static class DataHelper// 关于文件读写功能的静态类不是主要内容

    {
        public static(int r,int g,int b)Int2RGB(this int color)
		{
            return ((color >> 16) & 0xff, (color >> 8) & 0xff, (color) & 0xff);

        }
        public static (int a,int r, int g, int b) Int2ARGB(this int color)
        {
            return ((color >> 24) & 0xff, (color >> 16) & 0xff, (color >> 8) & 0xff, (color) & 0xff);

        }

        public static int RGB2Int(this (int r, int g, int b) color)
        {
            return (( 255) << 24) | ((color.r&255)<<16)| ((color.g & 255) << 8)| ((color.b & 255) ) ;

        }

        public static int RGB2Int(this (byte r, byte g, byte b) color)
        {
            return ((255) << 24) | ((color.r & 255) << 16) | ((color.g & 255) << 8) | ((color.b & 255));

        }
        public static int ARGB2Int(this (byte a,byte r, byte g, byte b) color)
        {
            return ((color.a & 255) << 24)|((color.r & 255) << 16) | ((color.g & 255) << 8) | ((color.b & 255));

        }
        public static int ARGB2Int(this (int a, int r, int g, int b) color)
        {
            return ((color.a & 255) << 24) | ((color.r & 255) << 16) | ((color.g & 255) << 8) | ((color.b & 255));

        }
        public static string getExtensionName(this string name)// 获取扩展名
		{
            string[] list = name.Split('.');
            if(list.Length>0)
			{
                return list.Last().ToLower();
			}
            return "";
		}
        public static bitmap[] getBitmaps(this string path)//获取路径下所有图像
		{
            List<bitmap> b = new List<bitmap>();
            string e;
            if(Directory.Exists(path))
			{
                foreach(string name in Directory.GetFiles(path))
				{
                    e = name.getExtensionName();
					if (e.Equals("jpg") || e.Equals("png")||e.Equals("bmp")||e.Equals("tif"))
					{
                        b.Add(new bitmap(name));
					}
				}
			}
            return b.ToArray();
		}
        public static int GetAttributeInt(this XmlElement xmlElement, string name)//获取xml文件的int值
        {
            return int.Parse(xmlElement.GetAttribute(name));
        }
        public static HaarInfo[] loadHaarInfos(string file)
        {
            HaarInfo[] listl = new HaarInfo[0];
            XmlDocument xml = new XmlDocument();
            xml.Load(file); int i;
            if (xml.DocumentElement.Name.Equals("HaarInfos"))
            {
                int count = int.Parse(xml.DocumentElement.GetAttribute("Count"));
                listl = new HaarInfo[count];
                foreach (XmlElement element in xml.DocumentElement)
                {
                    if (element.Name.Equals("HaarInfo"))
                    {
                        i = int.Parse(element.GetAttribute("Index"));
                        listl[i] = new HaarInfo().readXml(element);
                    }
                }
            }
            return listl;
        }
        public static int[][] split(this int[] vs, int count)
        {

            int total = vs.Length;
            if (total < count)
                count = total;
            if (count == 0)
            {
                return new int[0][];
            }

            int[][] r = new int[count][];
            int part = total / count;
            int lastPart = total - part * count;
            lastPart = lastPart == 0 ? part : lastPart+part;
            int pos = 0;
            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                {
                    r[i] = new int[lastPart];
                    Array.Copy(vs, pos, r[i], 0, lastPart);
                }
                else
                {
                    r[i] = new int[part];
                    Array.Copy(vs, pos, r[i], 0, part);
                    pos += part;
                }
            }
            return r;
        }
        public static HaarWorkingList getHaarWorkingList(this ICollection<HaarInfo> infos)
        {
            return new HaarWorkingList(infos);
        }
        public static AdaData selectRange(this AdaData data, int minInclude, int maxExclude)
        {
            AdaData d = new AdaData(0);
            d.Data = data.Data.selectRange(minInclude, maxExclude);
            d.Label = data.Label;
            return d;
        }
        public static List<HaarInfo> resetDimension(this AdaBoost ai, IReadOnlyCollection<HaarInfo> list)
        {
            List<HaarInfo> infol = getInfoFromTrainedADA(ai, list);
            resetDimension(ai);
            return infol;

        }
        public static List<HaarInfo> resetDimension(this AdaBoost ai, ICollection<HaarInfo> list)
        {
            List<HaarInfo> infol = getInfoFromTrainedADA(ai, list);
            resetDimension(ai);
            return infol;

        }
        public static XmlElement writeAdaHaarSets(this XmlElement element , ICollection<AdaBoost> ais,ICollection<HaarWorkingList> lists)
		{
            int count = ais.Count;
            element.SetAttribute("Count", count.ToString());
            XmlElement set;
            XmlElement temp;
            AdaBoost ai;
            HaarWorkingList ls;
            for(int i=0;i<count;i++)
			{
                set = element.OwnerDocument.CreateElement("AdaHaarSet");
                set.SetAttribute("Pos", i.ToString());
                element.AppendChild(set);
                temp = element.OwnerDocument.CreateElement("AdaBoost");
                ais.ElementAt(i).writeXml(temp);
                set.AppendChild(temp);
                temp= element.OwnerDocument.CreateElement("Haar");
                lists.ElementAt(i).writeXml(temp);
                set.AppendChild(temp);

            }
            return element;

		}
        public static (AdaBoost[],HaarWorkingList[]) readAdaHaarSets(this XmlElement element)
        {
            int count = element.GetAttributeInt("Count");

            AdaBoost[] ais = new AdaBoost[count];
            HaarWorkingList[] lists = new HaarWorkingList[count];
            XmlElement set;
            XmlElement temp;
            AdaBoost ai;
            HaarWorkingList ls;int i;
           foreach(XmlElement e in element)
			{
                i = e.GetAttributeInt("Pos");
                foreach(XmlElement e1 in e)
				{
                    if(e1.Name.Equals("AdaBoost"))
					{
                        ai = new AdaBoost().readXml(e1);
                        ais[i] = ai;
					}
                    else if(e1.Name.Equals("Haar"))
					{
                        ls = new HaarWorkingList().readXml(e1);
                        lists[i] = ls;
					}
				}
			}
            return (ais,lists);

        }
        public static HaarWorkingList resetDimension(this AdaBoost ai, HaarWorkingList list)
        {
            HaarWorkingList infol = getInfoFromTrainedADA(ai, list);
            resetDimension(ai);
            return infol;

        }
        public static void resetDimension(this AdaBoost ai)
        {
            int n = 0;
            foreach (DecisionStump s in ai.m_WeakClassifiers)
            {
                s.Dimension = n;
                n++;
            }
        }

        public static HaarWorkingList getInfoFromTrainedADA(this AdaBoost ai, HaarWorkingList list)
        {

            return list.getInfoFromFullTrainedADA(ai);
        }
        public static List<HaarInfo> getInfoFromTrainedADA(this AdaBoost ai, IReadOnlyCollection<HaarInfo> list)
        {
            List<HaarInfo> l = new List<HaarInfo>();
            foreach (DecisionStump s in ai.m_WeakClassifiers)
            {
                l.Add(list.ElementAt(s.Dimension));
            }
            return l;
        }
        public static List<HaarInfo> getInfoFromTrainedADA(this AdaBoost ai,ICollection<HaarInfo> list)
        {
            List<HaarInfo> l = new List<HaarInfo>();
            foreach (DecisionStump s in ai.m_WeakClassifiers)
            {
                l.Add(list.ElementAt(s.Dimension));
            }
            return l;
        }
        public static int getSourceIndex(this int index, int minInclude)
        {
            return index + minInclude;
        }
        public static int getSourceIndex(this int index, int[] range)
        {
            return range[index];
        }
        public static int getFinalIndex(this int index, int minInclude)
        {
            return index - minInclude;
        }
        public static sbyte toSbyte(this byte b)
        {
            if (b < 128)
            {
                return (sbyte)b;
            }
            else
            {
                return (sbyte)(b - 256);
            }
        }
        public static int[] getRange(this int i)
        {
            int[] ii = new int[i];
            for (int j = 0; j < i; j++)
            {
                ii[j] = j;
            }
            return ii;
        }
        public static int getFinalIndex(this int index, int[] range)
        {
            int n = 0;
            foreach (int i in range)
            {
                if (i == index)
                {
                    return n;
                }
                n++;
            }
            return -1;
        }
        public static AdaData selectRange(this AdaData data, ICollection<int> range)
        {
            AdaData d = new AdaData(0);
            d.Data = data.Data.selectRange(range);
            d.Label = data.Label;
            return d;
        }
        public static HaarWorkingList selectRange(HaarWorkingList data, int minInclude, int maxExclude)
        {


            return data.selectRange(minInclude, maxExclude);
        }
        public static HaarWorkingList selectRange(HaarWorkingList data, ICollection<int> range)
        {


            return data.selectRange(range);
        }
        public static HaarInfo[] selectRange(this ICollection<HaarInfo> data, int minInclude, int maxExclude)
        {
            HaarInfo[] fdata = new HaarInfo[maxExclude - minInclude];
            Array.Copy(data.ToArray(), minInclude, fdata, 0, maxExclude - minInclude);
            return fdata;
        }
        public static double[] selectRange(this double[] data, int minInclude, int maxExclude)
        {
            double[] fdata = new double[maxExclude - minInclude];
            Array.Copy(data, minInclude, fdata, 0, maxExclude - minInclude);
            return fdata;
        }
        public static double[] selectRange(this double[] data, ICollection<int> range)
        {
            double[] fdata = new double[range.Count];
            int n = 0;
            foreach (int i in range)
            {
                fdata[n] = data[range.ElementAt(i)];
            }
            return fdata;
        }
        public static HaarInfo[] selectRange(this ICollection<HaarInfo> data, ICollection<int> range)
        {
            HaarInfo[] fdata = new HaarInfo[range.Count];
            int n = 0;
            foreach (int i in range)
            {
                fdata[n] = data.ElementAt(range.ElementAt(i));
            }
            return fdata;
        }
        public static void saveHaarInfos(this ICollection<HaarInfo> infos, string path)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement e = xml.CreateElement("HaarInfos");
            e.SetAttribute("Count", infos.Count.ToString());
            xml.AppendChild(e);
            XmlElement x;
            int i = 0;
            foreach (HaarInfo info in infos)
            {
                x = xml.CreateElement("HaarInfo");
                info.writeXml(x);
                x.SetAttribute("Index", i.ToString())
                    ;
                i++;
                e.AppendChild(x);
            }
            xml.Save(path);
        }
        public static adaSumMap[] loadADAMapFile(string path)
        {

            if (File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                byte[] temp = new byte[4];
                stream.Read(temp, 0, 4);
                int count = BitConverter.ToInt32(temp, 0);
                adaSumMap[] maps = new adaSumMap[count];
                for (int i = 0; i < count; i++)
                {
                    maps[i] = new adaSumMap().read(stream);
                }
                stream.Close();
                return maps;
            }

            return new adaSumMap[0];
        }
        public static void savaADAMapFile(this ICollection<adaSumMap> list, string path)
        {
            int n = list.Count;
            if (n > 0)
            {

                FileStream stream = new FileStream(path, FileMode.Create);
                stream.Write(BitConverter.GetBytes(n), 0, 4);
                foreach (adaSumMap data in list)
                {
                    data.write(stream);
                    stream.Flush();
                }
                stream.Flush();
                stream.Close();
                stream.Dispose();
                GC.Collect();
            }

        }
        public static void saveADADataFile(this ICollection<DataVector<double, int>> list, string path)
        {
            int n = list.Count;
            if (n > 0)
            {
                int dim = list.First().Dimension;
                FileStream stream = new FileStream(path, FileMode.Create)
                    ;


                stream.Write(BitConverter.GetBytes(dim), 0, 4);
                stream.Write(BitConverter.GetBytes(n), 0, 4);
                foreach (DataVector<double, int> data in list)
                {
                    stream.Write(BitConverter.GetBytes(data.Label), 0, 4);
                    foreach (double d in data.Data)
                    {
                        stream.Write(BitConverter.GetBytes(d), 0, 8);
                    }
                    stream.Flush();
                }
                stream.Flush();
                stream.Close();
                stream.Dispose();
                GC.Collect();
            }


        }

        public static List<DataVector<double, int>> loadDADDataFile(string s)
        {
            byte[] temp = new byte[8]; int dim, n, l;
            double data;
            double[] dtemp;
            DataVector<double, int> dAData;
            List<DataVector<double, int>> datas = new List<DataVector<double, int>>();
            if (File.Exists(s))
            {

                FileStream f = new FileStream(s, FileMode.Open);
                f.Read(temp, 0, 4);
                dim = BitConverter.ToInt32(temp, 0);
                f.Read(temp, 0, 4);
                n = BitConverter.ToInt32(temp, 0);
                datas = new List<DataVector<double, int>>(n);
                for (int i = 0; i < n; i++)
                {
                    f.Read(temp, 0, 4);
                    l = BitConverter.ToInt32(temp, 0);

                    dtemp = new double[dim];
                    for (int j = 0; j < dim; j++)
                    {
                        f.Read(temp, 0, 8);
                        dtemp[j] = BitConverter.ToDouble(temp, 0);

                    }
                    dAData = new DataVector<double, int>(dim);
                    dAData.Data = dtemp;
                    dAData.Label = l;
                    datas.Add(dAData);
                }
                f.Close();
                f.Dispose();
                GC.Collect();
            }

            return datas;
        }
    }

}
namespace MachineLearning
{

    public class DecisionStump : IxmlObject<DecisionStump>// 一个简单决策树(弱分类器)
    {
        /// <summary>
        /// 分类器权重
        /// </summary>
        public double Alpha { get; set; }
        /// <summary>
        /// 加权错误率
        /// </summary>
        public double Error { get; set; }
        /// <summary>
        /// 在哪个维度上切分
        /// </summary>
        public int Dimension { get; set; }
        /// <summary>
        /// 切分边界
        /// </summary>
        public double Boundary { get; set; }
        /// <summary>
        /// 是否按大于来切分
        /// </summary>
        public bool GreaterThan { get; set; }
        /// <summary>
        /// 此分类器在训练数据集上的分类结果
        /// </summary>
        public int[] Results { get; set; }

        public XmlElement writeXml(XmlElement element)
        {
            element.SetAttribute("Alpha", Alpha.ToString());
            element.SetAttribute("Error", Error.ToString());
            element.SetAttribute("Dimension", Dimension.ToString());
            element.SetAttribute("Boundary", Boundary.ToString());
            element.SetAttribute("GreaterThan", GreaterThan.ToString());
            return element;
        }

        public DecisionStump readXml(XmlElement element)
        {
            DecisionStump stump = this;
            stump.Alpha = double.Parse(element.GetAttribute("Alpha"));
            stump.Error = double.Parse(element.GetAttribute("Error"));
            stump.Dimension = int.Parse(element.GetAttribute("Dimension"));
            stump.Boundary = double.Parse(element.GetAttribute("Boundary"));
            stump.GreaterThan = bool.Parse(element.GetAttribute("GreaterThan"));
            return this;
        }
    }
 
    public class DataVector<TData, TLabel>
    {
        public int temp;
        /// <summary>
        /// N维数据
        /// </summary>
        public TData[] Data { get; internal set; }
        /// <summary>
        /// 分类标签
        /// </summary>
        public TLabel Label { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="dimension">数据维度</param>
        public DataVector(int dimension)
        {
            Data = new TData[dimension];
        }
        /// <summary>
        /// 维度数量
        /// </summary>
        public int Dimension
        {
            get { return this.Data.Length; }
        }
    }
    public class KNNData// knn算法的数据
    {
        internal int Count;
        internal double[] Data;
        internal String lable;
    }
    public class KNN// knn算法没有使用
    {
        public enum DistanceType
        {
            Euclidean, Minkowski, Manhattan
        }
        public double p = 2;
        public void setType(DistanceType type)
        {
            this.type = type;
        }
        DistanceType type = DistanceType.Euclidean;

        /// <summary>
        /// 样本数据
        /// </summary>
        private List<KNNData> sampleList;

        /// <summary>
        /// 未分类数据
        /// </summary>
        private List<KNNData> unclassifyList;

        /// <summary>
        /// K值
        /// </summary>
        private int k;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sampleList">样本数据</param>
        /// <param name="unclassifyList">未分类数据</param>
        /// <param name="k">k值</param>
        public KNN(List<KNNData> sampleList, List<KNNData> unclassifyList, int k)
        {
            this.sampleList = sampleList;
            this.unclassifyList = unclassifyList;
            this.k = k;
        }

        /// <summary>
        /// 分类
        /// </summary>
        public void Classify()
        {
            int sampleCount = sampleList.Count;
            int unclassifyCount = unclassifyList.Count;

            // 
            for (int i = 0; i < unclassifyCount; i++)
            {
                Tuple<string, double>[] tupleArray = new Tuple<string, double>[sampleCount];
                for (int j = 0; j < sampleCount; j++)
                {
                    double distance = CalculateDistance(sampleList[j], unclassifyList[i]);
                    string species = sampleList[j].lable;
                    tupleArray[j] = Tuple.Create(species, distance);
                }

                //
                IEnumerable<Tuple<string, double>> selector = tupleArray.OrderBy(t => t.Item2).Take(k);
                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                foreach (Tuple<string, double> tuple in selector)
                {
                    if (dictionary.ContainsKey(tuple.Item1))
                    {
                        dictionary[tuple.Item1]++;
                    }
                    else
                    {
                        dictionary.Add(tuple.Item1, 1);
                    }
                }

                // 
                IEnumerable<KeyValuePair<string, int>> keyValuePair = dictionary.OrderByDescending(t => t.Value).Take(1);
                foreach (KeyValuePair<string, int> kvp in keyValuePair)
                {
                    unclassifyList[i].lable = kvp.Key;
                }

                // 
                sampleList.Add(unclassifyList[i]);
                sampleCount++;
            }

        }

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="sample">样本数据</param>
        /// <param name="unclassify">未分类数据</param>
        /// <returns>两者欧氏距离</returns>
        public double CalculateDistance(KNNData sample, KNNData unclassify)
        {
            if (type == DistanceType.Euclidean)
            {
                double sum = 0, d;
                for (int i = 0; i < k; i++)
                {
                    d = sample.Data[i] - unclassify.Data[i];
                    d *= d;
                    sum += d;
                }
                return Math.Sqrt(sum);
            }
            else
                if (type == DistanceType.Manhattan)
            {
                double sum = 0, d;
                for (int i = 0; i < k; i++)
                {
                    d = sample.Data[i] - unclassify.Data[i];
                    d = Math.Abs(d);
                    sum += d;
                }
                return Math.Sqrt(sum);
            }
            else
            {
                double sum = 0, d;
                double q = 1 / p;
                for (int i = 0; i < k; i++)
                {
                    d = sample.Data[i] - unclassify.Data[i];
                    d = Math.Pow(d, p);
                    sum += d;
                }
                return Math.Pow(sum, q);
            }
        }



    }
    public class adaDataList:List<DataVector<double,int>>// 封装好的adaboost数据序列
	{
        public int Dimension { get; private set; }
        public adaDataList(int dim)
		{
            Dimension = dim;
		}
        public adaDataList(int dim,ICollection<DataVector<double,int>> datas):base(datas.Count)
        {
            Dimension = dim;
            foreach(DataVector<double,int> d in datas)
			{
                Add(d);
			}
        }
        public adaDataList(int dim,int capacity) : base(capacity)
		{
            Dimension = dim;
		}
	}
    public class adaSumMap//便于计算错误率的adaboost数据类
	{

   
        public static adaSumMap[] getMaps(ICollection<AdaData> datas)//从原始数据生成
		{
            int dim = datas.First().Dimension;
            adaSumMap[] maps = new adaSumMap[dim];
         
            Parallel.ForEach(dim.getRange().split(16), index =>
            {
                foreach(int i in index)
				{
 maps[i] = new adaSumMap(datas, i);
				}
            });
            return maps;
		}
        public int count { get; internal set; }
        public double[] data { get; internal set; }
        public sbyte[] label { get; internal set; }
        public int[] index { get; internal set; }
        public int P { get; internal set; }
        public int N { get; internal set; }

        public adaSumMap read(Stream s)// 读取文件流
        {
            byte[] data = new byte[8];
            s.Read(data, 0, 4);
            count = BitConverter.ToInt32(data,0);
            s.Read(data, 0, 4);
            P = BitConverter.ToInt32(data, 0);
            s.Read(data, 0, 4);
            N = BitConverter.ToInt32(data, 0);
            this.data = new
                double[count];
            label = new sbyte[count];
            index = new int[count];
            for(int i=0;i<count;i++)
			{
                s.Read(data, 0, 8);
               this. data[i]= BitConverter.ToSingle(data, 0);
                s.Read(data, 0, 1);
                this.label[i] = 
                    data[0].toSbyte();
                s.Read(data, 0, 4);
                this.index[i] = BitConverter.ToInt32(data, 0);
            }
            return this;
        }
        public void write(Stream s)//写文件流
        {
            s.Write(BitConverter.GetBytes(count), 0, 4);
            s.Write(BitConverter.GetBytes(P), 0, 4);
            s.Write(BitConverter.GetBytes(N), 0, 4);
            for(int i=0;i< count;i++)
			{
                s.Write(BitConverter.GetBytes(data[i]), 0, 8);
                s.Write(BitConverter.GetBytes((short)label[i]), 0,1);
                s.Write(BitConverter.GetBytes(index[i]), 0, 4);
			}
           
        }
        internal adaSumMap()
		{

		}
        public adaSumMap(ICollection< DataVector<double,int>> data,int dim)//从原始数据创建单个数据维度的数据
        {
       
            int ind = 0;
           foreach(AdaData ada in data)
			{
                ada.temp = ind;
                ind++;
			}
         
            N = P = 0;
            
            count = data.Count;
            index = new int[count];
            this.data = new double[count];
            label = new sbyte[count];

      

            var result = 
                (from DataVector<double, int> dt in data orderby dt.Data[dim] ascending select dt);
            int n = 0;
          
                foreach (AdaData dt in result)
			{


              
                this.data[n] = (float)dt.Data[dim];
                this.label[n] = (sbyte)dt.Label;
      
                index[n] = dt.temp;
                if(dt.Label==1)
				{
                    P++;
				}
                else
				{
                    N++;
				}
                n++;
			}
		}

    }
    public class AdaBoost:IxmlObject<AdaBoost>
    {
        public enum trainMode
		{
            Positive,//更关注把正确识别为错误
            Nagtive,//更关注把错误识别为正确
            Balance//平衡

		}
        /// <summary>
        /// 弱分类器列表
        /// </summary>
       internal  List<DecisionStump> m_WeakClassifiers=new List<DecisionStump>();
        public AdaBoost()
		{

		}
        public AdaBoost(string path)
        {
            load(path);
        }
        public void loadFromXml(XmlElement element)//从xml文件加载
        {
            m_WeakClassifiers = new List<DecisionStump>();
            foreach (XmlElement element1 in element.ChildNodes)
            {
                if (element1.Name.Equals("DecisionStump"))
                {
                    m_WeakClassifiers.Add(new DecisionStump().readXml(element1));
                }
            }
        }
        public bool load(string path)
		{
			try
			{
                XmlDocument x = new XmlDocument()
                    ;
                x.Load(path);
				if (x.DocumentElement.Name.Equals("WeakClassifiers"))
				{
                    loadFromXml(x.DocumentElement);
                    return true;
				}
                else
				{
                    return false;
				}
			}
            catch
			{
                return false;
			}
		}
        public bool saveData(string path)
		{
            XmlDocument xml = new XmlDocument();
           XmlElement e= xml.CreateElement("WeakClassifiers");
            xml.AppendChild(e);
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
        /// <summary>
        /// 训练只使用adaSumMap格式的数据
        /// </summary>
        /// <param name="trainingSet">训练数据集</param>
        /// <param name="iterateCount">迭代次数，即弱分类器数量</param>

        public void Train(trainMode mode, ICollection<adaSumMap> trainingset, int iterateCount = 50,double maxError=0.15, int[] dimindex = null
         )
        {

            if (maxError > 1)
            {
                maxError = 1;
            }
            if (maxError < 0)
            {
                maxError = 0;
            }
            double maxError_ =Math.Sqrt( 1 - maxError);
            maxError_ = 0.5 + maxError_ / 2;

            int data_num = trainingset.First().count;
            int dim = trainingset.Count;
            if (dimindex == null)
            {
                dimindex = new int[dim];
                for (int i = 0; i < dimindex.Length; i++)
                {
                    dimindex[i] = i;
                }
            }
            m_WeakClassifiers = new List<DecisionStump>();

            var D = new double[data_num];
            var guessResults = new double[data_num];
            double D_0 = 1.0 / data_num;
            for (int i = 0; i < data_num; ++i)
            {
                //权重初始化为1/n
                D[i] = D_0;
                //猜测结果初始化为0，后面累加要用
                guessResults[i] = (0.0);
            }

            //迭代指定次数
            for (int i = 0; i < iterateCount; ++i)
            {
                //在当前权重下生成一棵错误率最低的单层决策树
                DecisionStump stump;


                stump = CreateDecisionStump_usemap_threads( D, dimindex,trainingset);



                //计算Alpha（注意stump.Error有可能为0，要防止除0错误）
                stump.Alpha = 0.5 * Math.Log((1 - stump.Error) / Math.Max(stump.Error, 1e-16));

                //保存这个决策树到弱分类器
                m_WeakClassifiers.Add(stump);

                //根据猜测结果，重新计算下一轮的权重向量D(暂时未除以Sum(D)，下一步再处理)根据训练模式不同则分配方式不同
                adaSumMap map = trainingset.First();
                for (int j = 0; j < data_num; ++j)
                {
                    int index = map.index[j];
                    if(mode==trainMode.Balance)
					{
                    if (stump.Results[index] == map.label[j])
                        D[index] = D[index] * Math.Exp(-stump.Alpha);
                    else
                        D[index] = D[index] * Math.Exp(stump.Alpha);
					}
                    else if(mode == trainMode.Positive)
					{
                        if (stump.Results[index] != map.label[j])
                        {
                            if (map.label[j] == 1)
                                D[index] = D[index] * Math.Exp(stump.Alpha);
                            else
                            {
                                D[index] = D[index] * Math.Exp(stump.Alpha * maxError_);
                            }
                        }

                        else
                            D[index] = D[index] * Math.Exp(-stump.Alpha);
                    }
                    else
					{
                        if (stump.Results[index] != map.label[j] )
						{
                            if( map.label[j] == -1)
                            D[index] = D[index] * Math.Exp(stump.Alpha);
                            else
                            {
                                D[index] = D[index] * Math.Exp(stump.Alpha * maxError_);
                            }
                        }
                           
                        else 
                            D[index] = D[index] * Math.Exp(-stump.Alpha);
                    }
                
                }

                //保证Sum(D)==1
                double sum = D.Sum();
                double one_sum = 1 / sum;
                for (int j = 0; j < data_num; ++j)
                {
                    D[j] = D[j] * one_sum;
                    guessResults[j] += stump.Alpha * stump.Results[j];
                }

                //计算总错误率
                int errors = 0;
                int errors_p = 0;
                int errors_n = 0;
                for (int j = 0; j < data_num; ++j)
                {
                    if (Math.Sign(guessResults[map.index[j]]) != map.label[j])
					{
                        ++errors;
                        if (map.label[j] == 1)
                        {
                            errors_p++;
                        }
                        else
                        {
                            errors_n++;
                        }
                    }
                }
                double P_error = errors / (double)data_num;
                //如果没有错误，可以提前退出循环，但一般很难达到
                if (mode == trainMode.Balance)
                {
                    if (errors == 0)
                        break;
                }
                else if (mode == trainMode.Positive)
                {
                    if (errors_p == 0&&P_error<=maxError)
                        break;
                }
                else
                {
                    if (errors_n == 0&&P_error<=maxError)
                        break;
                }
            }

            GC.Collect();
        }
        //训练，使用adaSumMap加速
        public void Train(trainMode mode, List<DataVector<double, int>> trainingSet,ICollection<adaSumMap> maps, int iterateCount = 50, double maxError = 0.15, int[] dimindex = null
      )
        {

            if (maxError > 1)
            {
                maxError = 1;
            }
            if (maxError < 0)
            {
                maxError = 0;
            }
            double maxError_ = Math.Sqrt(1 - maxError);
            maxError_ = 0.5 + maxError_ / 2;


            int data_num = trainingSet.Count;
            if (dimindex == null)
            {
                dimindex = new int[trainingSet[0].Dimension];
                for (int i = 0; i < dimindex.Length; i++)
                {
                    dimindex[i] = i;
                }
            }
            m_WeakClassifiers = new List<DecisionStump>();

            var D = new double[data_num];
            var guessResults = new double[data_num];
            double D_0 = 1.0 / trainingSet.Count;
            for (int i = 0; i < trainingSet.Count; ++i)
            {
                //权重初始化为1/n
                D[i] = D_0;
                //猜测结果初始化为0，后面累加要用
                guessResults[i] = (0.0);
            }

            //迭代指定次数
            for (int i = 0; i < iterateCount; ++i)
            {
                //在当前权重下生成一棵错误率最低的单层决策树
                DecisionStump stump;


                stump = CreateDecisionStump_usemap_threads(D, dimindex,maps);



                //计算Alpha（注意stump.Error有可能为0，要防止除0错误）
                stump.Alpha = 0.5 * Math.Log((1 - stump.Error) / Math.Max(stump.Error, 1e-16));

                //保存这个决策树到弱分类器
                m_WeakClassifiers.Add(stump);

                //根据猜测结果，重新计算下一轮的权重向量D(暂时未除以Sum(D)，下一步再处理)

                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    if(mode==trainMode.Positive)
					{
     if (stump.Results[j] != trainingSet[j].Label)
					{
                        if(trainingSet[j].Label==1)
                             D[j] = D[j] * Math.Exp(stump.Alpha);
                            else
                            {
                                D[j] = D[j] * Math.Exp(stump.Alpha * maxError_);
                            }
                        }
                      
                    else
                        D[j] = D[j] * Math.Exp(-stump.Alpha);
					}else
                    if (mode == trainMode.Nagtive)
                    {
                        if (stump.Results[j] != trainingSet[j].Label)
                        {
                            if (trainingSet[j].Label == -1)
                                D[j] = D[j] * Math.Exp(stump.Alpha);
                            else
                            {
                                D[j] = D[j] * Math.Exp( stump.Alpha * maxError_);
                            }
                        }

                        else
                            D[j] = D[j] * Math.Exp(-stump.Alpha);
                    }
                    else
					{
                        if (stump.Results[j] != trainingSet[j].Label)
                        {
                            
                                D[j] = D[j] * Math.Exp(stump.Alpha);
                        }

                        else
                            D[j] = D[j] * Math.Exp(-stump.Alpha);
                    }

                }

                //保证Sum(D)==1
                double sum = D.Sum();
                double one_sum = 1 / sum;
                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    D[j] = D[j] * one_sum;
                    guessResults[j] += stump.Alpha * stump.Results[j];
                }

                //计算总错误率
                int errors = 0;
                int errors_p = 0;
                int errors_n = 0;
                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    if (Math.Sign(guessResults[j]) != trainingSet[j].Label)
					{
                              ++errors;
                        if(trainingSet[j].Label==1)
						{
                            errors_p++;
						}
                        else
						{
                            errors_n++;
						}
					}
                       
                }
                double P_error = errors / (double)data_num;
                //如果没有错误，可以提前退出循环，但一般很难达到
                if (mode == trainMode.Balance)
                {
                    if (errors == 0)
                        break;
                }
                else if (mode == trainMode.Positive)
                {
                    if (errors_p == 0 && P_error <= maxError)
                        break;
                }
                else
                {
                    if (errors_n == 0 && P_error <= maxError)
                        break;
                }

            }

            GC.Collect();
        }
        // 使用原始数据训练，非常慢
        public void Train(trainMode mode, List<DataVector<double, int>> trainingSet, int iterateCount = 50, double maxError = 0.15, int[] dimindex=null
            )
        {
            if (maxError > 1)
            {
                maxError = 1;
            }
            if (maxError < 0)
            {
                maxError = 0;
            }
            double maxError_ = Math.Sqrt(1 - maxError);
            maxError_ = 0.5 + maxError_ / 2;


            int data_num = trainingSet.Count;
            if(dimindex==null)
			{
                dimindex = new int[trainingSet[0].Dimension];
                for(int i=0;i<dimindex.Length;i++)
				{
                    dimindex[i] = i;
				}
			}
            m_WeakClassifiers = new List<DecisionStump>();

            var D =new double[data_num];
            var guessResults = new double[data_num];
            double D_0 = 1.0 / trainingSet.Count;
            for (int i = 0; i < trainingSet.Count; ++i)
            {
                //权重初始化为1/n
                D[i] = D_0;
                //猜测结果初始化为0，后面累加要用
                guessResults[i]=(0.0);
            }

            //迭代指定次数
            for (int i = 0; i < iterateCount; ++i)
            {
                //在当前权重下生成一棵错误率最低的单层决策树
                DecisionStump stump;       
            
        
                    stump= CreateDecisionStump(trainingSet, D, dimindex);
                
               

                //计算Alpha（注意stump.Error有可能为0，要防止除0错误）
                stump.Alpha = 0.5 * Math.Log((1 - stump.Error) / Math.Max(stump.Error, 1e-16));

                //保存这个决策树到弱分类器
                m_WeakClassifiers.Add(stump);

                //根据猜测结果，重新计算下一轮的权重向量D(暂时未除以Sum(D)，下一步再处理)

                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    if(mode==trainMode.Balance)
					{
                    if (stump.Results[j] == trainingSet[j].Label)
                        D[j] = D[j] * Math.Exp(-stump.Alpha);
                    else
                        D[j] = D[j] * Math.Exp(stump.Alpha);
					}
                    else if(mode == trainMode.Positive)
					{
                        if (stump.Results[j] != trainingSet[j].Label)
						{
                            if(trainingSet[j].Label==1)
                              D[j] = D[j] * Math.Exp(stump.Alpha);
                            else
							{
                                D[j] = D[j] * Math.Exp(stump.Alpha*maxError_);
                            }
						}
                            
                        else
                            D[j] = D[j] * Math.Exp(-stump.Alpha);
                    }
                    else
					{
                        if (stump.Results[j] != trainingSet[j].Label)
                        {
                            if (trainingSet[j].Label == -1)
                                D[j] = D[j] * Math.Exp(stump.Alpha);
                            else
                            {
                                D[j] = D[j] * Math.Exp(stump.Alpha * maxError_);
                            }
                        }

                        else
                            D[j] = D[j] * Math.Exp(-stump.Alpha);
                    }

                    
                }

                //保证Sum(D)==1
                double sum = D.Sum();
                double one_sum = 1 / sum;
                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    D[j] = D[j]*one_sum;
                    guessResults[j] += stump.Alpha * stump.Results[j];
                }

                //计算总错误率
                //计算总错误率
                int errors = 0;
                int errors_p = 0;
                int errors_n = 0;
                for (int j = 0; j < trainingSet.Count; ++j)
                {
                    if (Math.Sign(guessResults[j]) != trainingSet[j].Label)
                    {
                        ++errors;
                        if (trainingSet[j].Label == 1)
                        {
                            errors_p++;
                        }
                        else
                        {
                            errors_n++;
                        }
                    }

                }

                double P_error = errors / (double)data_num;
                //如果没有错误，可以提前退出循环，但一般很难达到
                if (mode == trainMode.Balance)
                {
                    if (errors == 0)
                        break;
                }
                else if (mode == trainMode.Positive)
                {
                    if (errors_p == 0 && P_error <= maxError)
                        break;
                }
                else
                {
                    if (errors_n == 0 && P_error <= maxError)
                        break;
                }
            }

            GC.Collect();
        }

        /// <summary>
        /// 分类
        /// </summary>
        /// <param name="vector">待测数据</param>
        /// <returns>分类结果，1或-1</returns>
        public int Classify(DataVector<double, int> vector)
        {
            double result = 0.0;   //用每一个弱分类器的结果乘以相应的alpha，累加得到最终的猜测结果
            foreach (var c in m_WeakClassifiers)
            {
                var stumpResults = ClassifyByDecisionStump(vector, c.Dimension, c.Boundary, c.GreaterThan);
                result += stumpResults * c.Alpha;
            }

            //根据正负决定输出1还是-1
            return Math.Sign(result);
        }

        /// <summary>
        /// 根据单层决策树进行一次分类
        /// </summary>
        /// <param name="dataSet">数据集</param>
        /// <param name="dimension">在哪个维度上分类</param>
        /// <param name="boundary">分类边界</param>
        /// <param name="greaterThan">是否按大于来划分数据</param>
        /// <returns>结果</returns>
        /// 

        
        private int ClassifyByDecisionStump(DataVector<double, int> dataSet, int dimension, double boundary, bool greaterThan)
        {
            int result = 0;
          
                if (greaterThan)
                    result = dataSet.Data[dimension] > boundary ? 1 : -1;
                else
                    result= dataSet.Data[dimension] <= boundary ? 1 : -1;
            return result;
        }

        /// <summary>
        /// 对一组数据分类
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="dimension"></param>
        /// <param name="boundary"></param>
        /// <param name="greaterThan"></param>
        /// <returns></returns>
        private int[] ClassifyByDecisionStump(List<DataVector<double, int>> dataSet, int dimension, double boundary, bool greaterThan)
        {
            var result = new int[dataSet.Count];
            int n = 0;
            foreach (var item in dataSet)
            {
                if (greaterThan)
                    result[n]=item.Data[dimension] > boundary ? 1 : -1;
                else
                    result[n]=item.Data[dimension] <= boundary ? 1 : -1;

                n++;
            }
            

            return result;
        }
        /// <summary>
        /// 对一组数据分类,使用adaSumMap格式
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="dimension"></param>
        /// <param name="boundary"></param>
        /// <param name="greaterThan"></param>
        /// <returns></returns>
        private int[] ClassifyByDecisionStump(ICollection<adaSumMap> maps , int dimension, double boundary, bool greaterThan)
        {
            adaSumMap map = maps.ElementAt(dimension);
            var result = new int[map.count];
            int n = 0;
            foreach (var item in map.data)
            {
                if (greaterThan)
                    result[map.index[n]] = item > boundary ? 1 : -1;
                else
                    result[map.index[n]] = item <= boundary ? 1 : -1;

                n++;
            }


            return result;
        }
        /// <summary>
        /// 构建一个单层决策树
        /// </summary>
        /// <param name="dataSet">数据集</param>
        /// <param name="D">权重</param>
        /// <returns>此权重下的最佳单层决策树</returns>
        private DecisionStump CreateDecisionStump(List<DataVector<double, int>> dataSet, double[] D,int[] dimIndex)
        {
            var stump = new DecisionStump();
            double minError = double.PositiveInfinity;

            int dim = dataSet[0].Dimension;
          

            //遍历每一个维度
          foreach(int i in dimIndex) { 
                //找此维度的最大最小值
                double maxValue = double.NegativeInfinity;
                double minValue = double.PositiveInfinity;

                foreach (var item in dataSet)
                {
                    if (item.Data[i] > maxValue)
                        maxValue = item.Data[i];
                    if (item.Data[i] < minValue)
                        minValue = item.Data[i];
                }

                //做10次切分，计算步长
                double stepSize = (maxValue - minValue) / 10.0;
                for (int j = 0; j < 10; ++j)
                {
                    //边界
                    double boundary = minValue + stepSize * j;

                    //分别计算边界两边的情况
                    for (int k = 0; k < 2; ++k)
                    {
                        var results = ClassifyByDecisionStump(dataSet, i, boundary, k == 0);

                        //计算错误，注意是加权的错误
                        double weightError = 0.0;
                        for (int idx = 0; idx < results.Length; ++idx)
                        {
                            if (results[idx] != dataSet[idx].Label)
                                weightError += D[idx];
                        }
                        
                    
                        
                        //保留最小错误的分类器
                        if (weightError < minError)
                        {
                            minError = weightError;
                            stump.Error = Math.Min(weightError, 1.0);         //此分类器的错误比例
                            stump.Boundary = boundary;                        //分类边界
                            stump.Dimension = i;                              //在哪个维度上分类
                            stump.GreaterThan = k==0;                        //大于还是小于
                            stump.Results = results;                          //用此分类器得出的结果
                        }
                    }
                }
            }

            return stump;
        }

        // 构建一个单层决策树,使用adaSumMap格式，并使用多线程加速
        private DecisionStump CreateDecisionStump_usemap_threads(double[] D, int[] dimIndex, ICollection<adaSumMap> maps)
        {
            int count_ = maps.First().count;
            DecisionStump stumpfinal = new DecisionStump();
            stumpfinal.Error = double.PositiveInfinity;
       


            
           
          
      
            int[][] indexlist=
            dimIndex.split(16);      
            DecisionStump[] decisionStumps = new DecisionStump[indexlist.Length];
      

            Parallel.For(0,indexlist.Length, indexs_i =>
            {
                DecisionStump stump = new DecisionStump();
                int[] indexs = indexlist[indexs_i];
                double[] errorN ;
                double[] errorP ;
                double[] errorN_R ;
                double[] errorP_R ; 
                double minError = double.PositiveInfinity;
                foreach (int i in indexs)
				{
                    int k;
                    int count = count_;
                    
                   
              errorN = new double[count + 1];
                  errorP = new double[count + 1];
                   errorN_R = new double[count + 1];
                    errorP_R = new double[count + 1];
                    adaSumMap map = maps.ElementAt(i);
                    //累加求和计算错误率的过程
                    for (int j = 0; j < count; j++)
                    {
                        if (map.label[j] == -1)
                        {
                            errorN[j + 1] = errorN[j] + D[map.index[j]];
                            errorP[j + 1] = errorP[j];
                        }
                        else
                        {
                            errorP[j + 1] = errorP[j] + D[map.index[j]];
                            errorN[j + 1] = errorN[j];
                        }
                        k = count - j - 1;
                        if (map.label[k] == -1)
                        {
                            errorN_R[k] = errorN_R[k + 1];
                            errorP_R[k] = errorP_R[k + 1] + D[map.index[k]];
                        }
                        else
                        {
                            errorN_R[k] = errorN_R[k + 1] + D[map.index[k]];
                            errorP_R[k] = errorP_R[k + 1];

                        }
                    }

                    for (int j = 0; j <= count; j++)
                    {
                        errorP[j] += errorP_R[j];
                        errorN[j] += errorN_R[j];


                        if (errorP[j] < errorN[j])
                        {
                            double temp = errorP[j];

                            if (temp < minError)
                            {
                                minError = temp;

                                if (j == 0)
                                {
                                    stump.Boundary = map.data[0] - 1e-16;

                                }
                                else if (j == count)
                                {
                                    stump.Boundary = map.data[count - 1] + 1e-16;
                                }
                                else
                                {
                                    stump.Boundary = map.data[j] + map.data[j - 1];
                                    stump.Boundary *= 0.5;

                                }


                                stump.Error = Math.Min(temp, 1.0);         //此分类器的错误比例                      //分类边界
                                stump.Dimension = i;                              //在哪个维度上分类
                                stump.GreaterThan = true;

                                //大于还是小于
                                //用此分类器得出的结果
                            }
                        }
                        else
                        {
                            double temp = errorN[j];
                            if (temp < minError)
                            {
                                minError = temp;
                                if (j == 0)
                                {
                                    stump.Boundary = map.data[0] - 1e-16;

                                }
                                else if (j == count)
                                {
                                    stump.Boundary = map.data[count - 1] + 1e-16;
                                }
                                else
                                {
                                    stump.Boundary = map.data[j] + map.data[j - 1];
                                    stump.Boundary *= 0.5;

                                }

                                stump.Error = Math.Min(temp, 1.0);         //此分类器的错误比例
                                stump.Dimension = i;                              //在哪个维度上分类
                                stump.GreaterThan = false;

                                //大于还是小于
                                //用此分类器得出的结果
                            }
                        }


                    }
                    
                }
decisionStumps[indexs_i] = stump;
            });

       
         
            DecisionStump temps; 
            int l = decisionStumps.Length;
            for (int i = 0; i < l; i++)
            {
                temps = decisionStumps[i];
                if (temps.Error < stumpfinal.Error)
                {
                    stumpfinal = temps;
                }
            }
            stumpfinal.Results = ClassifyByDecisionStump(maps, stumpfinal.Dimension, stumpfinal.Boundary, stumpfinal.GreaterThan);
            //遍历每一个维度
            GC.Collect();

            return stumpfinal;
        }
        // 构建一个单层决策树,使用adaSumMap格式，没有多线程加速
        private DecisionStump CreateDecisionStump_usemap(double[] D, int[] dimIndex, ICollection<adaSumMap> maps)
        {
            int count = maps.First().count;
            var stump = new DecisionStump();
            double minError = double.PositiveInfinity;
            double []errorN ;
            double[] errorP;
            double[] errorN_R ;
            double[] errorP_R;
            int dim = maps.Count;
            double temp = 0 ;
            adaSumMap map;
            int k;
            foreach (int i in dimIndex)
            {
                errorN = new double[count + 1];
                errorP = new double[count + 1];
                errorN_R = new double[count + 1];
                errorP_R = new double[count + 1];
                map = maps.ElementAt(i);
               for(int j=0;j<count;j++)
				{
                    if(map.label[j]==-1)
					{
                        errorN[j + 1] = errorN[j] + D[map.index[j]];
                        errorP[j + 1] = errorP[j];
                    }
                    else
					{
                        errorP[j + 1] = errorP[j] + D[map.index[j]];
                        errorN[j + 1] = errorN[j];
                    }
                    k = count - j - 1;
                    if(map.label[k]==-1)
					{
                        errorN_R[k] = errorN_R[k + 1];
                        errorP_R[k] = errorP_R[k+1] + D[map.index[k]];
                    }
                    else
					{
                        errorN_R[k] = errorN_R[k + 1] + D[map.index[k]];
                        errorP_R[k] = errorP_R[k + 1];

                    }
				}
            
                for (int j=0; j<=count;j++)
				{
                    errorP[j] += errorP_R[j];
                    errorN[j] += errorN_R[j];
                    if(errorP[j]<errorN[j])
					{
                        temp = errorP[j];
                        if (temp< minError)
                        {
                            if(j==0)
							{
                                stump.Boundary = map.data[0] - 1e-16;

                            }else if(j==count)
							{
                                stump.Boundary = map.data[count - 1] + 1e-16;
							}
                            else
							{
                                stump.Boundary = map.data[j] + map.data[j - 1];
                                stump.Boundary *= 0.5;

                            }

                            minError = temp;
                            stump.Error = Math.Min(temp, 1.0);         //此分类器的错误比例                      //分类边界
                            stump.Dimension = i;                              //在哪个维度上分类
                            stump.GreaterThan = true;
                          
                           //大于还是小于
                                                  //用此分类器得出的结果
                        }
                    }
                    else
					{
                        temp = errorN[j];
                        if (temp < minError)
                        {
                            if (j == 0)
                            {
                                stump.Boundary = map.data[0] - 1e-16;

                            }
                            else if (j == count)
                            {
                                stump.Boundary = map.data[count - 1] + 1e-16;
                            }
                            else
                            {
                                stump.Boundary = map.data[j] + map.data[j - 1];
                                stump.Boundary *= 0.5;

                            }
                            minError = temp;
                            stump.Error = Math.Min(temp, 1.0);         //此分类器的错误比例
                          stump.Dimension = i;                              //在哪个维度上分类
                            stump.GreaterThan = false;
                         
                            //大于还是小于
                            //用此分类器得出的结果
                        }
                    }
				}

              

            }
            stump.Results = ClassifyByDecisionStump(maps, stump.Dimension, stump.Boundary, stump.GreaterThan);
            //遍历每一个维度
       

            return stump;
        }

		public XmlElement writeXml(XmlElement element)
		{
    
            XmlElement e = element;
            XmlElement child;
            foreach (DecisionStump stump in m_WeakClassifiers)
            {
                child = e.OwnerDocument.CreateElement("DecisionStump");
                stump.writeXml(child);
                e.AppendChild(child);
            }

            return element;
        }

		public AdaBoost readXml(XmlElement element)
		{
            loadFromXml(element);
            return this;
        }
	}
}




