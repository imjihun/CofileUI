using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CofileUI.Classes
{

	public class RelayCommand : ICommand
	{
		#region Fields

		readonly Action<object> execute;
		readonly Predicate<object> canExecute;

		#endregion // Fields

		#region Constructors

		public RelayCommand(Action<object> _execute)
			: this(_execute, null)
		{
		}

		public RelayCommand(Action<object> _execute, Predicate<object> _canExecute)
		{
			if(_execute == null)
				throw new ArgumentNullException("execute");

			execute = _execute;
			canExecute = _canExecute;
		}
		#endregion // Constructors

		#region ICommand Members

		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public void Execute(object parameter)
		{
			execute(parameter);
		}

		#endregion // ICommand Members
	}

}
