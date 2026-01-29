using ROV_GUI_Control.ViewModels;
namespace ROV_GUI_Control
{
    public class WindowService : IWindowService
    {
        public SettingsWindowViewModel settingsWVM;
        public Settings settings;
        public (ModalResult Result, string, int, int, int, int, int, VariableGroup) ShowWindow(string ip, int cam1_port, int cam2_port, int cam3_port, int joy_port, int mav_port, VariableGroup pid)
        {
            settingsWVM = new SettingsWindowViewModel
            {
                IP = ip,
                Cam1_Port = cam1_port,
                Cam2_Port = cam2_port,
                Cam3_Port = cam3_port,
                Joy_Port = joy_port,
                Mav_Port = mav_port,
                PID = pid
            };
            settings = new Settings
            {
                DataContext = settingsWVM
            };
            ModalResult result = ModalResult.Cancel;
            settingsWVM.CloseAction = modalResult =>
            {
                result = modalResult;
                settings.DialogResult = true;
            };
            bool? closed = settings.ShowDialog();
            return (result, settingsWVM.IP, settingsWVM.Cam1_Port, settingsWVM.Cam2_Port, settingsWVM.Cam3_Port, settingsWVM.Joy_Port, settingsWVM.Mav_Port, settingsWVM.PID);
        }
        public void setip(string ip)
        {
            settingsWVM.IP = ip;
        }
    }
}
