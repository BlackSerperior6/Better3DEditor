using HelixToolkit.Wpf;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3DRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counter;
        public List<LinesVisual3D> Figures;

        public MainWindow()
        {
            Figures = new List<LinesVisual3D>();
            counter = 0;
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            PointsEditor editor = new PointsEditor();

            bool? result = editor.ShowDialog();

            if ((bool)!result)
                return;

            Point3DCollection point3Ds = [.. editor.points];
            point3Ds.Add(point3Ds[0]);

            var lines = new LineWrapper($"Линия {++counter}")
            {
                Points = point3Ds,
                Color = Colors.Blue,
                Thickness = 5
            };

            ViewPoint.Children.Add(lines);
            FigureList.Items.Add(lines);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedLine = FigureList.SelectedItem;

            if (selectedLine == null)
                return;

            ViewPoint.Children.Remove(selectedLine as LineWrapper);
            FigureList.Items.Remove(selectedLine);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ViewPoint.Children.Count - 1; i >= 0; i--)
            {
                if (ViewPoint.Children[i] is ModelVisual3D visual && visual is not (GridLinesVisual3D or DefaultLights))
                    ViewPoint.Children.RemoveAt(i);
            }

            FigureList.Items.Clear();
        }
    }
}