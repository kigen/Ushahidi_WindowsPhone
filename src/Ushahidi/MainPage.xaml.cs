using System;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using Ushahidi.Library.Network;
using Ushahidi.Library.Data;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using Ushahidi.Maps;
using System.Windows.Controls;
using System.Threading;

namespace Ushahidi
{
    public partial class MainPage : PhoneApplicationPage
    {

        ApplicationBarIconButton mapViewButton;
        ApplicationBarIconButton addButton;
        ApplicationBarIconButton refreshButton;
        bool LoadedOnline = false;
        bool NaivgationNew = false;


        App app;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            app = Application.Current as App;

            mapViewButton = new ApplicationBarIconButton();
            mapViewButton.IconUri = new Uri("/icons/appbar.map.png", UriKind.Relative);
            mapViewButton.Text = "Map view";
            mapViewButton.Click += new EventHandler(mapViewButton_Click);

            refreshButton = new ApplicationBarIconButton();
            refreshButton.IconUri = new Uri("/icons/appbar.refresh.png", UriKind.Relative);
            refreshButton.Text = "refresh";
            refreshButton.Click += new EventHandler(refreshButton_Click);

            addButton = new ApplicationBarIconButton();
            addButton.IconUri = new Uri("/icons/appbar.add.rest.png", UriKind.Relative);
            addButton.Text = "Add";
            addButton.Click += new EventHandler(addButton_Click);


            ApplicationBar.Buttons.Add(addButton);
            ApplicationBar.Buttons.Add(mapViewButton);
            ApplicationBar.Buttons.Add(refreshButton);


        }

        void refreshButton_Click(object sender, EventArgs e)
        {
            if (app.IsAvailable)
            {
                GoOnline();
            }
            else
            {
                LoadFromDB();
            }
        }

        void mapViewButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainMapPage.xaml", UriKind.Relative));
        }

        void addButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddDeployments.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (NaivgationNew)
            {               
               
             LoadFromDB();           

                //Load categories..
             DownloadCategories();
            }
            NaivgationNew = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
            {
                NaivgationNew = true;
            }
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                LoadFromDB();
            }
        }


        void DownloadCategories()
        {
            WebTools CategoryTools = new WebTools();
            CategoryTools.DataDownloadComplete += new EventHandler<DownloadCompleteArgs>(CategoryTools_DataDownloadComplete);
            CategoryTools.DataDownloadCompleteWithError += new EventHandler<DownloadCompleteArgs>(CategoryTools_DataDownloadCompleteWithError);
            CategoryTools.DownloadDeploymentCategories();
        }

        void CategoryTools_DataDownloadCompleteWithError(object sender, DownloadCompleteArgs e)
        {
            //keep Quiet...
        }

        void CategoryTools_DataDownloadComplete(object sender, DownloadCompleteArgs e)
        {
            List<DeploymentCategory> categories = (List<DeploymentCategory>)e.DownloadObject;
            foreach (DeploymentCategory ct in categories)
            {
                App.DataBaseUtility.SaveDeploymentCategory(ct);
            }
        }

        void GoOnline()
        {
            WebTools tools = new WebTools(app.GlobalSettings);
            tools.DataDownloadComplete += new EventHandler<DownloadCompleteArgs>(tools_DeploymentDownloadComplete);
            tools.DataDownloadCompleteWithError += new EventHandler<DownloadCompleteArgs>(tools_DataDownloadCompleteWithError);
            ProgressBar.Visibility = System.Windows.Visibility.Visible;
            tools.DownloadDeployments();
            LoadFromDB();

        }

        void tools_DataDownloadCompleteWithError(object sender, DownloadCompleteArgs e)
        {
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            MessageBox.Show("Error Fetching deployments online\nYou might not have internet connection", "Download Error", MessageBoxButton.OK);
        }

        void LoadFromDB()
        {
            app.GlobalDeployments = App.DataBaseUtility.getAllDeployments();
            RefreshList();
        }

        void RefreshList()
        {
            DeploymentsListBox.ItemsSource = app.GlobalDeployments;
        }

        void tools_DeploymentDownloadComplete(object sender, DownloadCompleteArgs e)
        {
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            List<Deployments> dp = (List<Deployments>)e.DownloadObject;
            foreach (Deployments d in dp)
            {
                App.DataBaseUtility.saveDeployment(d);
            }

            LoadFromDB();
        }

        private void DeploymentsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DeploymentsListBox != null && DeploymentsListBox.SelectedIndex > -1)
            {
                app.SelectedDeployment = app.GlobalDeployments[DeploymentsListBox.SelectedIndex];
                NavigationService.Navigate(new Uri("/DeploymentViewPage.xaml", UriKind.Relative));
            }
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MainPivot.SelectedIndex = 1;
        }

        private void StackPanel_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddDeployments.xaml", UriKind.Relative));
        }

        private void StackPanel_Tap_2(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void StackPanel_Tap_3(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            ListBoxItem selectedListBoxItem = this.DeploymentsListBox.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            if (selectedListBoxItem == null)
            {
                return;
            }
            Deployments deployment = selectedListBoxItem.DataContext as Deployments;
            switch (menuItem.Header.ToString())
            {

                case "Edit":
                    app.ToEdit = deployment;
                    NavigationService.Navigate(new Uri("/AddDeployments.xaml", UriKind.Relative));
                    break;
                case "Delete":

                    if (MessageBox.Show("Are you sure you want to delete this entry?", "Deployment Removal", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        App.DataBaseUtility.DeleteDeployment(deployment);
                        LoadFromDB();
                    }

                    break;
            }

        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 1)
            {

                ApplicationBar.IsVisible = true;
                if (!LoadedOnline)
                {

                    if (app.IsAvailable)
                    {
                        Dispatcher.BeginInvoke(() =>
            {
                GoOnline();
            });

                    }
                    LoadedOnline = true;
                }

            }
            else
            {
                ApplicationBar.IsVisible = false;

            }
        }


        void SearchDeployments_DataDownloadCompleteWithError(object sender, DownloadCompleteArgs e)
        {
            Error error = e.DownloadObject as Error;
            SearchTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            MessageBox.Show(error.message, "Search Failed", MessageBoxButton.OK);
        }

        void SearchDeployments_DataDownloadComplete(object sender, DownloadCompleteArgs e)
        {
            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            List<Deployments> dp = (List<Deployments>)e.DownloadObject;
            foreach (Deployments d in dp)
            {
                App.DataBaseUtility.saveDeployment(d);
            }
            SearchTextBlock.Text = "";
            SearchTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            app.GlobalDeployments = dp;
            RefreshList();

        }

        private void StackPanel_Tap_4(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchTextBlock.Text = "";
            MainPivot.SelectedIndex = 1;
            Thread.Sleep(100);
            SearchTextBlock.Visibility = System.Windows.Visibility.Visible;
        }

        private void SearchTextBlock_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                WebTools SearchDeployments = new WebTools();
                SearchDeployments.DataDownloadComplete += new EventHandler<DownloadCompleteArgs>(SearchDeployments_DataDownloadComplete);
                SearchDeployments.DataDownloadCompleteWithError += new EventHandler<DownloadCompleteArgs>(SearchDeployments_DataDownloadCompleteWithError);
                SearchDeployments.SearchDeployments(SearchTextBlock.Text);
                ProgressBar.Visibility = System.Windows.Visibility.Visible;

            }
        }
    }
}