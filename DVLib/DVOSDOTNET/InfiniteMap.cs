using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Images;
using MathBase;
using Bitmap =Images. bitmap;
using IntMap = Images.bitmap;

namespace DVOSLib
{
    public enum MarkType
	{
        CROSS,CROSS_45,BLOCK,BLOCK_45,DOT,CIRCLE
	}

    public static class MarkHelper
	{
        public static void drawCross(this bitmap bitmap,double size,int color,Vector2 position)
		{
            size /= 2;
            int x =(int) position.X;
            int y =(int) position.Y;
           for (int i = -(int)size; i < size; i++)
			{
                bitmap.setCheckRange((int)(position.X + i), y, color);
                bitmap.setCheckRange(x, (int)(position.Y + i), color);
            }
		}
        public static void drawCross_45(this bitmap bitmap, double size, int color, Vector2 position)
        {
            size /= 2;
            Vector2 vdx = new Vector2(1, 1).nolrmalized();
            Vector2 vdy = new Vector2(1, -1).nolrmalized();
            Vector2 vx = position - vdx * size;
            Vector2 vy = position - vdy * size;
            for (int i = -(int)size; i < size; i++)
            {
                bitmap.setCheckRange((int)vx.X, (int)vx.Y, color);
                bitmap.setCheckRange((int)vy.X, (int)vy.Y, color);
                vx += vdx;
                vy += vdy;
            }
        }
        public static void drawBlock(this bitmap bitmap, double size, int color, Vector2 position)
		{
            size /= 2;
            int maxX=(int)(position.X+size);
            int maxY=(int)(position.Y+size);
            int minX=(int)(position.X-size);
            int minY=(int)(position.Y-size);
            int x = minX;
            int y = minY;
            for (int i = -(int)size; i < size; i++)
            {
                bitmap.setCheckRange(x,minY, color);
                bitmap.setCheckRange(x,maxY, color); 
                bitmap.setCheckRange(minX, y, color);
                bitmap.setCheckRange(maxX, y, color);
                x++;
                y++;
            }

        }
        static double r2 = Math.Sqrt(2);
        static double ir2 = 1/Math.Sqrt(2);
        public static void drawBlock_45(this bitmap bitmap, double size, int color, Vector2 position)
        {
            size /= 2;
            Vector2 p0 = position.add(size, 0);
            Vector2 p1 = position.add(-size, 0);
            Vector2 p2 = position.add(0,size);
            Vector2 p3 = position.add(0, -size) ;

            Vector2 v1 = new Vector2(ir2, ir2);
            Vector2 v0 = -v1;
            Vector2 v2= new Vector2(ir2, -ir2);
            Vector2 v3 = -v2;

            int l=(int)(size*r2);

            for (int i =0; i <l; i++)
            {
                bitmap.setCheckRange((int)p0.X, (int)p0.Y, color);
                bitmap.setCheckRange((int)p1.X, (int)p1.Y, color);
                bitmap.setCheckRange((int)p2.X, (int)p2.Y, color);
                bitmap.setCheckRange((int)p3.X, (int)p3.Y, color);
                p0 += v0;
                p1 += v1;
                p2 += v2;
                p3 += v3;
            }

        }

        public static void drawCircle(this bitmap bitmap, double size, int color, Vector2 position)
		{
            double s = Math.PI * size;
            size /= 2;
            double dtheta = Math.PI * 2 / s;
            double theta = 0;
            double max = Math.PI /2;
            double d1= Math.PI /2;
            double d2 = Math.PI;
            double d3 = d1 + d2;
            while(theta<=max)
			{
                bitmap.setCheckRange((int)(position.X+  Math.Cos(theta)*size)       , (int)(position.Y + Math.Sin(theta) * size),color);
                bitmap.setCheckRange((int)(position.X + Math.Cos(theta+d1) * size), (int)(position.Y + Math.Sin(theta+d1) * size), color);
                bitmap.setCheckRange((int)(position.X + Math.Cos(theta+d2) * size), (int)(position.Y + Math.Sin(theta+d2) * size), color);
                bitmap.setCheckRange((int)(position.X + Math.Cos(theta+d3) * size), (int)(position.Y + Math.Sin(theta+d3) * size), color);



                theta += dtheta;
            }

		}

        public static void drawDot(this bitmap bitmap, double size, int color, Vector2 position)
        {
            size /= 2;
            bitmap.setCheckRange((int)position.X, (int)position.Y, color);
          for(int i = 1; i <= size; i++)
			{
                drawCircle(bitmap, i, color, position);
			}

        }
    }
    public class Mark
	{
        public Vector2 position;
        public int color;
        public string name;
        public string id;
        public double size;
        public bool selected;
        public MarkType type;

        public Mark(Vector2 pos, double size, int color, MarkType type, string name, string id)
		{
			this.position = pos;
			this.size = size;
			this.color = color;
			this.name = name;
			this.id = id;
            this.type = type;
		}

		public void select()
		{
            selected = true;
		}
        public void iSelect()
		{
            selected = false;

		}
	}
    public class InfiniteMap
    {


        static  int size = 64;

        static bitmap EmptyBitmap;

        static InfiniteMap()
        {
        EmptyBitmap = new Bitmap(size, size);
    }

 
    List<List<Bitmap>> PP = new List<List<Bitmap>>();
    List<List<Bitmap>> PN = new List<List<Bitmap>>();
        List<List<Bitmap>> NN = new List<List<Bitmap>>();
        List<List<Bitmap>> NP = new List<List<Bitmap>>();

        int[][] current;
    int currentX;
    int currentY;
        public List<List<IntMap>> getPP()
        {
            return PP;
        }
        public List<List<IntMap>> getPN()
        {
            return PN;
        }

        public List<List<IntMap>> getNN()
        {
            return NN;
        }

        public List<List<IntMap>> getNP()
        {
            return NP;
        }

        public void setNN(List<List<IntMap>> NN)
        {
            this.NN = NN;
        }

        public void setPP(List<List<IntMap>> PP)
        {
            this.PP = PP;
        }

        public void setPN(List<List<IntMap>> PN)
        {
            this.PN = PN;
        }

        public void setNP(List<List<IntMap>> NP)
        {
            this.NP = NP;
        }
        public static int[] getIndex(int x, int z)
    {
        int i = ((x + (x >> 31)) ^ (x >> 31));
        int i1 = ((z + (z >> 31)) ^ (z >> 31));
        return new int[] { i >> 6, i1 >> 6, i & 0x0000003f, i1 & 0x0000003f, (1 - ((x >> 31) << 1)), (1 - ((z >> 31) << 1)), x, z };
    }

    public int getA(int x, int y)
    {
        return (int) (getColor(x, y) & 0xff000000 >> 24);
    }

    public int getR(int x, int y)
    {
        return (getColor(x, y) & 0x00ff0000) >> 16;
    }

    public int getG(int x, int y)
    {
        return (getColor(x, y) & 0x0000ff00) >> 8;
    }

    public int getB(int x, int y)
    {
        return getColor(x, y) & 0x000000ff;
    }

    public int getRealHdxz(int x, int z)
    {
        return getA(x, z) - getA(x - 1, z);
    }

    public int getRealHdzx(int x, int z)
    {
        return getA(x, z) - getA(x, z - 1);
    }

    public int getRealHdx(int x, int z)
    {
        return getA(x, z) - getA(x - 1, z);
    }

    public int getRealHdz(int x, int z)
    {
        return getA(x, z) - getA(x, z - 1);
    }

    public int getHdu(int x, int z)
    {
        return getHdx(x, z) + getHdz(x, z) + getHdxz(x, z) + getHdzx(x, z);
    }

    public int getHdx(int x, int z)
    {
        int h = getRealHdx(x, z);
        if (h > 0)
        {
            return 32;
        }
        else if (h < 0)
        {
            return -32;
        }
        else
        {
            return 0;
        }
    }

    public int getHdz(int x, int z)
    {
        int h = getRealHdz(x, z);
        if (h > 0)
        {
            return 15;
        }
        else if (h < 0)
        {
            return -15;
        }
        else
        {
            return 0;
        }
    }

    public int getHdxz(int x, int z)
    {
        int h = getRealHdxz(x, z);
        if (h > 0)
        {
            return 7;
        }
        else if (h < 0)
        {
            return -7;
        }
        else
        {
            return 0;
        }
    }

    public int getHdzx(int x, int z)
    {
        int h = getRealHdzx(x, z);
        if (h > 0)
        {
            return 3;
        }
        else if (h < 0)
        {
            return -3;
        }
        else
        {
            return 0;
        }
    }

    public int getColor(int x, int z)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            return getColorFromIndex(PP, index);
        }
        else if (x >= 0 && z < 0)
        {
            return getColorFromIndex(PN, index);
        }
        else if (x < 0 && z < 0)
        {
            return getColorFromIndex(NN, index);
        }
        else if (x < 0 && z >= 0)
        {
            return getColorFromIndex(NP, index);
        }
        return 0;
    }

    public int getColor(int x, int z, BitmapProvider provider)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            return getColorFromIndex(PP, index, provider);
        }
        else if (x >= 0 && z < 0)
        {
            return getColorFromIndex(PN, index, provider);
        }
        else if (x < 0 && z < 0)
        {
            return getColorFromIndex(NN, index, provider);
        }
        else if (x < 0 && z >= 0)
        {
            return getColorFromIndex(NP, index, provider);
        }
        return 0;
    }

    public void reload(int x, int z, BitmapProvider provider)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            reloadFromIndex(PP, index, provider);
        }
        else if (x >= 0 && z < 0)
        {
            reloadFromIndex(PN, index, provider);
        }
        else if (x < 0 && z < 0)
        {
            reloadFromIndex(NN, index, provider);
        }
        else if (x < 0 && z >= 0)
        {
            reloadFromIndex(NP, index, provider);
        }
    }

    public void reloadFromIndex(List<List<Bitmap>> bitmap, int[] index, BitmapProvider bitmapProvider)
    {
        while (index[0] >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }

        List<Bitmap> b = bitmap[index[0]];
        while (index[1] >= b.Count)
        {

            b.Add(null);


        }
        b[index[1]]= bitmapProvider.get(index);
    }

    public int getColorFromIndex(List<List<Bitmap>> bitmap, int[] index, BitmapProvider bitmapProvider)
    {
        while (index[0] >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }

        List<Bitmap> b = bitmap[index[0]];
        while (index[1] >= b.Count)
        {

            b.Add(null);


        }

        Bitmap bitmap1 = b[index[1]];
        if (bitmap1 == null)
        {

            b[index[1]]= bitmapProvider.get(index);
            return 0;
        }
        return b[index[1]][index[2], index[3]];
    }

  

    public int getColorQuickly(int x, int z, BitmapProvider provider)
    {
        if (x >= 0 && z >= 0)
        {
            return getColorDirectly(PP, x, z, provider);
        }
        else if (x >= 0 && z < 0)
        {
            return getColorDirectly(PN, x, z, provider);
        }
        else if (x < 0 && z < 0)
        {
            return getColorDirectly(NN, x, z, provider);
        }
        else if (x < 0 && z >= 0)
        {
            return getColorDirectly(NP, x, z, provider);
        }
        return 0;
    }

    public int getColorQuickly(int x, int z)
    {


        if (x >= 0 && z >= 0)
        {
            return getColorDirectly(PP, x, z);
        }
        else if (x >= 0 && z < 0)
        {
            return getColorDirectly(PN, x, z);
        }
        else if (x < 0 && z < 0)
        {
            return getColorDirectly(NN, x, z);
        }
        else if (x < 0 && z >= 0)
        {
            return getColorDirectly(NP, x, z);
        }
        return 0;
    }

    public int getColorDirectly(List<List<Bitmap>> bitmap, int x, int z)
    {

        int x0 = ((x + (x >> 31)) ^ (x >> 31));
        int z0 = ((z + (z >> 31)) ^ (z >> 31));

        int x1 = x0 >> 6;
        int z1 = z0 >> 6;

        if (x1 >= bitmap.Count)
        {
            return 0;
        }
        List<Bitmap> b = bitmap[x1];
        if (z1 >= b.Count)
        {
            return 0;
        }
        Bitmap bb = b[z1];
        if (bb != null)
        {

            return bb[x0 & 0x0000003f, z0 & 0x0000003f];
        }


        return 0;


    }

    public int getColorDirectly(List<List<Bitmap>> bitmap, int x, int z, BitmapProvider bitmapProvider)
    {

        int x0 = ((x + (x >> 31)) ^ (x >> 31));
        int z0 = ((z + (z >> 31)) ^ (z >> 31));

        int x1 = x0 >> 6;
        int z1 = z0 >> 6;


        while (x1 >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }

        List<Bitmap> b = bitmap[x1];
        while (z1 >= b.Count)
        {

            b.Add(null);


        }

        Bitmap bitmap1 = b[z1];
        if (bitmap1 == null)
        {

            b[z1]= bitmapProvider.get(x, z);
            return 0;
        }
        else
        {
            return bitmap1[x0 & 0x0000003f, z0 & 0x0000003f];
        }


    }

    public int getColorFromIndex(List<List<Bitmap>> bitmap, int[] index)
    {
        if (index[0] >= bitmap.Count)
        {
            return 0;
        }
        List<Bitmap> b = bitmap[index[0]];
        if (index[1] >= b.Count)
        {
            return 0;
        }
        if (b[index[1]] != null)
            return b[index[1]][index[2], index[3]];

        return 0;
    }

    public void setBitmapIfAbsence(int x, int z, Bitmap provider)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            setBitmapFromIndexIfAbsence(PP, index, provider);
        }
        else if (x >= 0 && z < 0)
        {
            setBitmapFromIndexIfAbsence(PN, index, provider);
        }
        else if (x < 0 && z < 0)
        {
            setBitmapFromIndexIfAbsence(NN, index, provider);
        }
        else if (x < 0 && z >= 0)
        {
            setBitmapFromIndexIfAbsence(NP, index, provider);
        }
    }

    public void setBitmap(int x, int z, Bitmap provider)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            setBitmapFromIndex(PP, index, provider);
        }
        else if (x >= 0 && z < 0)
        {
            setBitmapFromIndex(PN, index, provider);
        }
        else if (x < 0 && z < 0)
        {
            setBitmapFromIndex(NN, index, provider);
        }
        else if (x < 0 && z >= 0)
        {
            setBitmapFromIndex(NP, index, provider);
        }
    }

     public void setData(InfiniteMap map)
		{
            this.current = map.current;
            this.currentX = map.currentX;
            this.currentY = map.currentY;
            this.NN = map.NN;
            this.PP=map.PP;
            this.NP = map.NP;
            this.PN = map.PN;
		}

    public Bitmap getBitmap(int x, int z)
    {
        int[] index = getIndex(x, z);
        if (x >= 0 && z >= 0)
        {
            return getBitmapFromIndex(PP, index);
        }
        else if (x >= 0 && z < 0)
        {
            return getBitmapFromIndex(PN, index);
        }
        else if (x < 0 && z < 0)
        {
            return getBitmapFromIndex(NN, index);
        }
        else if (x < 0 && z >= 0)
        {
            return getBitmapFromIndex(NP, index);
        }
        return null;
    }

    public Bitmap getBitmapFromIndex(List<List<Bitmap>> bitmap, int[] index)
    {
        if (index[0] < bitmap.Count)
        {
            List<Bitmap> b = bitmap[index[0]];
            if (index[1] < b.Count)
            {
                return b[index[1]];
            }
        }

        return null;
    }

    public void setBitmapFromIndexIfAbsence(List<List<Bitmap>> bitmap, int[] index, Bitmap map)
    {
        while (index[0] >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }
        List<Bitmap> b = bitmap[index[0]];
        while (index[1] >= b.Count)
        {
            b.Add(null);
        }

        if (b[index[1]] == null)
            b[index[1]]= map;
    }

    public void setBitmapFromIndex(List<List<Bitmap>> bitmap, int[] index, Bitmap map)
    {
        while (index[0] >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }
        List<Bitmap> b = bitmap[index[0]];
        while (index[1] >= b.Count)
        {
            b.Add(null);
        }

        b[index[1]]= map;
    }

    public void setColorFromIndex(List<List<Bitmap>> bitmap, int[] index, int value)
    {
        while (index[0] >= bitmap.Count)
        {
            bitmap.Add(new List<Bitmap>());
        }
            List<Bitmap> b = bitmap [index[0]];
        while (index[1] > b.Count)
        {
            b.Add(new Bitmap(size, size));
        }
        b[index[1]][index[2], index[3]]= value;
    }

    public interface BitmapProvider
    {
        Bitmap get(int[] index);

        Bitmap get(int x, int z);
    }



}

    public class InfiniteMapDisplayer
    {
        static  float mind = 0.01f;
        static  float maxd = 1000f;
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 scanPosition = new Vector3(0, 0, 0);
        public float d = 1;
        public int size = 128;
        RenderMode last = RenderMode.ColorAndEdge;
        Bitmap colorInfo=new Bitmap(128,128);
        Bitmap outPut = new Bitmap(128, 128);
        Bitmap HeightInfo = new Bitmap(128, 128);
        //List<Point> Marks = new List<>();
        public double angle =0;
        RenderMode mode = RenderMode.ColorAndEdge;
        float scanD = 1;
        InfiniteMap Map;
        public InfiniteMapDisplayer(InfiniteMap map)
        {
            Map = map;
        }
        public InfiniteMapDisplayer()
        {
        }

        public void setDateSource(InfiniteMap map)
		{
            this.Map = map;
		}
        public Bitmap getOutPut()
        {
            return outPut;
        }
        public void changeRenderMode()
        {
            int i = (int)mode;
            i++;
            if (i >= RenderMode.GetValues(RenderMode.Color.GetType()).Length)
            {
                i = 0;
            }
            mode = (RenderMode)i;
        }

        public int getHeihgtFromWorldPos(float x, float y)
        {
           
            if (Map != null)
            {
                return Map.getColor((int)x, (int)y) >> 24;
            }
            else return 0;
        }

        public InfiniteMapDisplayer setPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public InfiniteMapDisplayer setSize(int size)
        {
            this.size = size;
            return this;
        }

        public InfiniteMapDisplayer setperiod(float t)
        {
            d = t;
            return this;
        }

        public Vector3i getBlockPos(double x, double z)
        {

            Vector2 v = new Vector2(x * d - size / 2f * d, z * d - size / 2f * d).row( angle);

            double v1 = v.X;
            double v2 = v.Y;
            if (position.x + v1 < 0)
            {
                v1++;
            }
            if (position.z + v2 < 0)
            {
                v2++;
            }

            Vector3i p = new Vector3i(position.x + v1, 0, position.z + v2);
            return p.add(0, getHeihgtFromWorldPos(p.x, p.z), 0);
        }

        public Vector2 getScreenPos(double x, double z)
        {
            if (x < 0)
                x--;
            if (z < 0)
                z--;
            Vector2 v = new Vector2((x - position.x) / d, (z - position.z) / d).row( -angle);
            double x0 = v.X + size / 2;
            double z0 = v.Y + size / 2;


            return new Vector2 ( x0, z0 );
        }

        public Bitmap getColorInfo()
        {
            if (colorInfo != null)
                return colorInfo;
            return new Bitmap(size, size);
        }
        /*
        public void sendToClient(boolean notice)
        {
            WorldBitmap data = WorldManager.world.getDataFromUUID(player.getUniqueID());
            BlockPos zore = new BlockPos(position.add(-size / 2 * d, 0, -size / 2 * d));
            BlockPos one = new BlockPos(position.add(size / 2 * d + WorldBitmap.size, 0, size / 2 * d + WorldBitmap.size));
            int x0 = zore.getX();
            int z0 = zore.getZ();
            int x1 = one.getX();
            int z1 = one.getZ();
            for (int i = x0; i <= x1; i += WorldBitmap.size)
            {
                for (int j = z0; j <= z1; j += WorldBitmap.size)
                {
                    Bitmap b = data.getBitmap(i, j);
                    if (b != null)
                    {
                        Networking.sendToOne(player, new BitmapPack(player, b, new BlockPos(i, 0, j)));
                    }
                }
            }
            if (notice)
            {
                Networking.sendToOne(player, new MapUpdatePack());
            }
        }
        */


        /*
        public void drawMark(Point p, float factor)
        {

            float[] vs = getScreenPos(p.x, p.y);
            float x = vs[0] * factor;
            float y = vs[1] * factor;
            if (x < 0 || x > outPut.width || y < 0 || y > outPut.height)
            {
                return;
            }

            if (outPut != null)
            {

                if (p.addintionalInfo instanceof String) {

                    outPut.add(x + 1, y + 1, p.color);
                    outPut.add(x - 1, y - 1, p.color);
                    outPut.add(x - 1, y + 1, p.color);
                    outPut.add(x + 1, y - 1, p.color);
                    outPut.add(x + 2, y, p.color);
                    outPut.add(x - 2, y, p.color);
                    outPut.add(x, y + 2, p.color);
                    outPut.add(x, y - 2, p.color);
                    for (int i = 1; i < 5; i++)
                    {
                        outPut.add(x, y + i, p.color);
                        outPut.add(x, y - i, p.color);
                        outPut.add(x - i, y, p.color);
                        outPut.add(x + i, y, p.color);
                    }
                } else if (p.addintionalInfo instanceof Integer) {
                    int n = (Integer)p.addintionalInfo;
                    outPut.add(x, y, p.color);

                    for (int i = 1; i < n; i++)
                    {
                        outPut.add(x + i, y + i, p.color);
                        outPut.add(x - i, y - i, p.color);
                        outPut.add(x - i, y + i, p.color);
                        outPut.add(x + i, y - i, p.color);
                    }

                } else if (p.addintionalInfo instanceof Double) {
                    double n = (Double)p.addintionalInfo;
                    outPut.add(x, y, p.color);

                    for (int i = 1; i < n; i++)
                    {
                        outPut.add(x + i, y, p.color);
                        outPut.add(x - i, y, p.color);
                        outPut.add(x, y + i, p.color);
                        outPut.add(x, y - i, p.color);
                    }

                } else if (p.addintionalInfo instanceof Float) {
                    int n = (int)(float)p.addintionalInfo;
                    outPut.add(x, y, p.color);

                    for (int i = 1; i < n * 2; i++)
                    {
                        outPut.add(x - n + i, y + n, p.color);
                        outPut.add(x - n + i, y - n, p.color);
                        outPut.add(x + n, y - n + i, p.color);
                        outPut.add(x - n, y - n + i, p.color);
                    }

                } else if (p.addintionalInfo instanceof Byte) {
                    int n = (int)(byte)p.addintionalInfo;
                    outPut.add(x, y, p.color);

                    double m = 2 * n;
                    int r = n / 2;
                    for (int i = 1; i < n; i++)
                    {
                        outPut.add(x - r + i, y + r, 0xff00ffff);
                        outPut.add(x - r + i, y - r, 0xff00ffff);
                        outPut.add(x + r, y - r + i, 0xff00ffff);
                        outPut.add(x - r, y - r + i, 0xff00ffff);
                    }
                    for (int i = 1; i < m; i++)
                    {
                        outPut.add(x - n + i, y + n, p.color);
                        outPut.add(x - n + i, y - n, p.color);
                        outPut.add(x + n, y - n + i, p.color);
                        outPut.add(x - n, y - n + i, p.color);
                    }

                } else if (p.addintionalInfo instanceof UUID) {
                    float f = 2;

                    for (float x0 = -f; x0 <= f; x0++)
                    {
                        for (float y0 = -f; y0 <= f; y0++)
                        {
                            if (x0 * x0 + y0 * y0 <= f * f)
                            {
                                outPut.add(x + x0, y + y0, p.color);
                            }

                        }
                    }

                } else
                {
                    outPut.add(x, y, p.color);
                    outPut.add(x + 1, y + 1, p.color);
                    outPut.add(x - 1, y - 1, p.color);
                    outPut.add(x - 1, y + 1, p.color);
                    outPut.add(x + 1, y - 1, p.color);
                    outPut.add(x + 2, y + 2, p.color);
                    outPut.add(x - 2, y - 2, p.color);
                    outPut.add(x - 2, y + 2, p.color);
                    outPut.add(x + 2, y - 2, p.color);
                }
            }
        }
        */

        /*
        public void scan0()
        {

            if (d > maxd)
            {
                d = maxd;
            }
            if (d < mind)
            {
                d = mind;
            }


            WorldBitmap data = WorldManager.world.getDataFromUUID(player.getUniqueID());
            if (player.world.isRemote)
            {
                data = WorldBitmap.client;
            }


            float r = size * d / 2;

            int i = 0;
            int j = 0;
            for (float x = -r; i < size; x += 16)
            {
                j = 0;
                for (float z = -r; j < size; z += 16)
                {
                    if (data != null)
                    {
                        data.getColor((int)(position.getX() + x), (int)(position.getZ() + z), new WorldBitmap.WorldScanner(player));

                    }


                    j++;
                }
                i++;

            }
        }
        *//*
        public void updateMarks()
        {
            Marks.clear();
            List<LivingEntity> l = player.world.getEntitiesWithinAABB(LivingEntity.class, player.getBoundingBox().grow(128, 128, 128), new Predicate<LivingEntity>() {
            @Override
            public boolean test(LivingEntity livingEntity)
        {
            UUID uuid = player.getHeldItemOffhand().getOrCreateTag().contains("cannon") ?
                    player.getHeldItemOffhand().getOrCreateTag().getUniqueId("cannon") : null;
            return livingEntity != player && (!(livingEntity instanceof DavidCannon) || (!livingEntity.getUniqueID().equals(uuid) && ((DavidCannon)livingEntity).getOwner() == player));
        }
    });
        for (LivingEntity le : l
        ) {

            if (le instanceof DavidCannon) {
                Point p = new Point((float)le.getPosX(), (float)le.getPosZ(), 0xff008f00).setName(le.getName().getString() + "[" + (int)le.getPosX() + "." + (int)le.getPosY() + "." + (int)le.getPosZ() + "]");
    p.addintionalInfo = le.getUniqueID();
                Marks.add(p);
            } else
{

    int y = getHeihgtFromWorldPos((float)le.getPosX(), (float)le.getPosZ());

    Point p = new Point((float)le.getPosX(), (float)le.getPosZ(), le.getPosY() > y ? (mode == RenderMode.Height ? 0xffffffff : 0xffffff00) : (mode == RenderMode.Height ? 0xff8f8f8f : 0xff3f3f00)).setName(le.getName().getString() + "[" + (int)le.getPosX() + "." + (int)le.getPosY() + "." + (int)le.getPosZ() + "]");
    p.addintionalInfo = le.getUniqueID();
    Marks.add(p);
}

        }
    }
        */


public void scan( bool forceUpdate,Vector3 lookVec)
{
            if(Map==null)
			{
                return;
			}

    if (forceUpdate || !position.Equals(scanPosition) || scanD != d || last != mode)
    {
        scanD = d;
        scanPosition = position;
        if (d > maxd)
        {
            d = maxd;
        }
        if (d < mind)
        {
            d = mind;
        }

        Vector3 dir =lookVec;
        dir = new Vector3(dir.x, 0, dir.z).nolrmalized();
        Vector2 di = new Vector2(dir.x, dir.z);
        angle = di.angle() + 90;
        InfiniteMap data = Map;

        HeightInfo = new Bitmap((int)(size ), (int)(size ));
        colorInfo = new Bitmap((int)(size ), (int)(size ));

        float r = size * d / 2;

        /* Vector2f dirr = new Vector2f(x, z);
         dirr = MathHelper.row(dirr, angle);*/

        Vector2 dix = new Vector2(1, 0);
        dix = dix.row( angle);
        double xdx = dix.X;
        double xdy = dix.Y;

        Vector2 diz = new Vector2(0, 1);
        diz = diz.row(angle);
        double zdx = diz.X;
        double zdy = diz.Y;

        int i = 0;
        int j = 0;
        for (float x = -r; i < (int)(size ); x += d  )
        {
            j = 0;
            for (float z = -r; j < (int)(size ); z += d)
            {



                /*Vector2f dirr = new Vector2f(x, z);
               dirr = MathHelper.row(dirr, angle);*/

               
                {
                    int px = (int)(position.x+ x * xdx + z * zdx);
                    int py = (int)(position.z + x * xdy + z * zdy);
                    int v = Map.getColorQuickly(px, py);
                    int c;
                    switch (mode)
                    {
                        case RenderMode. ColorAndEdge:
                            c = MathHelper.mixColor((int)(0xff000000 | v), data.getHdx(px, py) + data.getHdz(px, py));
                            break;
                        case RenderMode.ColorAndHeight:
                            c =(int)( 0xff000000 | MathHelper.mixColor(v));
                            break;
                        case RenderMode.Height:
                            c = v >> 24;
                            c = (int)((c << 8) | (c << 16) | (c) | 0xff000000);
                            break;
                        case RenderMode.Color:
                            c =(int)( v | 0xff000000);
                            break;
                        default:
                            c =(int)( v | 0xff000000);
                            break;
                    }

                    colorInfo[i, j]= c;
                    HeightInfo[i, j]= v >> 24;
                }


                j++;
            }
            i++;

        }
    }

    outPut = colorInfo.Clone();
    last = mode;

    }


    public enum RenderMode
{
    Color, ColorAndHeight, ColorAndEdge, Height
}


}


}
