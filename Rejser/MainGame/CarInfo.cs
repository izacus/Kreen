using System;
using System.Collections.Generic;
using System.Text;

namespace Kreen.MainGame
{
    class CarInfo
    {
        private string identifier;
        private float topSpeed;
        private float turnDivisor;
        private string modelPath;
        private string textureBodyPath;
        private string textureCockpitPath;
        private string textureWheelPath;
        private string textureRimPath;

        public CarInfo(string identifier,
                       float topSpeed,
                       float turnDivisor,
                       string modelPath,
                       string textureBodyPath,
                       string textureCockpitPath,
                       string textureWheelPath,
                       string textureRimPath)
        {
            this.identifier = identifier;
            this.topSpeed = topSpeed;
            this.turnDivisor = turnDivisor;
            this.modelPath = modelPath;
            this.textureBodyPath = textureBodyPath;
            this.textureRimPath = textureRimPath;
            this.textureWheelPath = textureWheelPath;
            this.textureCockpitPath = textureCockpitPath;
        }

        public float TopSpeed
        {
            get { return this.topSpeed; }
        }

        public float TurnDivisor
        {
            get { return this.turnDivisor; }
        }

        public string Model
        {
            get { return this.modelPath; }
        }

        public string BodyTexture
        {
            get { return this.textureBodyPath; }
        }

        public string RimTexture
        {
            get { return this.textureRimPath; }
        }

        public string WheelTexture
        {
            get { return this.textureWheelPath; }
        }

        public string CockpitTexture
        {
            get { return this.textureCockpitPath; }
        }

        public string Name
        {
            get { return this.identifier; }
        }
    }
}
