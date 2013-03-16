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
using Microsoft.Phone.Controls.Maps;
using Ushahidi.Library.Data;
using Ushahidi.Maps;
using System.Device.Location;

namespace Ushahidi
{
	public partial class MainMapPage : PhoneApplicationPage
	{
		App app;
		public MainMapPage()
		{
			InitializeComponent();
			app = Application.Current as App;
		}
		
		   void addPushpins()
		{
			List<Pushpin> pushpins = new List<Pushpin>();
			foreach (Deployments dp in app.GlobalDeployments)
			{
				Pushpin push = new Pushpin();
				push.Location = dp.Location;
				push.Tag = dp;
				push.Content = dp.name;
				pushpins.Add(push);
				push.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(push_Tap);
			}

			var clusterer = new PushpinClusterer(DeploymentsMap, pushpins,
			this.Resources["ClusterTemplate"] as DataTemplate);

			DeploymentsMap.SetView(new GeoCoordinate(-20, 20), 3);
		}
		
		void push_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			Pushpin p = sender as Pushpin;
			Deployments deployment = p.Tag as Deployments;
			app.SelectedDeployment = deployment;
			NavigationService.Navigate(new Uri("/DeploymentViewPage.xaml", UriKind.Relative));

		}

	}
}
