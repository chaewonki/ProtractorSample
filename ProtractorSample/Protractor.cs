using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProtractorSample
{
    public class Protractor : Shape
    {

        #region Dependency Properties & Properties

        public static readonly DependencyProperty StartPointPorperty =
            DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(Protractor),
                new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EndPoint1Property =
            DependencyProperty.Register(nameof(EndPoint1), typeof(Point), typeof(Protractor),
                new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EndPoint2Property =
            DependencyProperty.Register(nameof(EndPoint2), typeof(Point), typeof(Protractor),
                new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointPorperty);
            set => SetValue(StartPointPorperty, value);
        }

        public Point EndPoint1
        {
            get => (Point)GetValue(EndPoint1Property);
            set => SetValue(EndPoint1Property, value);
        }

        public Point EndPoint2
        {
            get => (Point)GetValue(EndPoint2Property);
            set => SetValue(EndPoint2Property, value);
        }

        #region 각도기 표시를 위한 상수 정의

        private const double ARCRADIUS = 50;
        private const double TEXTOFFSET = 60;
        private const int FONTSIZE = 16;

        #endregion

        #endregion

        #region 메소드 재정의

        protected override Geometry DefiningGeometry
        {
            get
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    DrawLines(ctx);
                    DrawArc(ctx);
                }

                return geometry;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            DrawAngleText(drawingContext);
        }

        #endregion

        #region private 메소드

        private void DrawLines(StreamGeometryContext ctx)
        {
            ctx.BeginFigure(StartPoint, false, false);
            ctx.LineTo(EndPoint1, true, false);

            ctx.BeginFigure(StartPoint, false, false);
            ctx.LineTo(EndPoint2, true, false);
        }

        private void DrawArc(StreamGeometryContext ctx)
        {
            var (vector1, vector2, _) = CalculateVectorsAndAngle();

            var arcStart = StartPoint + vector1 * ARCRADIUS;
            var arcEnd = StartPoint + vector2 * ARCRADIUS;

            ctx.BeginFigure(arcStart, false, false);
            ctx.ArcTo(arcEnd, new Size(ARCRADIUS, ARCRADIUS), 0, false, SweepDirection.Clockwise, true, false);
        }

        private void DrawAngleText(DrawingContext drawingContext)
        {
            var (vector1, _ , angle) = CalculateVectorsAndAngle();

            var angleText = $"{Math.Round(angle, 2)}°";
            var formattedText = new FormattedText(
                angleText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                FONTSIZE,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var textPosition = StartPoint + vector1 * TEXTOFFSET;
            drawingContext.DrawText(formattedText, textPosition);
        }

        private (Vector vector1, Vector vector2, double angle) CalculateVectorsAndAngle()
        {
            var vector1 = EndPoint1 - StartPoint;
            var vector2 = EndPoint2 - StartPoint;

            vector1.Normalize();
            vector2.Normalize();

            var angle = Vector.AngleBetween(vector1, vector2);

            if (angle < 0)
                angle += 360;

            if (angle > 180)
            {
                angle = 360 - angle;
                (vector1, vector2) = (vector2, vector1);
            }

            return (vector1, vector2, angle);
        }

        #endregion
    }
}
