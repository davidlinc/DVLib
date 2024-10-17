using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.MathObjectSystem
{
public	partial class ChagedObject
	{
		int changeId = 0;
		public int ID { get { return changeId; } }
		public void markChanged()
		{
			changeId++;
		}
		public bool isChanged(int oldId) { return oldId != changeId; }
	}
}
