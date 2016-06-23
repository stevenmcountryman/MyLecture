using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MyLecture.Controls
{
    class SelectionPen : InkToolbarCustomPen
    {
        protected override InkDrawingAttributes CreateInkDrawingAttributesCore(Brush brush, double strokeWidth)
        {
            var attributes = new InkDrawingAttributes()
            {
                PenTip = PenTipShape.Circle,
                Size = new Windows.Foundation.Size(2, 2),
                Color = (brush as SolidColorBrush).Color,
                IgnorePressure = false,
                DrawAsHighlighter = false,
                FitToCurve = false
            };
            return attributes;
        }
    }
}
