using DVOSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper
{
	public enum EventType
	{
		NewSet,NewValue,ChangeValue,RemoveValue,
		RemoveSet,ChangeSetName,ChangeSetText,ChangeName,ChangeText

	}

	public delegate void DataSetEventHandler(DataSet set,EventType type);
	public class DataSet
	{
	    public	string name{get;internal set;}
		public string describe { get; internal set; }
		internal List<double> data=new List<double>();
		public int Count { get { return data.Count; } }
		public DataSet(string name="New Data",string describe="",int capcity=100)
		{
			this.name=name;
			this.describe=describe;
		}

		public double this[int x]
		{
			get { return data[x]; }
		}
		public double this[int x,DataConverter dc]
		{
			get { return  dc(data[x]); }
		}
		public double getMean(DataConverter converter)
		{
			return converter(Mean);
		}
		public double getMax(DataConverter converter)
		{
			return converter(Max);
		}
		public double getMin(DataConverter converter)
		{
			return converter(Min);
		}

		public double[] getData(DataConverter c = null)
		{
			
				double[] doubles=new double[data.Count];
			if (c != null)
			{ 
			for (int i = 0; i < data.Count; i++)
				{
					doubles[i] = c(data[i]);
				}
			}
			else
			{
				for (int i = 0; i < data.Count; i++)
				{
					doubles[i] = data[i];
				}
			}
			return doubles;
		}

		public double Min { get { if (data.Count == 0) return 0; return (from double d in data orderby d ascending select d).First(); } }
		public double Max { get { if (data.Count == 0) return 0; return (from double d in data orderby d descending select d).First(); } }
		public double Mean { get {
				if(data.Count == 0)
					return 0;
				double sum = 0;
				foreach (double d in data)
				{
					sum += d;
				}
				return sum/data.Count; } }
		public void write(InfoStream info)
		{
			info.write(name,describe,data.ToArray());
		}

		public DataSet read(InfoStream info) {
		name=info.readString();
        describe=info.readString();
			data.Clear();
			foreach(double d in info.readDoubleArray()) {
			data.Add(d);
			}
			return this;
		}

		public override string ToString()
		{
			return name;
		}

	}
}
