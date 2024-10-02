using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace WpfUserControlTest
{
    abstract public class BaseModel : INotifyPropertyChanged
    {
        public ViewModels.ViewModelBase viewModel;

        public void OnSendCommandHandler(string commandName) =>
            GetType().GetMethod(commandName)?.Invoke(this, null);
        

        #region INotifyPropertyChanged code

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            viewModel.OnPropertyChanged(prop);
        }

        #endregion
    }
}
