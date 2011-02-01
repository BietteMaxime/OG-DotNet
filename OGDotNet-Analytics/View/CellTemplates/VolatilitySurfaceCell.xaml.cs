using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace OGDotNet_Analytics.View.CellTemplates
{
    /// <summary>
    /// TODO work out how to present this
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {

        const double size = 5.0;

        private bool _haveInitedData;
        private readonly DispatcherTimer _timer;

        public VolatilitySurfaceCell()
        {
            InitializeComponent();
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1000.0)};

            // Start the timer
            double t = 0;
            double direction = 0.01;
            SetCamera(0);
            _timer.Start();

            _timer.Tick += delegate
                               {
                                   _timer.Interval = TimeSpan.FromMilliseconds(50.0);

                                   SetCamera(t);


                                   if (t > 1.5)
                                   {
                                       direction = -Math.Abs(direction);
                                   }
                                   if (t<-1.5)
                                   {
                                       direction = Math.Abs(direction);
                                   }
                                   t += direction;
                               };
        }

        private void SetCamera(double t)
        {
            camera.Position = new Point3D(0.5*size, size * (0.5 + (Math.Sin(t) * 2)), size * (0.5 + (Math.Cos(t) * 2)));
            camera.LookDirection = new Point3D(0.5 * size, 0.5 * size, 0.5 * size) - camera.Position;
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
            popup.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            popup.IsOpen = false;
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

            Color color = GetColor(colorQuotient);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(color));
            var model = new GeometryModel3D(mesh, material);
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

            var data = (VolatilitySurfaceData)DataContext;

            var xs = data.Xs.ToList();
            var ys = data.Ys.ToList();


            double xScale = size/(xs.Count-1);
            double yScale = size / (ys.Count -1);

            double colorScale = 1/ xs.SelectMany(x => ys.Select(y => data[x, y])).Max();
            double heightScale = size/xs.SelectMany(x => ys.Select(y => data[x, y])).Max();
            



            for (int yi   = 0; yi < ys.Count-1; yi++)
            {
                for (int xi = 0; xi < xs.Count-1; xi++)
                {
                    var at = GetPoint(xi, yi, xs, ys, data);
                    var right = GetPoint(xi+1, yi, xs, ys, data);
                    var above = GetPoint(xi, yi+1, xs, ys, data);

                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi*xScale, yi*yScale, at*heightScale),
                        new Point3D((xi + 1) * xScale, (yi) * yScale, right * heightScale),
                        new Point3D((xi) * xScale, (yi + 1) * yScale, above * heightScale), (float) (colorScale * at)));

                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi * xScale, yi * yScale, at * heightScale),
                        new Point3D((xi) * xScale, (yi + 1) * yScale, above * heightScale), 
                        new Point3D((xi + 1) * xScale, (yi) * yScale, right * heightScale),
                        (float)(colorScale * at)));
                }
            }
            for (int yi = 1; yi < ys.Count; yi++)
            {
                for (int xi = 1; xi < xs.Count; xi++)
                {
                    var at = GetPoint(xi, yi, xs, ys, data);
                    var left = GetPoint(xi - 1, yi, xs, ys, data);
                    var below = GetPoint(xi, yi - 1, xs, ys, data);

                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi * xScale, yi * yScale, at * heightScale),
                        new Point3D((xi - 1) * xScale, (yi) * yScale, left * heightScale),
                        new Point3D((xi) * xScale, (yi - 1) * yScale, below * heightScale), (float)(colorScale * at)));
                    group.Children.Add(CreateTriangleModel(
                        new Point3D(xi * xScale, yi * yScale, at * heightScale),
                        new Point3D((xi) * xScale, (yi - 1) * yScale, below * heightScale),
                        new Point3D((xi - 1) * xScale, (yi) * yScale, left * heightScale),
                        (float)(colorScale * at)));
                }
            }


            group.Children.Add(CreateTriangleModel(
                new Point3D(0,0,0), 
                new Point3D(size,0,0), 
                new Point3D(0,size,0), 
                0
                                   ));
            group.Children.Add(CreateTriangleModel(
                new Point3D(0, size, 0),                
                new Point3D(size, 0, 0),
                new Point3D(size, size, 0),
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

        private static double GetPoint(int xi, int yi, List<Tenor> xs, List<Tenor> ys, VolatilitySurfaceData data)
        {
            return data[xs[xi], ys[yi]];
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is VolatilitySurfaceData)
                BuildModel();
        }
    }
}
