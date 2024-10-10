using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        private PathFigure _arcPathFigure;
        private PathGeometry _arcPathGeometry;
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
            // 처음에는 화면에 보이지 않을 각도기 개체 생성
            _protractor = new Protractor()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StartPoint = currentPoint,
            };

            // 각도표시용 텍스트블럭
            _tbDegree = new TextBlock();
            _tbDegree.Text = "";
            _tbDegree.FontFamily = new FontFamily("Arial");
            _tbDegree.FlowDirection = FlowDirection.LeftToRight;
            _tbDegree.FontSize = FONTSIZE;
            _tbDegree.Foreground = Brushes.Black;
            cnvDrawing.Children.Add(_tbDegree);

            // 화면에 표시되는 첫 번째 선
            _firstLine = CreateLine(currentPoint.X, currentPoint.Y);
            cnvDrawing.Children.Add(_firstLine);

            cnvDrawing.CaptureMouse();
        }

        private void DrawSecondLine()
        {
            // 화면에 표시되는 두 번째 선
            _secondLine = CreateLine(_firstLine.X1, _firstLine.Y1);
            cnvDrawing.Children.Add(_secondLine);

            /////////////// 이하 호 표시 ////////////////////
            // 각도기 호의 색과 두께(Protractor 클래스와 상응)
            _arcPath = new Path()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
            };

            // 호의 시작점
            _arcPathFigure = new PathFigure();

            // 호의 크기와 모양, 끝점
            // 크기는 시작점에서 떨어진 거리 
            // 180도 이하 사이각으로 볼록하게 표시
            _arcSegment = new ArcSegment()
            {
                Size = new Size(ARCRADIUS, ARCRADIUS),
                IsLargeArc = false,
                SweepDirection = SweepDirection.Clockwise,
            };

            _arcPathFigure.Segments.Add(_arcSegment);

            _arcPathGeometry = new PathGeometry();
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

            Debug.WriteLine("TEST");
        }

        private void UpdateDegreeAndArc(Point currentPoint)
        {
            // 벡터의 방향과 함께 설명
            Point startPoint = new Point(_firstLine.X1, _firstLine.Y1);
            Vector vector1 = new Point(_firstLine.X2, _firstLine.Y2) - startPoint;
            Vector vector2 = currentPoint - startPoint;

            // 단위벡터 변환
            vector1.Normalize();
            vector2.Normalize();

            double angle = Vector.AngleBetween(vector1, vector2);

            // 시계 방향은 양수, 반시계 방향은 음수
            // 반시계 방향으로 움직일 때 음수를 양수로 보정
            if (angle < 0)
                angle += 360;

            // 180도 이하 보정, 두 벡터를 교환해야 올바른 호를 그릴 수 있다.
            if (angle > 180)
            {
                angle = 360 - angle;
                (vector1, vector2) = (vector2, vector1);
            }

            // 각도는 소수점 셋째 자리에서 반올림하여 둘째 자리까지 표시하고
            // 적절한 위치에 표시한다.
            string angleText = $"{Math.Round(angle, 2)}°";
            _tbDegree.Text = angleText;
            _degreePoint = startPoint + vector1 * TEXTOFFSET;
            Canvas.SetLeft(_tbDegree, _degreePoint.X);
            Canvas.SetTop(_tbDegree, _degreePoint.Y);

            // 호의 지름에 맞춰 호의 시작점과 끝점을 설정해 주고
            // 최종 UIElement를 위해 앞서 설정한 도형 관련 클래스에 대입한다.
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

            // 각도기 두 번째 점 설정과 동시에 화면에 표시하고 
            // 기존화면에 있던, 사라질 각도기를 구성했던 호, 텍스트, 선 제거
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