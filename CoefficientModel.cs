using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfChallenge
{
    public class CoefficientModel
    {
        public CoefficientModel(OxyColor color) 
        {
            Color = color.ToColor();
        }
        public Color Color { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public string Text => $"a: {A:0.##}, b: {B:0.##}";
    }
}
