using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ModelTriangleBoxProcessor
{
    public class MeshInfo
    {
        public class Triangle
        {
            private Vector3[] points;

            public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                this.points = new Vector3[3];
                points[0] = p0;
                points[1] = p1;
                points[2] = p2;
            }

            public Vector3[] Points
            { get { return this.points; } }

            public Vector3 P0
            { get { return this.points[0]; } }
            public Vector3 P1
            { get { return this.points[1]; } }
            public Vector3 P2
            { get { return this.points[2]; } }
        }

        private readonly List<Triangle> triangles;
        private readonly BoundingBox boundingBox;

        public MeshInfo(List<Triangle> triangles, BoundingBox boundingBox)
        {
            this.triangles = triangles;
            this.boundingBox = boundingBox;
        }

        public List<Triangle> Triangles
        {
            get { return triangles;  }
        }

        public BoundingBox Box
        {
            get { return boundingBox; }
        }
    }
}
