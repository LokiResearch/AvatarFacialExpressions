using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Point = UnityEngine.Vector3;
using Line = UnityEngine.Ray;
using Sphere = UnityEngine.BoundingSphere;
using System;

namespace utils
{
    public static class CollisionUtil
    {
        

        /// <summary></summary>
        public static bool Collide(Line l, Plane p) { return Collide(p, l); }
        /// <summary></summary>
        public static bool Collide(Plane p, Line l)
        {
            Point d = l.direction, n = p.normal;
            return Point.Dot(d, n) != 0;
        }

        /// <summary></summary>
        public static bool Collide(Plane p1, Plane p2)
        {
            return Point.Cross(p1.normal, p2.normal).magnitude != 0;
        }


        /// <summary></summary>
        public static bool Collide(Point p, Sphere s) { return Collide(s, p); }
        /// <summary></summary>
        public static bool Collide(Sphere s, Point p)
        {
            return DistanceUtil.Dist(s, p) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(Line l, Sphere s) { return Collide(s, l); }
        /// <summary></summary>
        public static bool Collide(Sphere s, Line l)
        {
            return DistanceUtil.Dist(s, l) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(Plane p, Sphere s) { return Collide(s, p); }
        /// <summary></summary>
        public static bool Collide(Sphere s, Plane p)
        {
            return DistanceUtil.Dist(s, p) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(Sphere s1, Sphere s2)
        {
            return DistanceUtil.Dist(s1, s2) <= 0;
        }



        /// <summary></summary>
        public static bool Collide(Point p, SphereCollider s) { return Collide(s, p); }
        /// <summary></summary>
        public static bool Collide(SphereCollider s, Point p) { return Collide(DistanceUtil.ToSphere(s), p); }

        /// <summary></summary>
        public static bool Collide(Line l, SphereCollider s) { return Collide(s, l); }
        /// <summary></summary>
        public static bool Collide(SphereCollider s, Line l) { return Collide(DistanceUtil.ToSphere(s), l);}

        /// <summary></summary>
        public static bool Collide(Plane p, SphereCollider s) { return Collide(s, p); }
        /// <summary></summary>
        public static bool Collide(SphereCollider s, Plane p) { return Collide(DistanceUtil.ToSphere(s), p); }

        /// <summary></summary>
        public static bool Collide(SphereCollider s1, SphereCollider s2) { return Collide(DistanceUtil.ToSphere(s2), DistanceUtil.ToSphere(s2)); }



        /// <summary></summary>
        public static bool Collide(Plane p, Point[] pts) { return Collide(pts, p); }
        /// <summary></summary>
        public static bool Collide(Point[] pts, Plane p)
        {
            return DistanceUtil.Dist(pts, p) <= 0;
        }





        /// <summary></summary>
        public static bool Collide(Point p, BoxCollider b) { return Collide(b, p); }
        /// <summary></summary>
        public static bool Collide(BoxCollider b, Point p)
        {
            return DistanceUtil.Dist(p, b.ClosestPoint(p)) < 0.000001;
        }
        
        /// <summary></summary>
        public static bool Collide(Line l, BoxCollider b) { return Collide(b, l); }
        /// <summary></summary>
        public static bool Collide(BoxCollider b, Line l)
        {
            if (Collide(b, l.origin))
                return true;
            RaycastHit hit;
            if (b.Raycast(l, out hit, 10000))
                return true;
            l.direction = -l.direction;
            return b.Raycast(l, out hit, 10000);
        }

        /// <summary></summary>
        public static bool Collide(Plane p, BoxCollider b) { return Collide(b, p); }
        /// <summary></summary>
        public static bool Collide(BoxCollider b, Plane p)
        {
            return Collide(b.GetVertices(), p);
        }

        /// <summary></summary>
        public static bool Collide(Sphere s, BoxCollider b) { return Collide(b, s); }
        /// <summary></summary>
        public static bool Collide(BoxCollider b, Sphere s)
        {
            return DistanceUtil.Dist(b, s) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(BoxCollider b, SphereCollider s) { return Collide(s, b); }
        /// <summary></summary>
        public static bool Collide(SphereCollider s, BoxCollider b) { return Collide(DistanceUtil.ToSphere(s), b); }

        /// <summary></summary>
        public static bool Collide(BoxCollider b1, BoxCollider b2)
        {
            Sphere s1 = new Sphere(b1.transform.position, 0);
            Sphere s2 = new Sphere(b2.transform.position, 0);
            foreach (Point pt in b1.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s1.position);
                if (d > s1.radius)
                    s1.radius = d;
            }
            foreach (Point pt in b2.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s2.position);
                if (d > s2.radius)
                    s2.radius = d;
            }
            if (!Collide(s1, s2))
                return false;
            return DistanceUtil.Dist(b1, b2) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(Point p, MeshCollider m) { return Collide(m, p); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, Point p)
        {
            return DistanceUtil.Dist(m, p) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(Line l, MeshCollider m) { return Collide(m, l); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, Line l)
        {
            if (Collide(m, l.origin))
                return true;
            RaycastHit hit;
            if (m.Raycast(l, out hit, 10000))
                return true;
            l.direction = -l.direction;
            return m.Raycast(l, out hit, 10000);
        }

        /// <summary></summary>
        public static bool Collide(Plane p, MeshCollider m) { return Collide(m, p); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, Plane p)
        {
            return Collide(m.GetVertices(), p);
        }

        /// <summary></summary>
        public static bool Collide(Sphere s, MeshCollider m) { return Collide(m, s); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, Sphere s)
        {
            return DistanceUtil.Dist(m, s) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(SphereCollider s, MeshCollider m) { return Collide(m, s); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, SphereCollider s) { return Collide(m, DistanceUtil.ToSphere(s)); }

        /// <summary></summary>
        public static bool Collide(BoxCollider b, MeshCollider m) { return Collide(m, b); }
        /// <summary></summary>
        public static bool Collide(MeshCollider m, BoxCollider b)
        {
            Sphere s1 = new Sphere(m.transform.position, 0);
            Sphere s2 = new Sphere(b.transform.position, 0);
            foreach (Point pt in m.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s1.position);
                if (d > s1.radius)
                    s1.radius = d;
            }
            foreach (Point pt in b.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s2.position);
                if (d > s2.radius)
                    s2.radius = d;
            }
            if (!Collide(s1, s2))
                return false;
            return DistanceUtil.Dist(m, b) <= 0;
        }

        /// <summary></summary>
        public static bool Collide(MeshCollider m1, MeshCollider m2)
        {
            Sphere s1 = new Sphere(m1.transform.position, 0);
            Sphere s2 = new Sphere(m2.transform.position, 0);
            foreach (Point pt in m1.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s1.position);
                if (d > s1.radius)
                    s1.radius = d;
            }
            foreach (Point pt in m2.GetVertices())
            {
                float d = DistanceUtil.Dist(pt, s2.position);
                if (d > s2.radius)
                    s2.radius = d;
            }
            if (!Collide(s1, s2))
                return false;
            return DistanceUtil.Dist(m1, m2) <= 0;
        }




    }


    



}
