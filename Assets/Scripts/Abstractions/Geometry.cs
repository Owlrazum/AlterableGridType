using System;
using UnityEngine;
using System.Collections.Generic;
using Utilities;

namespace Geometry 
{
    public class Point2D
    {
        public Point2D(GameObject gmObj)
        {
            Pos = new Vector2(gmObj.transform.position.x, gmObj.transform.position.z);
        }

        public Point2D(Vector3 pos)
        {
            Pos = new Vector2(pos.x, pos.z);
        }

        public Point2D(Vector2 pos)
        {
            Pos = pos;
        }

        public Point2D(float x, float y)
        {
            Pos = new Vector2(x, y);
        }

        public Vector2 Pos { get; set; }
    }
    public class Edge2D
    {
        public Edge2D(Point2D p1, Point2D p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public bool DoesIntersect(Edge2D other)
        {
            Vector2 A = P1.Pos;
            Vector2 B = P2.Pos;
            Vector2 C = other.P1.Pos;
            Vector2 D = other.P2.Pos;
            Vector2 CmP = new Vector2(C.x - A.x, C.y - A.y);
            Vector2 r = new Vector2(B.x - A.x, B.y - A.y);
            Vector2 s = new Vector2(D.x - C.x, D.y - C.y);

            float CmPxr = CmP.x * r.y - CmP.y * r.x;
            float CmPxs = CmP.x * s.y - CmP.y * s.x;
            float rxs = r.x * s.y - r.y * s.x;

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }

        public float GetArea()
        {
            float x1 = P1.Pos.x;
            float x2 = P2.Pos.x;
            float y1 = P1.Pos.y;
            float y2 = P2.Pos.y;

            return (x2 - x1) * (y2 + y1);
        }

        public Point2D P1 { get; }
        public Point2D P2{ get; }
    }
    public class Triangle2D
    {
        public Triangle2D(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            V1 = new Point2D(p1);
            V2 = new Point2D(p2);
            V3 = new Point2D(p3);

            T1 = null;
            T2 = null;
            T3 = null;
            CircumCircle = new Circle(V1, V2, V3);
        }

        public Triangle2D(Point2D p1, Point2D p2, Point2D p3) 
        {
            V1 = p1;
            V2 = p2;
            V3 = p3;

            T1 = null;
            T2 = null;
            T3 = null;
            CircumCircle = new Circle(V1, V2, V3);
        }

        public Point2D V1 { get; }
        public Point2D V2 { get; }
        public Point2D V3 { get; }

        private Triangle2D t1;
        private Triangle2D t2;
        private Triangle2D t3;

        public Triangle2D T1
        {
            get { return t1; }
            set
            {
                if (!HasNeighbors) HasNeighbors = true;
                t1 = value;
            }
        }

        public Triangle2D T2
        {
            get { return t2; }
            set
            {
                if (!HasNeighbors) HasNeighbors = true;
                t2 = value;
            }
        }
        public Triangle2D T3
        {
            get { return t3; }
            set
            {
                if (!HasNeighbors) HasNeighbors = true;
                t3 = value;
            }
        }

        public bool HasNeighbors { get; set; }

        /*8 public void AssignTriangle(Vector2 point, Triangle t)
         {
             if (point == V1)
             {
                 T1 = t;
             }
             else if (point == V2)
             {
                 T2 = t;
             }
             else if (point == V3)
             {
                 T3 = t;
             }
             else 
             {
                 Debug.LogError("No Such Point " + point); 
             }
         }
         */

        public float FindArea()
        {
            float x1 = V1.Pos.x;
            float x2 = V2.Pos.x;
            float x3 = V3.Pos.x;

            float y1 = V1.Pos.y;
            float y2 = V2.Pos.y;
            float y3 = V3.Pos.y;

            return Mathf.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2;
        }

        public Circle CircumCircle {get;}
    }
    public struct Circle
    {
        public Circle(Vector2 c, float radius)
        {
            Center = new Point2D(c);
            Radius = radius;
        }
        public Circle(Point2D c, float radius)
        {
            Center = c;
            Radius = radius;
        }
        public Circle(Point2D first, Point2D second, Point2D third)
        {
            StraightLine firstBisector = StraightLine.PerpBisector(first.Pos, second.Pos);
            StraightLine secondBisector = StraightLine.PerpBisector(second.Pos, third.Pos);
            Center = new Point2D(0, 0);
            Center.Pos = firstBisector.IntersectionPoint(secondBisector);
            if (Center.Pos.x == Mathf.Infinity)
            {
                Debug.LogError("Found a Center with infinite x");
            }
            Radius = Methods.FindDistance(Center.Pos, first.Pos);
        }
        public Point2D Center { get; }
        public float Radius { get; }
    }
    public class StraightLine
    {
        public StraightLine(bool isVertical, float coord)
        {
            IsVertHoriz = true;
            IsVert = isVertical;
            Coord = coord;
        }
        public StraightLine(float a, float b, float c) 
        {
            A = a;
            B = b;
            C = c;

            S = -A / B;   // slope intercept form
            I = -C / B;
            CheckVertHoriz();
        }
        public StraightLine(float s, float i)
        {
            S = s;
            I = i;

            A = -S;
            B = 1;
            C = -I;
            CheckVertHoriz();
        }
        public StraightLine(Vector2 first, Vector2 second)
        {
            A = second.y - first.y;
            B = first.x - second.x;
            C = -(A * first.x + B * first.y);

            P1 = first;
            P2 = second;

            S = -A / B;   // slope intercept form
            I = -C / B;
            CheckVertHoriz();
        }
        private void CheckVertHoriz()
        {
            if (A == 0)
            {
                Coord = -C / B;
                IsVert = false;
                IsVertHoriz = true;
            }
            else if (B == 0)
            {
                Coord = -C / A;
                IsVert = true;
                IsVertHoriz = true;
            }
            else 
            {
                IsVertHoriz = false;
            }
        }

        // To remove in final version

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        // endRemove

        public float A { get; }
        public float B { get; }
        public float C { get; }

        public float S { get; }
        public float I { get; }

        public bool IsVertHoriz { get; private set; }
        public bool IsVert { get; private set; }
        public float Coord { get; private set; }
        public Vector2 IntersectionPoint(StraightLine other)
        {
            Vector2 noInter = new Vector2(Mathf.Infinity, Mathf.Infinity);
            float A2 = other.A;
            float B2 = other.B;
            float C2 = other.C;

            float a = S;   // slope intercept form
            float b = other.S;

            float c = I;
            float d = other.I;

            #region Special cases
            if (IsVertHoriz || other.IsVertHoriz)
            {
                bool nonFirst = IsVertHoriz ? other.IsVertHoriz : IsVertHoriz;
                if (nonFirst)
                {
                    if (IsVert == other.IsVert)
                    {
                        return noInter;
                    } else
                    {
                        if (IsVert)
                        {
                            return new Vector2(Coord, other.Coord);
                        }
                        else
                        {
                            return new Vector2(other.Coord, Coord);
                        }
                    }
                }
                else
                {
                    float x = 0, y = 0;
                    if (IsVertHoriz)
                    {
                        if (IsVert)
                        {
                            x = Coord;
                            y = x * b + d;
                        }
                        else
                        {
                            y = Coord;
                            x = (y - d) / b;
                        }
                        return new Vector2(x, y);
                    }
                    else 
                    {
                        if (other.IsVert)
                        {
                            x = other.Coord;
                            y = x * a + c;
                        }
                        else
                        {
                            y = other.Coord;
                            x = (y - c) / a;
                        }
                        return new Vector2(x, y);
                    }
                }
            }
            #endregion
            if (a == c)
            {
                return new Vector2(Mathf.Infinity, Mathf.Infinity);
            }
            else
            {
                float x = (d - c) / (a - b);
                float y = a * x + c;
                return new Vector2(x, y);
            }

            float det = A * B2 - A2 * B;
            if (det == 0) 
            {
                return new Vector2(Mathf.Infinity, Mathf.Infinity);
            }
            else 
            {
                float x = -(B * C2 - C * B2) / det;
                float y = -(C * A2 - A * C2) / det;
                return new Vector2(x, y);
            }
        }

        public bool IsPointUnder(Vector2 pos)
        {
            if (pos.y < S * pos.x + I)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float DistanceFromPoint(Vector2 pos)
        {
            float numerator = Mathf.Abs(A * pos.x + B * pos.y + C);
            float denominator = Mathf.Sqrt(A * A + B * B);
            return numerator / denominator;
        }

        public float GetY(float x)
        {
            return (A * x + C) / -B;
        }

        public static StraightLine PerpBisector(Vector2 first, Vector2 second)
        {
            Vector2 midPoint = new Vector2((first.x + second.x) / 2, (first.y + second.y) / 2);
            StraightLine L = new StraightLine(first, second);
            if (L.S == 0)
            {
                if (L.IsVert)
                {
                    return new StraightLine(false, midPoint.y);
                }
                else
                {
                    return new StraightLine(true, midPoint.x);
                }
            }
            float rS = -1 / L.S;
            float rI = midPoint.y - rS * midPoint.x; 
            return new StraightLine(rS, rI);
        }
    }
    public class Methods
    {
        public static bool IsPointInsideCircle(Point2D point, Circle circle)
        {
            float xDif = point.Pos.x - circle.Center.Pos.x;
            float yDif = point.Pos.y - circle.Center.Pos.y;
            if (Mathf.Pow(xDif, 2) + Mathf.Pow(yDif, 2) < Mathf.Pow(circle.Radius, 2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsPointInsideCircle(Vector2 point, Circle circle)
        {
            float xDif = point.x - circle.Center.Pos.x;
            float yDif = point.y - circle.Center.Pos.y;
            if (Mathf.Pow(xDif, 2) + Mathf.Pow(yDif, 2) < Mathf.Pow(circle.Radius, 2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static float FindDistance(Vector2 first, Vector2 second) 
        {
            Vector2 dif = first - second;
            float dist = Mathf.Sqrt(Mathf.Pow(dif.x, 2) + Mathf.Pow(dif.y, 2));
            return dist;
        }
    }
}

/*

(x - x1)2 + (y - y1)2 = (x - x2)2 + (y - y2)2 

62                     38

x^2 - 12x + 36 + y^2 - 4y + 16 = x^2 -6x + 9 + y^2 - 16y + 64
6x  - 12y = -21

*/