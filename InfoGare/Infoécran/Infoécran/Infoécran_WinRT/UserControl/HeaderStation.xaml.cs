using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Infoécran.AppStates;

namespace Infoécran.UserControl
{
    public sealed partial class HeaderStation : global::Windows.UI.Xaml.Controls.UserControl
    {
        private Frame _frame;
        public HeaderStation()
        {
            this.InitializeComponent();
        }

        internal void Initialize(string stationName, bool isSNCF, Frame frame)
        {
            _frame = frame;
            Sncf.Visibility = isSNCF ? Visibility.Visible : Visibility.Collapsed; 
            StationName.Text = stationName; 
        }

        private void backButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _frame.Navigate(typeof (FirstLandingState));
        }
    }
}
