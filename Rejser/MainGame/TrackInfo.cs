using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Kreen.MainGame
{
    class TrackInfo
    {
        private string identifier;
        private Vector3 carStartingPosition;
        private string modelPath;
        private string texturePath;

        public TrackInfo(string identifier,
                         string modelPath,
                         string texturePath,
                         Vector3 carStartingPosition)
        {
            this.identifier = identifier;
            this.modelPath = modelPath;
            this.texturePath = texturePath;
            this.carStartingPosition = carStartingPosition;
        }

        public string Name
        {
            get { return this.identifier; }
        }

        public string Model
        {
            get { return this.modelPath; }
        }

        public string TexturePath
        {
            get { return this.texturePath; }
        }

        public Vector3 StartingPosition
        {
            get { return this.carStartingPosition; }
        }
    }
}
