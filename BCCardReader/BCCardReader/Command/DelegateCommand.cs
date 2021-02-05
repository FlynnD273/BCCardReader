using System;
using System.Windows.Input;

namespace BCCardReader.Command
{
    class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        protected readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public virtual bool CanExecute(object parameter) => _canExecute == null ||
            ((parameter is T t || _TryConvert(parameter, out t)) && _canExecute(t));

        public void Execute(object parameter) => _execute((T)Convert.ChangeType(parameter, typeof(T)));

        private bool _TryConvert(object parameter, out T t)
        {
            try
            {
                t = (T)Convert.ChangeType(parameter, typeof(T));
                return true;
            }
            catch (Exception e) when
            (e is InvalidCastException || e is FormatException || e is OverflowException)
            {
                t = default(T);
                return false;
            }
        }
    }

    class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute != null ? (Func<object, bool>)(_ => canExecute()) : null)
        { }

        public override bool CanExecute(object parameter) => _canExecute?.Invoke(null) ?? true;
    }
}
