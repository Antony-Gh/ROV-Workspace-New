using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ROV_GUI_Control.ViewModels
{
    public class VariableGroup : INotifyPropertyChanged
    {
        public ObservableCollection<SettingsVariables> Variables { get; }
       = new ObservableCollection<SettingsVariables>();

        private readonly Dictionary<string, SettingsVariables> _map
            = new Dictionary<string, SettingsVariables>();
        public SettingsVariables this[string name]
        {
            get => _map.ContainsKey(name) ? _map[name] : null;
        }

        public void Add(string name, float initialValue = 0f)
        {
            var variable = new SettingsVariables(name, initialValue);
            variable.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(name);
            };

            Variables.Add(variable);
            _map[name] = variable;
            OnPropertyChanged(name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
