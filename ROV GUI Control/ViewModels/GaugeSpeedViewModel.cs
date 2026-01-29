using System.ComponentModel;

namespace ROV_GUI_Control.ViewModels
{
    public class GaugeSpeedViewModel : INotifyPropertyChanged
    {
        private double _value;
        public double Speed_Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Speed_Value));
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