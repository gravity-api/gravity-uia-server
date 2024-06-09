using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using UiaWebDriverServer.Contracts;
using UiaWebDriverServer.FindByOcr;
/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
namespace UiaWebDriverServer.Extensions
{
    public static partial class LocationStrategyExtensions
    {

        [GeneratedRegex("\\[\\d+,\\d+]")]
        private static partial Regex GetCordsNumberPattern();

        [GeneratedRegex("(?i)//cords\\[\\d+,\\d+]", RegexOptions.None, "en-US")]
        private static partial Regex GetCordsPattern();

        /// <summary>
        /// Gets a manually instantiated Element created from text found by OCR.
        /// </summary>
        /// <param name="locationStrategy">LocationStrategy object to get text from.</param>
        /// <returns>An Element object with a flat click-able point.</returns>
        public static Element GetElementByText(this LocationStrategy locationStrategy)
        {
            //ExternalMethods.SetProcessDpiAwarenessContext(-4);
            var textToFind = GetOcrTextValue(locationStrategy);

            // not found
            if (string.IsNullOrEmpty(textToFind))
            {
                return null;
            }
            using var bitmap = DotnetExtensions.CaptureScreenshot();

            using var ocr = new OcrDriver();
            var word = ocr.GetWordFromImage(textToFind, bitmap);


            // not found
            if (string.IsNullOrEmpty(word.Text))
            {
                return null;
            }
            
            var location = new Location { Left = word.Region.Left, Right = word.Region.Right, Bottom = word.Region.Bottom, Top = word.Region.Top };
            var point = location.GetMidCenterPoint(1.0D);

            var element = new Element
            {
                ClickablePoint = point,
                Location = location,
                Node = new XText(textToFind),
                Id = $"{Guid.NewGuid()}"
            };

            return element;
        }

        /// <summary>
        /// Gets a flat (x, y) click-able point (//cords[x, y]) wrapped in an Element.
        /// </summary>
        /// <param name="locationStrategy">LocationStrategy object to get cords from.</param>
        /// <returns>An Element object with a flat click-able point.</returns>
        public static Element GetFlatPointElement(this LocationStrategy locationStrategy)
        {
            // setup conditions
            var isCords = GetCordsPattern().IsMatch(locationStrategy.Value);

            // not found
            if (!isCords)
            {
                return null;
            }

            // load cords
            var cords = JsonSerializer.Deserialize<int[]>(GetCordsNumberPattern().Match(locationStrategy.Value).Value);
            return new Element { ClickablePoint = new ClickablePoint(xpos: cords[0], ypos: cords[1]) };
        }


        /// <summary>
        /// Gets the text to find by OCR, removing the "OCR:" prefix.
        /// </summary>
        /// <param name="locationStrategy"></param>
        /// <returns>The target text to search by OCR.</returns>
        private static string GetOcrTextValue(LocationStrategy locationStrategy)
        {
            var index = locationStrategy.Value.IndexOf("OCR:", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return default;
            }
            return locationStrategy.Value.Substring(index + "OCR:".Length).Trim();
        }
    }
}