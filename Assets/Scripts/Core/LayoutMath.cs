namespace NightBufe.Core
{
    /// <summary>Axis-aligned rectangle; world rects are center-based, y up.</summary>
    public struct LayoutRect
    {
        public float x, y, width, height;

        public LayoutRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public float CenterX => x + width * 0.5f;
        public float CenterY => y + height * 0.5f;

        public override string ToString() => $"({x:0.##}, {y:0.##}, {width:0.##}x{height:0.##})";
    }

    /// <summary>
    /// Maps positions measured on the background image (pixels, origin top-left, y down)
    /// to world space, wherever and however large the image is drawn.
    /// </summary>
    public static class LayoutMath
    {
        /// <summary>
        /// World rect (min corner + size, y up) of an image drawn centered in a view.
        /// cover = fill the view and crop; otherwise fit inside (letterbox).
        /// </summary>
        public static LayoutRect FitImage(float imageWidth, float imageHeight, float viewWidth, float viewHeight, bool cover)
        {
            float scaleX = viewWidth / imageWidth;
            float scaleY = viewHeight / imageHeight;
            float scale = cover ? System.Math.Max(scaleX, scaleY) : System.Math.Min(scaleX, scaleY);
            float w = imageWidth * scale;
            float h = imageHeight * scale;
            return new LayoutRect(-w * 0.5f, -h * 0.5f, w, h);
        }

        /// <summary>Pixel point on the reference image → world point.</summary>
        public static (float x, float y) PixelToWorld(float px, float py, float refWidth, float refHeight, LayoutRect imageWorld)
        {
            float u = px / refWidth;
            float v = py / refHeight;
            return (imageWorld.x + u * imageWorld.width, imageWorld.y + (1f - v) * imageWorld.height);
        }

        /// <summary>Pixel rect (top-left x/y, w, h) → world rect (min corner, y up).</summary>
        public static LayoutRect PixelRectToWorld(LayoutRect pixelRect, float refWidth, float refHeight, LayoutRect imageWorld)
        {
            var (minX, maxY) = PixelToWorld(pixelRect.x, pixelRect.y, refWidth, refHeight, imageWorld);
            var (maxX, minY) = PixelToWorld(pixelRect.x + pixelRect.width, pixelRect.y + pixelRect.height, refWidth, refHeight, imageWorld);
            return new LayoutRect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
