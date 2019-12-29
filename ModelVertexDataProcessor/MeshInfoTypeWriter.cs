using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace TrackModelProcessor
{
    [ContentTypeWriter]
    public class MeshInfoTypeWriter : ContentTypeWriter<MeshInfo>
    {
        protected override void Write(ContentWriter output, MeshInfo value)
        {
            output.Write(value.Triangles.Count);
            for (int i = 0; i < value.Triangles.Count; i++)
            {
                output.WriteObject(value.Triangles[i].P0);
                output.WriteObject(value.Triangles[i].P1);
                output.WriteObject(value.Triangles[i].P2);
            }
            output.WriteObject(value.Box);
        }

        public override string GetRuntimeReader(Microsoft.Xna.Framework.Content.Pipeline.TargetPlatform targetPlatform)
        {
            return typeof (MeshInfoTypeReader).AssemblyQualifiedName;
        }
    }
}
