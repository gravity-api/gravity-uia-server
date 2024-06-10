using System;
using System.Drawing;
using System.Linq;

using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

using static Emgu.CV.OCR.Tesseract;

namespace UiaWebDriverServer.FindByOcr
{
    public class OcrDriver : IDisposable
    {
        private bool disposedValue;
        private  readonly Tesseract tesseract;

        public OcrDriver() : this(@"..\UiaWebDriverServer.FindByOcr\TrainData\", "eng", OcrEngineMode.Default)
        {

        }

        public OcrDriver(string dataPath, string language, OcrEngineMode mode)
        {
            tesseract = new Tesseract(dataPath, language, mode)
            {
                PageSegMode = PageSegMode.SparseText
            };
        }

        public Word GetWordFromImage(string text, Bitmap bitmap)
        {
            bitmap.SetResolution(300, 300);
            using var image = bitmap.ToImage<Bgr, byte>();

            // Convert to grayscale
            Image<Gray, byte> grayImage = image.Convert<Gray, byte>();

            // Perform OCR
            tesseract.SetImage(grayImage);
            tesseract.Recognize();

            // Get the result
            var words = tesseract.GetWords();

            return Array.Find(words, word => word.Text == text);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tesseract.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
