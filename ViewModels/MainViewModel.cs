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
        private ObservableCollection<string> counterpointComments;
        public ObservableCollection<string> CounterpointComments
        {
            get { return counterpointComments; }
            set
            {
                counterpointComments = value;
                RaisePropertyChanged("CounterpointComments");
            }
        }
        private ObservableCollection<string> modes = new ObservableCollection<string>();
        public ObservableCollection<string> Modes
        {
            get { return modes; }
            set
            {
                modes = value;
                RaisePropertyChanged("Modes");
            }
        }
        private string selectedMode;
        public string SelectedMode
        {
            get { return selectedMode; }
            set
            {
                selectedMode = value;
                RaisePropertyChanged("SelectedMode");
            }
        }
        private ObservableCollection<string> tonics = new ObservableCollection<string>();
        public ObservableCollection<string> Tonics
        {
            get { return tonics; }
            set
            {
                tonics = value;
                RaisePropertyChanged("Tonics");
            }

        }
        private string selectedTonic;
        public string SelectedTonic
        {
            get { return selectedTonic; }
            set
            {
                selectedTonic = value;
                RaisePropertyChanged("SelectedTonic");
            }
        }
        private ObservableCollection<int> voiceIds = new ObservableCollection<int> {1, 2, 3, 4};
        public ObservableCollection<int> VoiceIds
        {
            get { return voiceIds; }
            set
            {
                voiceIds = value;
                RaisePropertyChanged("VoiceIds");
            }
        }
        private string selectedVoiceId;
        public string SelectedVoiceId
        {
            get { return selectedVoiceId; }
            set
            {
                selectedVoiceId = value;
                RaisePropertyChanged("SelectedVoiceId");
            }
        }

        private string defaultPath;      

        public MainViewModel()
        {
            foreach(CounterpointSpecie specie in Enum.GetValues(typeof(CounterpointSpecie)))
            {
                CounterpointSpecies.Add(specie.ToUserFriendlyString());
            }
            foreach(Mode mode in Enum.GetValues(typeof(Mode)))
            {
                Modes.Add(mode.ToString());
            }
            foreach(Tonic tonic in Enum.GetValues(typeof(Tonic)))
            {
                Tonics.Add(tonic.ToString());
            }

            ChooseFileCommand = new RelayCommand(ExecuteChooseFileDialog);
            OpenFileCommand = new RelayCommand(ExecuteOpenFile,
                () => String.IsNullOrEmpty(selectedPath) == false && SelectedCounterpointSpecie != null 
                && SelectedMode != null
                && SelectedTonic != null);
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
            Mode mode;
            Tonic tonic;
        
            Enum.TryParse(SelectedTonic, out tonic);
            Enum.TryParse(SelectedMode, out mode);

            int cantusFirmusId = Int32.Parse(SelectedVoiceId) - 1;

            Models.CounterpointAnalysis counterpointAnalysis = new Models.CounterpointAnalysis(selectedPath,
                                                                                               selectedCounterpointSpecie.FromUserFriendlyString(),
                                                                                               mode,
                                                                                               tonic,
                                                                                               cantusFirmusId
                                                                                               );
            CounterpointComments = new ObservableCollection<string>(counterpointAnalysis.CounterpointCommentsLog);
            
            
        }
    }




}
