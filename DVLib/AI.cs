using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVOSLib
{
public	struct MeaningVector
	{
		int Dim;
		float[] Values;
		public MeaningVector(params float[] floats)
		{
			Values = new float[floats.Length];
			Dim = floats.Length;
			Array.Copy(floats, Values,floats.Length);
		}
		
	}public enum WordType
		{
			N,Adj,V,Be,W
		}
		public class WordStruct
		{
			WordType[] types;
			public WordStruct(params WordType[] types)
			{
				this.types = types;
			}
		public static WordStruct Normal1 = new WordStruct(WordType.N,WordType.Be,WordType.N);
		public static WordStruct Normal2 = new WordStruct(WordType.N, WordType.Be);
		public static WordStruct Normal3 = new WordStruct( WordType.Be, WordType.N);
		public static WordStruct Normal4 = new WordStruct(WordType.N, WordType.V, WordType.N);
		public static WordStruct Normal5 = new WordStruct(WordType.N, WordType.V);
		public static WordStruct Normal6 = new WordStruct( WordType.V, WordType.N);

		
	}
	public class AI
	{
	}
}
