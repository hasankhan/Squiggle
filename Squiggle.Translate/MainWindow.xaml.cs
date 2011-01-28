using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Squiggle.Translate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string[] stopWords = new[] { "Squiggle", "Idle", "IP", "Port" };
        const string stopWordSymbol = "|";
        
        class LineMap
        {
            public List<string> Stopped { get; private set; }
            public string Line { get; set; }

            public LineMap()
            {
                Stopped = new List<string>();
            }
        }

        List<LineMap> maps = new List<LineMap>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var lines = (from node in XDocument.Load("Translation.xaml").Descendants()
                         where IsStringNode(node)
                         select node.Value).ToList();

            foreach (string line in lines)
            {
                var cleanLine = line.Replace("_", "");
                var map = new LineMap();
                foreach (string word in line.Split(' ', '"', ':', '-'))
                {
                    if (stopWords.Any(s => s.Equals(word, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        map.Stopped.Add(word);
                        cleanLine = ReplaceFirstOccurrance(cleanLine, word, stopWordSymbol);
                    }                    
                }
                map.Line = cleanLine.ToString();
                maps.Add(map);
            }

            inputText.Text = String.Join("\r\n", maps.Select(m => m.Line));
        }

        private static bool IsStringNode(XElement node)
        {
            return node.Name.LocalName.Equals(typeof(String).Name);
        }

        static string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            int loc = original.IndexOf(oldValue);
            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mapQueue = new Queue<LineMap>(maps);
            var lineQueue = new Queue<string>(outputText.Text.Split(new[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries));
            
            var translationDoc = XDocument.Load("Translation.xaml");
            foreach (var node in translationDoc.Descendants())
                if (node.Name.LocalName.Equals("FlowDirection"))
                    node.Value = Direction.SelectedItem.ToString();
                else if (IsStringNode(node))
                {
                   LineMap map = mapQueue.Dequeue();
                   node.Value = lineQueue.Dequeue();
                   foreach (string word in map.Stopped)
                       node.Value = ReplaceFirstOccurrance(node.Value, stopWordSymbol, word);
                }

            translationDoc.Save("Translation." + language.Text + ".xaml");
            MessageBox.Show("Translation file generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
