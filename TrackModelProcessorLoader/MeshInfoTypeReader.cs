using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace TrackModelProcessor
{
    public class MeshInfoTypeReader : ContentTypeReader<MeshInfo>
    {
        protected override MeshInfo Read(ContentReader input, MeshInfo existingInstance)
        {
            List<TrackModelProcessor.MeshInfo.Triangle> triangles = new List<MeshInfo.Triangle>();

            int num = input.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                Vector3 p0 = input.ReadObject<Vector3>();
                Vector3 p1 = input.ReadObject<Vector3>();
                Vector3 p2 = input.ReadObject<Vector3>();

                TrackModelProcessor.MeshInfo.Triangle triangle = new MeshInfo.Triangle(p0, p1, p2);
                triangles.Add(triangle);
            }

            BoundingBox box = input.ReadObject<BoundingBox>();
            MeshInfo info = new MeshInfo(triangles, box);

            return info;
        }
    }
}
