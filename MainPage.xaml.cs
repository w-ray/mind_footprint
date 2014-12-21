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

namespace Build1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        BT_In = MyBT;

        bool isBrainConnected = false;

        bool isConnectOK = false;


        public MainPage()
        {
            InitializeComponent();


            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            MyBT = new BT_In;

            Discovered_BT_List.ItemsSource = MyBT.pairedDevicesList;
            
            


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
                    Mindwave.Current.StateChanged +=Current_StateChanged;
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

            sensor_status_t.Text = e.CurrentState + "  : " + Mindwave.Current.Err_S + "\r\n" + sensor_status_t.Text;
    }

    void Current_CurrentValueChanged(object sender, MindwaveReadingEventArgs e)
    {
 	    //throw new NotImplementedException();
    }
    private void BTDeviceCon_B_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
        if (!isBrainConnected)
        {
            sensor_status_t.Text = "";

            ConnectToBrain();

            BTDeviceCon_b.Content = "Turn OFF Mindwave";
        }
        else
        {
            isBrainConnected = false;
            BTDeviceCon_b.Content = "Turn ON Mindwave";
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
