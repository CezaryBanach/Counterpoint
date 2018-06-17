using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.ViewModels
{
    class OpenFileDialogViewModel : ObservableObject
    {
        public static RelayCommand OpenCommand {get; set;}
        private string selectedPath;
        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                selectedPath = value;
                RaisePropertyChanged("SelectedPath");
            }
        }

        private string defaultPath;

        public OpenFileDialogViewModel()
        {
            OpenCommand = new RelayCommand(ExecuteOpenFileDialog);
        }

        public OpenFileDialogViewModel(string defaultPath)
            :this()
        {
            this.defaultPath = defaultPath;            
        }

        private void ExecuteOpenFileDialog()
        {
            var dialog = new OpenFileDialog() { InitialDirectory = defaultPath };
            dialog.Filter = "Midi files(.midi)| *.midi";
            dialog.ShowDialog();
            SelectedPath = dialog.FileName;
        }

                      
    }
}
