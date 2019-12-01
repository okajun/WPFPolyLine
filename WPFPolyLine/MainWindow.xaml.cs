using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WPFPolyLine
{
    sealed class PointList : IList<Point>
    {
        /// ラップ対象.
        private readonly IEnumerable<Point> points;
        /// 現在のEnumeratorの位置.
        private int position = -1;
        /// インデクサで利用するIEnumerator.
        private IEnumerator<Point> itr = null;

        public PointList(IEnumerable<Point> points, int count)
        {
            this.points = points ?? throw new ArgumentNullException();
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.Count = count;
        }
        /// これは実装する必要がある.
        public Point this[int index]
        {
            get
            {
                if (index >= Count || index < 0)
                {
                    //インデックスが不正.
                    throw new IndexOutOfRangeException();
                }
                if (-1 == position)
                {
                    //最初のアクセス時にIEnumeratorを取得.
                    itr = points.GetEnumerator();
                }
                //前回アクセス時と今回アクセス時の差分.
                var difference = index - position;
                if (0 > difference)
                {
                    //差分が0未満の場合は前回アクセスよりも前の位置となるのでEnumeratorを巻き戻してやり直し.
                    // StreamGeometryContext.PolyLineToを利用してこの処理に突入することは無い.
                    itr.Reset();
                    if (!itr.MoveNext())
                    {
                        throw new IndexOutOfRangeException();
                    }
                    difference = index;
                }
                //差分の分だけEnumeratorを進める.
                for (int i = 0; i < difference; ++i)
                {
                    if (!itr.MoveNext())
                    {
                        //インデックスが不正.
                        throw new IndexOutOfRangeException();
                    }
                }
                //現在位置をindexへ設定.
                position = index;
                //値を返す.
                return itr.Current;
            }
            set => throw new NotImplementedException();
        }
        /// これは実装する必要がある.
        public int Count { get; }
        /// Readonly.
        public bool IsReadOnly => true;

        public void Add(Point item) { throw new NotImplementedException(); }

        public void Clear() { throw new NotImplementedException(); }

        public bool Contains(Point item) { throw new NotImplementedException(); }

        public void CopyTo(Point[] array, int arrayIndex) { throw new NotImplementedException(); }
        //一応実装しとく.
        public IEnumerator<Point> GetEnumerator() { return points.GetEnumerator(); }

        public int IndexOf(Point item) { throw new NotImplementedException(); }

        public void Insert(int index, Point item) { throw new NotImplementedException(); }

        public bool Remove(Point item) { throw new NotImplementedException(); }

        public void RemoveAt(int index) { throw new NotImplementedException(); }
        //一応実装しとく.
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return points.GetEnumerator(); }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static void DrawPolygonEnumeratable(StreamGeometryContext ctx, IEnumerable<Point> points, int countOfPoints)
        {
            if (2 > countOfPoints)
            {
                return;
            }
            ctx.BeginFigure(points.First(), true, true);
            //ctx.PolyLineTo(new PointList(points.Skip(1), countOfPoints - 1), true, false);
            ctx.PolyLineTo(new PointList(points, countOfPoints), true, false);
        }

        private void DrawPolygonCopyList(StreamGeometryContext ctx, IList<Point> points)
        {
            if (2 > points.Count)
            {
                return;
            }
            ctx.BeginFigure(points[0], true, true);
            //ctx.PolyLineTo(points.Skip(1).ToArray(), true, false);
            ctx.PolyLineTo(points, true, false);
        }

        private readonly Drawing drawing;

        public MainWindow()
        {
            InitializeComponent();

            var points = new Point[]
            {
                new Point(20d,  0d),
                new Point(40d, 40d),
                new Point( 0d, 40d),
            };
            var geometry = new StreamGeometry();
            geometry.FillRule = FillRule.Nonzero;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (var ctx = geometry.Open())
            {
                for (int y = 0; y < 100; ++y)
                {
                    for (int x = 0; x < 100; ++x)
                    {
                        //DrawPolygonCopyList(ctx, points.Select(p => Point.Add(p, new Vector(x * 40d, y * 40d))).ToList());
                        DrawPolygonEnumeratable(ctx, points.Select(p => Point.Add(p, new Vector(x * 40d, y * 40d))), points.Length);
                    }
                }
            }
            stopwatch.Stop();
            var microsec = stopwatch.ElapsedTicks * 10000 / System.Diagnostics.Stopwatch.Frequency;
            this.Time.Content = string.Format($"{microsec}µs");

            geometry.Freeze();
            var pen = new Pen() { Brush = Brushes.Black, Thickness = 1d, };
            drawing = new GeometryDrawing(Brushes.Red, pen, geometry);
            drawing.Freeze();

            Box.SizeChanged += (s, e) =>
            {
                var viewport = new Rect(0d, 0d, Box.ActualWidth, Box.ActualHeight);
                var fillBrush = new DrawingBrush()
                {
                    Stretch = Stretch.None,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Viewport = viewport,
                    Viewbox = viewport,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Drawing = drawing,
                };
                fillBrush.Freeze();
                Box.Fill = fillBrush;
            };
        }
    }
}
