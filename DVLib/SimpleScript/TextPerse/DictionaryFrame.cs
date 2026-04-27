using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.MathObjectSystem
{
	public class DictionaryFrame<T>
	{
		Dictionary<string, T> dict=new Dictionary<string, T>();


		public void add(string key, T value)
		{
			dict.Add(key, value);
		}


	}
}
