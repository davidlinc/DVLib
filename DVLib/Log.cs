using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib
{

	public class Log
	{
		string path;

		FileStream FileStream;
		StreamWriter FileWriter;

		public Log(string path)
		{
			FileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			FileWriter = new StreamWriter(FileStream);
		}
		public void writeLine(string msg)
		{
			FileWriter.WriteLine(msg);
			FileWriter.Flush();
		}
		public void writeLineWithTime(string msg)
		{
			FileWriter.WriteLine(DateTime.Now.ToString() + ":" + msg);
			FileWriter.Flush();
		}
		public void Close()
		{
			FileWriter?.Close();
			FileStream?.Close();
		}

	}
}
