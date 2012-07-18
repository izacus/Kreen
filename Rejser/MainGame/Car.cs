using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Kreen.StateManager;
using TrackModelProcessor;

namespace Kreen.MainGame
{
    /// <summary>
    /// Represents a racing car
    /// </summary>
    class Car
    {
        /// <summary>
        /// Represents a single racing car's wheel
        /// </summary>
        public class Wheel
        {
            private BoundingBox wheelBox;
            private ModelMesh wheelMesh;
            private Vector3 position;

            private float rollAngle = 0.0f;
            private Matrix transformationMatrix = Matrix.Identity;
             
            public Wheel(BoundingBox wheelBox, ModelMesh wheelMesh)
            {
                this.wheelBox = wheelBox;
                this.wheelMesh = wheelMesh;
            }

            /// <summary>
            /// Updates wheel position, transformation and bounding box
            /// </summary>
            public void Update(Matrix[] transforms, ref Matrix world)
            {
                Matrix positionTranslation = transforms[this.wheelMesh.ParentBone.Index] * world;
                this.position = Vector3.Transform(Vector3.Zero, positionTranslation);

                BoundingBox meshBox = ((MeshInfo)wheelMesh.Tag).Box;

                wheelBox.Min = Vector3.Transform(meshBox.Min, transforms[wheelMesh.ParentBone.Index]);
                wheelBox.Max = Vector3.Transform(meshBox.Max, transforms[wheelMesh.ParentBone.Index]);
            }

            /// <summary>
            /// Creates new wheel roll and direction rotation transformation
            /// </summary>
            /// <param name="angle">Angle to roll wheel with</param>
            /// <param name="directionAngle">Wheel direction angle</param>
            public void RotateWheel(float angle, float directionAngle)
            {
                rollAngle += angle;

                if (rollAngle > 2 * Math.PI)
                {
                    rollAngle = (float)(rollAngle % 2 * Math.PI);
                }

                Quaternion rollRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), rollAngle);
                Quaternion directionRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), directionAngle);

                transformationMatrix = Matrix.CreateFromQuaternion(directionRotation * rollRotation);
            }

            #region Getters and setters
            public BoundingBox Box
            {
                get { return this.wheelBox; }
            }

            public Matrix TransformationMatrix
            {
                get { return this.transformationMatrix; }
            }

            public Vector3 Position
            {
                get { return this.position;  }
                set { this.position = value; }
            }

            public ModelMesh Mesh
            {
                get { return this.wheelMesh;  }
            }

            #endregion
        }

        private StateGame mainGameState;
        private CarInfo carInfo;
        private Vector3 position;
        private Wheel[] wheels;

        // Model world transform
        private Matrix modelWorld;

        #region Graphics
        private ContentManager contentManager;
        private Model model;
        Matrix[] transforms;
        private Texture2D textureBody;
        private Texture2D textureCockpit;
        private Texture2D textureRim;
        private Texture2D textureWheel;
        private Matrix scalingMatrix;
        private Effect effect;

        // Difference in wheel heights
        private float wheelDelta;
        // "Actual" model position and wheel cross position difference
        private Vector3 positionOffset;

        // Resource names
        private Vector3 trackStartingPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        public Car(StateGame mainGameState)
        {
            this.mainGameState = mainGameState;
            this.scalingMatrix = Matrix.CreateScale(0.2f);
        }

        public void setCar(CarInfo carInfo, Vector3 carStartingPosition)
        {
            this.trackStartingPosition = carStartingPosition;
            this.position = carStartingPosition;

            this.carInfo = carInfo;
        }

        /// <summary>
        /// Content loader
        /// </summary>
        public void LoadContent(GameStateManager gameStateManager)
        {
            contentManager = new ContentManager(gameStateManager.Game.Services, "Content");

            // Load model
            model = contentManager.Load<Model>(carInfo.Model);
            effect = contentManager.Load<Effect>(@"Effects\ShadowMapping");

            ChangeEffect(ref model, effect);

            transforms = new Matrix[model.Bones.Count];
            // Copy relevant model transforms
            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Load textures
            textureBody = contentManager.Load<Texture2D>(carInfo.BodyTexture);
            textureCockpit = contentManager.Load<Texture2D>(carInfo.CockpitTexture);
            textureRim = contentManager.Load<Texture2D>(carInfo.RimTexture);
            textureWheel = contentManager.Load<Texture2D>(carInfo.WheelTexture);

            // Create wheel objects
            wheels = new Wheel[4];
            wheels[0] = new Wheel(((MeshInfo)model.Meshes["lf_guma"].Tag).Box, model.Meshes["lf_guma"]);
            wheels[1] = new Wheel(((MeshInfo)model.Meshes["rf_guma"].Tag).Box, model.Meshes["rf_guma"]);
            wheels[2] = new Wheel(((MeshInfo)model.Meshes["lb_guma"].Tag).Box, model.Meshes["lb_guma"]);
            wheels[3] = new Wheel(((MeshInfo)model.Meshes["rb_guma"].Tag).Box, model.Meshes["rb_guma"]);

            setupWheelsOnPosition();
        }

        public void UnloadContent()
        {
            this.contentManager.Unload();
        }

        /// <summary>
        /// Sets corrent wheel positions based on "actual" model position
        /// </summary>
        private void setupWheelsOnPosition()
        {
            Vector3 wheelCaluculatedPosition = Vector3.Zero;

            model.Root.Transform = Matrix.Identity;
            model.CopyAbsoluteBoneTransformsTo(transforms);

            for (int i = 0; i < wheels.Length; i++)
            {
                Matrix positionTranslation = transforms[wheels[i].Mesh.ParentBone.Index] * Matrix.CreateTranslation(this.position);
                wheels[i].Position = Vector3.Transform(Vector3.Zero, positionTranslation);
                wheelCaluculatedPosition += wheels[i].Position;
            }

            // Get position offset for the model
            wheelCaluculatedPosition /= 4.0f;
            positionOffset = position - wheelCaluculatedPosition;
            Debug.WriteLine("Calculated position offset: " + positionOffset);


            Vector3 front = (wheels[0].Position + wheels[1].Position) / 2.0f;
            Vector3 back = (wheels[2].Position + wheels[3].Position) / 2.0f;
            Vector3 backToFront = front - back;
            wheelDelta = back.Y - front.Y;
        }

        /// <summary>
        /// Draw car
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="projection">Projection matrix</param>
        /// <param name="view">View matrix</param>
        /// <param name="technique">Technique to render the model with</param>
        /// <param name="lightViewProjection">View * projection matrix of the lights perspective</param>
        /// <param name="shadowMap">Shadow map texture</param>
        /// <param name="lightDirection">Directional light direction vector</param>
        public void Draw(ref Matrix world, 
                         ref Matrix projection, 
                         ref Matrix view, 
                         string technique, 
                         ref Matrix lightViewProjection,
                         ref Texture2D shadowMap,
                         ref Vector3 lightDirection)
        {
            Debug.WriteLine(position);

            // Draw pre-set submeshes
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Setup HLSL shader parameters
                    effect.CurrentTechnique = effect.Techniques[technique];
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["LightViewProjection"].SetValue(lightViewProjection);
                    effect.Parameters["LightDirection"].SetValue(lightDirection);
                    effect.Parameters["ShadowMap"].SetValue(shadowMap);

                    // Draw correct texture and transform
                    switch(mesh.ParentBone.Name)
                    {
                        case "lf_guma":
                            effect.Parameters["Texture"].SetValue(textureWheel);
                            effect.Parameters["World"].SetValue(wheels[0].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "rf_guma":
                            effect.Parameters["Texture"].SetValue(textureWheel);
                            effect.Parameters["World"].SetValue(wheels[1].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "lb_guma":
                            effect.Parameters["Texture"].SetValue(textureWheel);
                            effect.Parameters["World"].SetValue(wheels[2].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "rb_guma":
                            effect.Parameters["Texture"].SetValue(textureWheel);
                            effect.Parameters["World"].SetValue(wheels[3].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "body":
                            effect.Parameters["Texture"].SetValue(textureBody);
                            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                            break;
                        case "cockpit":
                            effect.Parameters["Texture"].SetValue(textureCockpit);
                            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                            break; 
                        case "lf_rim":
                            effect.Parameters["Texture"].SetValue(textureRim);
                            effect.Parameters["World"].SetValue(wheels[0].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "rf_rim":
                            effect.Parameters["Texture"].SetValue(textureRim);
                            effect.Parameters["World"].SetValue(wheels[1].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "rb_rim":
                            effect.Parameters["Texture"].SetValue(textureRim);
                            effect.Parameters["World"].SetValue(wheels[2].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "lb_rim":
                            effect.Parameters["Texture"].SetValue(textureRim);
                            effect.Parameters["World"].SetValue(wheels[3].TransformationMatrix * transforms[mesh.ParentBone.Index]);
                            break;
                        case "spojler":
                            effect.Parameters["Texture"].SetValue(textureCockpit);
                            effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                            break;
                    }
                }

                mesh.Draw();
            }
        }

        public Vector3 Position
        {
            get { return position; }
            set { this.position = value; }
        }

        #endregion

        #region Physics

        // Keystroke status
        private bool wheelLeft = false;
        private bool wheelRight = false;
        private bool accelerate = false;
        private bool decelerate = false;

        // Constants
        private Vector3 gravity = new Vector3(0, -0.035f, 0);

        // Car velocity and facing
        private Vector3 velocity = Vector3.Zero;
        private Vector3 facing = Vector3.Forward;

        // Wheel direction angle
        private float wheelAngle = 0.0f;
        // Car current speed
        private float speed = 0.0f;

        /// <summary>
        /// Applies gravity to the car
        /// </summary>
        private void ApplyGravity(GameTime gameTime, ref Matrix world)
        {
            // Iterate over wheels and move them
            for (int i = 0; i < wheels.Length; i++)
            {
                Wheel currentWheel = wheels[i];

                // Increase velocity by gravity
                velocity += gravity;

                // Create movement vector
                Vector3 move = velocity * gameTime.ElapsedGameTime.Milliseconds;

                // Check distance to nearest triangle in movement direction
                float? distance = CollisionDetection.CheckWheelTrackCollision(currentWheel,
                                                                              mainGameState.RaceTrack,
                                                                              move);

                Debug.WriteLine("Wheel " + i + ": " + distance);

                // If there is a distance
                if (distance.HasValue)
                {
                    distance -= 2.0f;
                    // Valid move, free space.
                    if (distance.Value > move.Length())
                    {
                        currentWheel.Position += move;
                    }
                    // Move is invalid, the distance is smaller than move, do a partial move
                    else if (distance.Value > 0f)
                    {
                        currentWheel.Position += new Vector3(0, -distance.Value, 0);
                    }
                    // We're sunken under the track, do a reverse movement
                    else if (distance.Value < 0f)
                    {
                        currentWheel.Position += new Vector3(0, Math.Abs(distance.Value), 0);
                    }
                }
                else
                {
                    currentWheel.Position += move;
                }
            }

            // Apply all model updates
            UpdateModelWorld(ref world);

            Debug.WriteLine("===============================================");
        }

        private void ApplyMovement(GameTime gameTime)
        {
            // Acceleration
            if (accelerate)
            {
                if (speed >= 0)
                    speed += (carInfo.TopSpeed - speed) / 15f;
                else
                    speed += 0.5f;
            }

            // Decceleration
            if (decelerate)
            {
                if (speed > 0)
                    speed -= 0.5f;
                else
                    speed -= (carInfo.TopSpeed - Math.Abs(speed)) / 15f;
            }

            // "Friction"
            if (!accelerate && !decelerate)
            {
                if (speed > 0)
                    speed -= 0.2f;
                else
                    speed += 0.2f;
            }

            // Prevent rounding errors
            if (Math.Abs(speed) < 0.3f)
            {
                speed = 0.0f;
            }

            Debug.WriteLine("Speed: " + speed);
            
            Vector3 move = speed * facing * (gameTime.ElapsedGameTime.Milliseconds / 10.0f);

            // Change wheel angle based on pressed keys
            if (wheelLeft)
                wheelAngle = Math.Min(wheelAngle + 0.4f * (gameTime.ElapsedGameTime.Milliseconds / 100.0f), 0.7f);

            if (wheelRight)
                wheelAngle = Math.Max(wheelAngle - 0.4f * (gameTime.ElapsedGameTime.Milliseconds / 100.0f), -0.7f);

            if (!wheelLeft && !wheelRight)
            {
                if (wheelAngle > 0)
                    wheelAngle -= 0.4f * (gameTime.ElapsedGameTime.Milliseconds / 100.0f);
                if (wheelAngle < 0)
                    wheelAngle += 0.4f * (gameTime.ElapsedGameTime.Milliseconds / 100.0f);
            }

            if (Math.Abs(wheelAngle) < 0.01)
                wheelAngle = 0;

            // Change car facing
            facing = Vector3.Transform(facing, Matrix.CreateRotationY(wheelAngle * speed / carInfo.TurnDivisor));

            // Update wheel model rotations
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].Position += move;

                if (i < 2)
                {
                    wheels[i].RotateWheel((speed < 0 ? -move.LengthSquared() : move.LengthSquared()) / 10.0f, wheelAngle);
                }
                else
                {
                    wheels[i].RotateWheel((speed < 0 ? -move.LengthSquared() : move.LengthSquared()) / 10.0f, 0);
                }
            }
        }

        public void Update(GameTime gameTime, ref Matrix world)
        {
            ApplyGravity(gameTime, ref world);
            ApplyMovement(gameTime);

            UpdateModelWorld(ref world);

            // Check if the user fell off the track
            if (position.Y < -2000.0f)
            {
                Debug.WriteLine("RESETTING!");
                speed = 0;
                wheelAngle = 0;
                position = trackStartingPosition;
                velocity = Vector3.Zero;
                facing = Vector3.Forward;

                for (int i = 0; i < wheels.Length; i++)
                {
                    wheels[i].RotateWheel(0, 0);
                }

                CollisionDetection.setupTaggedSegments(mainGameState.RaceTrack);

                setupWheelsOnPosition();
                UpdateModelWorld(ref world);
            }

            Debug.WriteLine("Facing : " + facing);
        }

        private void UpdateModelWorld(ref Matrix world)
        {
            // Determine all required transformations

            // FRONT/BACK ROTATION
            Vector3 front = (wheels[0].Position + wheels[1].Position) / 2.0f;
            Vector3 back = (wheels[2].Position + wheels[3].Position) / 2.0f;
            Vector3 backToFront = front - back;
            float yDelta = back.Y - front.Y - wheelDelta;
            float fbAngle = (float)Math.Atan2(yDelta, backToFront.Length());


            Quaternion bfRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), fbAngle);

            // Update model position
            Quaternion facingRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
                                                                       (float)(Math.Atan2(facing.X, facing.Z) + Math.PI));



            // LEFT-RIGHT ROTATION
            Vector3 left = (wheels[0].Position + wheels[2].Position) / 2.0f;
            Vector3 right = (wheels[1].Position + wheels[3].Position) / 2.0f;
            Vector3 rightToLeft = left - right;

            float horizDelta = left.Y - right.Y;
            float rlAngle = (float) Math.Atan2(horizDelta, rightToLeft.Length());

            Quaternion rlRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), rlAngle);

            Quaternion totalRotations = bfRotation * rlRotation * facingRotation;
            Matrix rotationMatrix = Matrix.CreateFromQuaternion(totalRotations);
            // Translate
            Vector3 pos = Vector3.Zero;

            // Calculate model position
            for (int i = 0; i < wheels.Length; i++)
            {
                pos += wheels[i].Position;
            }

            pos /= 4.0f;
            position = pos + Vector3.Transform(positionOffset, rotationMatrix);

            Matrix translationMatrix = Matrix.CreateTranslation(position);

            modelWorld = rotationMatrix * translationMatrix * world;
            model.Root.Transform = modelWorld;
            model.CopyAbsoluteBoneTransformsTo(transforms);


            // Update wheels
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].Update(transforms, ref world);
            }
        }

        #region Getters and setters
        public bool WheelLeft
        {
            get { return wheelLeft; }
            set { this.wheelLeft = value; }
        }

        public bool WheelRight
        {
            get { return wheelRight; }
            set { this.wheelRight = value; }
        }

        public bool Accelerating
        {
            get { return accelerate; }
            set { this.accelerate = value; }
        }

        public bool Deccelerating
        {
            get { return decelerate; }
            set { this.decelerate = value; }
        }

        public Vector3 Facing
        {
            get { return this.facing; }
        }
        #endregion

        #endregion

        private void ChangeEffect(ref Model model, Effect newEffect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart parts in mesh.MeshParts)
                {
                    parts.Effect = effect.Clone(mainGameState.GameStateManager.GraphicsDevice);
                }
            }
        }
    }
}
