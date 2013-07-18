using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ushahidi.Library.Data;
using Ushahidi.Library;
using System.IO.IsolatedStorage;

namespace Ushahidi
{
    public partial class App : Application
    {
        
        public bool isFirstTime { get; set; }

        public static readonly string DBLocation = "Data Source=isostore:/LocalDatabase.sdf";

        public static DataUtil DataBaseUtility { get; set; }


        public int LastReportCount { get; set; }


        public Deployments SelectedDeployment
        {
            get;
            set;
        }

        public Deployments ToEdit { get; set; }

        public List<Deployments> GlobalDeployments { get; set; }


        public List<Incident> ActiveDeploymentIncidents { get; set; }

        public Settings GlobalSettings { get; set; }

        public Incident ActiveIncident { get; set; }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                // Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }


            using (Database db = new Database(DBLocation))
            {
                //Inorder to clean the db uncomment the next line (or, if debugging, rebuild solution)
                //db.DeleteDatabase();
                if (!db.DatabaseExists())
                {
                    db.CreateDatabase();
                }
                db.SubmitChanges();
            }

            DataBaseUtility = new DataUtil(DBLocation);



            bool firstRun = false;
            //Check if it is running for the first time...
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("firstRun", out firstRun);
            if (!firstRun)
            {
                isFirstTime = true;
                Settings settings = new Settings();
                settings.ReportLimit = 100;
                settings.Distance = 100;
                settings.Location = new System.Device.Location.GeoCoordinate(-1, 36);

                //Show application Dialog.
                IsolatedStorageSettings.ApplicationSettings.Add("settings", settings);
                IsolatedStorageSettings.ApplicationSettings.Add("firstRun", true);
                IsolatedStorageSettings.ApplicationSettings.Save();
                GlobalSettings = settings;
            }
            else
            {
                isFirstTime = false;
                //Pull settings from isolated storage
                Settings settings = new Settings();
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("settings", out settings);
                GlobalSettings = settings;

            }

        }

        public void SaveSettings()
        {
            IsolatedStorageSettings.ApplicationSettings["settings"] = GlobalSettings;
            IsolatedStorageSettings.ApplicationSettings.Save();

        }


        public bool IsAvailable
        {
            get
            {
                return (
                    Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
                    Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None
                    );
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveSettings();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}