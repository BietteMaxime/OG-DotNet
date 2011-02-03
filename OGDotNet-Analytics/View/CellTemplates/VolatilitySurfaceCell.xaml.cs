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
using OGDotNet_Analytics.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet_Analytics.Mappedtypes.Util.Time;
using OGDotNet_Analytics.Properties;

namespace OGDotNet_Analytics.View.CellTemplates
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

            double speed = 0.01;

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

        private void InitData()
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
            InitData();
            detailsPopup.IsOpen = true;
            _timer.IsEnabled = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
            toolTip.IsOpen = false;
            _timer.IsEnabled = true;
        }

        /// <summary>
        /// TODO There is no reason for all these separate model groups, should be reusing the vertices
        /// </summary>
        private static Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, float colorQuotient0, float colorQuotient1, float colorQuotient2)
        {
            var color0 = GetColor(colorQuotient0);
            var color1 = GetColor(colorQuotient1);
            var color2 = GetColor(colorQuotient2);

            return CreateTriangleModel(p0, p1, p2, color0, color1, color2);
        }

        private static Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, Color color0, Color color1, Color color2)
        {
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(0.5, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));

            var linearGradientBrush = new LinearGradientBrush(
                new GradientStopCollection
                    {
                        new GradientStop(color0, 0),
                        new GradientStop(color1, 0.5),
                        new GradientStop(color2, 1)
                    })
                                          {
                                              StartPoint = new Point(0, 0),
                                              EndPoint = new Point(1, 0)
                                          };

            var diffuseMaterial = new DiffuseMaterial(linearGradientBrush);

            var model = new GeometryModel3D(mesh, diffuseMaterial) { BackMaterial = diffuseMaterial };
            var group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }

        private static Color GetColor(float colorQuotient)
        {

            return (Colors.White * (1-colorQuotient) + Colors.Red* colorQuotient);
        }

        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private void BuildModel()
        {
            var group = new Model3DGroup();

            var xKeys = Surface.Xs;
            var yKeys = Surface.Ys;
            
            const double zRange = 100.0;
            const double colorScale = 1 / zRange;
            const double zScale = 1.0 / zRange;


            if (ToScale)//TODO make a decision about ToScale
            {
                var xMax = Surface.Xs.Select(GetScaledValue).Max();
                double xScale = 1.0 / xMax;
                var yMax = Surface.Ys.Select(GetScaledValue).Max();
                double yScale = 1.0 / yMax;

                var scaleMatrix = new Matrix3D(
                    xScale,     0,      0,      0,
                    0,          yScale, 0,      0,
                    0,          0,      zScale, 0,

                    0, 0, 0, 1);


                for (int yi = 0; yi < yKeys.Count - 1; yi++)
                {
                    for (int xi = 0; xi < xKeys.Count - 1; xi++)
                    {

                        var zValue = Surface[xKeys[xi], yKeys[yi]];
                        var right= Surface[xKeys[xi+1], yKeys[yi]];
                        var above = Surface[xKeys[xi], yKeys[yi+1]];

                        group.Children.Add(CreateTriangleModel(
                            GetPoint(xi, yi, Surface, scaleMatrix),
                            GetPoint(xi + 1, yi, Surface, scaleMatrix),
                            GetPoint(xi, yi + 1, Surface, scaleMatrix)
                            , (float)(colorScale * zValue), (float)(colorScale * right), (float)(colorScale * above)));
                    }
                }

                for (int yi = 1; yi < yKeys.Count; yi++)
                {
                    for (int xi = 1; xi < xKeys.Count; xi++)
                    {
                        var zValue = Surface[xKeys[xi], yKeys[yi]];
                        var left = Surface[xKeys[xi - 1], yKeys[yi]];
                        var below = Surface[xKeys[xi], yKeys[yi - 1]];

                        group.Children.Add(CreateTriangleModel(
                            GetPoint(xi, yi, Surface, scaleMatrix),
                            GetPoint(xi - 1, yi, Surface, scaleMatrix),
                            GetPoint(xi, yi - 1, Surface, scaleMatrix)
                            , (float)(colorScale * zValue), (float)(colorScale * left), (float)(colorScale * below)));
                    }
                }
            }
            else
            {
                var xs = Surface.Xs.ToList();
                var ys = Surface.Ys.ToList();
                
                double xScale = 1.0 / (xs.Count - 1);
                double yScale = 1.0 / (ys.Count - 1);




                for (int yi = 0; yi < ys.Count - 1; yi++)
                {
                    for (int xi = 0; xi < xs.Count - 1; xi++)
                    {
                        var at = Surface[xs[xi], ys[yi]];
                        var right = Surface[xs[xi + 1], ys[yi]];
                        var above = Surface[xs[xi], ys[yi + 1]];

                        group.Children.Add(CreateTriangleModel(
                            new Point3D(xi * xScale, yi * yScale, at * zScale),
                            new Point3D((xi + 1) * xScale, (yi) * yScale, right * zScale),
                            new Point3D((xi) * xScale, (yi + 1) * yScale, above * zScale), (float)(colorScale * at), (float)(colorScale * right), (float)(colorScale * above)));
                    }
                }
                for (int yi = 1; yi < ys.Count; yi++)
                {
                    for (int xi = 1; xi < xs.Count; xi++)
                    {
                        var at = Surface[xs[xi], ys[yi]];
                        var left = Surface[xs[xi - 1], ys[yi]];
                        var below = Surface[xs[xi], ys[yi - 1]];

                        
                        group.Children.Add(CreateTriangleModel(
                            new Point3D(xi * xScale, yi * yScale, at * zScale),
                            new Point3D((xi - 1) * xScale, (yi) * yScale, left * zScale),
                            new Point3D((xi) * xScale, (yi - 1) * yScale, below * zScale), (float)(colorScale * at), (float)(colorScale * left), (float)(colorScale * below)));
                    }
                }

            }

            group.Children.Add(CreateTriangleModel(
                new Point3D(0,0,0), 
                new Point3D(1,0,0), 
                new Point3D(0,1,0), 
                0,0,0
                ));
            group.Children.Add(CreateTriangleModel(
                new Point3D(0, 1, 0),                
                new Point3D(1, 0, 0),
                new Point3D(1, 1, 0),
                0,0,0
                ));

            var model = new ModelVisual3D {Content = group};
            
            for (int i = 0; i < mainViewport.Children.Count; i++)
            {
                if (mainViewport.Children[i] is ModelVisual3D)
                {
                    mainViewport.Children.RemoveAt(i);
                    i--;
                }
            }
            var lightModel = new ModelVisual3D {Content = new DirectionalLight(Colors.White, new Vector3D(0, 0, -1))};

            mainViewport.Children.Clear();
            mainViewport.Children.Add(lightModel);
            mainViewport.Children.Add(model);
            
        }

        private static double GetScaledValue(Tenor t)
        {
            return t.TimeSpan.TotalDays;
        }

        private VolatilitySurfaceData Surface
        {
            get { return (VolatilitySurfaceData) DataContext; }
        }

        private static Point3D GetPoint(int xi, int yi, VolatilitySurfaceData surface, Matrix3D scaleMatrix)
        {
            var xTenor = surface.Xs[xi];
            var yTenor = surface.Ys[yi];
            var xValue= GetScaledValue(xTenor);
            var yValue = GetScaledValue(yTenor);
            var zValue = surface[xTenor, yTenor];
            return new Point3D(xValue, yValue, zValue) * scaleMatrix;
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

                toolTipBox.Text = string.Format("{0},{1},{2}", x,y, Surface[x,y]);
                toolTip.IsOpen = true;
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }
    }
}
