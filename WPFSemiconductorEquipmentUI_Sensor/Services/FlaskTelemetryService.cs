using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class FlaskTelemetryService : IRemoteTelemetryService
    {
        private static readonly TimeSpan SensorPostInterval = TimeSpan.FromSeconds(1);
        private readonly AppSettingsStore _settings;
        private readonly JavaScriptSerializer _serializer;
        private readonly object _sensorPostSync = new object();
        private DateTime _lastSensorPostedAtUtc = DateTime.MinValue;
        private bool _sensorPostInFlight;

        public FlaskTelemetryService(AppSettingsStore settings)
        {
            _settings = settings;
            _serializer = new JavaScriptSerializer();
        }

        public void SendSensorSnapshot(SensorSnapshotRecord snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            var nowUtc = DateTime.UtcNow;
            lock (_sensorPostSync)
            {
                if (_sensorPostInFlight || (_lastSensorPostedAtUtc != DateTime.MinValue && nowUtc - _lastSensorPostedAtUtc < SensorPostInterval))
                {
                    return;
                }

                _lastSensorPostedAtUtc = nowUtc;
                _sensorPostInFlight = true;
            }

            PostInBackground("/api/sensor", new
            {
                type = "sensor",
                created_at = nowUtc.ToString("o"),
                capturedAt = snapshot.CapturedAt.ToString("o"),
                pressure = new { raw = snapshot.PressureRaw, value = snapshot.PressureValue, unit = "bar" },
                vibration = new { raw = snapshot.VibrationRaw, value = snapshot.VibrationValue, unit = "level" },
                temperature = new { raw = snapshot.TemperatureRaw, value = snapshot.TemperatureValue, unit = "C" },
                humidity = new { raw = snapshot.HumidityRaw, value = snapshot.HumidityValue, unit = "%" },
                digitalInputs = new
                {
                    di1 = snapshot.DigitalInput1,
                    di2 = snapshot.DigitalInput2,
                    di3 = snapshot.DigitalInput3,
                    di4 = snapshot.DigitalInput4,
                    optical = snapshot.OpticalSensor,
                    inductive = snapshot.InductiveSensor
                }
            }, true);
        }

        public void SendActivityLog(ActivityLogItem log)
        {
            if (log == null)
            {
                return;
            }

            PostInBackground("/api/log", new
            {
                type = "activity_log",
                logId = log.Id,
                occurredAt = (log.OccurredAt == DateTime.MinValue ? DateTime.Now : log.OccurredAt).ToString("o"),
                time = log.Time,
                source = log.Source,
                user = log.User,
                eventText = log.Event,
                severity = log.Severity,
                saved = log.Saved
            });
        }

        private void PostInBackground(string path, object payload, bool isSensorPayload = false)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Post(path, payload);
                }
                catch
                {
                    // Telemetry must not interrupt equipment UI polling or command handling.
                }
                finally
                {
                    if (isSensorPayload)
                    {
                        lock (_sensorPostSync)
                        {
                            _sensorPostInFlight = false;
                        }
                    }
                }
            });
        }

        private void Post(string path, object payload)
        {
            var baseUrl = (_settings.ApiBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = AppSettingsStore.DefaultApiBaseUrl;
            }

            var request = (HttpWebRequest)WebRequest.Create(baseUrl + path);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            request.Timeout = 2000;
            request.ReadWriteTimeout = 2000;

            var json = _serializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
