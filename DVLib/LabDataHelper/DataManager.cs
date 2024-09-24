using DVOSLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper
{

	public delegate double DataConverter(double rawData);
	public class DataManager:IEnumerable<DataSet>
	{
		string name_;
	public	string name{ get { return name_; } set { name_ = value;OnChnage(null, EventType.ChangeName); } }

		string text;
		public string describe{ get { return text; } set { text = value;OnChnage(null, EventType.ChangeText); } }
		List<DataSet> dataSets=new List<DataSet>();
		public int Count { get { return dataSets.Count; } }
		public event DataSetEventHandler OnChnage=(a,b)=> { };

		public DataManager(string name,string des="")
		{
			this.name_ = name;
			this.text = des;
		}
	
		public DataSet this[int x]
		{ 	get { return dataSets[x];
			}}
		
		public int addNewData(string name,string decribe)
		{
			var D = new DataSet(name, decribe);
			dataSets.Add(D);
			OnChnage(D,EventType.NewSet);
			return Count - 1;

		}
		public void addValue(int index,double value,bool aloud=true)
		{
			dataSets[index].data.Add(value);
			if(aloud)
			OnChnage(dataSets[index],EventType.NewValue);

		}
		public void changeValue(int index,int index2,double value,bool aloud=true)
		{
			dataSets[index].data[index2]=value;
			if(aloud)
			OnChnage(dataSets[index],EventType.ChangeValue);

		}
		public void delete(int start,int count)
		{
			List<DataSet> doubles=new List<DataSet>();
			for(int i=0;i<dataSets.Count;i++)
			{
				if(i>=start&&i<start+count)
				{

				}
				else
				{
					doubles.Add(dataSets[i]);
				}
			}
			dataSets = doubles;
		}
		public void removeValue(int index, int index2,bool aloud=true)
		{
			dataSets[index].data.RemoveAt(index2);
			if(aloud)
			OnChnage(dataSets[index],EventType.RemoveValue);
		}
		public void changeName(int index,string name)
		{
			dataSets[index].name = name;
			OnChnage(dataSets[index],EventType.ChangeName);
		}

		
		public double[] getDataFromMean(int length,DataConverter converter=null)
		{
			if (converter == null)
			{
				converter = d => d;
			}
			length = Math.Min(Count, length);
			double[] doubles = new double[length];
			for (int i = 0; i < length; i++)
			{
				
					doubles[i] = converter(dataSets[i].Mean);
				
			}
			return doubles;
		}
		public double[] getRefData(double[] doubles,double refNum=0)
		{
			double refd = doubles[0];
			double[] r = new double[doubles.Length];
			for (int i = 0; i < doubles.Length; i++)
			{

				r[i] = doubles[i] - refd+refNum;
			}

			return r;
		}
		public double[] getDataFromDescribe(int length,DataConverter converter=null,bool refZero=true)
		{
			if(converter==null)
			{
				converter = d => d;
			}

			length=Math.Min(Count, length);
			double[] doubles = new double[length];
			double rz = 0;
			if(refZero)
			{
				rz = double.Parse(dataSets[0].describe);

            }
			for(int i = 0; i < length; i++)
			{
				try
				{
					doubles[i] =converter( double.Parse(dataSets[i].describe)-rz);
				}
				catch
				{
					doubles[i]= 0;
				}
			}
			return doubles;
		}
	public	static double CalculateRSquared(double[] actual, double[] predicted)
		{
			

			double actualMean = actual.Average();

			// 计算总平方和 (Total Sum of Squares, SST)
			double sst = actual.Sum(val => Math.Pow(val - actualMean, 2));

			// 计算残差平方和 (Residual Sum of Squares, SSR)
			double ssr = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();

			// 计算 R²
			return 1 - (ssr / sst);
		}
	
	public void changeDescribe(int index, string describe)
		{
			dataSets[index].describe = describe;
			OnChnage(dataSets[index],EventType.ChangeText);
		}
		public void removeDate(int index) {
		dataSets.RemoveAt(index);
			OnChnage(null,EventType.RemoveSet);
		}

		public void save(string path)
		{
			FileStream fs=new FileStream(path, FileMode.Create);
			InfoStream info=new InfoStream();
			write(info);
			fs.Write(info.getToSave(), 0, info.Length);
			fs.Flush();
			fs.Close();
		}
		public void load(string path)
		{
			FileStream fileStream=new FileStream (path, FileMode.Open);
			byte[] bytes=new byte[fileStream.Length];
			fileStream.Read(bytes, 0, bytes.Length);
			InfoStream info = new InfoStream(bytes,1);
			read(info);

 			fileStream.Close();
			
		}

		public void clear()
		{
			dataSets.Clear();
		}
		public void write(InfoStream stream)
		{
			stream.writeString(name);
			stream.writeString(describe);
			stream.writeInt(dataSets.Count);
			for(int i = 0; i < dataSets.Count; i++)
			{
				dataSets[i].write(stream);	
			}
		}

		public void read(InfoStream stream)
		{

			name = stream.readString();
			describe = stream.readString();
			int c= stream.readInt();
			dataSets.Clear();
			for(int i = 0;i < c;i++) {
			dataSets.Add(new DataSet().read(stream));
			}
		}

		public IEnumerator<DataSet> GetEnumerator()
		{
			return dataSets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dataSets.GetEnumerator();
		}
	}
}
