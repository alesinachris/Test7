using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfUserControlTest.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Commands

        public delegate void CommandHandler(string commandName);
        public event CommandHandler OnSendCommand;

        public RelayCommand UniversalCommand
        {
            get
            {
                RelayCommand rc = new RelayCommand(obj =>
                {
                    string commandName = obj.ToString();
                    OnSendCommand(commandName);
                });
                return rc;
            }
        }

        #endregion

        #region INotifyPropertyChanged code

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #endregion
    }
}
