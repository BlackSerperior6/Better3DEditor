using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.IO;
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
            counter = 0;
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
            var path = PathLabel.Content;

            if (!File.Exists((string?)path) ||
                    System.IO.Path.GetExtension((string?)path).ToLower() != ".ger")
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Ger Files (*.ger)|*.ger|All Files (*.*)|*.*",
                    DefaultExt = ".ger"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;
                
                path = saveFileDialog.FileName;
            }

            Save((string) path);
            PathLabel.Content = path;
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Ger Files (*.ger)|*.ger|All Files (*.*)|*.*",
                DefaultExt = ".ger"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Save(saveFileDialog.FileName);
                PathLabel.Content = saveFileDialog.FileName;
            }
        }

        private void Save(string path)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(counter.ToString());
            sb.AppendLine($"{TranslateXTextBox.Text}/{TranslateYTextBox.Text}/{TranslateZTextBox.Text}/" +
                $"{RotateAngleTextBox.Text}/{ScaleXTextBox.Text}/{ScaleYTextBox.Text}/{ScaleZTextBox.Text}");

            foreach (var figure in FigureList.Items)
            {
                if (figure == null || figure is not LineWrapper line)
                    continue;

                string lineToString = line.Name;

                for (int i = 0; i < line.Points.Count; i++)
                    lineToString += "/" + $"{line.Points[i].X};{line.Points[i].Y};{line.Points[i].Z}";

                var matrixLine = line.Transform.Value.ToString();

                lineToString += "/" + matrixLine;

                Console.WriteLine(matrixLine);

                sb.AppendLine(lineToString);
            }

            File.WriteAllText(path, sb.ToString());
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Ger Files (*.ger)|*.ger|All Files (*.*)|*.*",
                DefaultExt = ".ger"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (!File.Exists(openFileDialog.FileName) || 
                    System.IO.Path.GetExtension(openFileDialog.FileName).ToLower() != ".ger")
                {
                    MessageBox.Show($"Ошибка! Файл неверного формата или не существует!");
                    return;
                }

                NewButton_Click(sender, e);

                string[] content = File.ReadAllLines(openFileDialog.FileName);

                counter = int.Parse(content[0]);

                string[] firstLine = content[1].Split('/');

                TranslateXTextBox.Text = firstLine[0];
                TranslateYTextBox.Text = firstLine[1];
                TranslateZTextBox.Text = firstLine[2];
                RotateAngleTextBox.Text = firstLine[3];
                ScaleXTextBox.Text = firstLine[4];
                ScaleYTextBox.Text = firstLine[5];
                ScaleZTextBox.Text = firstLine[6];

                for (int i = 2; i < content.Length; i++) 
                {
                    string[] currentLine = content[i].Split('/');
                    string lineName = currentLine[0];

                    Point3DCollection points = new Point3DCollection();

                    for (int j = 1; j < currentLine.Length - 1; j++)
                    {
                        string[] pointCoord = currentLine[j].Split(';');
                        points.Add(new Point3D(double.Parse(pointCoord[0]), double.Parse(pointCoord[1]),
                            double.Parse(pointCoord[2])));
                        
                    } 

                    var lines = new LineWrapper(lineName)
                    {
                        Points = points,
                        Color = Colors.Blue,
                        Thickness = 5
                    };

                    ViewPoint.Children.Add(lines);
                    FigureList.Items.Add(lines);

                    string[] lastLine = currentLine[^1].Split(";");

                    if (lastLine.Length > 1)
                    {
                        Matrix3D tarnsformMatrix = new Matrix3D(double.Parse(lastLine[0]), double.Parse(lastLine[1]), double.Parse(lastLine[2]), double.Parse(lastLine[3]),
                        double.Parse(lastLine[4]), double.Parse(lastLine[5]), double.Parse(lastLine[6]), double.Parse(lastLine[7]), double.Parse(lastLine[8]), double.Parse(lastLine[9]), double.Parse(lastLine[10]),
                        double.Parse(lastLine[11]), double.Parse(lastLine[12]), double.Parse(lastLine[13]), double.Parse(lastLine[14]), double.Parse(lastLine[15]));

                        lines.Transform = new MatrixTransform3D(tarnsformMatrix);
                    }
                }

                PathLabel.Content = openFileDialog.FileName;
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = ViewPoint.Children.Count - 1; i >= 0; i--)
            {
                if (ViewPoint.Children[i] is ModelVisual3D visual && visual is not (GridLinesVisual3D or DefaultLights))
                    ViewPoint.Children.RemoveAt(i);
            }

            FigureList.Items.Clear();
            counter = 0;

            TranslateXTextBox.Text = "0";
            TranslateYTextBox.Text = "0";
            TranslateZTextBox.Text = "0";
            RotateAngleTextBox.Text = "0";
            ScaleXTextBox.Text = "1";
            ScaleYTextBox.Text = "1";
            ScaleZTextBox.Text = "1";

            PathLabel.Content = "";
        }
    }
}