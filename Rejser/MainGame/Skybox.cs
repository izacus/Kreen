using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections;

namespace Kreen.MainGame
{
    class Skybox
    {
        private GraphicsDevice graphicsDevice;
        private Model model;
        private Texture2D texture;

        private Matrix positionTransformation;

        private Matrix[] transforms;

        public void LoadContent(ContentManager contentManager)
        {
            model = contentManager.Load<Model>(@"Models\skybox");
            transforms = new Matrix[model.Bones.Count];

            texture = contentManager.Load<Texture2D>(@"Textures\skymap");
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }


        public void Draw(ref Matrix world, ref Matrix projection, ref Matrix view, ref Vector3 cameraPosition)
        {
            graphicsDevice.RenderState.DepthBufferEnable = false;
            graphicsDevice.RenderState.DepthBufferWriteEnable = false;

            positionTransformation = Matrix.CreateTranslation(cameraPosition);

            // MODEL SETUP
            model.Root.Transform = positionTransformation * world;
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = texture;

                    effect.World = transforms[mesh.ParentBone.Index];
                    effect.Projection = projection;
                    effect.View = view;
                }

                mesh.Draw();
            }

            graphicsDevice.RenderState.DepthBufferEnable = true;
            graphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }
    }
}
