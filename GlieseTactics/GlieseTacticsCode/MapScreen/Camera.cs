using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gliese581g
{
    public class Camera
    {
        private Matrix _transform;

        public Matrix Transform
        {
            get { return _transform; }
        }

        private Vector2 _translation;
        private float _zoom;
        private float _rotation;
        private Vector2 _rotationCenter;

        public Vector2 Translation
        {
            get { return _translation; }
            set { 
                _translation = value; 
                //TODO: Do some bounds checking
                UpdateTransform();
            }
        }

        public void LimitTranslation(Vector2 sizeLimit)
        {
            bool needUpdate = false;

            if (_translation.X > sizeLimit.X)
            {
                _translation.X = sizeLimit.X; 
                needUpdate = true;
            }
            else if (_translation.X < -sizeLimit.X)
            {
                _translation.X = -sizeLimit.X;
                needUpdate = true;
            } 

            if (_translation.Y > sizeLimit.Y)
            {
                _translation.Y = sizeLimit.Y;
                needUpdate = true;
            } 
            else if (_translation.Y < -sizeLimit.Y)
            {
                _translation.Y = -sizeLimit.Y;
                needUpdate = true;
            }

            if (needUpdate)
                UpdateTransform();
        }


        public float Zoom 
        {
            get { return _zoom; }
            set { 
                _zoom = value; 
                if (_zoom < 0.1f) 
                    _zoom = 0.1f;
                UpdateTransform();
                } 
        }

        public float Rotation
        {
            get { return _rotation; }
            set { 
                _rotation = value;
                UpdateTransform();
            }
        }

        public Vector2 RotationCenter
        {
            get { return _rotationCenter; }
            set
            {
                _rotationCenter = value;
                //TODO: Do some bounds checking
                UpdateTransform();
            }
        }

        public Camera(Vector2 translation, float zoom, float rotation, Vector2 rotationCenter)
        {
            _translation = translation;
            _zoom = zoom;
            _rotation = rotation;
            _rotationCenter = rotationCenter;

            UpdateTransform();
        }

        public void UpdateTransform()
        {
            _transform = Matrix.CreateTranslation(new Vector3(Translation.X, Translation.Y, 0)) *
                         Matrix.CreateTranslation(new Vector3(-RotationCenter.X, -RotationCenter.Y, 0)) *
                         Matrix.CreateRotationZ(Rotation) *
                         Matrix.CreateScale(new Vector3(Zoom,Zoom,1)) *
                         Matrix.CreateTranslation(new Vector3(RotationCenter.X, RotationCenter.Y, 0)); 

        }

      


    }
}
