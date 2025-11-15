using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            counter = 0;

            Figures = new List<LinesVisual3D>();
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
            foreach (var line in FigureList.SelectedItems) 
            {
                if (line == null)
                    continue;

                ViewPoint.Children.Remove(line as LineWrapper);
                FigureList.Items.Remove(line);
            }
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

        private void TransformButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double xTranslate = Convert.ToDouble(TranslateXTextBox.Text);
                double yTranslate = Convert.ToDouble(TranslateYTextBox.Text);
                double zTranslate = Convert.ToDouble(TranslateZTextBox.Text);

                double angle = Convert.ToDouble(RotateAngleTextBox.Text);

                double xScale = Convert.ToDouble(ScaleXTextBox.Text);
                double yScale = Convert.ToDouble(ScaleYTextBox.Text);
                double zScale = Convert.ToDouble(ScaleZTextBox.Text);

                var selectedFigures = FigureList.SelectedItems;
                var group = new ModelVisual3D();

                foreach (var figure in selectedFigures)
                {
                    if (figure == null || figure is not LineWrapper line)
                        continue;

                    ViewPoint.Children.Remove(line);
                    group.Children.Add(line);
                }

                var transformGroup = new Transform3DGroup();

                transformGroup.Children.Add(new TranslateTransform3D(xTranslate, yTranslate, zTranslate));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angle)));
                transformGroup.Children.Add(new ScaleTransform3D(xScale, yScale, zScale));

                group.Transform = transformGroup;

                foreach (var figure in selectedFigures)
                {
                    if (figure == null || figure is not LineWrapper line)
                        continue;

                    BakeTransform(line, group);
                }

                /*var selected = FigureList.SelectedItem;

                if (selected == null || selected is not LineWrapper line) 
                    return;

                double xTranslate = Convert.ToDouble(TranslateXTextBox.Text);
                double yTranslate = Convert.ToDouble(TranslateYTextBox.Text);
                double zTranslate = Convert.ToDouble(TranslateZTextBox.Text);

                double angle = Convert.ToDouble(RotateAngleTextBox.Text);

                double xScale = Convert.ToDouble(ScaleXTextBox.Text);
                double yScale = Convert.ToDouble(ScaleYTextBox.Text);
                double zScale = Convert.ToDouble(ScaleZTextBox.Text);

                var transformGroup = new Transform3DGroup();

                transformGroup.Children.Add(new TranslateTransform3D(xTranslate, yTranslate, zTranslate));
                transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angle)));
                transformGroup.Children.Add(new ScaleTransform3D(xScale, yScale, zScale));

                line.Transform = transformGroup;*/
            }
            catch (FormatException)
            {
                MessageBox.Show("Пожалуйста, введите корректные координаты.",
                    "Ошибка ввода!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании 3D фигуры: {ex.Message}",
                    "Неизвестная ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFigures = FigureList.SelectedItems;

            foreach (var figure in selectedFigures)
            {
                if (figure == null || figure is not LineWrapper line)
                    continue;

                line.Transform = Transform3D.Identity;
            }
        }

        private void BakeTransform(LineWrapper model, ModelVisual3D parentGroup)
        {
            var combined = new Transform3DGroup();

            if (model.Transform != null && model.Transform != Transform3D.Identity)
                combined.Children.Add(model.Transform);

            if (parentGroup.Transform != null && parentGroup.Transform != Transform3D.Identity)
                combined.Children.Add(parentGroup.Transform);

            model.Transform = combined;

            parentGroup.Children.Remove(model);
            ViewPoint.Children.Add(model);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Ger Files (*.ger)|*.ger|All Files (*.*)|*.*",
                DefaultExt = ".ger"
            };

            if (saveFileDialog.ShowDialog() == true)
            { 

            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}