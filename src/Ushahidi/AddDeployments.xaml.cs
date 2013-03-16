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
using Ushahidi.Library.Data;
using System.Device.Location;

namespace Ushahidi
{
    public partial class AddDeployments : PhoneApplicationPage
    {
        GeoCoordinate selectedLocation;
        App app;
        public AddDeployments()
        {
            InitializeComponent();
            app = Application.Current as App;
        }
        bool isEdit = false;

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            if (isEdit)
            {
                saveEdit();
            }
            else
            {

                if (ValidateAndGetUri(UrlTextBox.Text))
                {
                    if (NameTextBox.Text.Trim() != string.Empty)
                    {
                        if (selectedLocation != null)
                        {
                            Deployments deployment = new Deployments();
                            deployment.name = NameTextBox.Text;
                            deployment.isLocal = true;
                            deployment.description = DescriptionTextBox.Text;
                            deployment.url = UrlTextBox.Text;
                            deployment.latitude = selectedLocation.Latitude.ToString();
                            deployment.longitude = selectedLocation.Longitude.ToString();
                            deployment.discovery_date = DateTime.Now.ToString();
                            App.DataBaseUtility.saveDeployment(deployment);
                            NavigationService.GoBack();

                        }
                        else
                        {
                            MessageBox.Show("Select Location from the map.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Deployment Name missing.");
                    }
                }
                else
                {
                    MessageBox.Show("URL is not valid");
                }
            }
           
        }


        void saveEdit()
        {
            if (ValidateAndGetUri(UrlTextBox.Text))
            {
                if (NameTextBox.Text.Trim() != string.Empty)
                {

                        Deployments deployment = app.ToEdit;
                        deployment.name = NameTextBox.Text;               
                        deployment.description = DescriptionTextBox.Text;
                        deployment.url = UrlTextBox.Text;
                        if (selectedLocation != null)
                        {
                            deployment.latitude = selectedLocation.Latitude.ToString();
                            deployment.longitude = selectedLocation.Longitude.ToString();
                        }
                        App.DataBaseUtility.updateAll();
                        app.ToEdit = null;
                        NavigationService.GoBack();
                   
                }
                else
                {
                    MessageBox.Show("Deployment Name missing.");
                }
            }
            else
            {
                MessageBox.Show("URL is not valid");
            }
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            if (isEdit)
            {
                TitleEditTextBlock.Text = "EDIT DEPLOYMENT";
                NameTextBox.Text = app.ToEdit.name;
                DescriptionTextBox.Text = app.ToEdit.description;
                UrlTextBox.Text = app.ToEdit.url;                                
            }

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (app.ToEdit != null)
            {
                isEdit = true;
            }

        }

        private void LocMap_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

          selectedLocation=LocMap.ViewportPointToLocation(e.GetPosition(LocMap));

          MessageBox.Show("Location Selected"); 
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
