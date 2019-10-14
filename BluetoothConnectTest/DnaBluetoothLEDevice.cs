using System;

namespace BluetoothConnectTest
{
    public class DnaBluetoothLEDevice
    {
        #region Public Properties
        public DateTimeOffset BroadcastTime { get; }
        public ulong Address { get; }
        public string Name { get; }
        public short SignalStrengthInDB { get; }
        #endregion

        #region Constructor

        public DnaBluetoothLEDevice(ulong address, string name, short rssi, DateTimeOffset broadcastTime)
        {
            Address = address;
            Name = name;
            SignalStrengthInDB = rssi;
            BroadcastTime = broadcastTime;
        }

        #endregion

        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? "[No Name]" : Name) } {Address} ({SignalStrengthInDB})";
        }
    }
}
