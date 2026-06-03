using System.Collections;
using System.Collections.Generic;
using Images;
using MathBase;
using DVOSLib;
using MachineLearning;

public class Cloth
{
    public float height_a { get; private set; }
    public float height_b { get; private set; }

    public float length_a { get; private set; }
    public int color { get; private set; }
    public string code()
    {
       var color_= color.Int2RGB();
        return height_a + "*" + height_b + "*" + length_a + "*" + color_.r/255f + "*" + color_.g/255f+ "*" + color_.b/255f;
    }

    public static Cloth fromCode(string code, int start = 0)
    {
        List<float> vs = code.readNum();
        return new Cloth(vs[start], vs[start + 1], vs[start + 2], ((int)(vs[start + 3]*255), (int)(vs[start + 4]*255), (int)(vs[start + 5]*255)).RGB2Int());
    }

    public Cloth (float a,float b ,float la,int c)
	{
        height_a = a;height_b = b;color = c;length_a = la;
	}
    public Cloth ()
	{
        height_a = 2.3f; height_b = 0.9f; color =Colors.Gray;length_a = 0.3f;
    }

    public static Cloth generateFemale(DvRandom random)
    {
        float ha = random.Nextfloat(0.5f, 2.2f);
        float hb = random.Nextfloat(0.1f, 0.95f);
        float la= random.Nextfloat(0.01f, 0.5f);
        int c = random.NextColor();
        return new Cloth(ha, hb, la, c);
    }
    public static Cloth generateMale(DvRandom random)
    {
        float ha = random.Nextfloat(1.5f, 2.3f);
        float hb = random.Nextfloat(0.1f, 0.95f);
        float la = random.Nextfloat(0.01f, 0.5f);
        int c = random.NextColor();
        return new Cloth(ha, hb, la, c);
    }
}
public class Hair
{
    public float height_hairline { get; private set; }
    public float width_hairline { get; private set; }
    public float height_temples { get; private set; }
    public float size_hair { get; private set; }
    public float size_gap { get; private set; }
    public float deviation_gap { get; private set; }
    public float length_hair { get; private set; }
    public int color { get; private set; }
    public string code()
    {
        var color = this.color.Int2RGB();
        return height_hairline + "*" + width_hairline + "*" + height_temples + "*" + size_hair + "*" + size_gap + "*" + deviation_gap + "*" + length_hair + "*" +color.r/255f + "*" +color.g/255f + "*" +color.b/255f;
    }

    public static Hair fromCode(string code, int start = 0)
    {
        List<float> vs = code.readNum();
        return new Hair(vs[start], vs[start+1], vs[start+2], vs[start+3], vs[start+4], vs[start+5],((int)(vs[start+7]*255), (int)(vs[start+8]*255), (int )(vs[start+9]*255)).RGB2Int(), vs[start+6]);
    }
    public Hair(float hh,float wh,float ht,float sh,float sg ,float dg,int color,float bh)

    {
        height_hairline = hh;
        width_hairline = wh;
        height_temples = ht;
        size_hair = sh;
        size_gap = sg;
        deviation_gap = dg;
        this.color = color;
        length_hair = bh;
    }
    public Hair()
    {
        height_hairline = 0.5f;
        width_hairline = 0.8f;
        height_temples = 0.2f;
        size_hair = 0.5f;
        size_gap = 0.3f;
        deviation_gap =- 0.5f;
        length_hair = 0.1f;
        this.color = 0;
    }
    public void setBackHair(float bh )
	{
        length_hair = bh;
	}
    public static Hair generateFemale(DvRandom random)
    {
        float hh = random.Nextfloat(0.1f,0.9f);
        float wh = random.Nextfloat(0.1f,0.9f);
        float ht = random.Nextfloat(-1f,0.5f);
        float sh = random.Nextfloat(0.3f,1.2f);
        float sg = random.Nextfloat(0.01f,1f);
        float dg = random.Nextfloat(-1f,1f);
        float lh = random.Nextfloat(0.5f,1.5f);
        int c = random.NextColor();
        return new Hair(hh, wh, ht, sh, sg, dg, c, lh);
    }
    public static Hair generateMale(DvRandom random)
    {
        float hh = random.Nextfloat(0.1f, 0.9f);
        float wh = random.Nextfloat(0.1f, 0.9f);
        float ht = random.Nextfloat(-0.1f, 0.9f);
        float sh = random.Nextfloat(0.3f, 0.7f);
        float sg = random.Nextfloat(0.01f, 1f);
        float dg = random.Nextfloat(-1f, 1f);
        float lh = random.Nextfloat(0.01f, 0.2f);
       int c = random.NextColor();
        return new Hair(hh, wh, ht, sh, sg, dg, c, lh);
    }

}
public class Torso
{
    public float width_neck { get; private set; }
    public float width_shoulder { get; private set; }
    public float height_shoulder { get; private set; }
    public float width_arm { get; private set; }
    public int left { get; private set; }
    public int right { get; private set; }
    public string code()
    {
        var left = this.left.Int2RGB();
        return width_neck + "*" + width_shoulder + "*" + height_shoulder + "*" + width_arm + "*" + left.r/255f + "*" + left.g/255f + "*" + left.b/255f;
    }

    public static Torso fromCode(string code, int start = 0)
    {
        List<float> vs = code.readNum();
        return new Torso(vs[start], vs[start+1], vs[start+2], vs[start+3], ((int)(vs[start+4]*255), (int)(vs[start+5]*255),( (int)(vs[start+6]*255))).RGB2Int());
  }

    public Torso(float wn,float ws,float wa,float hs,int color)
	{
        width_arm = wa;
        width_neck = wn;
        width_shoulder = ws;
        right = color;
        height_shoulder = hs;
        left = color.similarColor(20);
	}
    public Torso()
    {
        width_arm = 0.6f;
        width_neck = 0.5f;
        width_shoulder =0.6f;
        height_shoulder = 0.5f;
        right = (229, 220, 202).RGB2Int();
        left = right.similarColor(20);
    }
    public static Torso generateFemale(DvRandom random)
    {
        float wa = random.Nextfloat(0.5f, 0.6f);
        float wn = random.Nextfloat(0.4f, 0.5f);
        float ws = random.Nextfloat(0.5f, 0.6f);
        float hs = random.Nextfloat(0.4f, 0.5f);
        int c = (229 , 220 , 202).RGB2Int().similarColor(random.Next(0,60));
        return new Torso(wn, ws, wa, hs, c);
    }
    public static Torso generateMale(DvRandom random)
    {
        float wa = random.Nextfloat(0.5f, 0.7f);
        float wn = random.Nextfloat(0.45f, 0.55f);
        float ws = random.Nextfloat(0.5f, 0.7f);
        float hs = random.Nextfloat(0.45f, 0.6f);
        int c = (229, 220, 202).RGB2Int().similarColor(random.Next(0, 60));
        return new Torso(wn, ws, wa, hs,c);
    }
}

public class Face
{
    float height_top;
    float height_cheek;
    float height_gill;
    float width_jaw;
    float width_top;
    float width_cheek;
    float width_gill;
    float width_eye;
    float height_eye;
    float x_eye;
    float y_eye;
   int leftface;
   int rightface;
   int eye;
    public string code()
	{
        var leftface = this.leftface.Int2RGB();
        var eye = this.eye.Int2RGB();
        return height_top + "*" + height_cheek + "*" + height_gill + "*" + width_jaw + "*" + width_top + "*" + width_cheek + "*" + width_gill + "*" + width_eye + "*" + height_eye + "*" + x_eye + "*" + y_eye + "*" + leftface.r /255f+ "*" + leftface.g /255f+ "*" + leftface.b/255f + "*" + eye.r/255f + "*" + eye.g/255f + "*" + eye.b/255f;
     }

    public static Face fromCode(string code,int start=0)
	{
        List<float> vs = code.readNum();
        return new Face(vs[start], vs[start + 1], vs[start + 2], vs[start + 3], vs[start + 4], vs[start + 5], vs[start + 6], vs[start + 7], vs[start + 8], vs[start + 9], vs[start + 10],((int)(vs[start + 11]*255f),(int)( vs[start + 12]*255f), (int)(vs[start + 13] * 255f)).RGB2Int(), ((int)(vs[start + 14] * 255f), (int)(vs[start + 15] * 255f), (int)(vs[start + 16] * 255f)).RGB2Int());
	}

    public Face(float ht,float hc,float hg,float wj,float wt,float wc,float wg,float we,float he,float xe,float ye,int color,int eye)
    {
        height_top = ht;
        height_cheek = hc;
        height_gill = hg;
        width_jaw = wj;
        width_top = wt;
        width_cheek =wc;
        width_gill = wg;
        width_eye = we;
        height_eye = he;
        x_eye = xe;
        y_eye = ye;
        
        leftface = color;
        rightface = color.similarColor(20);
       this.eye = eye;



    }
    public Face()
	{
        height_top = 1f;
        height_cheek = 0.7f;
        height_gill = 0.2f;
        width_jaw = 0.15f;
        width_top = 0.3f;
        width_cheek = 0.75f;
        width_gill = 0.75f;
        width_eye = 0.02f;
        height_eye = 0.05f;
        x_eye = 0.15f;
        y_eye = 0;
        leftface =(249,236,228).RGB2Int();
        rightface = (249 , 236 , 228 ).RGB2Int().similarColor(22);
        eye = Colors.Black;



	}
    public static Face generateFemale(DvRandom random)
    {
        float ht = random.Nextfloat(0.9F, 1.2f);
        float hc = random.Nextfloat(0.7F, 0.9f);
        float hg = random.Nextfloat(0.15F, 0.45f);
        float wj = random.Nextfloat(0.01F, 0.25f);
        float wt = random.Nextfloat(0.1F, 0.4f);
        float wc = random.Nextfloat(0.6F, 0.7f);
        float wg = wc;
        float we = random.Nextfloat(0.015F, 0.07f);
        float he = random.Nextfloat(0.015F, 0.07f);
        float xe = random.Nextfloat(0.05F, 0.18f);
        float ye = random.Nextfloat(0.08F, 0.20f);
        int c1 = (249 , 236 , 228 ).RGB2Int().similarColor(random.Next(0,60));
       int c2 = Colors.Black.similarColor(random.Next(0, 60));
        return new Face(ht, hc, hg, wj, wt, wc, wg, we, he, xe, ye, c1, c2);
    }
    public static Face generateMale(DvRandom random)
    {
        float ht = random.Nextfloat(0.8F,1.1f);
        float hc = random.Nextfloat(0.6F, 0.8f);
        float hg = random.Nextfloat(0.1F, 0.4f);
        float wj = random.Nextfloat(0.01F, 0.3f);
        float wt = random.Nextfloat(0.1F, 0.5f);
        float wc = random.Nextfloat(0.6F, 0.9f);
        float wg =wc;
        float we = random.Nextfloat(0.015F, 0.07f);
        float he = random.Nextfloat(0.015F, 0.07f);
        float xe = random.Nextfloat(0.05F, 0.18f);
        float ye = random.Nextfloat(0.08F, 0.20f);
       int c1 = (249 , 236 , 228).RGB2Int().similarColor(random.Next(0,80));
     int c2 =0.similarColor(random.Next(0, 80));
        return new Face(ht, hc, hg, wj, wt, wc, wg, we, he, xe, ye, c1, c2);
    }
    public imageObject backHair(Hair hair)
	{
        Vector2 lcheek = new Vector2(-0.5f * width_cheek, height_cheek);
        Vector2 rcheek = new Vector2(-lcheek. X, lcheek. Y);
        Vector2 lgill = new Vector2(-0.5f * width_gill, height_gill);
        Vector2 lt = lgill + (lcheek - lgill) * hair.height_temples;
        if (hair.height_temples < 0)
        {
            lt = lgill + new Vector2(0, -(lt - lgill).value());
        }
        imageObject imageObject = new imageObject();
        float y = (-0.05f - height_gill) * hair.length_hair+height_gill;
        imageObject.add(new colorobject(new pointgroup(new Vector2(-0.25f * (width_gill+width_cheek), y),new Vector2(0.25f * (width_gill + width_cheek), y),rcheek,lcheek), hair.color.similarColor(22), new Vector2(0, 0), 1, 0));
        return imageObject;
    }

    public imageObject getMaskImage(int color)
	{
       Vector2 Lcheek = new Vector2(-0.5f * width_cheek, height_cheek);
        Vector2 Rcheek = new Vector2(0.5f * width_cheek, height_cheek);
        Vector2 Lgill = new Vector2(-0.5f * width_gill, height_gill);
        Vector2 Rgill = new Vector2(0.5f * width_gill, height_gill);
        Vector2 pL = Lcheek + Lgill;
        pL /= 2;
        Vector2 pR = Rcheek + Rgill;
        pR /= 2;

        Vector2 Lj = 0.75 * (Vector2)LeftJaw + 0.25 * Lgill;
        Vector2 Rj = 0.75 * (Vector2)RightJaw + 0.25 * Rgill;

        imageObject imageObject = new imageObject();
        imageObject.add(new colorobject(new pointgroup(pL,pR,Rgill,Rj,Lj,Lgill),color));
        return imageObject;
    }
    public imageObject hairImage(Hair hair)
    {
        
        Vector2 ltop = new Vector2(-0.5f * width_top, height_top);
        Vector2 rtop = new Vector2(0.5f * width_top, height_top);
        Vector2 lcheek = new Vector2(-0.5f * width_cheek, height_cheek);
        Vector2 rcheek = new Vector2(0.5f * width_cheek, height_cheek);
        Vector2 lgill = new Vector2(-0.5f * width_gill, height_gill);
        double hly = lcheek.Y + (ltop.Y - lcheek.Y) * hair.height_hairline;
        double width_max_half = (ltop.X - lcheek.X) / (ltop.X - lcheek.X) * (ltop.X - lcheek.X) * (1 - hair.height_hairline) + 0.5f * width_top;
        double hlx = width_max_half * hair.width_hairline;
        Vector2 lhairline = new Vector2(-hlx, hly);
        Vector2 lb = lhairline + (lcheek - ltop) / (ltop.Y - lcheek.Y) * (lhairline.Y - lcheek.Y);
        Vector2 lt = lgill + (lcheek - lgill) * hair.height_temples;
        if(hair.height_temples<0)
		{
            lt =new Vector2(lgill.X/2+lcheek.X/2,lgill.Y)+new Vector2(0, -(lt-lgill).value());
		}
        Line2d linea = new Line2d(ltop + new Vector2(0, hair.size_hair*0.15f), rtop + new Vector2(0, hair.size_hair*0.15f));
        Vector2 d = (ltop - lcheek).row(90).normalized()*hair.size_hair*0.15f;
        Line2d lineb =new Line2d(ltop + d, lcheek + d);
        Line2d linec =new  Line2d(rcheek, lcheek);
        Vector2 ld = linec.crosspoint(lineb);
        Vector2 le = lineb.crosspoint(linea);
       double gapw = -le.X * (1 - hair.size_gap);
       double gapy = le.Y - (le.Y - height_top) * hair.size_gap;
       double gapxl = le.X * hair.size_gap + gapw * hair.deviation_gap;
       double gapxr = gapxl + -le.X * hair.size_gap * 2;
        Vector2 lf = new Vector2(gapxl, le.Y);
        Vector2 rf = new Vector2(gapxr, le.Y);
        Vector2 lg = new Vector2((gapxl + gapxr) / 2, gapy);
        Vector2 lh = new Vector2(lg.X, lhairline.Y);
        Vector2 lde = (ld + le) / 2 + d;
        Vector2 rhairline = new Vector2(-lhairline.X, lhairline.Y);
        Vector2 rb = new Vector2(-lb.X, lb.Y);
        Vector2 rt = new Vector2(-lt.X, lt.Y);
        Vector2 rd = new Vector2(-ld.X, ld.Y);
        Vector2 re = new Vector2(-le.X, le.Y);
        Vector2 rde = new Vector2(-lde.X, lde.Y);
        Vector2 rg = lg;
        Vector2 rh = lh;
        colorobject l1 = new colorobject(new pointgroup(le,lf,lg,lh,lhairline), hair.color, new Vector2(0, 0), 1, 0);
        colorobject l2 = new colorobject(new pointgroup(lb,lde,le,lhairline), hair.color, new Vector2(0, 0), 1, 0);
        colorobject l3 = new colorobject(new pointgroup(lde,lb,lt,ld), hair.color, new Vector2(0, 0), 1, 0);
        colorobject r1 = new colorobject(new pointgroup(re, rf, rg, rh, rhairline), hair.color, new Vector2(0, 0), 1, 0);
        colorobject r2 = new colorobject(new pointgroup(rb, rde, re, rhairline), hair.color, new Vector2(0, 0), 1, 0);
        colorobject r3 = new colorobject(new pointgroup(rde, rb, rt, rd), hair.color, new Vector2(0, 0), 1, 0);
        imageObject image = new imageObject();
      
        image.add(l1);
        image.add(l2);
        image.add(l3);
        image.add(r1);
        image.add(r2);
        image.add(r3);
        return image;

    }
    public imageObject torsoImage(Torso body)
	{
        float maxwidth =width_gill+width_cheek;
        maxwidth /= 2;
        Vector2 lb = new Vector2(-maxwidth * 0.5f * body.width_neck, height_gill);
        Vector2 la = new Vector2(0, lb.Y);
        Vector2 lc = new Vector2(lb.X, -0.05f);
        Vector2 le = new Vector2(-maxwidth *1.2f* body.width_arm+lc.X, -0.5f);
        Vector2 ld = new Vector2(lc.X + (le.X - lc.X) * body.width_shoulder, -0.3f + 0.24f * body.height_shoulder);
        Vector2 lf = new Vector2(0, -0.5f);
        Vector2 ra = new Vector2(-la.X, la.Y);
        Vector2 rb = new Vector2(-lb.X, lb.Y);
        Vector2 rc = new Vector2(-lc.X, lc.Y);
        Vector2 rd = new Vector2(-ld.X, ld.Y);
        Vector2 re = new Vector2(-le.X, le.Y);
        Vector2 rf = lf;
        colorobject l1 = new colorobject(new pointgroup(la,lb,lc,lf), body.left, new Vector2(0, 0), 1, 0);
        colorobject l2 = new colorobject(new pointgroup(lc, ld, le, lf), body.left, new Vector2(0, 0), 1, 0);
        colorobject r1 = new colorobject(new pointgroup(ra, rb, rc, rf), body.right, new Vector2(0, 0), 1, 0);
        colorobject r2 = new colorobject(new pointgroup(rc, rd, re, rf), body.right, new Vector2(0, 0), 1, 0);
        imageObject image = new imageObject();
        image.add(l1);
        image.add(l2);
        image.add(r2);
        image.add(r1);
        return image;
    }
    public imageObject ClothImage(Torso body,Cloth cloth)
    {
        float maxwidth = width_gill + width_cheek;
        maxwidth /= 2;
        Vector2 lb = new Vector2(-maxwidth * 0.5f * body.width_neck, height_gill);
        Vector2 la = new Vector2(0, lb.Y);
        Vector2 lc = new Vector2(lb.X, -0.05f);
        Vector2 le = new Vector2(-maxwidth * 1.2f * body.width_arm + lc.X, -0.5f);
        Vector2 ld = new Vector2(lc.X + (le.X - lc.X) * body.width_shoulder, -0.3f + 0.24f * body.height_shoulder);
        Vector2 lf = new Vector2(0, -0.5f);
        Vector2 ra = new Vector2(-la.X, la.Y);
        Vector2 rb = new Vector2(-lb.X, lb.Y);
        Vector2 rc = new Vector2(-lc.X, lc.Y);
        Vector2 rd = new Vector2(-ld.X, ld.Y);
        Vector2 re = new Vector2(-le.X, le.Y);
        Vector2 rf = lf;
        imageObject image = new imageObject();
        Vector2 lp1,lp2,rp1,rp2,lp3,lp4,rp3,rp4;
        float pl = cloth.height_a - cloth.length_a;
      if(cloth.height_a<1)
		{
            lp1 = le + (ld - le) * cloth.height_a;
            lp2 = new Vector2(0, (lp1.Y + 0.5f) * cloth.height_b - 0.5f);
            rp1 = new Vector2(-lp1.X, lp1.Y);
            rp2 = new Vector2(-lp2.X, lp2.Y);
            colorobject left = new colorobject(new pointgroup(lp1, lp2, lf, le), cloth.color, new Vector2(0, 0), 1, 0);
            colorobject right = new colorobject(new pointgroup(rp1, rp2, rf, re), cloth.color.similarColor(25), new Vector2(0, 0), 1, 0);

            image.add(left);
            image.add(right);
        }
      else if(cloth.height_a<2)
		{
            lp1 = ld + (lc - ld) * (cloth.height_a-1);
            lp2 = new Vector2(0, (lp1.Y + 0.5f) * cloth.height_b - 0.5f);
            rp1 = new Vector2(-lp1.X, lp1.Y);
            rp2 = new Vector2(-lp2.X, lp2.Y);
            colorobject left = new colorobject(new pointgroup(lp1, lp2, lf, ld), cloth.color, new Vector2(0, 0), 1, 0);
            colorobject right = new colorobject(new pointgroup(rp1, rp2, rf, rd), cloth.color.similarColor(25), new Vector2(0, 0), 1, 0);
            colorobject left2 = new colorobject(new pointgroup(lf, le, ld), cloth.color, new Vector2(0, 0), 1, 0);
            colorobject right2 = new colorobject(new pointgroup(rf, re, rd), cloth.color.similarColor(25), new Vector2(0, 0), 1, 0);
            image.add(left);
            image.add(right);
            image.add(left2);
            image.add(right2);
        }
        else  
        {
            lp1 = lc+ (lb - lc) * (cloth.height_a - 2);
            lp2 = new Vector2(0, (lp1.Y + 0.5f) * cloth.height_b - 0.5f);
            rp1 = new Vector2(-lp1.X, lp1.Y);
            rp2 = new Vector2(-lp2.X, lp2.Y);
      

            colorobject left = new colorobject(new pointgroup(lp1, lp2, lf, lc), cloth.color, new Vector2(0, 0), 1, 0);
            colorobject right = new colorobject(new pointgroup(rp1, rp2, rf, rc), cloth.color.similarColor(22), new Vector2(0, 0), 1, 0);
            colorobject left2 = new colorobject(new pointgroup(lc, lf, le, ld), cloth.color, new Vector2(0, 0), 1, 0);
            colorobject right2 = new colorobject(new pointgroup(rc, rf, re, rd), cloth.color.similarColor(22), new Vector2(0, 0), 1, 0);

            image.add(left);
            image.add(right);
            image.add(left2);
            image.add(right2);
        }
    Line2d  l1= new Line2d(lp1, lp2).verticalline(lp2);
        Line2d l2 ;
        if (pl<1)
		{
            if (pl < 0)
                pl = 0;
            lp3 = le + (ld - le) * pl;
            l2 = new Line2d(lp3 + lp2 - lp1, lp3);
            lp4 = l1.crosspoint(l2);
            rp3 = new Vector2(-lp3.X, lp3.Y);
            rp4 = new Vector2(-lp4.X, lp4.Y);
            image.add(new colorobject(new pointgroup(lp2,lp1,lp3,lp4), cloth.color.similarColor(22)));
            image.add(new colorobject(new pointgroup(rp2, rp1, rp3, rp4), cloth.color));
        }
        else if(pl<2)
		{
            lp3 = ld + (lc - ld) * ( pl-1);
            l2 = new Line2d(lp3 + lp2 - lp1, lp3);
            lp4 = l1.crosspoint(l2);
            rp3 = new Vector2(-lp3.X, lp3.Y);
            rp4 = new Vector2(-lp4.X, lp4.Y);
            image.add(new colorobject(new pointgroup(lp2, lp1, lp3, lp4), cloth.color.similarColor(22)));
            image.add(new colorobject(new pointgroup(rp2, rp1, rp3, rp4), cloth.color));
        }
        else
		{
            lp3 = lc + (lb - lc) * (pl - 2);
            l2 = new Line2d(lp3 + lp2 - lp1, lp3);
            lp4 = l1.crosspoint(l2);
            rp3 = new Vector2(-lp3.X, lp3.Y);
            rp4 = new Vector2(-lp4.X, lp4.Y);
            image.add(new colorobject(new pointgroup(lp2, lp1, lp3, lp4), cloth.color.similarColor(22)));
            image.add(new colorobject(new pointgroup(rp2, rp1, rp3, rp4), cloth.color));
        }
        return image;
    }
    Vector2? rightjaw;
    public Vector2? RightJaw { get { if (rightjaw == null) { rightjaw = new Vector2(0.5 * width_jaw, 0); } return rightjaw; } }
    Vector2? leftjaw;
    public Vector2? LeftJaw { get { if (leftjaw == null) { leftjaw = new Vector2(-0.5 * width_jaw, 0); }return leftjaw; }}
    public imageObject image { get {
            pointgroup left = new pointgroup(new Vector2(0, 0), new Vector2(-0.5f * width_jaw, 0), new Vector2(-0.5f * width_gill, height_gill), new Vector2(-0.5f * width_cheek, height_cheek), new Vector2(-0.5f * width_top, height_top), new Vector2(0f, height_top));
            pointgroup right = new pointgroup(new Vector2(0, 0), new Vector2(0.5f * width_jaw, 0), new Vector2(0.5f * width_gill, height_gill), new Vector2(0.5f * width_cheek, height_cheek), new Vector2(0.5f * width_top, height_top), new Vector2(0f, height_top));
            Vector2 center = new Vector2(0, (height_gill + height_cheek) * 0.5f);
            Vector2 le = center + new Vector2(-x_eye, y_eye);
            Vector2 re = center + new Vector2(x_eye, y_eye);
            pointgroup eye_left = new pointgroup(new Vector2(0.5f * width_eye, 0.5f * height_eye) + le, new Vector2(-0.5f * width_eye, 0.5f * height_eye) + le, new Vector2(-0.5f * width_eye, -0.5f * height_eye) + le, new Vector2(0.5f * width_eye, -0.5f * height_eye) + le);
            pointgroup eye_rigth = new pointgroup(new Vector2(0.5f * width_eye, 0.5f * height_eye) + re, new Vector2(-0.5f * width_eye,0.5f * height_eye) + re, new Vector2(-0.5f * width_eye, -0.5f * height_eye) + re, new Vector2(0.5f * width_eye, -0.5f * height_eye) + re);
        
            imageObject image = new imageObject();
            colorobject temp = new colorobject(left, leftface, new Vector2(0, 0),1,0);
       
            image.add(temp);
            image.add(new colorobject(right, rightface, new Vector2(0, 0), 1, 0));
            image.add(new colorobject(eye_left, eye, new Vector2(0, 0), 1, 0));
            image.add(new colorobject(eye_rigth, eye, new Vector2(0, 0), 1, 0));
            return image;
        }
    }
}
public class Body
{
    public Face face { get; private set; }
    public Hair Hair { get; private set; }
    public Torso Torso { get; private set; }
    public Cloth cloth { get; private set; }
    public Body(Face face,Hair hair ,Torso body,Cloth cloth)
	{
        this.face = face;
        this.Hair = hair;
        Torso = body;
        this.cloth = cloth;
	}
    public Body(Face face, Hair hair, Torso body)
    {
        this.face = face;
        this.Hair = hair;
        Torso = body;
        this.cloth = null;
    }
    public bitmap Card
    {
        get
        {
            bitmap bitmap = new bitmap(500, 700);
            bitmap.paint(-1);
            this.image.draw(bitmap, new Vector2(250, 140), 300, 0);
            return bitmap;
        }
    }
    public Body()
    {
        this.face = new Face();
        this.Hair = new Hair();
        Torso = new Torso();
        cloth = new Cloth();
    }
    public static Body generateFemale(DvRandom random)
	{
        return new Body(Face.generateFemale(random), Hair.generateFemale(random), Torso.generateFemale(random), Cloth.generateFemale(random));
	}
    public static Body generateMale(DvRandom random)
	{
        return new Body(Face.generateMale(random), Hair.generateMale(random), Torso.generateMale(random), Cloth.generateMale(random));
    }
    public void setBackHair(float bh)
	{
        Hair.setBackHair(bh);
	}
  bitmap t2d;
    public bitmap card { get
		{
            if(t2d==null)
			{
                bitmap bitmap = new bitmap(500, 700);
                bitmap.paint(-1);
             image.draw(bitmap, new Vector2(250, 140), 300, 0);
                t2d = bitmap;
            }
            return t2d;
		} }
    public imageObject image{get{
            imageObject image = new imageObject();
            if(Hair.length_hair>0)
			{
                image.add(face.backHair(Hair));
			}
            image.add(face.torsoImage(Torso));
            if(cloth!=null)
			{
                image.add(face.ClothImage(Torso,cloth));
            }
            
            image.add(face.image);
            image.add(face.getMaskImage(0xffffff));
            image.add(face.hairImage(Hair));
            
            return image;

        ;}}

}
