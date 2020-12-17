using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ActiveMQExplorer.ViewModels
{
    internal class DelegateCommand : ICommand
    {
        private readonly Action<object> _targetMethod;

        public DelegateCommand(Action<object> actionMethod)
        {
            this._targetMethod = actionMethod;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Task.Factory.StartNew(() =>  _targetMethod.Invoke(parameter));
        }
    }
}