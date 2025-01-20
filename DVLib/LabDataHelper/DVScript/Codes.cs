using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVLib.LabDataHelper.DVScript
{
	internal class Codes
	{
		string ExpMaker(int c)
		{
			StringBuilder sb = new StringBuilder("public delegate Expression ExpMaker<");
			for (int i = 0; i < c; i++)
			{
				sb.Append("T");
				sb.Append(i + 1);
				if (i < c - 1)
				{
					sb.Append(',');
				}
			}

			sb.Append(">(");

			for (int i = 0; i < c; i++)
			{
				sb.Append("T");
				sb.Append(i + 1);
				sb.Append(" ");
				sb.Append("t");
				sb.Append(i + 1);
				if (i < c - 1)
				{
					sb.Append(',');
				}
			}
			sb.Append(");");

			return sb.ToString();

		}

		string ScriptClass(int c)
		{
			StringBuilder sb = new StringBuilder("internal class ScriptObject<");
			for (int i = 0; i < c; i++)
			{
				sb.Append("T");
				sb.Append(i + 1);
				if (i < c - 1)
				{
					sb.Append(',');
				}
			}

			sb.Append(">:ScriptObject");
			sb.AppendLine("{");

			for (int i = 0; i < c; i++)
			{
				sb.Append("ScriptObject s");
				sb.Append(i + 1);
				sb.AppendLine(";");

			}
			sb.Append("ExpMaker<");
			for (int i = 0; i < c; i++)
			{
				sb.Append("T");
				sb.Append(i + 1);
				if (i < c - 1)
				{
					sb.Append(',');
				}

			}
			sb.AppendLine("> expMaker;");

			sb.Append("internal ScriptObject(ExpMaker<");
			for (int i = 0; i < c; i++)
			{
				sb.Append("T");
				sb.Append(i + 1);
				if (i < c - 1)
				{
					sb.Append(',');
				}

			}
			sb.Append("> expMaker,");
			for (int i = 0; i < c; i++)
			{
				sb.Append("ScriptObject s");
				sb.Append(i + 1); if (i < c - 1)
				{
					sb.Append(',');
				}


			}
			sb.AppendLine(")\n{");
			sb.AppendLine("this.expMaker = expMaker;");
			for (int i = 0; i < c; i++)
			{
				sb.Append("this.s");
				sb.Append(i + 1);
				sb.Append("=s");
				sb.Append(i + 1);
				sb.AppendLine(";");

			}
			sb.AppendLine("}");

			sb.AppendLine("public override (Expression, HashSet<ParameterExpression>) getExpression()");
			sb.AppendLine("{");
			for (int i = 0; i < c; i++)
			{
				sb.Append("var a");
				sb.Append(i + 1);
				sb.Append("=s");
				sb.Append(i + 1);
				sb.AppendLine(".getExpression();");

			}

			sb.AppendLine("var List = new HashSet<ParameterExpression>();");

			for (int i = 0; i < c; i++)
			{
				sb.Append("foreach( var pp in a");
				sb.Append(i + 1);
				sb.AppendLine(".Item2)");
				sb.AppendLine("{");
				sb.AppendLine("List.Add(pp);");
				sb.AppendLine("}");

			}
			sb.Append("return (expMaker(");
			for (int i = 0; i < c; i++)
			{
				sb.Append("(T");
				sb.Append(i + 1);
				sb.Append(")(object)a");

				sb.Append(i + 1);
				sb.Append(".Item1");
				if (i < c - 1)
				{
					sb.Append(',');
				}

			}
			sb.Append("), List);");

			sb.AppendLine("}");

			sb.AppendLine("}");

			return sb.ToString();

		}
	}
}
