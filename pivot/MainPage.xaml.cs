using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MindFoorprint.Resources;
using LML_WP;
using MyMindWave.MindwaveSensor.WP8;
using Windows.Networking.Proximity;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.Windows.Shapes;

namespace MindFoorprint
{
    public partial class MainPage : PhoneApplicationPage
    {
        BT_In MyBT;

        bool isBrainConnected = false;

        bool isConnectOK = false;

        long s_alpha_H = 0, s_alpha_L = 0;
        long s_delta = 0, s_theta = 0;
        long s_gamma_H = 0, s_gamma_L = 0;
        long s_beta_H = 0, s_beta_L = 0;
        double s_attention = 0.0, s_meditation = 0.0, s_signal = 0.0;
        double s_drowsiness_val = 0.0, s_epilepsy = 0.0;
        string s_emergency_num;
        double t_mean = 0, tt = 1;
        double t_ms = 0;
        double d_mean = 0, dt = 1, d_ms = 0;
        double g_mean = 0, gt = 1, g_ms = 0;
        int i = 0, j = 0;
        double latitude = 0, longitude = 0;
        string status = " ";
        int userid = 0;
        bool message_sent = false;
        int data_id = 0;
        bool data_ready = false;
        int data_max = 20;
        long[] delta;
        long[] theta;
        long[] gamma;
        long delta_sum = 0, theta_sum = 0, gamma_sum = 0;
        long delta_s_sum = 0, theta_s_sum = 0, gamma_s_sum = 0;
        double[] d_amp;
        string myURL;


        GeoCoordinateWatcher myGPS = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            MyBT = new BT_In();

            Discovered_BT_List.ItemsSource = MyBT.pairedDevicesList;

            myGPS.MovementThreshold = 0.1;
            myGPS.PositionChanged += myGPS_PositionChanged;
            myGPS.Start();

            latitude = longitude = -999;

            delta = new long[data_max];
            theta = new long[data_max];
            gamma = new long[data_max];
            d_amp = new double[data_max];
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void myGPS_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //throw new NotImplementedException();
            try
            {
                latitude = e.Position.Location.Latitude;
                longitude = e.Position.Location.Longitude;

                latitude_t.Text = latitude.ToString();
                longitude_t.Text = longitude.ToString();

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
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


        private double getStandardDeviation(List<double> doubleList)
        {
            double average = doubleList.Average();
            double sumOfDerivation = 0;
            foreach (double value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (doubleList.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
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
            sensor_status_t2.Text = e.CurrentState + " : " + Mindwave.Current.Err_S + "\r\n" + sensor_status_t2.Text;
            /*sensor_status2_t.Text = e.CurrentState.ToString();
            if (isConnectOK)
                sensor_status2_t.Foreground = new SolidColorBrush(Colors.Green);
            else
                sensor_status2_t.Foreground = new SolidColorBrush(Colors.Red);
            */
        }
            


        void Current_CurrentValueChanged(object sender, MindwaveReadingEventArgs e)
        {
            //throw new NotImplementedException();
            List<double> delta_list = new List<double>();
            s_signal = e.SensorReading.Quality;
            s_signal_t.Text = s_signal.ToString();
            if (isConnectOK)
            {
                s_delta = e.SensorReading.Delta;
                s_theta = e.SensorReading.Theta;
                s_alpha_H = e.SensorReading.AlphaHigh;
                s_alpha_L = e.SensorReading.AlphaLow;
                s_beta_H = e.SensorReading.BetaHigh;
                s_beta_L = e.SensorReading.BetaLow;
                s_gamma_H = e.SensorReading.GammaMid;
                s_gamma_L = e.SensorReading.GammaLow;
                s_attention = e.SensorReading.eSenseAttention;
                s_meditation = e.SensorReading.eSenseMeditation;
                s_delta_t.Text = s_delta.ToString();
                s_theta_t.Text = s_theta.ToString();
                s_alpha_H_t.Text = s_alpha_H.ToString();
                s_alpha_L_t.Text = s_alpha_L.ToString();
                s_beta_H_t.Text = s_beta_H.ToString();
                s_beta_L_t.Text = s_beta_L.ToString();
                s_gamma_H_t.Text = s_gamma_H.ToString();
                s_gamma_L_t.Text = s_gamma_L.ToString();
                s_attention_t.Text = s_attention.ToString();
                s_meditation_t.Text = s_meditation.ToString();

                
                delta[data_id] = s_delta;
                theta[data_id] = s_theta;
                gamma[data_id] = (s_gamma_H + s_gamma_L) / 2;
                data_id++;

                if (data_id == data_max)
                {
                    data_id = 0;
                    data_ready = true;
                }

                if (data_ready)
                {
                    int a = 0;
                    for (i = 0; i < data_max; i++)
                    {
                        delta_sum = delta_sum + delta[i];
                        theta_sum = theta_sum + theta[i];
                        gamma_sum = gamma_sum + gamma[i];
                        delta_list.Add(delta[i]);
                        /*delta_s_sum = delta_s_sum + delta[i] * delta[i];
                        theta_s_sum = theta_s_sum + theta[i] * theta[i];
                        gamma_s_sum = gamma_s_sum + gamma[i] * gamma[i];*/
                    }

                    d_mean = delta_sum / data_max;
                    t_mean = theta_sum / data_max;
                    g_mean = gamma_sum / data_max;
                    d_ms = getStandardDeviation(delta_list);

/*                    d_ms = delta_s_sum / data_max - d_mean * d_mean;
                    t_ms = theta_s_sum / data_max - t_mean * t_mean;
                    g_ms = gamma_s_sum / data_max - g_mean * g_mean;*/



                    for (i = 0; i < data_max; i++)
                    {
                        d_amp[i] = d_ms + d_mean;
                        if (d_amp[i] > 1500000)
                        {
                            a++;
                        }
                    }
                    if (a > 5)
                    {
                        status = "In Coma/Sleep";
                        try
                        {
                            myURL = "http://api.pushingbox.com/pushingbox?devid=vEA2A919C89FD9D3&UserID=" + userid.ToString() + "&latitude=" + latitude.ToString() + "&longitude=" + longitude.ToString() + "&s_delta=" + s_delta.ToString() + "&s_theta=" + s_theta.ToString() + "&s_alpha_H=" + s_alpha_H.ToString() + "&s_alpha_L=" + s_alpha_L.ToString() + "&s_beta_H=" + s_beta_H.ToString() + "&s_beta_L=" + s_beta_L.ToString() + "&s_gamma_H=" + s_gamma_H.ToString() + "&s_gamma_L=" + s_gamma_L.ToString() + "&s_attention=" + s_attention.ToString() + "&s_meditation=" + s_meditation.ToString() + "&s_signal=" + s_signal.ToString() + "&s_drowsiness_val=" + t_ms.ToString() + "&s_epilepsy=" + g_ms.ToString() + "&s_coma_val=" + d_ms.ToString();
                            myURL += "&remark=\"" + status + "\"";
                            HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(myURL);

                            httpReq.BeginGetResponse(new AsyncCallback(GetAsyncResponse), httpReq);
                        }
                        catch(Exception err)
                        {
                            MessageBox.Show(err.Message + myURL);
                        }

                    }
                    else
                    {
                        status = "Normal";
                        try
                        {
                            myURL = "http://api.pushingbox.com/pushingbox?devid=vEA2A919C89FD9D3&UserID=" + userid.ToString() + "&latitude=" + latitude.ToString() + "&longitude=" + longitude.ToString() + "&s_delta=" + s_delta.ToString() + "&s_theta=" + s_theta.ToString() + "&s_alpha_H=" + s_alpha_H.ToString() + "&s_alpha_L=" + s_alpha_L.ToString() + "&s_beta_H=" + s_beta_H.ToString() + "&s_beta_L=" + s_beta_L.ToString() + "&s_gamma_H=" + s_gamma_H.ToString() + "&s_gamma_L=" + s_gamma_L.ToString() + "&s_attention=" + s_attention.ToString() + "&s_meditation=" + s_meditation.ToString() + "&s_signal=" + s_signal.ToString() + "&s_drowsiness_val=" + t_ms.ToString() + "&s_epilepsy=" + g_ms.ToString() + "&s_coma_val=" + d_ms.ToString();
                            myURL += "&remark=\"" + status + "\"";

                            HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(myURL);

                            httpReq.BeginGetResponse(new AsyncCallback(GetAsyncResponse), httpReq);

                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(err.Message);
                        }
                    }


                        
                }            
            }
        }

        public void GetAsyncResponse(IAsyncResult result)
        {

        }

        private void help_b_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            status = "User Need Help";

            myURL = "http://api.pushingbox.com/pushingbox?devid=vEA2A919C89FD9D3&UserID=" + userid.ToString() + "&latitude=" + latitude.ToString() + "&longitude=" + longitude.ToString() + "&s_delta=" + s_delta.ToString() + "&s_theta=" + s_theta.ToString() + "&s_alpha_H=" + s_alpha_H.ToString() + "&s_alpha_L=" + s_alpha_L.ToString() + "&s_beta_H=" + s_beta_H.ToString() + "&s_beta_L=" + s_beta_L.ToString() + "&s_gamma_H=" + s_gamma_H.ToString() + "&s_gamma_L=" + s_gamma_L.ToString() + "&s_attention=" + s_attention.ToString() + "&s_meditation=" + s_meditation.ToString() + "&s_signal=" + s_signal.ToString() + "&s_drowsiness_val=" + t_ms.ToString() + "&s_epilepsy=" + g_ms.ToString() + "&s_coma_val=" + d_ms.ToString();
            myURL += "&remark=\"" + status + "\"";

            HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(myURL);

            httpReq.BeginGetResponse(new AsyncCallback(GetAsyncResponse), httpReq);
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
