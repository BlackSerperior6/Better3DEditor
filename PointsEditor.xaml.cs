using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace _3DRedactor
{
    /// <summary>
    /// Логика взаимодействия для PointsEditor.xaml
    /// </summary>
    public partial class PointsEditor : Window
    {
        public PointsEditor() => InitializeComponent();

        public List<Point3D> points;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                double x = Convert.ToDouble(XBox.Text);
                double y = Convert.ToDouble(YBox.Text);
                double z = Convert.ToDouble(ZBox.Text);

                PointsList.Items.Add(new Point3D(x, y, z));
            }
            catch(FormatException) 
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PointsList.SelectedIndex == -1)
                return;

            PointsList.Items.RemoveAt(PointsList.SelectedIndex);
        }

        private void Ок_Click(object sender, RoutedEventArgs e)
        {
            points = PointsList.Items.Cast<Point3D>().ToList();
            DialogResult = true;

            Close();
        }
    }
}
