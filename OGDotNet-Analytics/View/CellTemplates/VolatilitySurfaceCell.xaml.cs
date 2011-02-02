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

namespace OGDotNet_Analytics.View.CellTemplates
{
    /// <summary>
    /// TODO work out how to present this
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {
        private bool _haveInitedData;
        private readonly DispatcherTimer _timer;

        public VolatilitySurfaceCell()
        {
            InitializeComponent();
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(80.0)};

            double speed = 0.06;

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
            Point3D center = new Point3D(0.5,0.5,0.5);

            const double circleRadius = 2.2;

            camera.Position = center + new Vector3D(Math.Sin(t), Math.Cos(t), 0) * circleRadius;
            Point3D lookTarget = center;
            camera.LookDirection = lookTarget - camera.Position;
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

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InitData();
            detailsPopup.IsOpen = true;
            _timer.IsEnabled = false;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            detailsPopup.IsOpen = false;
            toolTip.IsOpen = false;
            _timer.IsEnabled = true;
        }

        private static Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, float colorQuotient)
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

            var color = GetColor(colorQuotient);
            var brush = new SolidColorBrush(color);
            var diffuseMaterial = new DiffuseMaterial(brush);
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

            var xs = Surface.Xs.ToList();
            var ys = Surface.Ys.ToList();


            double xScale = 1.0/(xs.Count-1);
            double yScale = 1.0 / (ys.Count -1);

            const double zRange = 100.0;
            const double colorScale = 1 / zRange;
            const double zScale = 1.0 / zRange;
            



            for (int yi   = 0; yi < ys.Count-1; yi++)
            {
                for (int xi = 0; xi < xs.Count-1; xi++)
                {
                    var at = GetPoint(xi, yi, xs, ys, Surface);
                    var right = GetPoint(xi+1, yi, xs, ys, Surface);
                    var above = GetPoint(xi, yi+1, xs, ys, Surface);

                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi*xScale, yi*yScale, at*zScale),
                        new Point3D((xi + 1) * xScale, (yi) * yScale, right * zScale),
                        new Point3D((xi) * xScale, (yi + 1) * yScale, above * zScale), (float) (colorScale * at)));
                }
            }
            for (int yi = 1; yi < ys.Count; yi++)
            {
                for (int xi = 1; xi < xs.Count; xi++)
                {
                    var at = GetPoint(xi, yi, xs, ys, Surface);
                    var left = GetPoint(xi - 1, yi, xs, ys, Surface);
                    var below = GetPoint(xi, yi - 1, xs, ys, Surface);

                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi * xScale, yi * yScale, at * zScale),
                        new Point3D((xi - 1) * xScale, (yi) * yScale, left * zScale),
                        new Point3D((xi) * xScale, (yi - 1) * yScale, below * zScale), (float)(colorScale * at)));
                }
            }


            group.Children.Add(CreateTriangleModel(
                new Point3D(0,0,0), 
                new Point3D(1,0,0), 
                new Point3D(0,1,0), 
                0
                ));
            group.Children.Add(CreateTriangleModel(
                new Point3D(0, 1, 0),                
                new Point3D(1, 0, 0),
                new Point3D(1, 1, 0),
                0
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

        private VolatilitySurfaceData Surface
        {
            get { return (VolatilitySurfaceData) DataContext; }
        }

        private static double GetPoint(int xi, int yi, List<Tenor> xs, List<Tenor> ys, VolatilitySurfaceData data)
        {
            return data[xs[xi], ys[yi]];
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
