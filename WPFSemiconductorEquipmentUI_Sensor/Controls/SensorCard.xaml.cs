using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFSemiconductorEquipmentUI_Sensor.Controls
{
    public partial class SensorCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SensorCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(SensorCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(SensorCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty RangeTextProperty = DependencyProperty.Register("RangeText", typeof(string), typeof(SensorCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty BadgeTextProperty = DependencyProperty.Register("BadgeText", typeof(string), typeof(SensorCard), new PropertyMetadata("NORMAL"));
        public static readonly DependencyProperty ToneProperty = DependencyProperty.Register("Tone", typeof(string), typeof(SensorCard), new PropertyMetadata("Normal", OnToneChanged));
        public static readonly DependencyProperty IndicatorWidthProperty = DependencyProperty.Register("IndicatorWidth", typeof(double), typeof(SensorCard), new PropertyMetadata(145d));
        public static readonly DependencyProperty IndicatorBrushProperty = DependencyProperty.Register("IndicatorBrush", typeof(Brush), typeof(SensorCard), new PropertyMetadata(ToBrush("#16855C")));

        public SensorCard()
        {
            InitializeComponent();
        }

        public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }
        public string Value { get { return (string)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }
        public string Unit { get { return (string)GetValue(UnitProperty); } set { SetValue(UnitProperty, value); } }
        public string RangeText { get { return (string)GetValue(RangeTextProperty); } set { SetValue(RangeTextProperty, value); } }
        public string BadgeText { get { return (string)GetValue(BadgeTextProperty); } set { SetValue(BadgeTextProperty, value); } }
        public string Tone { get { return (string)GetValue(ToneProperty); } set { SetValue(ToneProperty, value); } }
        public double IndicatorWidth { get { return (double)GetValue(IndicatorWidthProperty); } set { SetValue(IndicatorWidthProperty, value); } }
        public Brush IndicatorBrush { get { return (Brush)GetValue(IndicatorBrushProperty); } set { SetValue(IndicatorBrushProperty, value); } }

        private static void OnToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (SensorCard)d;
            var tone = (card.Tone ?? string.Empty).ToLowerInvariant();
            card.IndicatorBrush = tone == "warning" || tone == "warn" ? ToBrush("#B7791F")
                : tone == "danger" || tone == "risk" ? ToBrush("#B42318")
                : ToBrush("#16855C");
        }

        private static Brush ToBrush(string color)
        {
            return (Brush)new BrushConverter().ConvertFromString(color);
        }
    }
}
