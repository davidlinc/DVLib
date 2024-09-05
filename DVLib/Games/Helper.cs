using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
public static class datehelper
{


	public static string part(this string text, string mark, int index)
	{
	
		int l = mark.Length;
	
		if (text.Length > 0 && text.Length > l && text.Substring(0, l) == mark)
			text = text.Remove(0, l);
		string result = "";

		if (index == 0)
		{
			if (text.Length < mark.Length)
			{
				return text;
			}
			for (int i = 0; i < text.Length + 1 - l; i++)
			{
				if (text.Substring(i, l) != mark)
				{
					result += text[i];
				}
				else
					break;

			}
			return result;
		}
		else
		{
			return part(text.Remove(0, part(text, mark, 0).Length), mark, index - 1);
		}
	}
public static	List<float> readNum(this string text, string mark = "*")
	{
		List<float> vs = new List<float>();
		int i = 0;
		string temp = text.part(mark, i);
		while (temp != "")
		{
			vs.Add((float)Convert.ToDouble(temp));
			i++;
			temp = text.part(mark, i);
		}
		return vs;
	}
	public static string part(this string text, int index)
	{
		
		if (text.Length > 0 && text[0] == '.')
			text = text.Remove(0, 1);
		string result = "";
		if (index == 0)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '.')
				{
					result += text[i];
				}
				else
					break;

			}
			return result;
		}
		else
		{
			return part(text.Remove(0, part(text, 0).Length), index - 1);
		}
	}
}
