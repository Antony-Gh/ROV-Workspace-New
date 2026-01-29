using ROV_GUI_Control.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ROV_GUI_Control
{
    public interface IWindowService
    {
        (ModalResult Result, string, int, int, int, int, int, VariableGroup) ShowWindow(string ip, int cam1_port, int cam2_port, int cam3_port, int joy_port, int mav_port, VariableGroup pid);
        public void setip(string ip);
    }
}
