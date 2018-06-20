using Counterpoint.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.ViewModels
{
    class MainViewModel : ObservableObject
    {

        public static RelayCommand ChooseFileCommand { get; set; }
        public static RelayCommand OpenFileCommand { get; set; }
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
        private ObservableCollection<string> counterpointSpecies = new ObservableCollection<string>();
        public ObservableCollection<string> CounterpointSpecies
        {
            get { return counterpointSpecies; }
            set
            {
                counterpointSpecies = value;
                RaisePropertyChanged("CounterpointSpecies");
            }
        }

        private string selectedCounterpointSpecie;
        public string SelectedCounterpointSpecie
        {
            get {              
                return selectedCounterpointSpecie; }
            set
            {
                selectedCounterpointSpecie = value;
                RaisePropertyChanged("SelectedCounterpointSpecie");
            }
        }

        private string defaultPath;      

        public MainViewModel()
        {
            foreach(CounterpointSpecie specie in Enum.GetValues(typeof(CounterpointSpecie)))
            {
                counterpointSpecies.Add(specie.ToUserFriendlyString());
            }

            ChooseFileCommand = new RelayCommand(ExecuteChooseFileDialog);
            OpenFileCommand = new RelayCommand(ExecuteOpenFile, () => String.IsNullOrEmpty(selectedPath) == false && selectedCounterpointSpecie != null );
        }

        public MainViewModel(string defaultPath)
            : this()
        {
            this.defaultPath = defaultPath;      
        }

        private void ExecuteChooseFileDialog()
        {
            var dialog = new OpenFileDialog() { InitialDirectory = defaultPath };
            dialog.Filter = "Midi files(.mid)| *.mid";
            dialog.ShowDialog();
            SelectedPath = dialog.FileName;
        }
        private void ExecuteOpenFile()
        {           
            Models.CounterpointAnalysis counterpointAnalysis = new Models.CounterpointAnalysis(selectedPath, selectedCounterpointSpecie.FromUserFriendlyString());

        }
    }




}
