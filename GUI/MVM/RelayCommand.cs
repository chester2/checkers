using System;
using System.Windows.Input;

namespace GUI
{
    public class RelayCommand : ICommand
    {
        private Action<object> action;
        public RelayCommand(Action<object> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
            => this.action(parameter);
    }
}
