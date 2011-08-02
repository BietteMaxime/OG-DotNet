//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using OGDotNet.AnalyticsViewer.Properties;
using OGDotNet.AnalyticsViewer.View.Charts;
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.WPFUtils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// TODO work out how to present this
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {
        const double labelHeight = 0.2;

        private static readonly bool ToScale = Settings.Default.ShowVolatilityCurveToScale;
        static readonly Point3D Center = new Point3D(0.5, 0.5, 0.5);

        const double GraphOffset = 0.4;
        const int ProjectedCurveSize = 400;

        private readonly DispatcherTimer _timer;

        private bool _haveInitedData;
        double _zRange = 100.0;

        public VolatilitySurfaceCell()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80.0) };

            double speed = 0.02;

            double t = Math.PI / 4.0;
            const double maxT = Math.PI / 2.0;
            const double minT = 0;
            SetCamera(t);
            _timer.Start();

            _timer.Tick += delegate
            {
                if (t > maxT)
                {
                    speed = -Math.Abs(speed);
                }

                if (t < minT)
                {
                    speed = Math.Abs(speed);
                }

                t += speed;

                SetCamera(t);
                UpdateToolTip(Mouse.GetPosition(mainViewport));
            };
        }

        private void SetCamera(double t)
        {
            const double circleRadius = 3.8;

            camera.Position = Center + (new Vector3D(Math.Sin(t), Math.Cos(t), 0) * circleRadius);
            camera.LookDirection = Center - camera.Position;
        }

        public double ZRange
        {
            get { return _zRange; }
            set { _zRange = value; }
        }

        private void InitTableData()
        {
            if (_haveInitedData) return;
            _haveInitedData = true;

            var data = (VolatilitySurfaceData<Tenor, Tenor>)DataContext;

            while (detailsList.Columns.Count > 1)
            {
                detailsList.Columns.RemoveAt(1);
            }

            foreach (var x in data.Xs)
            {
                var textBlockStyle = new Style(typeof(FrameworkElement));
                var binding = BindingUtils.GetIndexerBinding(x.ToString());
                binding.Converter = new ValueToColorConverter(_zRange);
                textBlockStyle.Setters.Add(new Setter(BackgroundProperty, binding));

                detailsList.Columns.Add(new DataGridTextColumn
                {
                    Width = 60,
                    Header = x,
                    CellStyle = textBlockStyle,
                    Binding = BindingUtils.GetIndexerBinding(x.ToString())
                });
            }

            var rows = new List<Dictionary<string, object>>();

            foreach (var y in data.Ys)
            {
                var row = new Dictionary<string, object>();
                row["Length"] = y;

                foreach (var x in data.Xs)
                {
                    double value;
                    if (data.TryGet(x, y, out value))
                    {
                        row[x.ToString()] = value;
                    }
                }

                rows.Add(row);
            }

            detailsList.ItemsSource = rows;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _timer.IsEnabled = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
            _timer.IsEnabled = true;
        }

        private void detailsButton_Checked(object sender, RoutedEventArgs e)
        {
            InitTableData();
            detailsPopup.IsOpen = true;
            _timer.IsEnabled = false;
        }

        private void detailsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            detailsPopup.IsOpen = false;
            _timer.IsEnabled = true;
        }

        private void detailsPopup_Closed(object sender, EventArgs e)
        {
            detailsButton.IsChecked = false;
        }

        private void BuildModel()
        {
            var surfaceModel = BuildSurfaceModel();

            var models = new Model3DCollection
                            {
                                new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)), 
                                BuildBaseModel(surfaceModel), 
                                BuildGraphModel(), 
                                surfaceModel
                            };

            _groupModel = new ModelVisual3D { Content = new Model3DGroup { Children = models } };

            mainViewport.Children.Clear();
            mainViewport.Children.Add(_groupModel);
        }

        /// <summary>
        /// NOTE: these graphs are not on the same axis as the surface, since they are to scale
        /// </summary>
        private Model3DGroup BuildGraphModel()
        {
            return new Model3DGroup
            {
                Children = new Model3DCollection
                                        {
                                            BuildXSliceModel(), 
                                            BuildYSliceModel(), 
                                        }
            };
        }

        private Model3DGroup BuildXSliceModel()
        {
            //LAP-85 Model3DGroup labelModel = BuildXSliceLabel();

            BuildXSliceGraphModel();
            var ret = new Model3DGroup();
            ret.Children.Add(_xSliceModel);
            //LAP-85 ret.Children.Add(labelModel);
            return ret;
        }

        private CurveControl _xSliceCurveControl;
        private GeometryModel3D _xSliceModel;

        private void BuildXSliceGraphModel()
        {
            _xSliceCurveControl = new CurveControl { Width = ProjectedCurveSize, Height = ProjectedCurveSize, YMin = 0, YMax = _zRange, StrokeThickness = 5.0, ShowName = false };
            var brush = new VisualBrush(_xSliceCurveControl);

            var material = new DiffuseMaterial(brush);

            var normal = new Vector3D(1, 0, 0);

            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                                               {
                                                   new Point3D(-GraphOffset, 0, 0), 
                                                   new Point3D(-GraphOffset, 1, 0), 
                                                   new Point3D(-GraphOffset, 0, 1), 
                                                   new Point3D(-GraphOffset, 1, 1)
                                               },
                Normals = new Vector3DCollection
                                             {
                                                 normal, 
                                                 normal, 
                                                 normal, 
                                                 normal
                                             },
                TriangleIndices = new Int32Collection
                                                     {
                                                         0, 1, 3, 
                                                         0, 3, 2
                                                     },
                TextureCoordinates = new PointCollection
                                                        {
                                                            new Point(0, ProjectedCurveSize), 
                                                            new Point(ProjectedCurveSize, ProjectedCurveSize), 
                                                            new Point(0, 0), 
                                                            new Point(ProjectedCurveSize, 0), 
                                                        }
            };
            _xSliceModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        private static Model3DGroup BuildYSliceLabel()
        {
            Model3DGroup ret = GetUnitLitText("<- Swap Length");

            var transform3Ds = new Transform3D[]
                                   {
                                       new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)),
                                       new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180)),
                                       new ScaleTransform3D(1.0, 1.0, labelHeight), 
                                       new TranslateTransform3D(1, -GraphOffset, 1 + labelHeight)
                                   };
            ret.Transform = new Transform3DGroup() { Children = new Transform3DCollection(transform3Ds) };
            return ret;
        }

        private static Model3DGroup BuildXSliceLabel()
        {
            Model3DGroup ret = GetUnitLitText("Option Expiry ->");

            var transform3Ds = new Transform3D[]
                                   {
                                       new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)),
                                       new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90)),
                                       new ScaleTransform3D(1.0, 1.0, labelHeight), 
                                       new TranslateTransform3D(-GraphOffset, 0, 1 + labelHeight)
                                   };
            ret.Transform = new Transform3DGroup() { Children = new Transform3DCollection(transform3Ds) };
            return ret;
        }

        private static Model3DGroup GetUnitLitText(string text)
        {
            GeometryModel3D textModel = GetUnitText(text);
            var lightModel = new AmbientLight(Colors.White);
            return new Model3DGroup()
            {
                Children = new Model3DCollection(new Model3D[] { textModel, lightModel })
            };
        }

        /// <param name="text">The text to put on the label</param>
        /// <returns>a text model with z=0, of unit size</returns>
        private static GeometryModel3D GetUnitText(string text)
        {
            TextBlock b = new TextBlock
            {
                Text = text,
                Background = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            DiffuseMaterial materialWithLabel = new DiffuseMaterial();
            materialWithLabel.Brush = new VisualBrush(b);
            materialWithLabel.Brush = new VisualBrush(b);

            var cuboid = new MeshGeometry3D();
            cuboid.Positions = new Point3DCollection(
                new[] { new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 1, 0), });
            cuboid.TextureCoordinates = new PointCollection(cuboid.Positions.Select(p => new Point(p.X, p.Y)));

            cuboid.TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, 1, 3, 2 });

            return new GeometryModel3D(cuboid, materialWithLabel)
            {
                BackMaterial = materialWithLabel
            };
        }

        private CurveControl _ySliceCurveControl;
        private GeometryModel3D _ySliceModel;

        private Model3DGroup BuildYSliceModel()
        {
            //LAP-85 Model3DGroup labelModel = BuildYSliceLabel();

            var graphModel = BuildYSliceGraphModel();
            var ret = new Model3DGroup();
            ret.Children.Add(graphModel);
            //LAP-85 ret.Children.Add(labelModel);
            return ret;
        }

        private GeometryModel3D BuildYSliceGraphModel()
        {
            _ySliceCurveControl = new CurveControl { Width = ProjectedCurveSize, Height = ProjectedCurveSize, YMin = 0, YMax = _zRange, StrokeThickness = 5.0, ShowName = false };
            var brush = new VisualBrush(_ySliceCurveControl);

            var material = new DiffuseMaterial(brush);

            var normal = new Vector3D(0, -1, 0);

            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                                               {
                                                   new Point3D(0, -GraphOffset, 0), 
                                                   new Point3D(1, -GraphOffset, 0), 
                                                   new Point3D(0, -GraphOffset, 1), 
                                                   new Point3D(1, -GraphOffset, 1)
                                               },
                Normals = new Vector3DCollection
                                             {
                                                 normal, 
                                                 normal, 
                                                 normal, 
                                                 normal
                                             },
                TriangleIndices = new Int32Collection
                                                     {
                                                         0, 1, 3, 
                                                         0, 3, 2
                                                     },
                TextureCoordinates = new PointCollection
                                                        {
                                                            new Point(0, ProjectedCurveSize), 
                                                            new Point(ProjectedCurveSize, ProjectedCurveSize), 
                                                            new Point(0, 0), 
                                                            new Point(ProjectedCurveSize, 0), 
                                                        }
            };

            _ySliceModel = new GeometryModel3D(mesh, material) { BackMaterial = material };
            return _ySliceModel;
        }

        private static GeometryModel3D BuildBaseModel(GeometryModel3D buildSurfaceModel)
        {
            var geometryModel3D = new GeometryModel3D(buildSurfaceModel.Geometry, buildSurfaceModel.Material) { BackMaterial = buildSurfaceModel.BackMaterial };
            var transform = new Matrix3D(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1);
            geometryModel3D.Transform = new MatrixTransform3D(transform);
            return geometryModel3D;
        }

        private Color _minSurfaceColor = Colors.Yellow;
        public Color MinSurfaceColor
        {
            get { return _minSurfaceColor; }
            set { _minSurfaceColor = value; }
        }

        private Color _maxSurfaceColor = Colors.Red;
        public Color MaxSurfaceColor
        {
            get { return _maxSurfaceColor; }
            set { _maxSurfaceColor = value; }
        }

        private GeometryModel3D BuildSurfaceModel()
        {
            var mesh = new MeshGeometry3D();

            var keys = GetSurfaceKeys();
            List<Tenor> xKeys = keys.Item1;
            List<Tenor> yKeys = keys.Item2;

            // TODO this should really be a texture generated by interpolating the surface
            var linearGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            const int bands = 2;
            for (int i = 0; i < bands; i++)
            {
                var offset = i / (float)bands;
                Color color = ValueToColorConverter.GetColor(offset * _zRange, MinSurfaceColor, MaxSurfaceColor);
                linearGradientBrush.GradientStops.Add(new GradientStop(color - Color.FromArgb(10, 0, 0, 0), offset));
            }

            var diffuseMaterial = new DiffuseMaterial(linearGradientBrush);

            // Points
            Matrix3D scaleMatrix = GetScaleMatrix(xKeys, yKeys);
            if (ToScale)
            {
                foreach (Tenor yKey in yKeys)
                {
                    foreach (Tenor xKey in xKeys)
                    {
                        var xValue = GetScaledValue(xKey);
                        var yValue = GetScaledValue(yKey);
                        var zValue = Surface[xKey, yKey];
                        var point3D = new Point3D(xValue, yValue, zValue) * scaleMatrix;

                        mesh.Positions.Add(point3D);
                    }
                }
            }
            else
            {
                for (int yi = 0; yi < yKeys.Count; yi++)
                {
                    for (int xi = 0; xi < xKeys.Count; xi++)
                    {
                        var zValue = Surface[xKeys[xi], yKeys[yi]];

                        mesh.Positions.Add(new Point3D(xi, yi, zValue) * scaleMatrix);
                    }
                }
            }

            // Triangles and normals
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var normals = new List<Vector3D>(4);

                    if (yi < yKeys.Count - 1 && xi < xKeys.Count - 1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi + 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    if (yi > 0 && xi > 0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi - 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi - 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    // We don't need triangles here, but we need normals
                    if (yi > 0 && xi < xKeys.Count - 1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi - 1) * xKeys.Count);
                        var p2 = xi + 1 + (yi * xKeys.Count);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    if (yi < yKeys.Count - 1 && xi > 0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi + 1) * xKeys.Count);
                        var p2 = xi - 1 + (yi * xKeys.Count);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    mesh.Normals.Add(normals.Aggregate(new Vector3D(0, 0, 0), (a, b) => a + b));
                }
            }

            // Texture co-ordinates
            double colorScale = 1 / _zRange;
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var zValue = Surface[xKeys[xi], yKeys[yi]];

                    // Try and make sure all the triangles are real in texture space
                    var fakeYValue = (xi / (float)xKeys.Count + yi / (float)yKeys.Count) * 0.5;

                    var point = new Point(zValue * colorScale, fakeYValue);
                    mesh.TextureCoordinates.Add(point);
                }
            }

            return new GeometryModel3D(mesh, diffuseMaterial) { BackMaterial = diffuseMaterial };
        }

        private Matrix3D GetScaleMatrix(List<Tenor> xKeys, List<Tenor> yKeys)
        {
            double zScale = 1.0 / _zRange;
            Matrix3D scaleMatrix;
            if (ToScale)
            {
                var xMax = xKeys.Select(GetScaledValue).Max();
                var xMin = xKeys.Select(GetScaledValue).Min();
                double xScale = 1.0 / (xMax - xMin);
                var yMax = yKeys.Select(GetScaledValue).Max();
                var yMin = yKeys.Select(GetScaledValue).Min();
                double yScale = 1.0 / (yMax - yMin);

                scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    -xMin * xScale, -yMin * yScale, 0, 1);
            }
            else
            {
                double xScale = 1.0 / (xKeys.Count - 1);
                double yScale = 1.0 / (yKeys.Count - 1);

                scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    0, 0, 0, 1);
            }
            return scaleMatrix;
        }

        private Tuple<List<Tenor>, List<Tenor>> GetSurfaceKeys()
        {
            //TODO: handle sparse surfaces properly
            var xKeys = new List<Tenor>(Surface.Xs);
            var yKeys = new List<Tenor>(Surface.Ys);

            foreach (var x in xKeys.ToList())
            {
                double v;
                if (!yKeys.Any(y => Surface.TryGet(x, y, out v)))
                {
                    xKeys.Remove(x);
                }
            }

            foreach (var y in yKeys.ToList())
            {
                double v;
                if (!xKeys.Any(x => Surface.TryGet(x, y, out v)))
                {
                    yKeys.Remove(y);
                }
            }
            return Tuple.Create(xKeys, yKeys);
        }

        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private static double GetScaledValue(Tenor t)
        {
            return Math.Log(t.TimeSpan.TotalDays);
        }

        private VolatilitySurfaceData<Tenor, Tenor> Surface
        {
            get { return (VolatilitySurfaceData<Tenor, Tenor>)DataContext; }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _haveInitedData = false;
            if (DataContext is VolatilitySurfaceData<Tenor, Tenor>)
            {
                BuildModel();
            }
            else
            {
                mainViewport.Children.Clear();
                _groupModel = null;
            }
        }

        private void mainViewport_MouseMove(object sender, MouseEventArgs e)
        {
            _timer.IsEnabled = false;
            Point position = e.GetPosition(mainViewport);
            UpdateToolTip(position);

            if (IsDragging(e) && _dragStart != null)
            {
                Drag(_dragStart, position);
            }
        }

        private void Drag(Tuple<Point, Point3D> startTuple, Point position)
        {
            var transform3DGroup = (Transform3DGroup)_groupModel.Transform;

            var hits = GetHits(_ => HitTestFilterBehavior.Continue, position);
            var meshHits = hits.Cast<RayMeshGeometry3DHitTestResult>().Where(h => h.MeshHit == _dragObject);
            var dragObjectHit = meshHits.OrderBy(h => (h.PointHit - startTuple.Item2).LengthSquared).FirstOrDefault();
            if (dragObjectHit == null)
            {
                return;
            }
            Point3D start = startTuple.Item2;
            Point3D end = transform3DGroup.Transform(dragObjectHit.PointHit);

            Point3D origin = Center; // new Point3D();

            Vector3D startVector = start - origin;
            Vector3D endVector = end - origin;
            var axis = Vector3D.CrossProduct(startVector, endVector);
            var cosAngle = Vector3D.DotProduct(startVector, endVector) / (startVector.Length * endVector.Length);
            var angleRads = Math.Acos(cosAngle);

            axis.Normalize();
            var r = new QuaternionRotation3D(new Quaternion(axis, RadianToDegree(angleRads)));

            transform3DGroup.Children.RemoveAt(transform3DGroup.Children.Count - 1);
            transform3DGroup.Children.Add(new RotateTransform3D(r, origin));
        }

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private Tuple<Point, Point3D> _dragStart = null;
        private ModelVisual3D _groupModel;
        private MeshGeometry3D _dragObject;

        private void mainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(mainViewport);
            var hit = GetHit(position);
            var mesh = hit.Item2;
            if (mesh == _xSliceModel.Geometry || mesh == _ySliceModel.Geometry)
            {
                if (_xSliceModel.Transform == Transform3D.Identity)
                {
                    const double offset = GraphOffset * 2 + 1;
                    _xSliceModel.Transform = new TranslateTransform3D(offset, 0, 0);
                    _ySliceModel.Transform = new TranslateTransform3D(0, offset, 0);
                }
                else
                {
                    _xSliceModel.Transform = Transform3D.Identity;
                    _ySliceModel.Transform = Transform3D.Identity;
                }
            }
            else
            {
                if (IsDragging(e))
                {
                    _dragStart = Tuple.Create(position, _groupModel.Transform.Transform(hit.Item1));
                    _dragObject = hit.Item2;
                    if (_groupModel.Transform is MatrixTransform3D && ((MatrixTransform3D)_groupModel.Transform).Matrix.IsIdentity)
                    {
                        _groupModel.Transform = new Transform3DGroup();
                    }
                    ((Transform3DGroup)_groupModel.Transform).Children.Add(new MatrixTransform3D(Matrix3D.Identity));
                }
            }
        }

        private static bool IsDragging(MouseEventArgs e)
        {
            return e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released;
        }

        private void mainViewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragStart = null;
        }

        private Tuple<Point3D, MeshGeometry3D> GetHit(Point position)
        {
            HitTestFilterCallback htrcb = potentialHitTestTarget => HitTestFilterBehavior.Continue;

            IEnumerable<HitTestResult> results = GetHits(htrcb, position);
            var hitTestResult = results.FirstOrDefault();
            if (hitTestResult != null && hitTestResult.VisualHit != null)
            {
                if (!(hitTestResult is RayMeshGeometry3DHitTestResult))
                    throw new ArgumentException();
                var result = (RayMeshGeometry3DHitTestResult)hitTestResult;

                return Tuple.Create(result.PointHit, result.MeshHit);
            }
            return default(Tuple<Point3D, MeshGeometry3D>);
        }

        private IEnumerable<HitTestResult> GetHits(HitTestFilterCallback htrcb, Point position)
        {
            var results = new List<HitTestResult>();

            HitTestResultCallback cb = delegate(HitTestResult result)
            {
                results.Add(result);
                return HitTestResultBehavior.Continue;
            };

            VisualTreeHelper.HitTest(mainViewport, htrcb, cb, new PointHitTestParameters(position));
            return results;
        }

        private void UpdateToolTip(Point position)
        {
            if (Surface == null || ToScale)
            {
                // TODO this isn't right if we're 'ToScale'ing
                toolTip.IsOpen = false;
                return;
            }

            var surfaceKeys = GetSurfaceKeys();
            var xs = surfaceKeys.Item1;
            var ys = surfaceKeys.Item2;

            double xScale = 1.0 / (xs.Count - 1);
            double yScale = 1.0 / (ys.Count - 1);

            var hit = GetHit(position);
            if (hit != default(Tuple<Point3D, MeshGeometry3D>))
            {
                var point = hit.Item1;
                var yFloor = (int)Math.Round(point.Y / yScale);
                var xFloor = (int)Math.Round(point.X / xScale);

                if (xFloor < 0 || yFloor < 0 || xFloor >= xs.Count || yFloor >= ys.Count)
                {// The graph slices
                    toolTip.IsOpen = false;
                    return;
                }

                var x = xs[xFloor];
                var y = ys[yFloor];

                UpdateToolTip(x, y);
                UpdateCellSelection(x, y);
                SetXSliceGraph(x);
                SetYSliceGraph(y);
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }

        private void UpdateCellSelection(Tenor x, Tenor y)
        {
            InitTableData();

            Func<Dictionary<string, object>, bool> rowPredicate = row => row["Length"].Equals(y);

            Func<DataGridColumn, bool> columnPredicate = c => c.Header.Equals(x);

            DataGridColumn myColumn = detailsList.Columns.Where(columnPredicate).First();
            Dictionary<string, object> myRow = detailsList.Items.Cast<Dictionary<string, object>>().Where(rowPredicate).First();

            detailsList.SelectedCells.Clear();
            detailsList.SelectedCells.Add(new DataGridCellInfo(myRow, myColumn));
        }

        private void UpdateToolTip(Tenor x, Tenor y)
        {
            toolTipBox.Text = string.Format("{0},{1},{2}", x, y, Surface[x, y]);
            toolTip.IsOpen = true;
        }

        private void SetYSliceGraph(Tenor y)
        {
            var slice = Surface.GetYSlice(y);
            _ySliceCurveControl.DataContext = slice;
            leftCurveControl.DataContext = slice;
        }

        private void SetXSliceGraph(Tenor x)
        {
            var slice = Surface.GetXSlice(x);
            _xSliceCurveControl.DataContext = slice;
            rightCurveControl.DataContext = slice;
        }

        private void detailsList_CurrentCellChanged(object sender, EventArgs e)
        {
            var dataGridCellInfo = detailsList.CurrentCell;

            var x = dataGridCellInfo.Column == null ? null : dataGridCellInfo.Column.Header as Tenor;
            var y = dataGridCellInfo.Item as Dictionary<string, object>;

            if (x != null)
            {
                SetXSliceGraph(x);
            }

            if (y != null)
            {
                SetYSliceGraph((Tenor)y["Length"]);
            }
        }
    }
}