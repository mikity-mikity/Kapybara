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
using System.Windows.Shapes;

namespace mikity.visualize
{
    /// <summary>
    /// fullScreen.xaml の相互作用ロジック
    /// </summary>
    public partial class fullScreen : Window
    {
        public void activate()
        {
            this.view.Opacity = 1.0;
        }
        public void deactivate()
        {
            this.view.Opacity = 0;
        }
        public double getDt()
        {
            return dt.getDt();
        }
        public fullScreen()
        {
            InitializeComponent();
            drift0();
            setDbgText("Test");
        }
        public void addNorm(double v)
        {
            this.graph2.add(v);
        }
        public void renewPlot(Func<double,double> d)
        {
            graph.renew(d);
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }
        public void onFix()
        {
            _fix2.Content = "ON";
            _fix2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offFix()
        {
            _fix2.Content = "OFF";
            _fix2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void onRF()
        {
            _rf2.Content = "ON";
            _rf2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void offRF()
        {
            _rf2.Content = "OFF";
            _rf2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void material(string text)
        {
            _material2.Content = text;
        }
        public void onGo()
        {
            TT.Child = (UIElement)this.FindResource("D2");
        }
        public void resetGo()
        {
            TT.Child = (UIElement)this.FindResource("D1");
        }
        public void pauseGo()
        {

            TT.Child = (UIElement)this.FindResource("D3");

        }
        public void drift0()
        {
            _drift2.Content = "Off";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E2");
        }
        public void drift1()
        {
            _drift2.Content = "Proposed";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void drift2()
        {
            _drift2.Content = "Kinetic damping";
            _drift2.Effect = (System.Windows.Media.Effects.Effect)this.FindResource("E3");
        }
        public void move(double val)
        {
            graph.move(val);
        }
        public void setDbgText(string text)
        {
            this.dbg.Text = text;
        }
        public Action<string> _selectMaterial;
        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if(_selectMaterial!=null)
            _selectMaterial((string)((Label)((ComboBoxItem)sender).Content).Content);
        }
        public void initMaterial()
        {
            _selectMaterial((string)((Label)((ComboBoxItem)MaterialBox.SelectedItem).Content).Content);
        }
        public void dropMaterial()
        {
            MaterialBox.Focus();
        }
    }
}
