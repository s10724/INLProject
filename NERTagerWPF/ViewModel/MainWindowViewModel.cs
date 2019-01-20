using Caliburn.Micro;
using NERCore;
using System.Windows.Documents;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System;
using PropertyChanged;
using System.Threading.Tasks;

namespace NERTagerWPF
{
    public class MainWindowViewModel : Screen
    {
        public BindableCollection<SentenceViewModel> ResultItems { get; set; }
        public NERTagger Tagger { get; set; }
        public string Text { get; set; }
        public string ModelPath
        {
            get
            {
                return Properties.Settings.Default.CRFModel;
            }
            set
            {
                Properties.Settings.Default.CRFModel = value;
                NotifyOfPropertyChange(nameof(ModelPath));
            }
        }

        [AlsoNotifyFor(nameof(IsNotTagProcess))]
        public bool IsTagProcess { get; set; }
        public bool IsNotTagProcess
        {
            get
            {
                return !IsTagProcess;
            }
        }
        public MainWindowViewModel()
        {
          
            Tagger = new NERTagger();
         
        }

        public void OnLoad()
        {
            ResultItems = new BindableCollection<SentenceViewModel>();
            Text = "Pochodzący z Krakowa pisarz Jan Nowak pracujący w gazecie Codziennej uszęszcza na studia do PJATK. Jan Kowalski mieszka w Warszawie i pracuje w szkole podstawowej imienia Juliusza Słowackiego.";
            Tagger.StartServer(@"concraft-pl.exe", @"concraft_model.gz");
        }

        public async void TagText()
        {
            IsTagProcess = true;
            List<TaggerResult> result = new List<TaggerResult>() ;
            await Task.Run(() =>
            {
                result = Tagger.TagText(Text, ModelPath);
            });

            foreach (var item in result)
            {
                ResultItems.Add(new SentenceViewModel(item.Sentence, item.Tag));
            }
            IsTagProcess = false;
        }

        public void OnClose()
        {
            Properties.Settings.Default.Save();
           Tagger?.StopServer();
        }
    }
}