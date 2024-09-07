using DVOSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper
{

	public delegate double DataConverter(double rawData);
	public class DataManager
	{
		string name_;
	public	string name{ get { return name_; } set { name_ = value;OnChnage(null, EventType.ChangeName); } }

		string text;
		public string describe{ get { return text; } set { text = value;OnChnage(null, EventType.ChangeText); } }
		List<DataSet> dataSets=new List<DataSet>();
		public int Count { get { return dataSets.Count; } }
		public event DataSetEventHandler OnChnage;

		public DataManager(string name,string des="")
		{
			this.name_ = name;
			this.text = des;
		}

		public DataSet this[int x]
		{ 	get { return dataSets[x];
			}}
		
		public void addNewData(string name,string decribe)
		{
			var D = new DataSet(name, decribe);
			dataSets.Add(D);
			OnChnage(D,EventType.NewSet);

		}
		public void addValue(int index,double value)
		{
			dataSets[index].data.Add(value);
			OnChnage(dataSets[index],EventType.NewValue);

		}
		public void changeValue(int index,int index2,double value)
		{
			dataSets[index].data[index2]=value;
			OnChnage(dataSets[index],EventType.ChangeValue);

		}
		public void removeValue(int index, int index2)
		{
			dataSets[index].data.RemoveAt(index2);
			OnChnage(dataSets[index],EventType.RemoveValue);
		}
		public void changeName(int index,string name)
		{
			dataSets[index].name = name;
			OnChnage(dataSets[index],EventType.ChangeName);
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
	}
}
