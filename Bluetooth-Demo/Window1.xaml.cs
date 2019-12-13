using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using InTheHand.Net;
using System.Threading;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Bluetooth_Demo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        BackgroundWorker bg;
        BackgroundWorker bg_send;

        public Window1()
        {
            InitializeComponent();
            bg = new BackgroundWorker();
            bg_send = new BackgroundWorker();

            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);

            bg_send.DoWork += new DoWorkEventHandler(bg_send_DoWork);
            bg_send.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_send_RunWorkerCompleted);
        }


        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            device_list.ItemsSource = (List<Device>)e.Result;
            pb.Visibility = Visibility.Hidden;
            btn_find.IsEnabled = true;
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Device> devices = new List<Device>();
InTheHand.Net.Sockets.BluetoothClient bc = new InTheHand.Net.Sockets.BluetoothClient();
InTheHand.Net.Sockets.BluetoothDeviceInfo[] array = bc.DiscoverDevices();
int count = array.Length;
for (int i = 0; i < count; i++)
{
    Device device = new Device(array[i]);
    devices.Add(device);
}
            e.Result = devices;
        }

        private void btn_find_Click(object sender, RoutedEventArgs e)
        {
            if (!bg.IsBusy)
            {
                btn_find.IsEnabled = false;
                pb.Visibility = Visibility.Visible;
                bg.RunWorkerAsync();
            }
        }
        private void btn_pair_Click(object sender, RoutedEventArgs e)
        {
            string myPin = "1234";
            Device device = (Device)device_list.SelectedItem;
            var deviceInfo = device.DeviceInfo;
            BluetoothSecurity.PairRequest(deviceInfo, myPin);

            //if(device.Authenticated)this.txt_authenticated.Text = "True";
            //else this.txt_authenticated.Text = "False";

        }
        

    private void device_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (device_list.SelectedItem != null)
            {
                Device device = (Device)device_list.SelectedItem;

                txt_authenticated.Text = device.Authenticated.ToString();
                txt_connected.Text = device.Connected.ToString();
                txt_devicename.Text = device.DeviceName;
                
            }
        }
        public string FilePath { get; set; }
        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "File To Send";
            dlg.DefaultExt = "*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FilePath = dlg.FileName;
                btn_send.IsEnabled = true;
            }
            else
                btn_send.IsEnabled = false;
        }


        void bg_send_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                ObexStatusCode status = (ObexStatusCode)e.Result;
                MessageBox.Show("Sending File Status - " + status.ToString(), "Send File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void bg_send_DoWork(object sender, DoWorkEventArgs e)
        {
            Device device = (Device)e.Argument;
            ObexStatusCode response_status = SendModule.SendFile(device.DeviceInfo, FilePath);
            e.Result = response_status;
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            if (!bg_send.IsBusy && device_list.SelectedItem != null && !string.IsNullOrEmpty(FilePath))
            {
                Device device = (Device)device_list.SelectedItem;
                bg_send.RunWorkerAsync(device);
            }
        }
    }
}
