using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.DVScript
{
	public delegate Expression ExpMaker();
	public delegate Expression ExpMaker<T1>(T1 t1);
	public delegate Expression ExpMaker<T1, T2>(T1 t1, T2 t2);
	public delegate Expression ExpMaker<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
	public delegate Expression ExpMaker<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15);
	public delegate Expression ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16);


	internal class ScriptObject<T> : ScriptObject
	{
		ScriptObject script;
		ExpMaker<T> SingleExpMaker;

		public ScriptObject(Type type, ExpMaker<T> SingleExpMaker, ScriptObject script) : base(type)
		{
			this.script = script;
			this.SingleExpMaker = SingleExpMaker;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			HashSet<ParameterExpression> set = new HashSet<ParameterExpression>();
			var v = script.getExpression(force);
			foreach (var vv in v.Item2)
			{
				set.Add(vv);
			}
			return (SingleExpMaker((T)(object)v.Item1), set);
		}
	}
	internal class ScriptObject<T1, T2> : ScriptObject
	{

		internal ScriptObject a;
		ScriptObject b;

		ExpMaker<T1, T2> expMaker;

		internal ScriptObject(Type type, ExpMaker<T1, T2> expMaker, ScriptObject a, ScriptObject b) : base(type)
		{
			this.expMaker = expMaker;
			this.a = a;
			this.b = b;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var aa = a.getExpression(force);
			var bb = b.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in aa.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in bb.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)aa.Item1, (T2)(object)bb.Item1), List);
		}
	}
	internal class ScriptObject<T1, T2, T3> : ScriptObject
	{

		ScriptObject a;
		ScriptObject b;
		ScriptObject c;

		ExpMaker<T1, T2, T3> expMaker;

		internal ScriptObject(Type type, ExpMaker<T1, T2, T3> expMaker, ScriptObject a, ScriptObject b, ScriptObject c):base(type) 
		{
			this.expMaker = expMaker;
			this.a = a;
			this.b = b;
			this.c = c;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var aa = a.getExpression(force);
			var bb = b.getExpression(force);
			var cc = b.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in aa.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in bb.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in cc.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)aa.Item1, (T2)(object)bb.Item1, (T3)(object)cc.Item1), List);
		}


	}
	internal class ScriptObject<T1, T2, T3, T4> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ExpMaker<T1, T2, T3, T4> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ExpMaker<T1, T2, T3, T4, T5> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ExpMaker<T1, T2, T3, T4, T5, T6> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force = false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ScriptObject s12;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11, ScriptObject s12) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
			this.s12 = s12;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var a12 = s12.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a12.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1, (T12)(object)a12.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ScriptObject s12;
		ScriptObject s13;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11, ScriptObject s12, ScriptObject s13) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
			this.s12 = s12;
			this.s13 = s13;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var a12 = s12.getExpression(force);
			var a13 = s13.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a12.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a13.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1, (T12)(object)a12.Item1, (T13)(object)a13.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ScriptObject s12;
		ScriptObject s13;
		ScriptObject s14;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11, ScriptObject s12, ScriptObject s13, ScriptObject s14) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
			this.s12 = s12;
			this.s13 = s13;
			this.s14 = s14;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var a12 = s12.getExpression(force);
			var a13 = s13.getExpression(force);
			var a14 = s14.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a12.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a13.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a14.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1, (T12)(object)a12.Item1, (T13)(object)a13.Item1, (T14)(object)a14.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ScriptObject s12;
		ScriptObject s13;
		ScriptObject s14;
		ScriptObject s15;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11, ScriptObject s12, ScriptObject s13, ScriptObject s14, ScriptObject s15) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
			this.s12 = s12;
			this.s13 = s13;
			this.s14 = s14;
			this.s15 = s15;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var a12 = s12.getExpression(force);
			var a13 = s13.getExpression(force);
			var a14 = s14.getExpression(force);
			var a15 = s15.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a12.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a13.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a14.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a15.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1, (T12)(object)a12.Item1, (T13)(object)a13.Item1, (T14)(object)a14.Item1, (T15)(object)a15.Item1), List);
		}
	}

	internal class ScriptObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : ScriptObject
	{
		ScriptObject s1;
		ScriptObject s2;
		ScriptObject s3;
		ScriptObject s4;
		ScriptObject s5;
		ScriptObject s6;
		ScriptObject s7;
		ScriptObject s8;
		ScriptObject s9;
		ScriptObject s10;
		ScriptObject s11;
		ScriptObject s12;
		ScriptObject s13;
		ScriptObject s14;
		ScriptObject s15;
		ScriptObject s16;
		ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> expMaker;
		internal ScriptObject(Type type, ExpMaker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> expMaker, ScriptObject s1, ScriptObject s2, ScriptObject s3, ScriptObject s4, ScriptObject s5, ScriptObject s6, ScriptObject s7, ScriptObject s8, ScriptObject s9, ScriptObject s10, ScriptObject s11, ScriptObject s12, ScriptObject s13, ScriptObject s14, ScriptObject s15, ScriptObject s16) : base(type)
		{
			this.expMaker = expMaker;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
			this.s6 = s6;
			this.s7 = s7;
			this.s8 = s8;
			this.s9 = s9;
			this.s10 = s10;
			this.s11 = s11;
			this.s12 = s12;
			this.s13 = s13;
			this.s14 = s14;
			this.s15 = s15;
			this.s16 = s16;
		}
		public override (Expression, HashSet<ParameterExpression>) getExpression(bool force=false)
		{
			var a1 = s1.getExpression(force);
			var a2 = s2.getExpression(force);
			var a3 = s3.getExpression(force);
			var a4 = s4.getExpression(force);
			var a5 = s5.getExpression(force);
			var a6 = s6.getExpression(force);
			var a7 = s7.getExpression(force);
			var a8 = s8.getExpression(force);
			var a9 = s9.getExpression(force);
			var a10 = s10.getExpression(force);
			var a11 = s11.getExpression(force);
			var a12 = s12.getExpression(force);
			var a13 = s13.getExpression(force);
			var a14 = s14.getExpression(force);
			var a15 = s15.getExpression(force);
			var a16 = s16.getExpression(force);
			var List = new HashSet<ParameterExpression>();
			foreach (var pp in a1.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a2.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a3.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a4.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a5.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a6.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a7.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a8.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a9.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a10.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a11.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a12.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a13.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a14.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a15.Item2)
			{
				List.Add(pp);
			}
			foreach (var pp in a16.Item2)
			{
				List.Add(pp);
			}
			return (expMaker((T1)(object)a1.Item1, (T2)(object)a2.Item1, (T3)(object)a3.Item1, (T4)(object)a4.Item1, (T5)(object)a5.Item1, (T6)(object)a6.Item1, (T7)(object)a7.Item1, (T8)(object)a8.Item1, (T9)(object)a9.Item1, (T10)(object)a10.Item1, (T11)(object)a11.Item1, (T12)(object)a12.Item1, (T13)(object)a13.Item1, (T14)(object)a14.Item1, (T15)(object)a15.Item1, (T16)(object)a16.Item1), List);
		}
	}
}
