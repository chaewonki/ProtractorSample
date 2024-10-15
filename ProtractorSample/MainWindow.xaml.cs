using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProtractorSample
{
    public partial class MainWindow : Window
    {
        private bool _isFirstLineDrawn = false;
        private Line _firstLine;
        private Line _secondLine;
        private List<Line> _tempLines = new();
        private Protractor _protractor;

        private TextBlock _tbDegree;
        private Point _degreePoint;

        private Path _arcPath;
        private PathGeometry _arcPathGeometry;
        private PathFigure _arcPathFigure;
        private ArcSegment _arcSegment;
        private Point _arcStartPoint;
        private Point _arcEndPoint;

        private const double ARCRADIUS = 50;
        private const double TEXTOFFSET = 60;
        private const int FONTSIZE = 16;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void cnvDrawing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(cnvDrawing);

            if (!_isFirstLineDrawn)
            {
                DrawFirstLine(currentPoint);
            }
            else
            {
                DrawSecondLine();
            }
        }

        private void DrawFirstLine(Point currentPoint)
        {
            _protractor = new Protractor()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StartPoint = currentPoint,
            };

            _tbDegree = new TextBlock();
            _tbDegree.Text = "";
            _tbDegree.FontFamily = new FontFamily("Arial");
            _tbDegree.FlowDirection = FlowDirection.LeftToRight;
            _tbDegree.FontSize = FONTSIZE;
            _tbDegree.Foreground = Brushes.Black;
            cnvDrawing.Children.Add(_tbDegree);

            _firstLine = CreateLine(currentPoint.X, currentPoint.Y);
            _tempLines.Add(_firstLine);
            cnvDrawing.Children.Add(_firstLine);

            cnvDrawing.CaptureMouse();
        }

        private void DrawSecondLine()
        {
            _secondLine = CreateLine(_firstLine.X1, _firstLine.Y1);
            _tempLines.Add(_secondLine);
            cnvDrawing.Children.Add(_secondLine);

            _arcPath = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
            };

            _arcPathGeometry = new PathGeometry();

            _arcPathFigure = new PathFigure();

            _arcSegment = new ArcSegment()
            {
                Size = new Size(ARCRADIUS, ARCRADIUS),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise,
            };

            _arcPathFigure.Segments.Add(_arcSegment);

            _arcPathGeometry.Figures.Add(_arcPathFigure);

            _arcPath.Data = _arcPathGeometry;
            cnvDrawing.Children.Add(_arcPath);
        }

        private Line CreateLine(double x, double y)
        {
            return new Line()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y
            };
        }

        private void cnvDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(cnvDrawing);

            if (!_isFirstLineDrawn && e.LeftButton == MouseButtonState.Pressed)
            {
                _firstLine.X2 = currentPoint.X;
                _firstLine.Y2 = currentPoint.Y;
            }
            else if (_isFirstLineDrawn && e.LeftButton == MouseButtonState.Pressed)
            {
                _secondLine.X2 = currentPoint.X;
                _secondLine.Y2 = currentPoint.Y;

                UpdateDegreeAndArc(currentPoint);
            }
        }

        private void UpdateDegreeAndArc(Point currentPoint)
        {
            Point startPoint = new Point(_firstLine.X1, _firstLine.Y1);
            Vector vector1 = new Point(_firstLine.X2, _firstLine.Y2) - startPoint;
            Vector vector2 = currentPoint - startPoint;

            vector1.Normalize();
            vector2.Normalize();

            double angle = Vector.AngleBetween(vector1, vector2);

            if (angle < 0)
                angle += 360;

            if (angle > 180)
            {
                angle = 360 - angle;
                (vector1, vector2) = (vector2, vector1);
            }

            string angleText = $"{Math.Round(angle, 2)}°";
            _tbDegree.Text = angleText;
            _degreePoint = startPoint + vector1 * TEXTOFFSET;
            Canvas.SetLeft(_tbDegree, _degreePoint.X);
            Canvas.SetTop(_tbDegree, _degreePoint.Y);

            _arcStartPoint = startPoint + vector1 * ARCRADIUS;
            _arcEndPoint = startPoint + vector2 * ARCRADIUS;    

            _arcPathFigure.StartPoint = _arcStartPoint;
            _arcSegment.Point = _arcEndPoint;
        }

        private void cnvDrawing_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(cnvDrawing);

            if (!_isFirstLineDrawn)
                FinalizeFirstLine(currentPoint);
            else
                FinalizeSecondLine(currentPoint);

            cnvDrawing.ReleaseMouseCapture();
        }

        private void FinalizeFirstLine(Point currentPoint)
        {
            _protractor.EndPoint1 = currentPoint;

            _isFirstLineDrawn = true;
        }

        private void FinalizeSecondLine(Point currentPoint)
        {
            _protractor.EndPoint2 = currentPoint;
            cnvDrawing.Children.Add(_protractor);

            cnvDrawing.Children.Remove(_arcPath);
            cnvDrawing.Children.Remove(_tbDegree);
            foreach (var line in _tempLines)
                cnvDrawing.Children.Remove(line);
            _tempLines.Clear();

            _isFirstLineDrawn = false;
        }
    }
}