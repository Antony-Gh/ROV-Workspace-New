using System.ComponentModel;

namespace ROV_GUI_Control.ViewModels
{
    public class GaugeDepthViewModel : INotifyPropertyChanged
    {
        private double _value;
        public double Depth_Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Depth_Value));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}