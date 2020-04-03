//Script for multi-level masking
//Author: Andrew Yew
//See the unity scene for the masking setup
//Multi-level means successive shapes can either add to the mask shape or subtract from the mask shape
//All credit goes to the awesome folks who contribute to opencv, on which almost all this code is based!! 


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MxrMultiLevelMasking : MonoBehaviour
{
    Color includeColor = Color.red;
    Color excludeColor = new Color(0, 0, 0, 0);

    public void DewIt() // demo function
    {
        Texture2D tex = new Texture2D(1080, 2168, TextureFormat.RGBA32, false);

        Color fillColor = new Color(0, 0, 0, 0);
        Color [] fillColorArray = tex.GetPixels();
        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }
        tex.SetPixels(fillColorArray);

        List<Vector2Int> points = new List<Vector2Int>();
        points.Add(new Vector2Int(158, 1101));
        points.Add(new Vector2Int(158, 1108));
        points.Add(new Vector2Int(165, 1198));
        points.Add(new Vector2Int(178, 1281));
        points.Add(new Vector2Int(199, 1363));
        points.Add(new Vector2Int(239, 1434));
        points.Add(new Vector2Int(297, 1490));
        points.Add(new Vector2Int(365, 1531));
        points.Add(new Vector2Int(442, 1569));
        points.Add(new Vector2Int(529, 1528));
        points.Add(new Vector2Int(613, 1572));
        points.Add(new Vector2Int(682, 1537));
        points.Add(new Vector2Int(744, 1493));
        points.Add(new Vector2Int(793, 1436));
        points.Add(new Vector2Int(818, 1366));
        points.Add(new Vector2Int(830, 1289));
        points.Add(new Vector2Int(839, 1209));
        points.Add(new Vector2Int(842, 1124));
        points.Add(new Vector2Int(841, 1089));
        points.Add(new Vector2Int(158, 1101));

        float time = Time.realtimeSinceStartup;
        DrawPolygon(tex, points, includeColor);
        Debug.Log("Execution time of DrawPolygon 1 is " + (Time.realtimeSinceStartup - time));

        Vector2 center = new Vector2(499, 1094);
        Size2f axes = new Size2f(341, 375);
        time = Time.realtimeSinceStartup;
        EllipseEx(tex, center, axes, 0, 180, 360, includeColor);
        Debug.Log("Execution time of EllipseEx 1 is " + (Time.realtimeSinceStartup - time));

        Vector2 center2 = new Vector2(390, 1096);
        Size2f axes2 = new Size2f(73, 32);
        time = Time.realtimeSinceStartup;
        EllipseEx(tex, center2, axes2, 0, 0, 360, excludeColor);
        Debug.Log("Execution time of EllipseEx 2 is " + (Time.realtimeSinceStartup - time));

        Vector2 center3 = new Vector2(688, 1091);
        Size2f axes3 = new Size2f(69, 33);
        time = Time.realtimeSinceStartup;
        EllipseEx(tex, center3, axes3, 0, 0, 360, excludeColor);
        Debug.Log("Execution time of EllipseEx 3 is " + (Time.realtimeSinceStartup - time));

        points.Clear();
        points.Add(new Vector2Int(247, 1044));
        points.Add(new Vector2Int(295, 996));
        points.Add(new Vector2Int(363, 979));
        points.Add(new Vector2Int(433, 989));
        points.Add(new Vector2Int(497, 1018));
        points.Add(new Vector2Int(498, 1052));
        points.Add(new Vector2Int(434, 1021));
        points.Add(new Vector2Int(363, 1008));
        points.Add(new Vector2Int(295, 1022));
        points.Add(new Vector2Int(247, 1067));
        points.Add(new Vector2Int(247, 1044));

        time = Time.realtimeSinceStartup;
        DrawPolygon(tex, points, excludeColor);
        Debug.Log("Execution time of DrawPolygon 2 is " + (Time.realtimeSinceStartup - time));

        points.Clear();
        points.Add(new Vector2Int(585, 1011));
        points.Add(new Vector2Int(641, 986));
        points.Add(new Vector2Int(701, 977));
        points.Add(new Vector2Int(762, 987));
        points.Add(new Vector2Int(804, 1028));
        points.Add(new Vector2Int(804, 1051));
        points.Add(new Vector2Int(762, 1010));
        points.Add(new Vector2Int(701, 1000));
        points.Add(new Vector2Int(641, 1009));
        points.Add(new Vector2Int(585, 1034));
        points.Add(new Vector2Int(585, 1011));

        time = Time.realtimeSinceStartup;
        DrawPolygon(tex, points, excludeColor);
        Debug.Log("Execution time of DrawPolygon 3 is " + (Time.realtimeSinceStartup - time));

        tex.Apply();

        GetComponent<RawImage>().texture = tex;
    }

    public static void DrawPolygon(Texture2D img, List<Vector2Int> points, Color color)
    {
        FillConvexPoly(img, points, points.Count, 0, color);
    }

    public static void DrawPolygon(Texture2D img, List<Vector2> points, Color color)
    {
        for (int i = 1; i < points.Count; i++)
        {
            Line line = new Line();
            line.p1.x = points[i - 1].x;
            line.p1.y = points[i - 1].y;
            line.p2.x = points[i].x;
            line.p2.y = points[i].y;

            DrawLine(img, line, color);
        }

        Line last_line = new Line();
        last_line.p1.x = points[points.Count - 1].x;
        last_line.p1.y = points[points.Count - 1].y;
        last_line.p2.x = points[0].x;
        last_line.p2.y = points[0].y;

        DrawLine(img, last_line, color);
    }

    const int XY_SHIFT = 16, XY_ONE = 1 << XY_SHIFT, DRAWING_STORAGE_BLOCK = (1 << 12) - 256;

    static void EllipseEx(Texture2D img, Vector2 center, Size2f axes, int angle, int arc_start, int arc_end, Color color)
    {
        axes.width = Mathf.Abs(axes.width);
        axes.height = Mathf.Abs(axes.height);

        int delta = 5;

        List<Vector2> _v = ellipse2Poly(center, axes, angle, arc_start, arc_end, delta);

        List<Vector2Int> v = new List<Vector2Int>();
        Vector2Int prevPt = new Vector2Int(0xFFFF, 0xFFFF);

        for ( int i = 0; i < _v.Count; ++i)
        {
            Vector2Int pt = new Vector2Int();
            pt.x = Mathf.RoundToInt(_v[i].x / XY_ONE) << XY_SHIFT;
            pt.y = Mathf.RoundToInt(_v[i].y / XY_ONE) << XY_SHIFT;
            pt.x += Mathf.RoundToInt(_v[i].x - pt.x);
            pt.y += Mathf.RoundToInt(_v[i].y - pt.y);
            if (pt != prevPt)
            {
                v.Add(pt);
                prevPt = pt;
            }
        }

        FillConvexPoly(img, v, v.Count, 0, color);
    }

    static List<Vector2> ellipse2Poly(Vector2 center, Size2f axes, int angle, int arc_start, int arc_end, int delta)
    {
        List<Vector2> points = new List<Vector2>();

        float alpha, beta;
        int i;

        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;

        if (arc_start > arc_end)
        {
            i = arc_start;
            arc_start = arc_end;
            arc_end = i;
        }
        while (arc_start < 0)
        {
            arc_start += 360;
            arc_end += 360;
        }
        while (arc_end > 360)
        {
            arc_end -= 360;
            arc_start -= 360;
        }
        if (arc_end - arc_start > 360)
        {
            arc_start = 0;
            arc_end = 360;
        }

        sincos(angle, out alpha, out beta);

        points.Clear();

        for (i = arc_start; i < arc_end + delta; i += delta)
        {
            float x, y;
            angle = i;
            if (angle > arc_end)
                angle = arc_end;
            if (angle < 0)
                angle += 360;

            x = axes.width * SinTable[450 - angle];
            y = axes.height * SinTable[angle];
            Vector2 pt;
            pt.x = center.x + x * alpha - y * beta;
            pt.y = center.y + x * beta + y * alpha;
            points.Add(pt);
        }

        return points;
    }

    static void Organize(List<PolyEdge> edges)
    {
        edges.Sort(delegate (PolyEdge x, PolyEdge y) {
            return CmpEdges(x, y);
        });
    }

    static int CmpEdges(PolyEdge e1, PolyEdge e2)
    {
        return bool2int(int2bool(e1.y0 - e2.y0) ? e1.y0 < e2.y0 : int2bool(e1.x - e2.x) ? e1.x < e2.x : e1.dx < e2.dx);
    }

    static bool int2bool(int input)
    {
        if (input > 0) return true;
        else return false;
    }

    static int bool2int(bool input)
    {
        if (input) return 1;
        else return 0;
    }

    static void sincos(int angle, out float cosval, out float sinval)
    {
        angle += (angle < 0 ? 360 : 0);
        sinval = SinTable[angle];
        cosval = SinTable[450 - angle];
    }

    static void DrawLine(Texture2D img, Vector2Int p1, Vector2Int p2, Color color)
    {
        Line line = new Line(p1, p2);
        DrawLine(img, line, color);
    }

    static void DrawLine(Texture2D img, Line line, Color color)
    {
        //xiaolin wu's line drawing algorithm

        bool steep = Mathf.Abs(line.p2.y - line.p1.y) > Mathf.Abs(line.p2.x - line.p1.x);

        float temp;
        if (steep)
        {
            temp = line.p1.x; line.p1.x = line.p1.y; line.p1.y = temp;
            temp = line.p2.x; line.p2.x = line.p2.y; line.p2.y = temp;
        }
        if (line.p1.x > line.p2.x)
        {
            temp = line.p1.x; line.p1.x = line.p2.x; line.p2.x = temp;
            temp = line.p1.y; line.p1.y = line.p2.y; line.p2.y = temp;
        }

        float dx = line.p2.x - line.p1.x;
        float dy = line.p2.y - line.p1.y;
        float gradient = dy / dx;

        float xEnd = round(line.p1.x);
        float yEnd = line.p1.y + gradient * (xEnd - line.p1.x);
        float xGap = rfpart(line.p1.x + 0.5f);
        float xPixel1 = xEnd;
        float yPixel1 = ipart(yEnd);
        
        if (steep)
        {
            plot(img, yPixel1, xPixel1, rfpart(yEnd) * xGap, color);
            plot(img, yPixel1, xPixel1, rfpart(yEnd) * xGap, color);
            plot(img, yPixel1 + 1, xPixel1, fpart(yEnd) * xGap, color);
        }
        else
        {
            plot(img, xPixel1, yPixel1, rfpart(yEnd) * xGap, color);
            plot(img, xPixel1, yPixel1 + 1, fpart(yEnd) * xGap, color);
        }
        
        float intery = yEnd + gradient;

        xEnd = round(line.p2.x);
        yEnd = line.p2.y + gradient * (xEnd - line.p2.x);
        xGap = fpart(line.p2.x + 0.5f);
        float xPixel2 = xEnd;
        float yPixel2 = ipart(yEnd);

        if (steep)
        {
            plot(img, yPixel2, xPixel2, rfpart(yEnd) * xGap, color);
            plot(img, yPixel2 + 1, xPixel2, fpart(yEnd) * xGap, color);
        }
        else
        {
            plot(img, xPixel2, yPixel2, rfpart(yEnd) * xGap, color);
            plot(img, xPixel2, yPixel2 + 1, fpart(yEnd) * xGap, color);
        }
    }

    private static void plot(Texture2D img, float x, float y, float c, Color color)
    {
        int alpha = (int)(c * 255);
        if (alpha > 255) alpha = 255;
        if (alpha < 0) alpha = 0;
        color.a = (float)alpha / 255.0f;

        if(x > 0 && x < img.width)
            if(y > 0 && y < img.height)
                img.SetPixel((int)x, (int)y, color);       
    }

    static void FillConvexPoly(Texture2D img, List<Vector2Int> v, int npts, int shift, Color color)
    {
        int delta = 1 << shift >> 1;
        int i, y, imin = 0;
        int edges = npts;
        int xmin, xmax, ymin, ymax;
        int ptr = 0;
        Size size = new Size(img.width, img.height);

        int pix_size = 1;
        Vector2Int p0;
        int delta1, delta2;

        delta1 = delta2 = XY_ONE >> 1;

        p0 = v[npts - 1];
        p0.x <<= XY_SHIFT - shift;
        p0.y <<= XY_SHIFT - shift;

        //assert(0 <= shift && shift <= XY_SHIFT);
        xmin = xmax = v[0].x;
        ymin = ymax = v[0].y;

        for (i = 0; i < npts; i++)
        {
            Vector2Int p = v[i];
            if (p.y < ymin)
            {
                ymin = p.y;
                imin = i;
            }

            ymax = Mathf.Max(ymax, p.y);
            xmax = Mathf.Max(xmax, p.x);
            xmin = MIN(xmin, p.x);

            p.x <<= XY_SHIFT - shift;
            p.y <<= XY_SHIFT - shift;

            DrawLine(img, p0, p, color);

            p0 = p;
        }

        xmin = (xmin + delta) >> shift;
        xmax = (xmax + delta) >> shift;
        ymin = (ymin + delta) >> shift;
        ymax = (ymax + delta) >> shift;

        if (npts < 3 || (int)xmax < 0 || (int)ymax < 0 || (int)xmin >= size.width || (int)ymin >= size.height)
            return;

        Edge[] edge = new Edge[2];
        ymax = MIN(ymax, size.height - 1);
        edge[0].idx = edge[1].idx = imin;

        edge[0].ye = edge[1].ye = y = (int)ymin;
        edge[0].di = 1;
        edge[1].di = npts - 1;

        edge[0].x = edge[1].x = -XY_ONE;
        edge[0].dx = edge[1].dx = 0;

        ptr += y;

        do
        {
            if (y < (int)ymax || y == (int)ymin)
            {
                for (i = 0; i < 2; i++)
                {
                    if (y >= edge[i].ye)
                    {
                        int idx0 = edge[i].idx, di = edge[i].di;
                        int idx = idx0 + di;
                        if (idx >= npts) idx -= npts;
                        int ty = 0;

                        for (; edges-- > 0;)
                        {
                            ty = (int)((v[idx].y + delta) >> shift);
                            if (ty > y)
                            {
                                int xs = v[idx0].x;
                                int xe = v[idx].x;
                                if (shift != XY_SHIFT)
                                {
                                    xs <<= XY_SHIFT - shift;
                                    xe <<= XY_SHIFT - shift;
                                }

                                edge[i].ye = ty;
                                edge[i].dx = ((xe - xs) * 2 + (ty - y)) / (2 * (ty - y));
                                edge[i].x = xs;
                                edge[i].idx = idx;
                                break;
                            }
                            idx0 = idx;
                            idx += di;
                            if (idx >= npts) idx -= npts;
                        }
                    }
                }
            }

            if (edges < 0)
                break;

            if (y >= 0)
            {
                int left = 0, right = 1;
                if (edge[0].x > edge[1].x)
                {
                    left = 1; right = 0;
                }

                int xx1 = (int)((edge[left].x + delta1) >> XY_SHIFT);
                int xx2 = (int)((edge[right].x + delta2) >> XY_SHIFT);

                if (xx2 >= 0 && xx1 < size.width)
                {
                    if (xx1 < 0)
                        xx1 = 0;
                    if (xx2 >= size.width)
                        xx2 = size.width - 1;
                    ICV_HLINE(img, ptr, xx1, xx2, color, pix_size);
                }
            }
            else
            {
                // TODO optimize scan for negative y
            }

            edge[0].x += edge[0].dx;
            edge[1].x += edge[1].dx;
            ptr += 1;
        }
        while (++y <= (int)ymax);
    }

    static float fpart(float x)
    {
        if (x < 0) return (1 - (x - Mathf.Floor(x)));
        return (x - Mathf.Floor(x));
    }

    static float rfpart(float x)
    {
        return 1 - fpart(x);
    }

    static int ipart(double x) { return (int)x; }

    static int round(double x) { return ipart(x + 0.5); }

    static Func<int, int, int> MIN = (a, b) => ((a) > (b) ? (b) : (a));

    //fill horizontal row
    static void ICV_HLINE(Texture2D img, int ptr, int xl, int xr, Color color, int pix_size )
    {
        for(int i = xl; i <=xr; i++)
        {
            img.SetPixel(i, ptr, color);
        }
    }

    struct Edge
    {
        public int idx, di;
        public int x, dx;
        public int ye;
    }

    struct Size
    {
        public int width;
        public int height;

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    struct Size2f
    {
        public float width;
        public float height;

        public Size2f(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public struct Line
    {
        public Vector2 p1;
        public Vector2 p2;

        public Line(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    struct PolyEdge
    {
        public int y0, y1;
        public int x, dx;
    }


    static float[] SinTable = new float[]
    {
        0.0000000f, 0.0174524f, 0.0348995f, 0.0523360f, 0.0697565f, 0.0871557f,
    0.1045285f, 0.1218693f, 0.1391731f, 0.1564345f, 0.1736482f, 0.1908090f,
    0.2079117f, 0.2249511f, 0.2419219f, 0.2588190f, 0.2756374f, 0.2923717f,
    0.3090170f, 0.3255682f, 0.3420201f, 0.3583679f, 0.3746066f, 0.3907311f,
    0.4067366f, 0.4226183f, 0.4383711f, 0.4539905f, 0.4694716f, 0.4848096f,
    0.5000000f, 0.5150381f, 0.5299193f, 0.5446390f, 0.5591929f, 0.5735764f,
    0.5877853f, 0.6018150f, 0.6156615f, 0.6293204f, 0.6427876f, 0.6560590f,
    0.6691306f, 0.6819984f, 0.6946584f, 0.7071068f, 0.7193398f, 0.7313537f,
    0.7431448f, 0.7547096f, 0.7660444f, 0.7771460f, 0.7880108f, 0.7986355f,
    0.8090170f, 0.8191520f, 0.8290376f, 0.8386706f, 0.8480481f, 0.8571673f,
    0.8660254f, 0.8746197f, 0.8829476f, 0.8910065f, 0.8987940f, 0.9063078f,
    0.9135455f, 0.9205049f, 0.9271839f, 0.9335804f, 0.9396926f, 0.9455186f,
    0.9510565f, 0.9563048f, 0.9612617f, 0.9659258f, 0.9702957f, 0.9743701f,
    0.9781476f, 0.9816272f, 0.9848078f, 0.9876883f, 0.9902681f, 0.9925462f,
    0.9945219f, 0.9961947f, 0.9975641f, 0.9986295f, 0.9993908f, 0.9998477f,
    1.0000000f, 0.9998477f, 0.9993908f, 0.9986295f, 0.9975641f, 0.9961947f,
    0.9945219f, 0.9925462f, 0.9902681f, 0.9876883f, 0.9848078f, 0.9816272f,
    0.9781476f, 0.9743701f, 0.9702957f, 0.9659258f, 0.9612617f, 0.9563048f,
    0.9510565f, 0.9455186f, 0.9396926f, 0.9335804f, 0.9271839f, 0.9205049f,
    0.9135455f, 0.9063078f, 0.8987940f, 0.8910065f, 0.8829476f, 0.8746197f,
    0.8660254f, 0.8571673f, 0.8480481f, 0.8386706f, 0.8290376f, 0.8191520f,
    0.8090170f, 0.7986355f, 0.7880108f, 0.7771460f, 0.7660444f, 0.7547096f,
    0.7431448f, 0.7313537f, 0.7193398f, 0.7071068f, 0.6946584f, 0.6819984f,
    0.6691306f, 0.6560590f, 0.6427876f, 0.6293204f, 0.6156615f, 0.6018150f,
    0.5877853f, 0.5735764f, 0.5591929f, 0.5446390f, 0.5299193f, 0.5150381f,
    0.5000000f, 0.4848096f, 0.4694716f, 0.4539905f, 0.4383711f, 0.4226183f,
    0.4067366f, 0.3907311f, 0.3746066f, 0.3583679f, 0.3420201f, 0.3255682f,
    0.3090170f, 0.2923717f, 0.2756374f, 0.2588190f, 0.2419219f, 0.2249511f,
    0.2079117f, 0.1908090f, 0.1736482f, 0.1564345f, 0.1391731f, 0.1218693f,
    0.1045285f, 0.0871557f, 0.0697565f, 0.0523360f, 0.0348995f, 0.0174524f,
    0.0000000f, -0.0174524f, -0.0348995f, -0.0523360f, -0.0697565f, -0.0871557f,
    -0.1045285f, -0.1218693f, -0.1391731f, -0.1564345f, -0.1736482f, -0.1908090f,
    -0.2079117f, -0.2249511f, -0.2419219f, -0.2588190f, -0.2756374f, -0.2923717f,
    -0.3090170f, -0.3255682f, -0.3420201f, -0.3583679f, -0.3746066f, -0.3907311f,
    -0.4067366f, -0.4226183f, -0.4383711f, -0.4539905f, -0.4694716f, -0.4848096f,
    -0.5000000f, -0.5150381f, -0.5299193f, -0.5446390f, -0.5591929f, -0.5735764f,
    -0.5877853f, -0.6018150f, -0.6156615f, -0.6293204f, -0.6427876f, -0.6560590f,
    -0.6691306f, -0.6819984f, -0.6946584f, -0.7071068f, -0.7193398f, -0.7313537f,
    -0.7431448f, -0.7547096f, -0.7660444f, -0.7771460f, -0.7880108f, -0.7986355f,
    -0.8090170f, -0.8191520f, -0.8290376f, -0.8386706f, -0.8480481f, -0.8571673f,
    -0.8660254f, -0.8746197f, -0.8829476f, -0.8910065f, -0.8987940f, -0.9063078f,
    -0.9135455f, -0.9205049f, -0.9271839f, -0.9335804f, -0.9396926f, -0.9455186f,
    -0.9510565f, -0.9563048f, -0.9612617f, -0.9659258f, -0.9702957f, -0.9743701f,
    -0.9781476f, -0.9816272f, -0.9848078f, -0.9876883f, -0.9902681f, -0.9925462f,
    -0.9945219f, -0.9961947f, -0.9975641f, -0.9986295f, -0.9993908f, -0.9998477f,
    -1.0000000f, -0.9998477f, -0.9993908f, -0.9986295f, -0.9975641f, -0.9961947f,
    -0.9945219f, -0.9925462f, -0.9902681f, -0.9876883f, -0.9848078f, -0.9816272f,
    -0.9781476f, -0.9743701f, -0.9702957f, -0.9659258f, -0.9612617f, -0.9563048f,
    -0.9510565f, -0.9455186f, -0.9396926f, -0.9335804f, -0.9271839f, -0.9205049f,
    -0.9135455f, -0.9063078f, -0.8987940f, -0.8910065f, -0.8829476f, -0.8746197f,
    -0.8660254f, -0.8571673f, -0.8480481f, -0.8386706f, -0.8290376f, -0.8191520f,
    -0.8090170f, -0.7986355f, -0.7880108f, -0.7771460f, -0.7660444f, -0.7547096f,
    -0.7431448f, -0.7313537f, -0.7193398f, -0.7071068f, -0.6946584f, -0.6819984f,
    -0.6691306f, -0.6560590f, -0.6427876f, -0.6293204f, -0.6156615f, -0.6018150f,
    -0.5877853f, -0.5735764f, -0.5591929f, -0.5446390f, -0.5299193f, -0.5150381f,
    -0.5000000f, -0.4848096f, -0.4694716f, -0.4539905f, -0.4383711f, -0.4226183f,
    -0.4067366f, -0.3907311f, -0.3746066f, -0.3583679f, -0.3420201f, -0.3255682f,
    -0.3090170f, -0.2923717f, -0.2756374f, -0.2588190f, -0.2419219f, -0.2249511f,
    -0.2079117f, -0.1908090f, -0.1736482f, -0.1564345f, -0.1391731f, -0.1218693f,
    -0.1045285f, -0.0871557f, -0.0697565f, -0.0523360f, -0.0348995f, -0.0174524f,
    -0.0000000f, 0.0174524f, 0.0348995f, 0.0523360f, 0.0697565f, 0.0871557f,
    0.1045285f, 0.1218693f, 0.1391731f, 0.1564345f, 0.1736482f, 0.1908090f,
    0.2079117f, 0.2249511f, 0.2419219f, 0.2588190f, 0.2756374f, 0.2923717f,
    0.3090170f, 0.3255682f, 0.3420201f, 0.3583679f, 0.3746066f, 0.3907311f,
    0.4067366f, 0.4226183f, 0.4383711f, 0.4539905f, 0.4694716f, 0.4848096f,
    0.5000000f, 0.5150381f, 0.5299193f, 0.5446390f, 0.5591929f, 0.5735764f,
    0.5877853f, 0.6018150f, 0.6156615f, 0.6293204f, 0.6427876f, 0.6560590f,
    0.6691306f, 0.6819984f, 0.6946584f, 0.7071068f, 0.7193398f, 0.7313537f,
    0.7431448f, 0.7547096f, 0.7660444f, 0.7771460f, 0.7880108f, 0.7986355f,
    0.8090170f, 0.8191520f, 0.8290376f, 0.8386706f, 0.8480481f, 0.8571673f,
    0.8660254f, 0.8746197f, 0.8829476f, 0.8910065f, 0.8987940f, 0.9063078f,
    0.9135455f, 0.9205049f, 0.9271839f, 0.9335804f, 0.9396926f, 0.9455186f,
    0.9510565f, 0.9563048f, 0.9612617f, 0.9659258f, 0.9702957f, 0.9743701f,
    0.9781476f, 0.9816272f, 0.9848078f, 0.9876883f, 0.9902681f, 0.9925462f,
    0.9945219f, 0.9961947f, 0.9975641f, 0.9986295f, 0.9993908f, 0.9998477f,
    1.0000000f
    };
}
