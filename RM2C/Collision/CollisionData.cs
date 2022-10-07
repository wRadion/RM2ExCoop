using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using g3;

namespace RM2ExCoop.RM2C
{
    internal class CollisionData
    {
        public static readonly ushort[] Specials = new ushort[] { 0x0E, 0x24, 0x25, 0x27, 0x2C, 0x2D };

        public readonly List<Vector3i> Verts;
        public readonly Dictionary<ushort, List<ColTriangle>> Tris;
        public readonly List<Vector3i> DPV;

        public CollisionData()
        {
            Verts = new();
            Tris = new();
            DPV = new();
        }

        public void InitType(ushort type)
        {
            if (Tris.ContainsKey(type))
                return;
            Tris[type] = new List<ColTriangle>();
        }

        public void Write(StreamWriter colFile)
        {
            SplitCrossQuadrant();

            int vCount = Verts.Count + DPV.Count;
            colFile.WriteLine($"COL_VERTEX_INIT({vCount}),");

            foreach (var v in Verts)
                colFile.WriteLine($"COL_VERTEX( {v.x}, {v.y}, {v.z}),");

            foreach (var v in DPV)
                colFile.WriteLine($"COL_VERTEX( {v.x}, {v.y}, {v.z}),");

            foreach (var (k, v) in Tris)
            {
                colFile.WriteLine($"COL_TRI_INIT( {k}, {v.Count}),");

                if (Specials.Contains(k))
                {
                    foreach (var t in v)
                        colFile.WriteLine($"COL_TRI_SPECIAL( {t.ToString(true)}),");
                }
                else
                {
                    foreach (var t in v)
                        colFile.WriteLine($"COL_TRI( {t.ToString()}),");
                }
            }

            colFile.WriteLine("COL_TRI_STOP(),");
        }

        void SplitCrossQuadrant()
        {
            if (!Tris.ContainsKey(10))
                return;

            List<ColTriangle> newTri = new();
            int offset = Verts.Count;

            // Only check death plane for now
            foreach (var tri in Tris[10])
            {
                List<Vector3i> newV = new();
                List<Vector3d> verts = tri.VertIds.Select(id => (Vector3d)Verts[id]).ToList();
                int area = (int)MathUtil.Area(verts[0], verts[1], verts[2]);

                // There are some hackers who have reasonable sized tris
                // Don't split them up. This value is half the area of a death plane tri.
                if (area < 268_419_072)
                    continue;

                var edges = new Segment3d[] { new Segment3d(verts[0], verts[1]), new Segment3d(verts[1], verts[2]), new Segment3d(verts[2], verts[0]) };

                foreach (var edge in edges)
                {
                    // Split X Edge
                    if (Math.Max(edge.P0.x, edge.P1.x) > 0 && Math.Min(edge.P0.x, edge.P1.x) < 0)
                    {
                        AddLerpPointX((Vector3i)edge.P0, (Vector3i)edge.P1, newV);
                        AddLerpPointX((Vector3i)edge.P1, (Vector3i)edge.P0, newV);
                    }

                    // Split Z Edge
                    if (Math.Max(edge.P0.z, edge.P1.z) > 0 && Math.Min(edge.P0.z, edge.P1.z) < 0)
                    {
                        AddLerpPointZ((Vector3i)edge.P0, (Vector3i)edge.P1, newV);
                        AddLerpPointZ((Vector3i)edge.P1, (Vector3i)edge.P0, newV);
                    }
                }

                if (newV.Count > 0)
                {
                    DPV.AddRange(newV);
                    newTri.AddRange(MakeNewTris(newV, offset));
                    offset += newV.Count;
                }
                else
                    newTri.Add(tri);
            }

            if (newTri.Count > 0)
                Tris[10] = newTri;
        }

        static List<ColTriangle> MakeNewTris(List<Vector3i> pts, int offset)
        {
            List<ColTriangle> tris = new();

            IEnumerable<Vector2d> pts2d = pts.Select(v => new Vector2d(v.x, v.z));
            DelaunayTriangulation delaunay = new(pts2d);
            var triangles = delaunay.BowyerWatson();

            foreach (var tri in triangles)
            {
                var v1 = (Vector3d)pts[tri.VertId1];
                var v2 = (Vector3d)pts[tri.VertId2];
                var v3 = (Vector3d)pts[tri.VertId3];

                // Check Normal
                var cp = (v2 - v1).Cross(v3 - v1);

                if (cp.y > 0)
                    tris.Add(tri.Offset((ushort)offset));
                else
                    tris.Add(new ColTriangle(tri.VertId1, tri.VertId3, tri.VertId2).Offset((ushort)offset));
            }
 
            return tris;
        }

        // Rename this please
        static void AddLerpPointX(Vector3i pt, Vector3i other, List<Vector3i> newV)
        {
            int lerp = other.x - pt.x != 0 ? (pt.z * other.x - pt.x * other.z) / (other.x - pt.x) : 0;
            Vector3i lerpPt = new(0, pt.y, lerp); // todo: rename this

            if (pt.x > 0)
            {
                if (!newV.Contains(pt)) newV.Add(pt);
            }
            else
                if (!newV.Contains(lerpPt)) newV.Add(lerpPt);

            if (pt.x < 0)
            {
                if (!newV.Contains(pt)) newV.Add(pt);
            }
            else
                if (!newV.Contains(lerpPt)) newV.Add(lerpPt);
        }

        // Rename this please
        static void AddLerpPointZ(Vector3i pt, Vector3i other, List<Vector3i> newV)
        {
            int lerp = other.z - pt.z != 0 ? (pt.x * other.z - pt.z * other.x) / (other.z - pt.z) : 0;
            Vector3i lerpPt = new(lerp, pt.y, 0);

            if (pt.z > 0)
            {
                if (!newV.Contains(pt)) newV.Add(pt);
            }
            else
                if (!newV.Contains(lerpPt)) newV.Add(lerpPt);

            if (pt.z < 0)
            {
                if (!newV.Contains(pt)) newV.Add(pt);
            }
            else
                if (!newV.Contains(lerpPt)) newV.Add(lerpPt);
        }
    }
}
