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

        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

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
