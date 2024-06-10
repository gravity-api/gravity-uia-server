using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Extensions
{
    public static class LocationExtensions
    {
        public static ClickablePoint GetMidCenterPoint(this Location location, double scaleRatio)
        {
            scaleRatio = scaleRatio <= 0 ? 1 : scaleRatio;

            // range
            var hDelta = (location.Right - location.Left) / 2;
            var vDelta = (location.Bottom - location.Top) / 2;

            // setup
            var x = (int)((location.Left + hDelta) / scaleRatio);
            var y = (int)((location.Top + vDelta) / scaleRatio);

            return new ClickablePoint(xpos: x, ypos: y);
        }

    }
}
