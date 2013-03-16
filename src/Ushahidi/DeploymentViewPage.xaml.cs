using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Ushahidi.Library.Network;
using Ushahidi.Library.Data;
using Microsoft.Phone.Controls.Maps;
using Ushahidi.Maps;
using Microsoft.Phone.Shell;

namespace Ushahidi
{
    public partial class DeploymentViewPage : PhoneApplicationPage
    {
        App app;
        ApplicationBarIconButton mapViewButton;
        ApplicationBarIconButton AboutButton;
        private bool isMap { get; set; }
        public DeploymentViewPage()
        {
            InitializeComponent();
            app = Application.Current as App;

            mapViewButton = new ApplicationBarIconButton();
            mapViewButton.IconUri = new Uri("/icons/appbar.map.png", UriKind.Relative);
            mapViewButton.Text = "Map ";
            mapViewButton.Click += new EventHandler(mapViewButton_Click);

            AboutButton = new ApplicationBarIconButton();
            AboutButton.IconUri = new Uri("/icons/appbar.information.circle.png", UriKind.Relative);
            AboutButton.Text = "Info";
            AboutButton.Click += new EventHandler(AboutButton_Click);


            ApplicationBar.Buttons.Add(mapViewButton);
            ApplicationBar.Buttons.Add(AboutButton);
            isMap = false;
        }

        void AboutButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(app.SelectedDeployment.description, "About: " + app.SelectedDeployment.name, MessageBoxButton.OK);
        }

        void mapViewButton_Click(object sender, EventArgs e)
        {
            if (isMap)
            {
             
                IncidentListBox.Visibility = System.Windows.Visibility.Collapsed;
                IncidentMap.Visibility = System.Windows.Visibility.Visible;
                mapViewButton.IconUri = new Uri("/icons/appbar.map.png", UriKind.Relative);
                mapViewButton.Text = "map";
                isMap = false;
            }
            else
            {
                IncidentListBox.Visibility = System.Windows.Visibility.Visible;
                IncidentMap.Visibility = System.Windows.Visibility.Collapsed;
                mapViewButton.IconUri = new Uri("/icons/appbar.list.png", UriKind.Relative);
                mapViewButton.Text = "Reports";
                isMap = true;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
           
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
            {
                LoadData();
            }
            else
            {
                if (app.ActiveDeploymentIncidents == null)
                {
                    LoadData();
                }
            }
        }

        void LoadData()
        {
            DeploymentNameTextBox.DataContext = app.SelectedDeployment;
            ProgressIndicator.Visibility = System.Windows.Visibility.Visible;
            WebTools tools = new WebTools();
            tools.DataDownloadComplete += new EventHandler<DownloadCompleteArgs>(tools_DataDownloadComplete);
            tools.DataDownloadCompleteWithError += new EventHandler<DownloadCompleteArgs>(tools_DataDownloadCompleteWithError);
            tools.DownloadIncidents(app.SelectedDeployment);
        }

        void tools_DataDownloadCompleteWithError(object sender, DownloadCompleteArgs e)
        {
            Error error = e.DownloadObject as Error;
            MessageBox.Show(error.message);
            ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;
        }

        void tools_DataDownloadComplete(object sender, DownloadCompleteArgs e)
        {
            List<Incident> incidents = e.DownloadObject as List<Incident>;
            app.ActiveDeploymentIncidents = incidents;
            IncidentListBox.ItemsSource = incidents;
            ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;
            loadMap();
        }


        void loadMap()
        {
            if (app.ActiveDeploymentIncidents != null)
            {
                List<Pushpin> pushpins = new List<Pushpin>();
                foreach (Incident incident in app.ActiveDeploymentIncidents)
                {
                    Pushpin push = new Pushpin();
                    push.Location = incident.incident.Location;
                    push.Content = incident.incident.RelativeDate;
                    push.Tag = incident.incident;
                    pushpins.Add(push);

                }

                var clusterer = new PushpinClusterer(IncidentMap, pushpins,
                this.Resources["ClusterTemplate"] as DataTemplate);

                IncidentMap.SetView(app.SelectedDeployment.Location, 7);
            }
        }

        private void IncidentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IncidentListBox != null && IncidentListBox.SelectedIndex > -1)
            {

                app.ActiveIncident = app.ActiveDeploymentIncidents[IncidentListBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/IncidentPage.xaml", UriKind.Relative));
            }

        }

        
    }
}
