using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class FlaskAuthService : IAuthService
    {
        private readonly AppSettingsStore _settings;
        private readonly JavaScriptSerializer _serializer;

        public FlaskAuthService(AppSettingsStore settings)
        {
            _settings = settings;
            _serializer = new JavaScriptSerializer();
        }

        public AuthResult Login(string userId, string password)
        {
            return Post<AuthResult>("/api/auth/login", new
            {
                userId = userId,
                password = password
            });
        }

        public RegisterResult Register(string userId, string password, string displayName)
        {
            return Post<RegisterResult>("/api/auth/register", new
            {
                userId = userId,
                password = password,
                displayName = displayName
            });
        }

        public AuthResult GetStatus(string userId)
        {
            return Get<AuthResult>("/api/users/" + Uri.EscapeDataString(userId) + "/status");
        }

        private T Get<T>(string path)
        {
            var request = CreateRequest(path, "GET");
            return ReadResponse<T>(request);
        }

        private T Post<T>(string path, object payload)
        {
            var request = CreateRequest(path, "POST");
            var json = _serializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            return ReadResponse<T>(request);
        }

        private HttpWebRequest CreateRequest(string path, string method)
        {
            var baseUrl = (_settings.ApiBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = AppSettingsStore.DefaultApiBaseUrl;
            }

            var request = (HttpWebRequest)WebRequest.Create(baseUrl + path);
            request.Method = method;
            request.Accept = "application/json";
            request.Timeout = 5000;
            request.ReadWriteTimeout = 5000;
            return request;
        }

        private T ReadResponse<T>(HttpWebRequest request)
        {
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    return _serializer.Deserialize<T>(json);
                }
            }
            catch (WebException ex)
            {
                var message = ReadErrorMessage(ex);
                throw new InvalidOperationException(message, ex);
            }
        }

        private static string ReadErrorMessage(WebException ex)
        {
            if (ex.Response == null)
            {
                return "Flask API connection failed: " + ex.Message;
            }

            using (var stream = ex.Response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var body = reader.ReadToEnd();
                return string.IsNullOrWhiteSpace(body) ? "Flask API request failed: " + ex.Message : body;
            }
        }
    }
}
