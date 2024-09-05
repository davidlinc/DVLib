using System.Collections;
using System.Collections.Generic;
using Images;
using System.IO;
using MathBase;
using System;



public class pointgroup
	{
	public pointgroup(Vector2[] vectors)
	{

		points = new List<Vector2>();
		foreach(Vector2 vector2 in vectors)
		{
			points.Add(
				vector2);
		}
	}
	List<Vector2> points;
		public void set(int index, Vector2 vector2)
		{
			points[index] = vector2;
		}
		public pointgroup()
		{

			points = new List<Vector2>();
		}
	
		public pointgroup(Vector2 p0, params Vector2[] vectors)
		{
			points = new List<Vector2>();
			points.Add(p0);
			foreach (Vector2 vector2 in vectors)
			{
				points.Add(vector2);
			}
		}
		public pointgroup clone()
		{
			pointgroup p = new pointgroup();
			foreach (Vector2 vector2 in points)
				p.Add(vector2);
			return p;
		}
		public void Add(Vector2 vector)
		{
			points.Add(vector);
		}
		public Vector2[] toArray()
		{
			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = points[i];
			}
			return result;
		}
		public Vector2[] toArray(Vector2 P0)
		{
			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = points[i]+P0;
			}
			return result;
		}
		public Vector2[] toArray(Vector2 p1, Vector2 p2)
		{
			Vector2 P1 = points[0];
			Vector2 P2 = points[points.Count - 1];
			double k = (p2 - p1).value() / (P2 - P1).value();
			double angle = (p2 - p1).angle() - (P2 - P1).angle();
			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{

				result[i] = p1 + ((points[i] - P1).row(angle) * k);
			}
			return result;
		}
		public Vector2[] toArray(Vector2 P1, Vector2 P2, Vector2 p1, Vector2 p2)
		{

			double k = (p2 - p1).value()/ (P2 - P1).value();
			double angle = (p2 - p1).value() - (P2 - P1).value();
			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = p1 + (points[i] - P1).row(-angle) * k;
			}
			return result;
		}
		public Vector2[] toArray(Vector2 P1, float k, float angle)
		{

			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = P1 + (points[i] - P1).row(angle) * k;
			}
			return result;
		}
		public Vector2[] toArray(Vector2 P0,Vector2 P1, float k, float angle)
		{

			Vector2[] result = new Vector2[points.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = P0+P1 + (points[i] - P1).row(angle) * k;
			}
			return result;
		}
	}
public class colorobject
{
	bool usingedge = false;
	public pointgroup Pointgroup { private set; get; }
	public int color { private set; get; }
	public Vector2 position;
	public float size;
	public float angle;
	public void setUsingedge(bool b)
	{
		usingedge = b;
	}
	public colorobject(pointgroup points, int color, Vector2 position, float size, float angle)
	{
		Pointgroup = points;
		this.color = color;
		this.position = position;
		this.size = size;
		this.angle = angle;
	}
	public colorobject(pointgroup points, int color)
	{
		Pointgroup = points;
		this.color = color;
		this.position = new Vector2(0,0);
		this.size = 1;
		this.angle = 0;
	}
	public void setcolor(int c)
	{
		color = c;
	}
	public void set(Vector2 pos, float size, float angle)
	{
		position = pos; this.angle = angle; this.size = size;
	}
	public void setPosition(Vector2 pos)
	{
		position = pos;
	}
	public void setAngle(float angle)
	{
		this.angle = angle;
	}
	public void setsize(float size)
	{
		this.size = size;
	}
	public pointgroup output(Vector2 pos, float size, float angle)
	{
		pointgroup temp = new pointgroup(Pointgroup.toArray(new Vector2(0, 0), new Vector2(0, 0) - position, 1, angle));

		temp = new pointgroup(temp.toArray(pos + position * size, new Vector2(0, 0), size * this.size, this.angle));
		return temp;

	}
	public colorobject clone()
	{
		colorobject colorobject = new colorobject(Pointgroup.clone(), color, position, size, angle);
		colorobject.setUsingedge(usingedge);
		return colorobject;
	}
	public void draw(bitmap bitmap, Vector2 pos, float size, float angle)
	{
		if (usingedge)
		{
			drawWITHedge(bitmap, pos, size, angle);
		}
		else
		{
			Picture.paintarea1(bitmap, color, output(pos, size, angle).toArray());
		}

	}
	public void drawWITHedge(bitmap bitmap, Vector2 pos, float size, float angle)
	{

		Picture.paintareaWITHedge(bitmap, color, Colors.Black, output(pos, size, angle).toArray());
	}
}
public class imageObject
{
	List<colorobject> colorobjects = new List<colorobject>();
	public void row(float angle)
	{
		for (int i = 0; i < colorobjects.Count; i++)
		{
			colorobjects[i].angle += angle;
		}
	}

	public void move(Vector2 vector2)
	{
		for (int i = 0; i < colorobjects.Count; i++)
		{
			colorobjects[i].position += vector2;
		}
	}
	public imageObject(List<colorobject> colorobjects)
	{
		this.colorobjects = colorobjects;
	}
	public imageObject()
	{

	}
	public void add(colorobject colorobject)
	{
		colorobjects.Add(colorobject);
	}
	public void add(imageObject imageO)
	{
		foreach (colorobject co in imageO.colorobjects)
		{
			colorobjects.Add(co);
		}

	}
	public imageObject clone()
	{
		imageObject image = new imageObject();
		foreach (colorobject co in colorobjects)
		{
			image.add(co.clone());
		}
		return image;
	}
	public void clean()
	{
		colorobjects.Clear();
	}
	public void draw(bitmap bitmap, Vector2 pos, float size, float angle)
	{
		foreach (colorobject colorobject in colorobjects)
		{
			colorobject.draw(bitmap, pos, size, angle);
		}
	}
}

