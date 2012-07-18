using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace LowestPointModelProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Lowest Mesh point processor")]
    public class LPProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            List<Vector3> lowestPoints = new List<Vector3>();

            lowestPoints = FindLowestPoints(input, lowestPoints);

            ModelContent model = base.Process(input, context);

            int i = 0;

            foreach (ModelMeshContent mesh in model.Meshes)
            {
                mesh.Tag = lowestPoints[i++];
            }

            return model;
        }

        private List<Vector3> FindLowestPoints(NodeContent node, List<Vector3> lowestPoints)
        {
            Vector3? lowestPos = null;
            MeshContent mesh = node as MeshContent;

            foreach (NodeContent child in node.Children)
            {
                lowestPoints = FindLowestPoints(child, lowestPoints);
            }

            if (mesh != null)
            {
                foreach (GeometryContent geo in mesh.Geometry)
                {
                    foreach (Vector3 vertexPos in geo.Vertices.Positions)
                    {
                        if (lowestPos == null ||
                            (vertexPos.Y < lowestPos.Value.Y))
                        {
                            lowestPos = vertexPos;
                        }
                    }
                }

                lowestPoints.Add(lowestPos.Value);
            }

            return lowestPoints;
        }
    }
}