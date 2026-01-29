using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ROV_GUI_Control.ViewModels;
namespace ROV_GUI_Control
{
    public class SettingsWindowViewModel : ObservableObject
    {
        public VariableGroup PID { get; set; } = new VariableGroup();

        private string _ip;
        public string IP
        {
            get => _ip;
            set
            {
                SetProperty(ref _ip, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }

        private int _camport1;
        public int Cam1_Port
        {
            get => _camport1;
            set
            {
                SetProperty(ref _camport1, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }
       
        private int _camport2;
        public int Cam2_Port
        {
            get => _camport2;
            set
            {
                SetProperty(ref _camport2, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }
        
        private int _camport3;
        public int Cam3_Port
        {
            get => _camport3;
            set
            {
                SetProperty(ref _camport3, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }
       
        private int _joyport;
        public int Joy_Port
        {
            get => _joyport;
            set
            {
                SetProperty(ref _joyport, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }
        private int _mavport;
        public int Mav_Port
        {
            get => _mavport;
            set
            {
                SetProperty(ref _mavport, value);
                OkCommand.NotifyCanExecuteChanged();

            }
        }

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action<ModalResult> CloseAction { get; set; }

        public SettingsWindowViewModel()
        {
            OkCommand = new RelayCommand(
                () => CloseAction?.Invoke(ModalResult.Ok),
                () => !string.IsNullOrWhiteSpace(IP)
            );
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke(ModalResult.Cancel));
            /*PID.Add("SKP", 0f);
            PID.Add("SKI", 0f);
            PID.Add("SKD", 0f);
            PID.Add("SMAX", 0f);
            PID.Add("SMIN", 0f);
            PID.Add("SANTI", 0f);
            PID.Add("RKP", 0f);
            PID.Add("RKI", 0f);
            PID.Add("RKD", 0f);
            PID.Add("RMAX", 0f);
            PID.Add("RMIN", 0f);
            PID.Add("RANTI", 0f);
            PID.Add("PKP", 0f);
            PID.Add("PKI", 0f);
            PID.Add("PKD", 0f);
            PID.Add("PMAX", 0f);
            PID.Add("PMIN", 0f);
            PID.Add("PANTI", 0f);*/
        }
    }
}
