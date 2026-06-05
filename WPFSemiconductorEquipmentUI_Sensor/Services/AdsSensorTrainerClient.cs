using System;
using System.Runtime.InteropServices;
using System.Threading;
using TwinCAT.Ads;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class AdsSensorTrainerClient : ITrainerClient
    {
        private const int DefaultAdsPort = 851;
        private const int AdsTimeoutMilliseconds = 1000;
        private const string DigitalInputVariable = "GVL.NX_ID5342";
        private const string DigitalOutputVariable = "GVL.NX_OD5121";
        private const string AnalogInputVariable = "GVL.NX_AD4203";

        private bool _disposed;

        public bool IsConnected
        {
            get { return !_disposed; }
        }

        public void Connect()
        {
            ThrowIfDisposed();
        }

        public SensorTrainerSnapshot ReadSnapshot()
        {
            ThrowIfDisposed();

            using (var adsClient = new TcAdsClient())
            {
                var digitalInputHandle = 0;
                var analogInputHandle = 0;

                try
                {
                    adsClient.Timeout = AdsTimeoutMilliseconds;
                    adsClient.Connect(DefaultAdsPort);
                    digitalInputHandle = adsClient.CreateVariableHandle(DigitalInputVariable);
                    analogInputHandle = adsClient.CreateVariableHandle(AnalogInputVariable);

                    var rawInput = (DigitalInputRaw)adsClient.ReadAny(digitalInputHandle, typeof(DigitalInputRaw));
                    var analogBytes = (byte[])adsClient.ReadAny(analogInputHandle, typeof(byte[]), new[] { 16 });

                    return new SensorTrainerSnapshot(
                        BitConverter.ToInt16(analogBytes, 0),
                        BitConverter.ToInt16(analogBytes, 2),
                        BitConverter.ToInt16(analogBytes, 4),
                        BitConverter.ToInt16(analogBytes, 6),
                        IsBitSet(rawInput.Bits, 0),
                        IsBitSet(rawInput.Bits, 1),
                        IsBitSet(rawInput.Bits, 2),
                        IsBitSet(rawInput.Bits, 3),
                        IsBitSet(rawInput.Bits, 4),
                        IsBitSet(rawInput.Bits, 5));
                }
                finally
                {
                    TryDeleteHandle(adsClient, digitalInputHandle);
                    TryDeleteHandle(adsClient, analogInputHandle);
                }
            }
        }

        public void SetRunningLamp(bool isOn)
        {
            ThrowIfDisposed();
            SetDigitalOutputBit(0, isOn);
        }

        public void PulseDigitalOutput(int bitIndex, int durationMilliseconds)
        {
            ThrowIfDisposed();

            if (bitIndex < 1 || bitIndex > 15)
            {
                throw new ArgumentOutOfRangeException("bitIndex", "Command output bits must be between 1 and 15. Bit 0 is reserved for the running lamp.");
            }

            if (durationMilliseconds < 1 || durationMilliseconds > 5000)
            {
                throw new ArgumentOutOfRangeException("durationMilliseconds", "Pulse duration must be between 1 and 5000 milliseconds.");
            }

            SetDigitalOutputBit(bitIndex, true);
            Thread.Sleep(durationMilliseconds);
            SetDigitalOutputBit(bitIndex, false);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private void SetDigitalOutputBit(int bitIndex, bool isOn)
        {
            using (var adsClient = new TcAdsClient())
            {
                var digitalOutputHandle = 0;

                try
                {
                    adsClient.Timeout = AdsTimeoutMilliseconds;
                    adsClient.Connect(DefaultAdsPort);
                    digitalOutputHandle = adsClient.CreateVariableHandle(DigitalOutputVariable);

                    var rawOutput = (DigitalOutputRaw)adsClient.ReadAny(digitalOutputHandle, typeof(DigitalOutputRaw));
                    if (isOn)
                    {
                        rawOutput.Bits = (ushort)(rawOutput.Bits | (1 << bitIndex));
                    }
                    else
                    {
                        rawOutput.Bits = (ushort)(rawOutput.Bits & ~(1 << bitIndex));
                    }

                    adsClient.WriteAny(digitalOutputHandle, rawOutput);
                }
                finally
                {
                    TryDeleteHandle(adsClient, digitalOutputHandle);
                }
            }
        }

        private static bool IsBitSet(ushort value, int bit)
        {
            return (value & (1 << bit)) != 0;
        }

        private static void TryDeleteHandle(TcAdsClient adsClient, int handle)
        {
            if (handle == 0 || adsClient == null)
            {
                return;
            }

            try
            {
                adsClient.DeleteVariableHandle(handle);
            }
            catch
            {
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DigitalInputRaw
        {
            public ushort Bits;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DigitalOutputRaw
        {
            public ushort Bits;
        }
    }
}
