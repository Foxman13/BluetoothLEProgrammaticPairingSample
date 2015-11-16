using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BLEProgrammaticPairingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly BluetoothLEAdvertisementWatcher _watcher = new BluetoothLEAdvertisementWatcher();
        private readonly ObservableCollection<BluetoothLEDevice> _devices = new ObservableCollection<BluetoothLEDevice>();

        public MainPage()
        {
            this.InitializeComponent();

            _watcher.ScanningMode = BluetoothLEScanningMode.Active;
            _watcher.Received += _watcher_Received;

            DeviceListBox.ItemsSource = _devices;
        }

        private async void _watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {          
            if (args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await TryAddDeviceToList(args);
                });
            }
        }

        private async Task TryAddDeviceToList(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (_devices.All(d => d.BluetoothAddress != args.BluetoothAddress))
            {
                try
                {
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                    _devices.Add(device);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _watcher.Start();
        }

        private static void OnPairingRequested(DeviceInformationCustomPairing pairing, DevicePairingRequestedEventArgs args)
        {
            args.Accept("0");
        }

        private async void PairButton_OnClick(object sender, RoutedEventArgs e)
        {
            var device = DeviceListBox.SelectedItem as BluetoothLEDevice;
            if (device != null)
            {
                device.DeviceInformation.Pairing.Custom.PairingRequested += OnPairingRequested;

                var result = await device.DeviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ProvidePin);
                if (result.Status == DevicePairingResultStatus.Failed)
                {
                    var dialog = new MessageDialog("Pairing failed");
                    await dialog.ShowAsync();
                }
                if (result.Status == DevicePairingResultStatus.Paired)
                {
                    var dialog = new MessageDialog("Pairing successful");
                    await dialog.ShowAsync();
                }
                device.DeviceInformation.Pairing.Custom.PairingRequested -= OnPairingRequested;
            }
        }

        private async void UnpairButton_OnClick(object sender, RoutedEventArgs e)
        {
            var device = DeviceListBox.SelectedItem as BluetoothLEDevice;
            if (device != null)
            {
                var result = await device.DeviceInformation.Pairing.UnpairAsync();
                if (result.Status == DeviceUnpairingResultStatus.Failed)
                {
                    var dialog = new MessageDialog("Unpairing failed");
                    await dialog.ShowAsync();
                }
                if (result.Status == DeviceUnpairingResultStatus.Unpaired)
                {
                    var dialog = new MessageDialog("Unpairing successful");
                    await dialog.ShowAsync();
                }
            }
        }
    }
}
