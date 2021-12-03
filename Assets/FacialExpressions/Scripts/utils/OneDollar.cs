using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Point = UnityEngine.Vector2;

namespace OneDollarRecognizer
{

    public enum ScaleMethod
    {
        SCALE_TO_SQUARE,
        FIT_IN_SQUARE,
        NONE
    }


    public class OneDollar<K>
    {
        private const uint DEFAULT_SAMPLE = 32;
        private const float DEFAULT_SCALE_SIZE = 1;
        private const float DEFAULT_THETA = 45 * Mathf.Deg2Rad;
        private const float DEFAULT_THETA_STEP = 2 * Mathf.Deg2Rad;
        private const bool DEFAULT_ROTATE_TO_ZERO = true;
        private const bool DEFAULT_ADJUST_ROTATION = true;
        private const bool DEFAULT_REVERSIBLE = false;
        private const ScaleMethod DEFAULT_SCALE_METHOD = ScaleMethod.SCALE_TO_SQUARE;


        private const float PHI = 0.6180339887498948f; // (-1 + Mathf.Sqrt(5)) / 2;


        private Dictionary<K, List<Unistroke>> gestures;
        private List<K> hiddenKeysInFile = new List<K>();

        private readonly uint sample;
        private readonly float scaleSize, theta, thetaStep;
        private readonly bool rotateToZero, adjustRotation, reversible;
        private readonly ScaleMethod scaleMethod;


        public OneDollar(uint sample = DEFAULT_SAMPLE,
            float scaleSize = DEFAULT_SCALE_SIZE,
            float theta = DEFAULT_THETA,
            float thetaStep = DEFAULT_THETA_STEP,
            bool rotateToZero = DEFAULT_ROTATE_TO_ZERO,
            bool adjustRotation = DEFAULT_ADJUST_ROTATION,
            bool reversible = DEFAULT_REVERSIBLE,
            ScaleMethod scaleMethod = DEFAULT_SCALE_METHOD)
            : this(new Dictionary<K, List<Unistroke>>(), sample, scaleSize, theta, thetaStep, rotateToZero, adjustRotation, reversible, scaleMethod)
        {
        }

        public OneDollar(Dictionary<K, List<Unistroke>> gestures,
            uint sample = DEFAULT_SAMPLE,
            float scaleSize = DEFAULT_SCALE_SIZE,
            float theta = DEFAULT_THETA,
            float thetaStep = DEFAULT_THETA_STEP,
            bool rotateToZero = DEFAULT_ROTATE_TO_ZERO,
            bool adjustRotation = DEFAULT_ADJUST_ROTATION,
            bool reversible = DEFAULT_REVERSIBLE,
            ScaleMethod scaleMethod = DEFAULT_SCALE_METHOD)
        {
            this.gestures = gestures;
            this.sample = sample;
            this.scaleSize = scaleSize;
            this.theta = theta;
            this.thetaStep = thetaStep;
            this.rotateToZero = rotateToZero;
            this.adjustRotation = adjustRotation;
            this.reversible = reversible;
            this.scaleMethod = scaleMethod;
        }





        public void Add(K key, Unistroke gesture)
        {
            gesture = Filter(gesture);
            if (!gestures.ContainsKey(key))
                gestures[key] = new List<Unistroke>();
            gestures[key].Add(gesture);
        }

        public void ShowInFile(K key)
        {
            if (hiddenKeysInFile.Contains(key))
                hiddenKeysInFile.Remove(key);
        }

        
        
        public Tuple<K, float> Recognize(Unistroke points)
        {
            points = Filter(points);


            float b = float.PositiveInfinity;
            bool found = false;
            K bT = default;
            foreach (Tuple<K, Unistroke> T in GesturesEnumerated())
            {
                float d = adjustRotation ? DistanceAtBestAngle(points, T.Item2, -theta, theta, thetaStep) : Distance(points, T.Item2);
                if (d < b)
                {
                    b = d;
                    bT = T.Item1;
                    found = true;
                }
            }
            return found ? new Tuple<K, float>(bT, 1 - b / (Mathf.Sqrt(2 * scaleSize * scaleSize) / 2)) : null;
        }


        internal IEnumerable<Tuple<K, Unistroke>> GesturesEnumerated()
        {
            foreach (KeyValuePair<K, List<Unistroke>> i in gestures)
            {
                foreach (Unistroke s in i.Value)
                    yield return new Tuple<K, Unistroke>(i.Key, s);
            }
        }






        internal Unistroke Filter(Unistroke input)
        {
            input = input.Resampled(sample);

            if (rotateToZero)
                input = input.RotatedToZero();

            switch (scaleMethod)
            {
                case ScaleMethod.SCALE_TO_SQUARE:
                    input = input.ScaledToSquare(scaleSize);
                    break;
                case ScaleMethod.FIT_IN_SQUARE:
                    input = input.FitInSquare(scaleSize);
                    break;
                default:
                    break;
            }

            return input.TranslatedToOrigin();
        }









        internal float DistanceAtBestAngle(Unistroke points, Unistroke T, float angleA, float angleB, float angleD)
        {
            float x1 = PHI * angleA + (1 - PHI) * angleB;
            float f1 = DistanceAtAngle(points, T, x1);
            float x2 = PHI * angleB + (1 - PHI) * angleA;
            float f2 = DistanceAtAngle(points, T, x2);
            while (Mathf.Abs(angleB - angleA) > angleD)
            {
                if (f1 < f2)
                {
                    angleB = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = PHI * angleA + (1 - PHI) * angleB;
                    f1 = DistanceAtAngle(points, T, x1);
                }
                else
                {
                    angleA = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = PHI * angleB + (1 - PHI) * angleA;
                    f2 = DistanceAtAngle(points, T, x2);
                }
            }
            return Mathf.Min(f1, f2);
        }




        /// <summary>
        /// Rotate points by the specified angle and compute the PathDistance with T
        /// </summary>
        /// <param name="points">the Unistroke to be rotated</param>
        /// <param name="T">the other stroke</param>
        /// <param name="angle">the angle to rotate points, in radian</param>
        /// <returns></returns>
        internal float DistanceAtAngle(Unistroke points, Unistroke T, float angle)
        {
            return Distance(points.RotatedBy(angle), T);
        }

        internal float Distance(Unistroke A, Unistroke B)
        {
            if (A.Count == 0 && B.Count == 0) throw new System.Exception("Unistrokes A and B are empty.");
            if (A.Count == 0) throw new System.Exception("Unistrokes A is empty.");
            if (B.Count == 0) throw new System.Exception("Unistrokes B is empty.");
            if (A.Count != B.Count) throw new System.Exception("The two Unistrokes must have the same size (A.Count = " + A.Count + ", B.Count = " + B.Count + ").");

            float sumD = 0;
            for (int i = 0; i < A.Count; i++)
                sumD += Point.Distance(A[i], B[i]);

            if (!reversible)
                return sumD / A.Count;

            // compute the reversed distance
            float sumDRev = 0;
            for (int i = 0; i < A.Count; i++)
                sumDRev += Point.Distance(A[A.Count - i - 1], B[i]);

            return Mathf.Min(sumD, sumDRev) / A.Count;
        }









        public void Load(string path, Func<string, K> keyConverter)
        {
            if (!File.Exists(path))
                throw new Exception("1$: File '" + path + "' does not exist.");

            gestures.Clear();
            hiddenKeysInFile.Clear();

            int gestCount = 0;

            using (StreamReader r = new StreamReader(path))
            {
                bool hasKey = false;
                K key = default;
                string line;
                int lCount = 0;
                while(++lCount > 0 && (line = r.ReadLine()) != null)
                {
                    string[] lineSplit = line.Split(' ');
                    if (lineSplit[0] == "class")
                    {
                        K k = keyConverter(lineSplit[2]);
                        if (lineSplit[1] == "h")
                            hiddenKeysInFile.Add(k);
                        key = k;
                        hasKey = true;
                        continue;
                    }
                    if (lineSplit[0] == "stroke")
                    {
                        if (!hasKey)
                            throw new Exception("1$: File '" + path + "' defines a stroke without prior definition of a class (line " + lCount + ").");
                        Unistroke stroke = new Unistroke(lineSplit[1]);
                        int count = int.Parse(lineSplit[2], CultureInfo.InvariantCulture);
                        for (int i = 0; i < count; i++)
                        {
                            ++lCount;
                            line = r.ReadLine();
                            lineSplit = line.Split(' ');
                            stroke.Add(new Point(
                                float.Parse(lineSplit[0], CultureInfo.InvariantCulture),
                                float.Parse(lineSplit[1], CultureInfo.InvariantCulture)));
                        }
                        Add(key, stroke);
                        gestCount++;
                        continue;
                    }
                    throw new Exception("1$: File '" + path + "' contains an unrecognized line (line " + lCount + ").");

                }
            }

            Debug.Log("1$: File '" + path + "' loaded with " + gestures.Count + " classes (" + hiddenKeysInFile.Count + " hidden) and " + gestCount + " gestures.");
        }



        public void Save(string path, Func<K, string> keyConverter)
        {
            int gestCount = 0;
            using(StreamWriter o = File.CreateText(path))
            {
                o.NewLine = "\n";
                foreach (KeyValuePair<K, List<Unistroke>> v in gestures)
                {
                    gestCount += v.Value.Count;
                    o.WriteLine("class " + (hiddenKeysInFile.Contains(v.Key) ? "h" : "v") + " " + keyConverter(v.Key));
                    foreach (Unistroke s in v.Value)
                    {
                        o.Write("stroke " + s.Name + " " + s.Count.ToString(CultureInfo.InvariantCulture));
                        o.WriteLine();
                        foreach (Point p in s)
                        {
                            o.Write(p.x.ToString(CultureInfo.InvariantCulture));
                            o.Write(" ");
                            o.Write(p.y.ToString(CultureInfo.InvariantCulture));
                            o.WriteLine();
                        }
                    }
                }
            }
            
            Debug.Log("1$: Saved " + gestures.Count + " classes (" + hiddenKeysInFile.Count + " hidden) and " + gestCount + " gestures in '" + path + "'.");
        }





    }


    
    public class Unistroke : List<Point>
    {
        public string Name { get; set; }

        public Unistroke(string name)
        {
            Name = (name == null || name.Length == 0) ? "_" : name
                .Replace(' ', '_')
                .Replace('\n', '_')
                .Replace('\r', '_')
                .Replace('\t', '_')
                .Replace('\0', '_');
        }

        public Unistroke(Unistroke input) : base(input)
        {
            Name = input.Name;
        }

        public float Length
        {
            get
            {
                if (Count == 0)
                    throw new System.Exception("can’t do that on empty Unistroke.");
                float l = 0;
                for (int i = 1; i < Count; i++)
                    l += Point.Distance(this[i - 1], this[i]);
                return l;
            }
        }

        public Point Centroid
        {
            get
            {
                if (Count == 0)
                    throw new System.Exception("can’t do that on empty Unistroke.");
                Point sum = Point.zero;
                foreach (Point p in this)
                    sum += p;
                return sum / Count;
            }
        }

        public Rect BoundingBox
        {
            get
            {
                if (Count == 0)
                    throw new System.Exception("can’t do that on empty Unistroke.");
                float minX, minY, maxX, maxY;
                minX = maxX = this[0].x;
                minY = maxY = this[0].y;
                for (int i = 1; i < Count; i++)
                {
                    Point p = this[i];
                    if (p.x < minX) minX = p.x;
                    else if (p.x > maxX) maxX = p.x;
                    if (p.y < minY) minY = p.y;
                    else if (p.y > maxY) maxY = p.y;
                }
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }





        internal Unistroke Resampled(uint sample)
        {
            if (Count < 2 && sample >= 2)
                throw new System.Exception("Can't resample stroke that contains less than 2 points.");
            Unistroke points = new Unistroke(this); // clone input because he will be modified
            Unistroke newPoints = new Unistroke(Name);
            float I = points.Length / (sample - 1);
            float D = 0;
            newPoints.Add(points[0]);
            for (int i = 1; i < points.Count; i++)
            {
                float d = Point.Distance(points[i - 1], points[i]);
                if (D + d >= I)
                {
                    Point q = points[i - 1] + ((I - D) / d) * (points[i] - points[i - 1]);
                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0;
                }
                else
                    D += d;
            }
            if (newPoints.Count != sample)
            {   // happend because of floating point number precision.
                // So we just add the last point of rge original stroke to the end of the new one
                newPoints.Add(points[points.Count - 1]);
            }

            return newPoints;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle">in radian</param>
        /// <returns></returns>
        internal Unistroke RotatedBy(float angle)
        {
            Unistroke newPoints = new Unistroke(Name);
            Point c = Centroid;
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);
            foreach (Point p in this)
            {
                Point pc = p - c;
                newPoints.Add(new Point(pc.x * cosA - pc.y * sinA, pc.x * sinA + pc.y * cosA) + c);
            }
            return newPoints;
        }



        internal Unistroke RotatedToZero()
        {
            Point c = Centroid, p0 = this[0];
            return RotatedBy(Mathf.Atan2(c.y - p0.y, c.x - p0.x));
        }



        internal Unistroke ScaledToSquare(float size)
        {
            Rect B = BoundingBox;
            Unistroke newPoints = new Unistroke(Name);
            foreach (Point p in this)
                newPoints.Add(p / B.size * size);
            return newPoints;
        }

        internal Unistroke FitInSquare(float size)
        {
            Rect B = BoundingBox;
            float longestDim = Mathf.Max(B.width, B.height);
            Unistroke newPoints = new Unistroke(Name);
            foreach (Point p in this)
                newPoints.Add(p / longestDim * size);
            return newPoints;
        }

        internal Unistroke TranslatedToOrigin()
        {
            Point c = Centroid;
            Unistroke newPoints = new Unistroke(Name);
            foreach (Point p in this)
                newPoints.Add(p - c);
            return newPoints;
        }








    }
}



