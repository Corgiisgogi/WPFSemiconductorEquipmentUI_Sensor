using System.Windows;
using WPFSemiconductorEquipmentUI_Sensor.ViewModels;

namespace WPFSemiconductorEquipmentUI_Sensor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
