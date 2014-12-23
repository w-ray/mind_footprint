using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Build1.Resources;
using LML_WP;
using MyMindWave.MindwaveSensor.WP8;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
using Windows.Networking.Proximity;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Networking.Sockets;
using Microsoft.Phone.Tasks;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Spreadsheets;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location; // Provides the GeoCoordinate class.
using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;


namespace MySpreadsheetIntegration {

    class Program
    {
        static void Main(string[] args)
        {
            ////////////////////////////////////////////////////////////////////////////
            // STEP 1: Configure how to perform OAuth 2.0
            ////////////////////////////////////////////////////////////////////////////

            // TODO: Update the following information with that obtained from
            // https://code.google.com/apis/console. After registering
            // your application, these will be provided for you.

            string CLIENT_ID = "1066924954128-h6tn2nvbqq9vufg94qj2dqgki19v0r8b.apps.googleusercontent.com";

            // Space separated list of scopes for which to request access.
            string SCOPE = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";

            // This is the Redirect URI for installed applications.
            // If you are building a web application, you have to set your
            // Redirect URI at https://code.google.com/apis/console.
            string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob:auto";

            ////////////////////////////////////////////////////////////////////////////
            // STEP 2: Set up the OAuth 2.0 object
            ////////////////////////////////////////////////////////////////////////////

            // OAuth2Parameters holds all the parameters related to OAuth 2.0.
            OAuth2Parameters parameters = new OAuth2Parameters();

            // Set your OAuth 2.0 Client Id (which you can register at
            // https://code.google.com/apis/console).
            parameters.ClientId = CLIENT_ID;

            // Set your Redirect URI, which can be registered at
            // https://code.google.com/apis/console.
            parameters.RedirectUri = REDIRECT_URI;

            ////////////////////////////////////////////////////////////////////////////
            // STEP 3: Get the Authorization URL
            ////////////////////////////////////////////////////////////////////////////

            // Set the scope for this particular service.
            parameters.Scope = SCOPE;

            // Get the authorization url.  The user of your application must visit
            // this url in order to authorize with Google.  If you are building a
            // browser-based application, you can redirect the user to the authorization
            // url.
            string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
            Console.WriteLine(authorizationUrl);
            Console.WriteLine("Please visit the URL above to authorize your OAuth "
              + "request token.  Once that is complete, type in your access code to "
              + "continue...");
            parameters.AccessCode = Console.ReadLine();

            ////////////////////////////////////////////////////////////////////////////
            // STEP 4: Get the Access Token
            ////////////////////////////////////////////////////////////////////////////

            // Once the user authorizes with Google, the request token can be exchanged
            // for a long-lived access token.  If you are building a browser-based
            // application, you should parse the incoming request token from the url and
            // set it in OAuthParameters before calling GetAccessToken().
            OAuthUtil.GetAccessToken(parameters);
            string accessToken = parameters.AccessToken;
            Console.WriteLine("OAuth Access Token: " + accessToken);

            ////////////////////////////////////////////////////////////////////////////
            // STEP 5: Make an OAuth authorized request to Google
            ////////////////////////////////////////////////////////////////////////////

            // Initialize the variables needed to make the request
            GOAuth2RequestFactory requestFactory =
                new GOAuth2RequestFactory(null, "MySpreadsheetIntegration-v1", parameters);
            SpreadsheetsService service = new SpreadsheetsService("MySpreadsheetIntegration-v1");
            service.RequestFactory = requestFactory;            
        }
    }

}

namespace Build1
{
    
    
        public partial class MainPage : PhoneApplicationPage
        {
            // Constructor

            BT_In MyBT;

            bool isBrainConnected = false;

            bool isConnectOK = false;

            int s_alpha_H = 0, s_alpha_L = 0;
            int s_delta = 0, s_theta = 0;
            int s_gamma_H = 0, s_gamma_L = 0;
            int s_beta_H = 0, s_beta_L = 0;
            double s_attention = 0.0, s_meditation = 0.0, s_signal = 0.0;
            double s_drowsiness_val = 0.0, s_coma_val = 0.0, s_epilepsy = 0.0;
            int s_emergency_num = 0;
            double d_mean = 0, dt = 1;
            double d_ms = 0;

            private async void ShowLoc()
            {
                Geolocator myGeolocator = new Geolocator();
                Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
                Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
                GeoCoordinate myGeoCoordinate = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
            }



            public MainPage()
            {
                InitializeComponent();

                // Sample code to localize the ApplicationBar
                //BuildLocalizedApplicationBar();

                ShowLoc();

                MyBT = new BT_In();

                Discovered_BT_List.ItemsSource = MyBT.pairedDevicesList;



            }


        
            // Load data for the ViewModel Items
            protected override void OnNavigatedTo(NavigationEventArgs e)
            {
                if (!App.ViewModel.IsDataLoaded)
                {
                    App.ViewModel.LoadData();
                }
            }

            private void BTConfig_b_Tap(object sender, System.Windows.Input.GestureEventArgs e)
            {
                isBrainConnected = false;

                MyBT.RefreshPairedDevicesList();
            }

            private void ConnectToBrain()
            {
                try
                {
                    if (!Mindwave.Current.IsConnected)
                    {
                        PairedDeviceInfo pdi = Discovered_BT_List.SelectedItem as PairedDeviceInfo;
                        Windows.Networking.Proximity.PeerInformation peer = pdi.PeerInfo;

                        Mindwave.Current.Start(peer);

                        isBrainConnected = true;

                        //Mindwave.Current.allowBlinkSec = 2;
                        //Mindwave.Current.allowBlinkInterval = 2;
                        Mindwave.Current.CurrentValueChanged += Current_CurrentValueChanged;
                        Mindwave.Current.StateChanged += Current_StateChanged;
                        //Mindwave.Current.Blinking += Current_Blinking;
                        BTDeviceCon_t.Text = Mindwave.Current.PeerInformation.DisplayName + ":" + Mindwave.Current.PeerInformation.ServiceName + ":" + Mindwave.Current.IsConnected.ToString();
                    }
                    else
                    {
                        MessageBox.Show("No Mindwave is find!!!");
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("MindWave Connection Error: " + err.Message + "\r\n" + Mindwave.Current.Err_S);

                    Mindwave.Current.Dispose();
                }
            }

            void Current_StateChanged(object sender, MindwaveStateChangedEventArgs e)
            {
                //throw new NotImplementedException();

                if (Mindwave.Current.State == MindwaveServiceState.ConnectedWithData)
                {
                    isConnectOK = true;
                }
                else
                {
                    isConnectOK = false;
                }

                sensor_status_t.Text = e.CurrentState + " : " + Mindwave.Current.Err_S + "\r\n" + sensor_status_t.Text;
                sensor_status2_t.Text = e.CurrentState.ToString();
                if (isConnectOK)
                    sensor_status2_t.Foreground = new SolidColorBrush(Colors.Green);
                else
                    sensor_status2_t.Foreground = new SolidColorBrush(Colors.Red);

            }

            void Current_CurrentValueChanged(object sender, MindwaveReadingEventArgs e)
            {
                //throw new NotImplementedException();

                s_delta = e.SensorReading.Delta;

                s_delta_t.Text = s_delta.ToString();

                s_theta = e.SensorReading.Theta;

                s_theta_t.Text = s_theta.ToString();

                s_alpha_H = e.SensorReading.AlphaHigh;

                s_alpha_H_t.Text = s_alpha_H.ToString();

                s_alpha_L = e.SensorReading.AlphaLow;

                s_alpha_L_t.Text = s_alpha_L.ToString();

                s_beta_H = e.SensorReading.BetaHigh;

                s_beta_H_t.Text = s_beta_H.ToString();

                s_beta_L = e.SensorReading.BetaLow;

                s_beta_L_t.Text = s_beta_L.ToString();

                s_gamma_H = e.SensorReading.GammaMid;

                s_gamma_H_t.Text = s_gamma_H.ToString();

                s_gamma_L = e.SensorReading.GammaLow;

                s_gamma_L_t.Text = s_gamma_L.ToString();

                s_signal = e.SensorReading.Quality;

                s_signal_t.Text = s_signal.ToString();

                s_attention = e.SensorReading.eSenseAttention;

                s_attention_t.Text = s_attention.ToString();

                s_meditation = e.SensorReading.eSenseMeditation;

                s_meditation_t.Text = s_meditation.ToString();

                for (int a = 0; isConnectOK; a++)
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        d_mean = s_delta * dt + d_mean;

                    }
                    d_mean = d_mean / 10.0;

                    for (int i = 0; i <= 10; i++)
                    {
                        d_ms = ((s_delta - d_mean) * (s_delta - d_mean) * dt) + d_ms;
                    }
                    d_ms = d_ms / 10.0;



                  
                }


            }

            private void BTDeviceCon_b_Tap(object sender, System.Windows.Input.GestureEventArgs e)
            {
                if (!isBrainConnected)
                {
                    sensor_status_t.Text = "";

                    ConnectToBrain();

                    BTDeviceCon_b.Content = "Turn OFF\nMindwave";
                }
                else
                {
                    isBrainConnected = false;

                    BTDeviceCon_b.Content = "Turn ON\n Mindwave";

                    if (Mindwave.Current.IsConnected)
                    {
                        try
                        {
                            Mindwave.Current.CurrentValueChanged -= Current_CurrentValueChanged;
                            Mindwave.Current.StateChanged -= Current_StateChanged;
                            //Mindwave.Current.Blinking -= Current_Blinking;

                            Mindwave.Current.Stop();
                            BTDeviceCon_t.Text = "";
                        }
                        catch { }
                    }
                }
            }

            private void BT_s_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {


            }

            private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {

            }



            private void s_e_num_tbox_TextChanged(object sender, TextChangedEventArgs e)
            {
                s_emergency_num = Convert.ToInt32(s_e_num_tbox.Text);
            }










            // Sample code for building a localized ApplicationBar
            //private void BuildLocalizedApplicationBar()
            //{
            //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
            //    ApplicationBar = new ApplicationBar();

            //    // Create a new button and set the text value to the localized string from AppResources.
            //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            //    appBarButton.Text = AppResources.AppBarButtonText;
            //    ApplicationBar.Buttons.Add(appBarButton);

            //    // Create a new menu item with the localized string from AppResources.
            //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            //    ApplicationBar.MenuItems.Add(appBarMenuItem);
            //}
        
    }
}
