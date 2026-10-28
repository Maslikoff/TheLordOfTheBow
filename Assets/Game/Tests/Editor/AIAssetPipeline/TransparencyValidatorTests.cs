using NUnit.Framework;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline.Tests
{
    public sealed class TransparencyValidatorTests
    {
        [Test]
        public void AcceptsPngWithTransparentBorder()
        {
            byte[] png = CreatePng(true);

            TransparencyValidationResult result = TransparencyValidator.ValidatePng(png);

            Assert.IsTrue(result.IsValid, result.Message);
            Assert.GreaterOrEqual(result.TransparentBorderRatio, TransparencyValidator.MinimumTransparentBorderRatio);
        }

        [Test]
        public void RejectsOpaquePng()
        {
            byte[] png = CreatePng(false);

            TransparencyValidationResult result = TransparencyValidator.ValidatePng(png);

            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(result.HasAlpha);
        }

        private static byte[] CreatePng(bool transparentBorder)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        bool border = x < 4 || x >= 60 || y < 4 || y >= 60;
                        pixels[y * 64 + x] = border && transparentBorder
                            ? new Color32(0, 0, 0, 0)
                            : new Color32(200, 100, 20, 255);
                    }
                }
                texture.SetPixels32(pixels);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
