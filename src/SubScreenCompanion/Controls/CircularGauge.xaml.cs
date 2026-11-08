using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SubScreenCompanion.Controls
{
    public partial class CircularGauge : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularGauge),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(CircularGauge),
                new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, OnValueChanged));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(CircularGauge),
                new FrameworkPropertyMetadata(Brushes.Cyan, OnStrokeChanged));

        public static readonly DependencyProperty TrackStrokeProperty =
            DependencyProperty.Register(nameof(TrackStroke), typeof(Brush), typeof(CircularGauge),
                new FrameworkPropertyMetadata(Brushes.DarkGray, OnTrackStrokeChanged));

        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(CircularGauge),
                new FrameworkPropertyMetadata("%", OnTextChanged));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(CircularGauge),
                new FrameworkPropertyMetadata("LOAD", OnTextChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public Brush TrackStroke
        {
            get => (Brush)GetValue(TrackStrokeProperty);
            set => SetValue(TrackStrokeProperty, value);
        }

        public string Suffix
        {
            get => (string)GetValue(SuffixProperty);
            set => SetValue(SuffixProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public CircularGauge()
        {
            InitializeComponent();
            Loaded += (s, e) => Redraw();
            SizeChanged += (s, e) => Redraw();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularGauge gauge) gauge.Redraw();
        }

        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularGauge gauge && e.NewValue is Brush brush)
            {
                gauge.ProgressPath.Stroke = brush;
            }
        }

        private static void OnTrackStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularGauge gauge && e.NewValue is Brush brush)
            {
                gauge.TrackPath.Stroke = brush;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularGauge gauge) gauge.UpdateText();
        }

        private void UpdateText()
        {
            TxtTitle.Text = Title;
            TxtValue.Text = $"{(int)Math.Round(Value)}{Suffix}";
        }

        private void Redraw()
        {
            UpdateText();

            double size = 100;
            double radius = 42;
            Point center = new Point(size / 2, size / 2);

            // 240-градусная шкала: от 150° до 390° (угол в градусах, 0 = 3 часа)
            double startAngle = 140;
            double totalAngle = 260;

            TrackPath.Data = CreateArcGeometry(center, radius, startAngle, startAngle + totalAngle);

            double percent = Maximum > 0 ? Math.Clamp(Value / Maximum, 0.0, 1.0) : 0.0;
            double sweepAngle = totalAngle * percent;

            if (sweepAngle > 0.5)
            {
                ProgressPath.Data = CreateArcGeometry(center, radius, startAngle, startAngle + sweepAngle);
                ProgressPath.Visibility = Visibility.Visible;
            }
            else
            {
                ProgressPath.Visibility = Visibility.Collapsed;
            }
        }

        private PathGeometry CreateArcGeometry(Point center, double radius, double startAngleDeg, double endAngleDeg)
        {
            Point startPoint = PointOnCircle(center, radius, startAngleDeg);
            Point endPoint = PointOnCircle(center, radius, endAngleDeg);

            bool isLargeArc = (endAngleDeg - startAngleDeg) > 180.0;

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false
            };

            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        private static Point PointOnCircle(Point center, double radius, double angleDeg)
        {
            double angleRad = (Math.PI / 180.0) * angleDeg;
            return new Point(
                center.X + radius * Math.Cos(angleRad),
                center.Y + radius * Math.Sin(angleRad)
            );
        }
    }
}
