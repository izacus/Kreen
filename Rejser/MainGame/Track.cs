using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TrackModelProcessor;
using System.Collections;

namespace Kreen.MainGame
{
    class Track
    {
        // Drawing propertise
        private GraphicsDevice graphicsDevice;
        private TrackInfo trackInfo;
        private Model model;
        private Hashtable trackTextures;
        private Matrix[] transforms;
        private Effect effect;

        private BoundingBox[] boundingBoxes;

        private string trackPath = "";
        private string modelName = "";

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            ChangeEffect(ref model, effect);
        }

        public void setTrack(TrackInfo trackInfo)
        {
            this.trackInfo = trackInfo;
        }

        public void LoadContent(ContentManager contentManager)
        {
            // Load model
            model = contentManager.Load<Model>(trackInfo.Model);
            effect = contentManager.Load<Effect>(@"Effects\ShadowMapping");

            trackTextures = new Hashtable();

            foreach (ModelMesh mesh in model.Meshes)
            {
                Texture2D texture = contentManager.Load<Texture2D>(trackInfo.TexturePath + "\\" + mesh.Name);
                trackTextures.Add(mesh.Name, texture);
            }

            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            boundingBoxes = new BoundingBox[model.Bones.Count];

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                BoundingBox meshBox = ((MeshInfo)model.Meshes[i].Tag).Box;
                boundingBoxes[model.Meshes[i].ParentBone.Index] = new BoundingBox(Vector3.Transform(meshBox.Min, transforms[model.Meshes[i].ParentBone.Index]), Vector3.Transform(meshBox.Max, transforms[model.Meshes[i].ParentBone.Index]));
            }
        }

        public void Draw(ref Matrix world, 
                         ref Matrix projection, 
                         ref Matrix view,
                         string technique,
                         ref Matrix lightViewProjection, 
                         ref Texture2D shadowMap,
                         ref Vector3 lightDirection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {

                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["LightViewProjection"].SetValue(lightViewProjection);
                    effect.Parameters["LightDirection"].SetValue(lightDirection);
                    effect.Parameters["ShadowMap"].SetValue(shadowMap);
                    effect.Parameters["Texture"].SetValue((Texture2D)trackTextures[mesh.Name]);
                    effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world);
                }
                 
                mesh.Draw();
            }
        }

        private void ChangeEffect(ref Model model, Effect newEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart parts in mesh.MeshParts)
                {
                    parts.Effect = effect.Clone(graphicsDevice);
                }
            }
        }

        #region Getters and setters

        public Model TrackModel
        {
            get { return this.model; }
        }

        public Matrix[] Transforms
        {
            get { return this.transforms; }
        }

        public BoundingBox[] BoundingBoxes
        {
            get { return this.boundingBoxes; }
        }

        #endregion
    }
}
