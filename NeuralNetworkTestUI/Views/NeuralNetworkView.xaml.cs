using System.Windows.Controls;
using NeuralNetworkTestUI.Controls;

namespace NeuralNetworkTestUI.Views
{
    /// <summary>
    /// Interaction logic for NeuralNetworkView.xaml
    /// </summary>
    public partial class NeuralNetworkView : UserControl
    {
        public NeuralNetworkView()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            ((GLControl) gl).Refresh();
        }
    }
}
