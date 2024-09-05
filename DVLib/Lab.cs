using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVOSLib
{
	public delegate void Empty();
public	class Lab
	{
		List<TimeSpan> dateTimes = new List<TimeSpan>();
		public TimeSpan[] TimeSpans { get { return dateTimes.ToArray(); } }
		public void clear()
		{
			dateTimes.Clear();
		}
		public TimeSpan Execute(Empty e)
		{
			DateTime t1 = DateTime.Now;
			e();
			TimeSpan t = DateTime.Now - t1;
			dateTimes.Add(t);
			return t ;
		}

	}
}
