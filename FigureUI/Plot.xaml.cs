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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mikity.visualize
{
    /// <summary>
    /// Plot.xaml の相互作用ロジック
    /// </summary>
    public partial class Plot : UserControl
    {
        Func<double, double> converterX = (v) => { return 100 - v * 100; };
        Func<double, double> converterY = (v) => { return -v * 100 + 100; };

        public void move(double val)
        {
            line.X1 = converterX(val);
            line.X2 = converterX(val);
        }
        public void renew(Func<double, double> d)
        {
            for (int i = 0; i < 80; i++)
            {
                double x1 = i / 40d - 1.0;
                double x2 = (i + 1) / 40d - 1.0;

                lines[i].X1 = converterX(x1);
                lines[i].X2 = converterX(x2);
                lines[i].Y1 = converterY(d(x1));
                lines[i].Y2 = converterY(d(x2));
            }
        }
        Line[] lines = new Line[80];
        public Plot()
        {
            InitializeComponent();
            for (int i = 0; i < 80; i++)
            {
                lines[i] = new Line();
                lines[i].StrokeThickness = 2;
                lines[i].Stroke = Brushes.Green;
                canvas.Children.Add(lines[i]);
            }
            this.renew((v) => { if (v > 0)return v; else return 0; });
        }
    }
}
