using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JPG_Viewer.ViewModel
{
    public class CustomCommand<T> : ICommand
    {
        Action<T> action;
        public event EventHandler CanExecuteChanged;

        public CustomCommand(Action<T> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action((T)parameter);
        }
    }
}
