using UIAutomationClient;

using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Extensions
{
    public static class ElementExtensions
    {

        public static void NativeClick(this Element element, double scaleRatio)
        {
            var point = InvokeGetClickablePoint(scaleRatio, element);
            ExternalMethods.SetPhysicalCursorPos(point.XPos, point.YPos);


            InvokeNativeClick();
        }

        public static Location GetElementLocation(this Element element)
        {
            Location location = new();
            var uiaElement = element?.UIAutomationElement;
            if (uiaElement != null)
            {
                location.Right = uiaElement.CurrentBoundingRectangle.right;
                location.Left = uiaElement.CurrentBoundingRectangle.left;
                location.Top = uiaElement.CurrentBoundingRectangle.top;
                location.Bottom = uiaElement.CurrentBoundingRectangle.bottom;
            }
            else
            {
                location = element?.Location is null ? default : element.Location;
            }

            return location;
        }

        /// <summary>
        /// Calculates the clickable point relative to an uiElement based on alignment and offsets.
        /// </summary>
        /// <param name="element">The UI automation uiElement.</param>
        /// <param name="align">The alignment of the clickable point.</param>
        /// <param name="topOffset">The vertical offset from the alignment point.</param>
        /// <param name="leftOffset">The horizontal offset from the alignment point.</param>
        /// <param name="scaleRatio">The scale ratio of the session.</param>
        /// <returns>The calculated clickable point.</returns>
        public static ClickablePoint GetClickablePoint(
            this Element element,
            string align,
            int topOffset,
            int leftOffset,
            double scaleRatio)
        {
            // Ensure scaleRatio is positive
            scaleRatio = scaleRatio <= 0 ? 1 : scaleRatio;

            var location = GetElementLocation(element);
            switch (align.ToUpper())
            {
                // Calculate clickable point based on specified alignment
                case "TOPLEFT":
                    return new ClickablePoint
                    {
                        XPos = (int)(location.Left / scaleRatio) + leftOffset,
                        YPos = (int)(location.Top / scaleRatio) + topOffset
                    };
                case "TOPCENTER":
                    {
                        var horizontalDelta = (location.Right - location.Left) / 2;
                        return new ClickablePoint
                        {
                            XPos = (int)((location.Left + horizontalDelta) / scaleRatio) + leftOffset,
                            YPos = (int)(location.Top / scaleRatio) + topOffset
                        };
                    }
                case "TOPRIGHT":
                    return new ClickablePoint
                    {
                        XPos = (int)(location.Right / scaleRatio) + leftOffset,
                        YPos = (int)(location.Top / scaleRatio) + topOffset
                    };

                case "MIDDLELEFT":
                    {
                        var verticalDelta = (location.Bottom - location.Top) / 2;
                        return new ClickablePoint
                        {
                            XPos = (int)(location.Left / scaleRatio) + leftOffset,
                            YPos = (int)((location.Top + verticalDelta) / scaleRatio) + topOffset
                        };
                    }
                case "MIDDLERIGHT":
                    {
                        var verticalDelta = (location.Bottom - location.Top) / 2;
                        return new ClickablePoint
                        {
                            XPos = (int)(location.Right / scaleRatio) + leftOffset,
                            YPos = (int)((location.Top + verticalDelta) / scaleRatio) + topOffset
                        };
                    }
                case "BOTTOMLEFT":
                    return new ClickablePoint
                    {
                        XPos = (int)(location.Left / scaleRatio) + leftOffset,
                        YPos = (int)(location.Bottom / scaleRatio) + topOffset
                    };

                case "BOTTOMCENTER":
                    {
                        var horizontalDelta = (location.Right - location.Left) / 2;
                        return new ClickablePoint
                        {
                            XPos = (int)((location.Left + horizontalDelta) / scaleRatio) + leftOffset,
                            YPos = (int)(location.Bottom / scaleRatio) + topOffset
                        };
                    }
                case "BOTTOMRIGHT":
                    return new ClickablePoint
                    {
                        XPos = (int)(location.Right / scaleRatio) + leftOffset,
                        YPos = (int)(location.Bottom / scaleRatio) + topOffset
                    };

                // Invoke alternative method if alignment is MIDDLECENTER, or is not recognized
                default:
                    return InvokeGetClickablePoint(scaleRatio, element);
            }
        }

        private static ClickablePoint InvokeGetClickablePoint(double scaleRatio, Element element)
        {
            var location = GetElementLocation(element);

            return location != default ? location.GetMidCenterPoint(scaleRatio) : element?.ClickablePoint;
        }

        private static void InvokeNativeClick()
        {
            // get current mouse position
            ExternalMethods.GetPhysicalCursorPos(out tagPOINT position);

            // invoke click sequence
            ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftDown, position.x, position.y, 0, 0);
            ExternalMethods.mouse_event(ExternalMethods.MouseEventLeftUp, position.x, position.y, 0, 0);
        }
        
        /// <summary>
        /// Try to get a mouse click-able point of the element.
        /// </summary>
        /// <param name="element">The Element to get click-able point for.</param>
        /// <returns>A ClickablePoint object.</returns>
        public static ClickablePoint GetClickablePoint(this Element element)
        {
            return InvokeGetClickablePoint(scaleRatio: 1.0D, element);
        }

        /// <summary>
        /// Try to get a mouse click-able point of the element.
        /// </summary>
        /// <param name="element">The Element to get click-able point for.</param>
        /// <param name="scaleRatio">The screen scale ratio e.g., if the scale ratio is 250% this number will be 2.5, if 150% it will be 1.5, etc.</param>
        /// <returns>A ClickablePoint object.</returns>
        public static ClickablePoint GetClickablePoint(this Element element, double scaleRatio)
        {
            return InvokeGetClickablePoint(scaleRatio, element);
        }

        

        /// <summary>
        /// Invokes a pattern based MouseClick action on the uiElement. If not possible to use
        /// pattern, it will use native click.
        /// </summary>
        /// <param name="element">The uiElement to click on.</param>
        /// <remarks>This action will attempt to evaluate the center of the uiElement and click on it.</remarks>
        public static Element Click(this Element element)
        {
            return Click(scaleRatio: 1.0D, element);
        }

        /// <summary>
        /// Invokes a pattern based MouseClick action on the uiElement. If not possible to use
        /// pattern, it will use native click.
        /// </summary>
        /// <param name="element">The uiElement to click on.</param>
        /// <remarks>This action will attempt to evaluate the center of the uiElement and click on it.</remarks>
        public static Element Click(this Element element, double scaleRatio)
        {
            return Click(scaleRatio, element);
        }

        private static Element Click(double scaleRatio, Element element)
        {

            // setup conditions
            var uiElement = element?.UIAutomationElement;

            var pattern = GetElementPattern(uiElement);

            
            switch (pattern)
            {
                case IUIAutomationInvokePattern:
                    uiElement.Invoke();
                    break;

                case IUIAutomationExpandCollapsePattern: 
                    uiElement.ExpandCollapse(); 
                    break;

                case IUIAutomationSelectionItemPattern:
                    uiElement.Select();
                    break;

                default:
                    var point = InvokeGetClickablePoint(scaleRatio, element);
                    ExternalMethods.SetPhysicalCursorPos(point.XPos, point.YPos);
                    InvokeNativeClick();
                    break;
            }

            //var isInvoke = uiElement?.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId) != null;
            //var isExpandCollapse = !isInvoke && uiElement?.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) != null;
            //var isSelectable = !isInvoke && !isExpandCollapse && uiElement?.GetCurrentPattern(UIA_PatternIds.UIA_SelectionItemPatternId) != null;
            //var isCords = !isInvoke && !isExpandCollapse && !isSelectable;

            //// invoke
            //if (isInvoke)
            //{
            //    uiElement.Invoke();
            //}
            //else if (isExpandCollapse)
            //{
            //    uiElement.ExpandCollapse();
            //}
            //else if (isSelectable)
            //{
            //    uiElement.Select();
            //}
            //else if (isCords)
            //{
            //    var point = InvokeGetClickablePoint(scaleRatio, element);
            //    ExternalMethods.SetPhysicalCursorPos(point.XPos, point.YPos);
            //    InvokeNativeClick();
            //}

            // get
            return element;
        }

        private static object GetElementPattern(IUIAutomationElement uiElement)
        {
            if (uiElement is null)
            {
                return null;
            }
            var patterns = new[]
            {
                UIA_PatternIds.UIA_InvokePatternId,
                UIA_PatternIds.UIA_ExpandCollapsePatternId,
                UIA_PatternIds.UIA_SelectionItemPatternId
            };
            object patternObject = null;
            foreach(var pattern in patterns)
            {
                patternObject = uiElement.GetCurrentPattern(UIA_PatternIds.UIA_InvokePatternId);
                if(patternObject != null)
                {
                    break;
                }
            }
            return patternObject;
        }
    }
}
