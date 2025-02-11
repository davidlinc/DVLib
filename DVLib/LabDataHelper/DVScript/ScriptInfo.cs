using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using E = System.Linq.Expressions.Expression;

namespace DVLib.LabDataHelper.DVScript
{
	public class ScriptInfo:ObjectInfo<ScriptObject,ScriptInfo,ScriptManager>
	{
		
		public ScriptInfo() { }
		public ScriptInfo(string token,int priority, Factory<ScriptObject, ScriptInfo, ScriptManager> factory,string tag="") { 
		this.mark = token;
		this.priority = priority;
		this.factory = factory;
		this.tag = tag;	
			
		}
	    public ScriptInfo setReverse()
		{
			this.reverse = true;	
			return this;
		}
	}

}
