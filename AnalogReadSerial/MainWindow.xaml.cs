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
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;

namespace Mike.Arduino.AnalogReadSerial
{
    /// <summary>
    /// La placa arduino tiene que estar corriendo el Sketch http://arduino.cc/en/Tutorial/AnalogReadSerial
    /// </summary>
    public partial class MainWindow : Window
    {

        private SerialPort comPort;
        private Thread readThread;
        private bool continueReceiving;
        

        public MainWindow()
        {

            InitializeComponent();
            this.AvailablePortsComboBox.ItemsSource = SerialPort.GetPortNames();
            if (this.AvailablePortsComboBox.Items.Count > 0)
            {
                this.AvailablePortsComboBox.SelectedItem = this.AvailablePortsComboBox.Items[0];
                this.ConnectDisconnectButton.IsEnabled = true;
            }
            else
            {
                this.ConnectDisconnectButton.IsEnabled = false;
            }

        }

        private void ConnectDisconnectButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.ConnectDisconnectButton.Content.ToString() == "Conectar")
                this.ConnectPort();
            else
                this.DisconnectPort();

        }

        private void ConnectPort()
        {

            this.ConnectDisconnectButton.Content = "Desconectar";
            this.AvailablePortsComboBox.IsEnabled = false;

            string portName = this.AvailablePortsComboBox.SelectedItem.ToString();

            comPort = new SerialPort(portName, 9600);
            comPort.Open();

            readThread = new Thread(ReceiveComDataLoop);
            continueReceiving = true;
            readThread.Start();

        }


        private void ReceiveComDataLoop()
        {

            while (continueReceiving)
            {
                try
                {
                    this.ReceiveComData();
                }
                catch (TimeoutException) { }
            }
            
        }

        private void ReceiveComData()
        {
            string data = comPort.ReadLine();
            int value = 0;

            if (int.TryParse(data, out value))
            {

                DispatcherOperation op = Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<int>(ShowReceivedData), value);
                DispatcherOperationStatus status = op.Status;
                while (op.Status == DispatcherOperationStatus.Executing)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                }

            }
        }

        private void ShowReceivedData(int value)
        {
            this.CurrentValueTextBlock.Text = "Valor: " + value.ToString();
            this.CurrentValueProgressBar.Value = value;
        }
        

        private void DisconnectPort()
        {

            this.ConnectDisconnectButton.Content = "Conectar";
            this.AvailablePortsComboBox.IsEnabled = true;

            if (comPort != null && comPort.IsOpen)
            {
                continueReceiving = false;
                Thread.Sleep(100);
                readThread.Join();
                comPort.Close();
            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.DisconnectPort();
            base.OnClosing(e);
        }

    }
}
