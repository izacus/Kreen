using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace TrackModelProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// </summary>
    [ContentProcessor(DisplayName = "Track Model Processor")]
    public class TMProcessor : ModelProcessor
    {
        List<MeshInfo> information = new List<MeshInfo>();

        public override ModelContent Process(NodeContent node, ContentProcessorContext context)
        {
            ModelContent model = base.Process(node, context);

            ProcessChildren(node);

            int i = 0;

            foreach (ModelMeshContent mesh in model.Meshes)
            {
                mesh.Tag = information[i++];
            } 

            return model;
        }

        private void ProcessChildren(NodeContent node)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;

            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            MeshContent mesh = node as MeshContent;

            foreach (NodeContent child in node.Children)
            {
                ProcessChildren(child);
            }

            if (mesh != null)
            {
                foreach (Vector3 vertex in mesh.Positions)
                {
                    if (vertex.Z < minZ)
                        minZ = vertex.Z;
                    if (vertex.X < minX)
                        minX = vertex.X;
                    if (vertex.Y < minY)
                        minY = vertex.Y;

                    if (vertex.Z > maxZ)
                        maxZ = vertex.Z;
                    if (vertex.X > maxX)
                        maxX = vertex.X;
                    if (vertex.Y > maxY)
                        maxY = vertex.Y;
                }

                Matrix absoluteTransformation = mesh.AbsoluteTransform;
                List<MeshInfo.Triangle> triangleList = new List<MeshInfo.Triangle>();
                
                foreach (GeometryContent geo in mesh.Geometry)
                {
                    int triangles = geo.Indices.Count / 3;

                    for (int currentTriangle = 0; currentTriangle < triangles; currentTriangle++)
                    {
                        int[] indexes = new int[3];
                        
                        for (int i = 0; i < 3; i++)
                        {
                            indexes[i] = geo.Indices[currentTriangle * 3 + i];
                        }

                        Vector3[] vertices = new Vector3[3];
                        Vector3[] transformedVertices = new Vector3[3];

                        for (int i = 0; i < 3; i++)
                        {
                            vertices[i] = geo.Vertices.Positions[indexes[i]];
                            transformedVertices[i] = Vector3.Transform(vertices[i], absoluteTransformation);
                        } 

                        MeshInfo.Triangle triangle = new MeshInfo.Triangle(transformedVertices[0], transformedVertices[1], transformedVertices[2]);
                        triangleList.Add(triangle);
                    }
                }
                
                BoundingBox box = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
                MeshInfo info = new MeshInfo(triangleList, box);

                information.Add(info); 
            }
        }
    }
}