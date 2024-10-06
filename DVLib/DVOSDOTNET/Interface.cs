using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace DVOSLib
{
	public static class DVOS
	{
		public static WriteString stringWriter;
		public static void writeLine(object s)
		{
			if(stringWriter!= null)
			{
				 stringWriter(s.ToString() + "\n");
			}
		}
		static StringBuilder sb=new StringBuilder();
		public static void outPut(object s)
		{
			if (stringWriter != null)
			{
				if(s is System.Collections.IEnumerable)
				{
					sb.Clear();
					foreach(var  v in (System.Collections.IEnumerable)s)
					{
						sb.Append(v.ToString() + "\n");

					}
					stringWriter(sb.ToString() + "\n");
				}
				else
				{

					stringWriter(s.ToString() + "\n");
				}
			}
		}
	}
public	delegate void WriteString(string s);

       public	interface IxmlObject<T>
	{ 
		XmlElement writeXml(XmlElement element);

		T readXml(XmlElement element);

	}

	public interface IByteArrayObject<T>
	{
		byte[] getBytes(byte[] outArray);
		byte[] getBytes();
	    T  readBytes(byte[] bytes,int offset=0);

	}
	public interface IStreamObject<T> 
	{
		void writeStream(Stream stream);
		void writeStream(Stream stream, byte[] buffer);
		T readStream(Stream stream);
		T readStream(Stream stream, byte[] buffer);
	}
}
