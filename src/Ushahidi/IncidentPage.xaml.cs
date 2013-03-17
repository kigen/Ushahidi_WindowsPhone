using Microsoft.Phone.Controls;
using System.Windows;
using Microsoft.Phone.Shell;
using System;
using System.Windows.Media.Imaging;

namespace Ushahidi
{
    public partial class IncidentPage : PhoneApplicationPage
    {
        App app;
        ApplicationBarIconButton mapViewButton;
        private bool isMap { get; set; }
        public IncidentPage()
        {
            InitializeComponent();
            app = Application.Current as App;
            mapViewButton = new ApplicationBarIconButton();
            mapViewButton.IconUri = new Uri("/icons/appbar.map.png", UriKind.Relative);
            mapViewButton.Text = "Map ";
            mapViewButton.Click += new EventHandler(mapViewButton_Click);
            ApplicationBar.Buttons.Add(mapViewButton);
            isMap = false;
        }

        void mapViewButton_Click(object sender, EventArgs e)
        {
            if (isMap)
            {
                ReportDetailHolder.Visibility = System.Windows.Visibility.Visible;
                ReportMap.Visibility = System.Windows.Visibility.Collapsed;
                mapViewButton.IconUri = new Uri("/icons/appbar.map.png", UriKind.Relative);
                mapViewButton.Text = "map";
                isMap = false;
            }
            else
            {
                ReportDetailHolder.Visibility = System.Windows.Visibility.Collapsed;
                ReportMap.Visibility = System.Windows.Visibility.Visible;           

                mapViewButton.IconUri = new Uri("/icons/appbar.page.corner.png", UriKind.Relative);
                mapViewButton.Text = "Report";
                isMap = true;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ContentPanel.DataContext = app.ActiveIncident;
            ReportMap.SetView(app.ActiveIncident.incident.Location, 12);         
            

            loadPictures();
        }

        void loadPictures()
        {
            //TODO: Addd a photo/media viewer.

            if (app.ActiveIncident.media.Count > 0)
            {

                if (app.ActiveIncident.media[0].type == "1")
                {

                    string full_url = "";
                    if (ValidateAndGetUri(app.ActiveIncident.media[0].link))
                    {
                        full_url = app.ActiveIncident.media[0].link;

                    }
                    else
                    {
                        full_url = app.SelectedDeployment.url + "/media/uploads/" + app.ActiveIncident.media[0].link;

                    }

                    Uri imageuri = new Uri(full_url, UriKind.Absolute);
                    ImageContainer.Source = new BitmapImage(imageuri);
                    ImageContainer.ImageOpened += new EventHandler<RoutedEventArgs>(ImageContainer_ImageOpened);
                    ImageContainer.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(ImageContainer_ImageFailed);
                    ProgressBar.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    if (ValidateAndGetUri(app.ActiveIncident.media[0].link))
                    {
                        MainLink.Visibility = System.Windows.Visibility.Visible;
                        MainLink.Content = "Link #1";
                        MainLink.NavigateUri = new Uri(app.ActiveIncident.media[0].link, UriKind.Absolute);
                    }
                }
            }
            else
            {
                ImageContainer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void ImageContainer_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        void ImageContainer_ImageOpened(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private bool ValidateAndGetUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }
             
    }
}
