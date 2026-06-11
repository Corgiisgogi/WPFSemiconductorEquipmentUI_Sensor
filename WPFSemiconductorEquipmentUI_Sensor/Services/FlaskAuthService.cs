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

        public bool CheckHealth()
        {
            try
            {
                var request = CreateRequest("/api/health", "GET");
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                // 헬스 체크는 연결 실패를 예외가 아니라 "비정상"으로 보고한다.
                return false;
            }
        }

        private T Post<T>(string path, object payload)
        {
            var request = CreateRequest(path, "POST");
            var json = _serializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = bytes.Length;

            // 요청 전송(연결) 단계의 WebException도 응답 단계와 동일하게 친화적인 한국어 메시지로 감싼다.
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException(ReadErrorMessage(ex), ex);
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

        private string ReadErrorMessage(WebException ex)
        {
            if (ex.Response == null)
            {
                return "Flask API 서버에 연결할 수 없습니다. Settings의 Flask API URL과 서버 실행 상태를 확인하세요. (" + ex.Message + ")";
            }

            var response = ex.Response as HttpWebResponse;
            var responseStream = ex.Response.GetResponseStream();
            if (responseStream == null)
            {
                return response == null
                    ? "Flask API 요청에 실패했습니다."
                    : "Flask API 요청에 실패했습니다. HTTP " + (int)response.StatusCode + " " + response.StatusDescription;
            }

            using (var stream = responseStream)
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var body = reader.ReadToEnd();
                var message = ExtractJsonMessage(body);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    return response == null
                        ? "Flask API 요청에 실패했습니다."
                        : "Flask API 요청에 실패했습니다. HTTP " + (int)response.StatusCode + " " + response.StatusDescription;
                }

                return "Flask API 요청에 실패했습니다. 서버 응답을 확인하세요.";
            }
        }

        private string ExtractJsonMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                var payload = _serializer.Deserialize<AuthErrorResponse>(body);
                if (payload != null && !string.IsNullOrWhiteSpace(payload.message))
                {
                    return payload.message;
                }
            }
            catch
            {
            }

            return null;
        }

        private sealed class AuthErrorResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string approvalStatus { get; set; }
            public string userId { get; set; }
        }
    }
}
