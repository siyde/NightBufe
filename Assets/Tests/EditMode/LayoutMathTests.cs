using NightBufe.Core;
using NUnit.Framework;

namespace NightBufe.Tests
{
    public class LayoutMathTests
    {
        private const float Eps = 1e-4f;
        private const float ViewW = 160f / 9f; // 16:9 view, ortho size 5
        private const float ViewH = 10f;

        [Test]
        public void Cover_3x2ImageIn16x9View_FillsWidthAndCropsHeight()
        {
            var rect = LayoutMath.FitImage(1536f, 1024f, ViewW, ViewH, cover: true);
            Assert.AreEqual(ViewW, rect.width, Eps);
            Assert.Greater(rect.height, ViewH);
            Assert.AreEqual(0f, rect.CenterX, Eps);
            Assert.AreEqual(0f, rect.CenterY, Eps);
        }

        [Test]
        public void Contain_3x2ImageIn16x9View_FitsHeight()
        {
            var rect = LayoutMath.FitImage(1536f, 1024f, ViewW, ViewH, cover: false);
            Assert.AreEqual(ViewH, rect.height, Eps);
            Assert.AreEqual(15f, rect.width, Eps);
        }

        [Test]
        public void SameAspect_CoverAndContainMatch()
        {
            var a = LayoutMath.FitImage(1920f, 1080f, ViewW, ViewH, true);
            var b = LayoutMath.FitImage(1920f, 1080f, ViewW, ViewH, false);
            Assert.AreEqual(a.width, b.width, Eps);
            Assert.AreEqual(a.height, b.height, Eps);
        }

        [Test]
        public void PixelToWorld_CornersAndCenter()
        {
            var image = new LayoutRect(-8f, -5f, 16f, 10f);
            var (x0, y0) = LayoutMath.PixelToWorld(0f, 0f, 1600f, 1000f, image);
            Assert.AreEqual(-8f, x0, Eps);
            Assert.AreEqual(5f, y0, Eps, "pixel top = world top");

            var (cx, cy) = LayoutMath.PixelToWorld(800f, 500f, 1600f, 1000f, image);
            Assert.AreEqual(0f, cx, Eps);
            Assert.AreEqual(0f, cy, Eps);
        }

        [Test]
        public void PixelRectToWorld_FlipsYAndKeepsSize()
        {
            var image = new LayoutRect(-8f, -5f, 16f, 10f);
            // Bottom-left quarter of a 1600x1000 image.
            var world = LayoutMath.PixelRectToWorld(new LayoutRect(0f, 500f, 800f, 500f), 1600f, 1000f, image);
            Assert.AreEqual(-8f, world.x, Eps);
            Assert.AreEqual(-5f, world.y, Eps);
            Assert.AreEqual(8f, world.width, Eps);
            Assert.AreEqual(5f, world.height, Eps);
        }

        [Test]
        public void LayoutIsResolutionIndependent()
        {
            // The same slot measured on a 1536 image and a 3072 image lands at the same world rect.
            var image = LayoutMath.FitImage(1536f, 1024f, ViewW, ViewH, true);
            var a = LayoutMath.PixelRectToWorld(new LayoutRect(955f, 690f, 435f, 190f), 1536f, 1024f, image);
            var b = LayoutMath.PixelRectToWorld(new LayoutRect(1910f, 1380f, 870f, 380f), 3072f, 2048f, image);
            Assert.AreEqual(a.x, b.x, Eps);
            Assert.AreEqual(a.y, b.y, Eps);
            Assert.AreEqual(a.width, b.width, Eps);
            Assert.AreEqual(a.height, b.height, Eps);
        }
    }
}
