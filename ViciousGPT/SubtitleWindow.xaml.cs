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

namespace ViciousGPT
{
    public partial class SubtitleWindow : Window
    {
        public SubtitleWindow()
        {
            InitializeComponent();
        }

        public void Show(string text)
        {
            subtitleText.Text = text;
            // TODO: Move to the correct window and adjust the width
            base.Show();
            Topmost = true;
        }

        new public void Hide()
        {
            base.Hide();
            Topmost = false;
        }
    }
}
