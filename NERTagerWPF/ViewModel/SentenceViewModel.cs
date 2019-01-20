using Caliburn.Micro;
using System.Collections.Generic;
using System.Windows.Media;

namespace NERTagerWPF
{
    public class SentenceViewModel : PropertyChangedBase
    {    
        static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>()
        {
           {"persName", Colors.DarkRed },
           {"placeName", Colors.DarkBlue },
           {"geogName", Colors.DarkGreen },
           {"orgName", Colors.DarkGoldenrod },
           {"", Colors.Black }
        };
        public string Text { get; set; }
        public string Tag { get; set; }
        public SolidColorBrush Foreground { get; set; }


        public SentenceViewModel(string Text,string Tag)
        {
            this.Text = Text;
            this.Tag = Tag;
            if(colors.ContainsKey(Tag))
            {
                Foreground = new SolidColorBrush(colors[Tag]);
            }
            else
            {
                Foreground = new SolidColorBrush(Colors.Gray);
            }

        }
    }
}