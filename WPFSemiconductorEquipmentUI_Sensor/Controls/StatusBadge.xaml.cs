using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFSemiconductorEquipmentUI_Sensor.Controls
{
    public partial class StatusBadge : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(StatusBadge), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ToneProperty =
            DependencyProperty.Register("Tone", typeof(string), typeof(StatusBadge), new PropertyMetadata("Neutral", OnToneChanged));

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(StatusBadge), new PropertyMetadata(TextAlignment.Center));

        public StatusBadge()
        {
            InitializeComponent();
            ApplyTone();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Tone
        {
            get { return (string)GetValue(ToneProperty); }
            set { SetValue(ToneProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        private static void OnToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StatusBadge)d).ApplyTone();
        }

        private void ApplyTone()
        {
            if (BadgeBorder == null || BadgeTextBox == null)
            {
                return;
            }

            switch ((Tone ?? string.Empty).ToLowerInvariant())
            {
                case "normal":
                case "ok":
                    SetBrushes("#E7F5EF", "#16855C");
                    break;
                case "warning":
                case "warn":
                    SetBrushes("#FFF4D8", "#B7791F");
                    break;
                case "danger":
                case "risk":
                    SetBrushes("#FDECEC", "#B42318");
                    break;
                case "blue":
                case "info":
                    SetBrushes("#E7F0F7", "#1F5E8C");
                    break;
                case "disabled":
                    SetBrushes("#ECEFF3", "#8A93A0");
                    break;
                default:
                    SetBrushes("#F0F2F5", "#4E5968");
                    break;
            }
        }

        private void SetBrushes(string background, string foreground)
        {
            BadgeBorder.Background = (Brush)new BrushConverter().ConvertFromString(background);
            BadgeTextBox.Foreground = (Brush)new BrushConverter().ConvertFromString(foreground);
        }
    }
}
