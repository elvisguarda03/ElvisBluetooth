using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace BluetoothConnectTest
{
    public class DnaBluetoothLEAdvertisementWatcher
    {
        #region Private Members

        private readonly BluetoothLEAdvertisementWatcher mWatcher;

        private readonly Dictionary<ulong, DnaBluetoothLEDevice> mDiscoveredDevices =
            new Dictionary<ulong, DnaBluetoothLEDevice>();

        private object mThreadLock = new object();

        #endregion

        public bool Listening =>
            mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        public IReadOnlyCollection<DnaBluetoothLEDevice> DiscoveredDevices
        {
            get
            {
                lock (mThreadLock)
                {
                    return mDiscoveredDevices.Values.ToList().AsReadOnly();
                }
            }

        }

        #region Event
        public event Action StartedListening = () => { };
        public event Action StoppedListening = () => { };
        public event Action<DnaBluetoothLEDevice> NewDeviceDiscovered = (device) => { };
        public event Action<DnaBluetoothLEDevice> DeviceNameChanged = (device) => { };
        public event Action<DnaBluetoothLEDevice> DeviceDiscovered = (device) => { };
        #endregion

        public DnaBluetoothLEAdvertisementWatcher()
        {
            mWatcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            mWatcher.Received += WatcherAdvertisementReceived;
            mWatcher.Stopped += (watcher, e) => { StoppedListening(); };
        }

        public void StartListening()
        {
            if (Listening)
                return;
            mWatcher.Start();
            StartedListening();
        }

        private void WatcherAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            DnaBluetoothLEDevice device = null;

            var newDiscovered = !mDiscoveredDevices.ContainsKey(args.BluetoothAddress);

            var nameChanged = 
                !newDiscovered && 
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                mDiscoveredDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;
            lock (mThreadLock)
            {
                //Get the name of the device
                var name = args.Advertisement.LocalName;

                if (!string.IsNullOrEmpty(name) && !newDiscovered)
                    name = mDiscoveredDevices[args.BluetoothAddress].Name;

                device = new DnaBluetoothLEDevice
                (
                    address: args.BluetoothAddress,
                    name: name,
                    broadcastTime: args.Timestamp,
                    rssi: args.RawSignalStrengthInDBm
                );

                //Add/Update the device in the dictionary
                mDiscoveredDevices[args.BluetoothAddress] = device;
            }

            DeviceDiscovered(device);
            if (nameChanged)
                DeviceNameChanged(device);
            
            if (newDiscovered)
                NewDeviceDiscovered(device);
        }

        public void StopListening()
        {
            if (!Listening)
                return;
            mWatcher.Stop();
            
            lock (mThreadLock)
            {
                mDiscoveredDevices.Clear();
            }
        }
    }
}
