using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace Ushahidi
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            app = Application.Current as App;
        }
        App app;

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            switch (app.GlobalSettings.ReportLimit)
            {
                case 20:
                    ReportLimitListPicker.SelectedIndex = 0;
                    break;
                case 50:
                    ReportLimitListPicker.SelectedIndex = 1;
                    break;
                case 100:
                    ReportLimitListPicker.SelectedIndex = 2;
                    break;
            }

            switch (app.GlobalSettings.Distance)
            {
                case 100:
                    DistanceLimitListPicker.SelectedIndex = 0;
                    break;
                case 1000:
                    DistanceLimitListPicker.SelectedIndex = 1;
                    break;
                case 5000:
                    DistanceLimitListPicker.SelectedIndex = 2;
                    break;
            }


        }

        private void ReportLimitListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportLimitListPicker!= null)
            {
                switch (ReportLimitListPicker.SelectedIndex)
                {
                    case 0:
                        app.GlobalSettings.ReportLimit = 20;
                        break;
                    case 1:
                        app.GlobalSettings.ReportLimit = 50;
                        break;
                    case 2:
                        app.GlobalSettings.ReportLimit = 100;
                        break;
                }
                app.SaveSettings();
              
            }

        }

        private void DistanceLimitListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DistanceLimitListPicker!= null)
            {
                switch (DistanceLimitListPicker.SelectedIndex)
                {
                    case 0:
                        app.GlobalSettings.Distance = 100;
                        break;
                    case 1:
                        app.GlobalSettings.Distance = 1000;
                        break;
                    case 2:
                        app.GlobalSettings.Distance = 5000;
                        break;
                }
                app.SaveSettings();
            }
        }



    }
}
