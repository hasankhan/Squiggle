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
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Squiggle.Utilities;
using Squiggle.Translate.Properties;

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

        string[] stopWords = new[] { "Squiggle", "IP", "Port" };
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
            LoadTranslationFile();
            language.ItemsSource = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                              .Where(c => !c.IsNeutralCulture)
                                              .OrderBy(c=>c.EnglishName)
                                              .ToList();

            inputText.Text = String.Join("\r\n", maps.Select(m => m.Line));
        }

        void LoadTranslationFile()
        {
            var lines = (from node in XDocument.Load("Translation.xaml").Descendants()
                         where IsStringNode(node)
                         select node.Value).ToList();

            foreach (string line in lines)
            {
                var cleanLine = line.Replace("_", "");
                var map = new LineMap();
                foreach (string word in SplitWords(line))
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
        }

        static string[] SplitWords(string line)
        {
            return line.Split(' ', '"', ':', '-');
        }

        static bool IsStringNode(XElement node)
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
            string languageName = SelectedCulture.EnglishName.Split(' ').FirstOrDefault();
            string targetLanguage = SelectedCulture.TwoLetterISOLanguageName;
            string text = inputText.Text;
            string apiKey = key.Text;
            string direciton = Direction.Content.ToString();
            layoutRoot.IsEnabled = false;

            Task.Factory.StartNew(() =>
            {
                string output = TranslateText(targetLanguage, text, apiKey);
                string file = GenerateTranslationFile(languageName, direciton, output);
                return new
                {
                    Output = output,
                    File = file
                };
            }).ContinueWith(task =>
            {
                layoutRoot.IsEnabled = true;
                try
                {
                    outputText.Text = task.Result.Output;
                    MessageBox.Show("Translation file generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shell.ShowInFolder(task.Result.File);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not translate due to exception: " + ex.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        string GenerateTranslationFile(string languageName, string direction, string translatedText)
        {
            var mapQueue = new Queue<LineMap>(maps);
            var lineQueue = new Queue<string>(translatedText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            var translationDoc = XDocument.Load("Translation.xaml");
            foreach (var node in translationDoc.Descendants())
                if (node.Name.LocalName.Equals("FlowDirection"))
                    node.Value = direction;
                else if (IsStringNode(node))
                {
                    LineMap map = mapQueue.Dequeue();
                    node.Value = lineQueue.Dequeue();
                    foreach (string word in map.Stopped)
                        node.Value = ReplaceFirstOccurrance(node.Value, stopWordSymbol, word);
                }

            string fileName = "Translation." + languageName + ".xaml";
            translationDoc.Save(fileName);
            return fileName;
        }

        string TranslateText(string targetLanguage, string text, string apiKey)
        {
            var translations = new ConcurrentDictionary<int,string>();
            var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            Parallel.For(0, lines.Length, i =>
            {
                var client = new GoogleTranslator(apiKey);
                string line = lines[i];
                string translation = client.Translate("en", targetLanguage, line);
                translations.TryAdd(i, translation);
            });

            string[] output = translations.OrderBy(t => t.Key).Select(t => t.Value).ToArray();
            string result = String.Join("\r\n", output);
            return result;
        }

        CultureInfo SelectedCulture
        {
            get
            {
                return ((CultureInfo)language.SelectedItem);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
