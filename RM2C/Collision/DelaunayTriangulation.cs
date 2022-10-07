using g3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    /*
     * Delaunay Triangulation
     * Implementation of the Bowyer-Watson algorithm: https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm
     */
    internal class DelaunayTriangulation
    {
        readonly List<Vector2d> _points;

        readonly ushort _minXpt;
        readonly ushort _maxXpt;
        readonly ushort _minYpt;
        readonly ushort _maxYpt;

        double MinX => _points[_minXpt].x;
        double MaxX => _points[_maxXpt].x;
        double MinY => _points[_minYpt].y;
        double MaxY => _points[_maxYpt].y;

        public DelaunayTriangulation(IEnumerable<Vector2d> points)
        {
            _points = points.ToList();

            for (ushort i = 0; i < _points.Count; ++i)
            {
                var point = _points[i];
                if (point.x < MinX || (point.x == MinX && point.y < _points[_minXpt].y))
                    _minXpt = i;
                if (point.x > MaxX || (point.x == MaxX && point.y > _points[_maxXpt].y))
                    _maxXpt = i;
                if (point.y < MinY || (point.y == MinY && point.x < _points[_minYpt].x))
                    _minYpt = i;
                if (point.y > MaxY || (point.y == MaxY && point.x > _points[_maxYpt].x))
                    _maxYpt = i;
            }
        }

        // Triangulation using Bowyer-Watson algorithm
        public List<ColTriangle> BowyerWatson()
        {
            List<ColTriangle> triangulation = new();
            Triangle2d superTriangleTri = GenerateSuperTriangle();
            ColTriangle superTriangle = new(0, 0, 0);

            superTriangle.VertId1 = (ushort)_points.Count;
            _points.Add(superTriangleTri.V0);
            superTriangle.VertId2 = (ushort)_points.Count;
            _points.Add(superTriangleTri.V1);
            superTriangle.VertId3 = (ushort)_points.Count;
            _points.Add(superTriangleTri.V2);

            triangulation.Add(superTriangle);

            for (ushort i = 0; i < _points.Count; ++i)
            {
                // Find all the triangles that are no longer valid due to the insertion
                List<ColTriangle> badTriangles = FindBadTriangles(i, triangulation);

                // Find the boundary of the polygonal hole
                List<ColEdge> polygon = FindPolygonalHole(badTriangles);

                // Remove the bad triangles
                triangulation = triangulation.Except(badTriangles).ToList();

                // Re-triangulate the polygonal hole
                foreach (var edge in polygon)
                    triangulation.Add(new ColTriangle(i, edge.VertId1, edge.VertId2));
            }

            // Done inserting points, now clean up
            List<ColTriangle> toRemove = new();
            foreach (var triangle in triangulation)
            {
                // If triangle contains a vertex from original super triangle, remove it from triangulation
                ushort[] sprTri = superTriangle.VertIds;
                foreach (ushort vertId in triangle.VertIds)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        if (vertId == sprTri[i])
                            toRemove.Add(triangle);
                    }
                }
            }
            triangulation = triangulation.Except(toRemove).ToList();

            _points.RemoveRange(_points.Count - 3, 3);

            return triangulation;
        }

        public Triangle2d GenerateSuperTriangle()
        {
            return new Triangle2d(
                new Vector2d((MinX + MaxX) / 2f, int.MinValue),
                new Vector2d(int.MinValue, int.MaxValue),
                new Vector2d(int.MaxValue, int.MaxValue)
            );
        }

        List<ColTriangle> FindBadTriangles(int pointId, List<ColTriangle> triangles)
        {
            List<ColTriangle> badTriangles = new();

            foreach (var tri in triangles)
            {
                Triangle2d triangle = new(_points[tri.VertId1], _points[tri.VertId2], _points[tri.VertId3]);
                if (IsPointInsideCircumcircle(_points[pointId], triangle))
                    badTriangles.Add(tri);
            }

            return badTriangles;
        }

        static List<ColEdge> FindPolygonalHole(List<ColTriangle> badTriangles)
        {
            List<ColEdge> polygon = new();

            // Find all the edges that are not shared by any other triangles in badTriangles
            foreach (var triangle in badTriangles)
            {
                var edge1 = new ColEdge(triangle.VertId1, triangle.VertId2);
                var edge2 = new ColEdge(triangle.VertId2, triangle.VertId3);
                var edge3 = new ColEdge(triangle.VertId3, triangle.VertId1);

                foreach (var edge in new ColEdge[] { edge1, edge2, edge3 })
                {
                    if (polygon.Contains(edge))
                        polygon.Remove(edge);
                    else
                        polygon.Add(edge);
                }
            }

            return polygon;
        }

        static bool IsPointInsideCircumcircle(Vector2d point, Triangle2d triangle)
        {
            // Get Circumcenter
            // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
            // https://en.wikipedia.org/wiki/Circumscribed_circle
            var p0 = triangle.V0;
            var p1 = triangle.V1;
            var p2 = triangle.V2;
            var dA = p0.x * p0.x + p0.y * p0.y;
            var dB = p1.x * p1.x + p1.y * p1.y;
            var dC = p2.x * p2.x + p2.y * p2.y;

            var aux1 = (dA * (p2.y - p1.y) + dB * (p0.y - p2.y) + dC * (p1.y - p0.y));
            var aux2 = -(dA * (p2.x - p1.x) + dB * (p0.x - p2.x) + dC * (p1.x - p0.x));
            var div = (2 * (p0.x * (p2.y - p1.y) + p1.x * (p0.y - p2.y) + p2.x * (p1.y - p0.y)));

            if (div == 0)
                return false;

            var center = new Vector2d(aux1 / div, aux2 / div);
            var radiusSquared = (center.x - p0.x) * (center.x - p0.x) + (center.y - p0.y) * (center.y - p0.y);

            // Is Point Inside Circumcircle
            var dSquared = (point.x - center.x) * (point.x - center.x) + (point.y - center.y) * (point.y - center.y);
            return dSquared < radiusSquared;
        }

        // Convex hull algorithm
        public List<ushort> QuickHull()
        {
            List<ushort> convexHull = new();

            // Add left and right most points to the hull (say AB)
            var a = _minXpt;
            var b = _maxXpt;
            convexHull.Add(a);
            convexHull.Add(b);

            // Segment AB divides the reminaing n-2 points into 2 groups; S1 and S2
            // - S1 are points in S that are on the right side of the oriented line AB
            // - S2 are points in S that are on the left side of the oriented line AB (or right side of BA)
            List<ushort> ptIds = new();
            for (ushort i = 0; i < _points.Count; ++i)
                    ptIds.Add(i);

            var (S1, S2) = FindRightAndLeftPoints(ptIds, a, b);

            FindHull(convexHull, S1, a, b);
            FindHull(convexHull, S2, b, a);

            return convexHull;
        }

        void FindHull(List<ushort> convexHull, List<ushort> points, ushort p, ushort q)
        {
            if (points.Count == 0)
                return;

            var P = _points[p];
            var Q = _points[q];

            // From the given set of points, find the farthest point C from PQ
            var maxDist = 0.0;
            ushort c = 0;
            foreach (var i in points)
            {
                var pt = _points[i];
                var dist = Segment2d.FastDistanceSquared(ref P, ref Q, ref pt);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    c = i;
                }
            }

            // Add C to convex hull at the location between P and Q
            var index = convexHull.IndexOf(p);
            convexHull.Insert(index + 1, c);

            var (S1, _) = FindRightAndLeftPoints(points, p, c);
            var (S2, _) = FindRightAndLeftPoints(points, c, q);

            FindHull(convexHull, S1, p, c);
            FindHull(convexHull, S2, c, q);
        }

        (List<ushort>, List<ushort>) FindRightAndLeftPoints(List<ushort> points, ushort c, ushort d)
        {
            Segment2d line = new(_points[c], _points[d]);

            List<ushort> right = new();
            List<ushort> left = new();

            foreach (ushort i in points)
            {
                if (i == c || i == d)
                    continue;

                var pt = _points[i];
                var side = line.WhichSide(pt);

                if (side > 0)
                    right.Add(i);
                else
                    left.Add(i);
            }

            return (right, left);
        }
    }
}
