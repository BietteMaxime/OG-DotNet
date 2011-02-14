using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.AnalyticsViewer.Properties;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// TODO work out how to present this
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {
        private static readonly bool ToScale = Settings.Default.ShowVolatilityCurveToScale;
        static readonly Point3D Center = new Point3D(0.5, 0.5, 0.5);


        private bool _haveInitedData;
        private readonly DispatcherTimer _timer;

        public VolatilitySurfaceCell()
        {
            InitializeComponent();
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(80.0)};

            double speed = 0.02;

            double t = Math.PI / 4.0;
            const double maxT= Math.PI/2.0;
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
            const double circleRadius = 2.2;

            camera.Position = Center + new Vector3D(Math.Sin(t), Math.Cos(t), 0) * circleRadius;
            camera.LookDirection = Center - camera.Position;
        }

        private void InitTableData()
        {
            if (_haveInitedData) return;
            _haveInitedData = true;

            var data = (VolatilitySurfaceData)DataContext;

            var view = (GridView)detailsList.View;

            foreach (var x in data.Xs)
            {
                view.Columns.Add(new GridViewColumn
                                     {
                                         Width = Double.NaN,
                                         Header = x,
                                         DisplayMemberBinding = new Binding(string.Format("[{0}]", x))
                                     });
            }

            
            var rows = new List<Dictionary<string, object>>();

            foreach (var y in data.Ys)
            {
                var row = new Dictionary<string, object>();
                row["Length"] = y.ToString();

                foreach (var x in data.Xs)
                {
                    row[x.ToString()] = data[x, y];
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
        }

        private void detailsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            detailsPopup.IsOpen = false;
        }


        private void BuildModel()
        {
            var models = new Model3DCollection
                            {
                                new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)),
                                BuildBaseModel(),
                                BuildSurfaceModel()
                            };

            var groupModel = new ModelVisual3D { Content = new Model3DGroup { Children = models } };

            mainViewport.Children.Clear();
            mainViewport.Children.Add(groupModel);

        }

        private static GeometryModel3D BuildBaseModel()
        {
            var brush = new SolidColorBrush(Colors.WhiteSmoke);
            var material = new DiffuseMaterial(brush);

            var normal = new Vector3D(0, 0, 1);


            var mesh = new MeshGeometry3D
                           {
                               Positions = new Point3DCollection
                                               {
                                                   new Point3D(0, 0, 0),
                                                   new Point3D(1, 0, 0),
                                                   new Point3D(0, 1, 0),
                                                   new Point3D(1, 1, 0)
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
                                                         0,1,3,
                                                         0,3,2
                                                     }
                           };


            return new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        private GeometryModel3D BuildSurfaceModel()
        {
            const double zRange = 100.0;
            const double zScale = 1.0 / zRange;

            var mesh = new MeshGeometry3D();

            var xKeys = Surface.Xs;
            var yKeys = Surface.Ys;

            
            //TODO this should really be a texture generated by interpolating the surface
            var linearGradientBrush = new LinearGradientBrush(GetColor(0), GetColor(1), new Point(0, 0), new Point(1, 0));
            var diffuseMaterial = new DiffuseMaterial(linearGradientBrush);
            

            //Points
            if (ToScale)
            {
                var xMax = Surface.Xs.Select(GetScaledValue).Max();
                var xMin = Surface.Xs.Select(GetScaledValue).Min();
                double xScale = 1.0 / (xMax-xMin);
                var yMax = Surface.Ys.Select(GetScaledValue).Max();
                var yMin = Surface.Ys.Select(GetScaledValue).Min();
                double yScale = 1.0 / (yMax-yMin);

                var scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    -xMin*xScale, -yMin*yScale, 0, 1);

                for (int yi = 0; yi < yKeys.Count; yi++)
                {
                    for (int xi = 0; xi < xKeys.Count; xi++)
                    {
                        var xValue= GetScaledValue(xKeys[xi]);
                        var yValue = GetScaledValue(yKeys[yi]);
                        var zValue = Surface[xKeys[xi], yKeys[yi]];
                        var point3D = new Point3D(xValue, yValue, zValue) * scaleMatrix;

                        mesh.Positions.Add(point3D);
                    }
                }
            }
            else
            {
                double xScale = 1.0 / (xKeys.Count - 1);
                double yScale = 1.0 / (yKeys.Count - 1);

                var scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    0, 0, 0, 1);

                for (int yi = 0; yi < yKeys.Count; yi++)
                {
                    for (int xi = 0; xi < xKeys.Count; xi++)
                    {
                        var zValue = Surface[xKeys[xi], yKeys[yi]];

                        mesh.Positions.Add(new Point3D(xi, yi, zValue) * scaleMatrix);
                    }
                }
            }

            //Triangles and normals
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var normals = new List<Vector3D>(4);
                   
                    if (yi < yKeys.Count - 1 && xi < xKeys.Count-1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi + 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }
                    if (yi >0 && xi >0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi - 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi - 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    //We don't need triangles here, but we need normals
                    if (yi>0 && xi < xKeys.Count-1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi - 1) * xKeys.Count);
                        var p2 = xi + 1 + (yi * xKeys.Count);
                        
                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }
                    if (yi < yKeys.Count-1 && xi >0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi + 1) * xKeys.Count);
                        var p2 = xi - 1 + (yi * xKeys.Count);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    mesh.Normals.Add(normals.Aggregate(new Vector3D(0, 0, 0), (a, b) => a + b));
                }
            }

            //Texture co-ordinates
            const double colorScale = 1 / zRange;
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var zValue = Surface[xKeys[xi], yKeys[yi]];

                    //Try and make sure all the triangles are real in texture space
                    var fakeYValue = (xi / (float)xKeys.Count + yi / (float)yKeys.Count) * 0.5;

                    var point = new Point(zValue * colorScale, fakeYValue);
                    mesh.TextureCoordinates.Add(point);

                }
            }

            return new GeometryModel3D(mesh, diffuseMaterial) { BackMaterial = diffuseMaterial };
        }


        private static Color GetColor(float colorQuotient)
        {
            return (Colors.LightGreen * (1 - colorQuotient) + Colors.Red * colorQuotient);
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

        private VolatilitySurfaceData Surface
        {
            get { return (VolatilitySurfaceData) DataContext; }
        }


        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is VolatilitySurfaceData)
                BuildModel();
        }

        private void mainViewport_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(mainViewport);
            UpdateToolTip(position);
        }

        private void UpdateToolTip(Point position)
        {
            //TODO this isn't right if we're 'ToScale'ing
            var xs = Surface.Xs.ToList();
            var ys = Surface.Ys.ToList();


            double xScale = 1.0 / (xs.Count - 1);
            double yScale = 1.0 / (ys.Count - 1);


            HitTestResult hitTestResult = VisualTreeHelper.HitTest(mainViewport, position);
            if (hitTestResult!= null && hitTestResult.VisualHit != null)
            {
                if (!(hitTestResult is RayMeshGeometry3DHitTestResult))
                    throw new ArgumentException();
                var result = (RayMeshGeometry3DHitTestResult) hitTestResult;

                var point = result.PointHit;

                var floor = Math.Min((int) Math.Round(point.X/xScale), xs.Count-1);

                var x = xs[floor];
                floor = Math.Min((int)Math.Round(point.Y / yScale), ys.Count - 1);
                var y = ys[floor];

                UpdateToolTip(x, y);
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }

        private void UpdateToolTip(Tenor x, Tenor y)
        {
            toolTipBox.Text = string.Format("{0},{1},{2}", x,y, Surface[x,y]);
            toolTip.IsOpen = true;
            leftCurveControl.DataContext = Surface.GetXSlice(x);
            rightCurveControl.DataContext = Surface.GetYSlice(y);
        }

    }
}
