using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TextReader.ViewModel
{
    class WorkspaceViewModel : ViewModelBase
    {
        public EventHandler RequestClose;

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => OnRequestClose(param));
                }
                return _closeCommand;
            }
        }

        private void OnRequestClose(object sender)
        {
            if (RequestClose != null)
            {
                RequestClose.Invoke(sender, EventArgs.Empty);
            }
        }
    }
}
