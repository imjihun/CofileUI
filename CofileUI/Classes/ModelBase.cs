using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CofileUI
{
	public class ModelBase : INotifyPropertyChanged
	{
		public void RaisePropertyChanged(string prop)
		{
			if(PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
