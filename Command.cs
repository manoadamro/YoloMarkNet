using System;
using System.Windows.Input;

namespace YoloMarkNet
{
    public class Command : ICommand
    {
        private readonly Action<object> _action;

        public Command(Action<object> action) => _action = action;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}