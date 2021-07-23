using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TEiNRandomizer
{
    /// <summary>
    /// Interaction logic for TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    {
        //public string PromptText { get; set; }
        //public string ResultText { get; set; }

        public TextWindow(string promptText, string defaultResultText = "")
        {
            InitializeComponent();
            PromptTextBox.Text = promptText;
            ResultTextBox.Text = defaultResultText;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Result
        {
            get { return ResultTextBox.Text; }
        }

    }
}
