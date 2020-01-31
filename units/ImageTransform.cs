﻿namespace Orikivo.Drawing
{
    /// <summary>
    /// A transform object within a 2D space.
    /// </summary>
    public class ImageTransform
    {
        public static ImageTransform Default = new ImageTransform(Vector2.Zero, 0, Vector2.One);

        public ImageTransform(Vector2 position, AngleF rotation, Vector2? scale = null)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale ?? Vector2.One;
        }

        public Vector2 Position { get; set; }

        public AngleF Rotation { get; set; }

        public Vector2 Scale { get; set; }
    }
}
