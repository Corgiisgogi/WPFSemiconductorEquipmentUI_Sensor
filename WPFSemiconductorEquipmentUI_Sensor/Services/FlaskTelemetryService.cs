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
        private readonly AppSettingsStore _settings;
        private readonly JavaScriptSerializer _serializer;

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

            PostInBackground(new
            {
                type = "sensor",
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
            });
        }

        public void SendActivityLog(ActivityLogItem log)
        {
            if (log == null)
            {
                return;
            }

            PostInBackground(new
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

        private void PostInBackground(object payload)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Post(payload);
                }
                catch
                {
                    // Telemetry must not interrupt equipment UI polling or command handling.
                }
            });
        }

        private void Post(object payload)
        {
            var baseUrl = (_settings.ApiBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = AppSettingsStore.DefaultApiBaseUrl;
            }

            var request = (HttpWebRequest)WebRequest.Create(baseUrl + "/api/sensor");
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
