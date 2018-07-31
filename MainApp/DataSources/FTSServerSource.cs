using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;

namespace Tencent.DataSources {
    public class FTSServerSource : DependencyObject {
        public string sessionId { get; private set; }
        public OutputReadConfig config { get; private set; }
        public Floors[] floors { get; private set; }
        public Cameras[] cameras { get; private set; }

        public string ip { get; private set; }
        public string port { get; private set; }

        private string Host { get; set; }

        public class OutputLogin {
            public string sessionId { get; set; }
        }

        public async Task Login(string ip, string port, string account, string password) {
            this.ip = ip;
            this.port = port;
            Host = string.Format("http://{0}:{1}", ip, port);
            /// do login
            var uri = string.Format("{0}/users/login", Host);
            using (var client = new HttpClient()) {
                var byteContent = new StringContent(string.Format("{{ \"username\": \"{0}\", \"password\": \"{1}\" }}", account, password));
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var result = await client.PostAsync(uri, byteContent);
                var resultStr = await result.Content.ReadAsStringAsync();
                var jsonSerializerx = new JavaScriptSerializer();
                var user = jsonSerializerx.Deserialize<OutputLogin>(resultStr);
                var sessionId = user.sessionId;
                this.sessionId = sessionId;
            }

            await this.Prepare();
        }

        /// ReadConfig //////
        public class OutputReadCMSConfig {
            public string ip { get; set; }
            public int port { get; set; }
            public string account { get; set; }
            public string password { get; set; }
        }
        public class OutputReadFRSConfig {
            public string ip { get; set; }
            public int port { get; set; }
            public int wsport { get; set; }
            public string account { get; set; }
            public string password { get; set; }
        }
        public class OutputReadFTSGroupInfoConfig {
            public string name { get; set; }
            public string color { get; set; }
            public string glowcolor { get; set; }
        }
        public class OutputReadFTSConfig {
            public int searchDurationSeconds { get; set; }
            public int possibleCompanionDurationSeconds { get; set; }
            public OutputReadFTSGroupInfoConfig[] groupInfo { get; set; }
        }
        public class OutputReadConfig {
            public OutputReadCMSConfig cms { get; set; }
            public OutputReadFRSConfig frs { get; set; }
            public OutputReadFTSConfig fts { get; set; }
        }
        /////////////////////

        public async Task ReadConfig() {
            var uri = string.Format("{0}/config?sessionId={1}", Host, sessionId);
            using (var client = new HttpClient()) {
                var result = await client.GetAsync(uri);
                var resultStr = await result.Content.ReadAsStringAsync();
                var jsonSerializerx = new JavaScriptSerializer();
                this.config = jsonSerializerx.Deserialize<OutputReadConfig>(resultStr);
            }
        }

        /// Read Floors ///////////
        public class Floors {
            public string objectId { get; set; }
            public string name { get; set; }
            public int floor { get; set; }
            public string image { get; set; }
        }
        public class OutputReadFloors {
            public Floors[] results { get; set; }
        }
        ///////////////////////////

        public async Task ReadFloors() {
            var uri = string.Format("{0}/floors?sessionId={1}&all=true", Host, sessionId);
            using (var client = new HttpClient()) {
                var result = await client.GetAsync(uri);
                var resultStr = await result.Content.ReadAsStringAsync();
                var jsonSerializerx = new JavaScriptSerializer();
                this.floors = jsonSerializerx.Deserialize<OutputReadFloors>(resultStr).results;
            }
        }

        /// Read Cameras //////////
        public class Cameras {
            public string objectId { get; set; }
            public Floors floor { get; set; }
            public string name { get; set; }
            public string sourceId { get; set; }
            public double x { get; set; }
            public double y { get; set; }
            public double angle { get; set; }
        }
        public class OutputReadCameras {
            public Cameras[] results { get; set; }
        }
        ///////////////////////////

        public async Task ReadCameras() {
            var uri = string.Format("{0}/cameras?sessionId={1}&all=true", Host, sessionId);
            using (var client = new HttpClient()) {
                var result = await client.GetAsync(uri);
                var resultStr = await result.Content.ReadAsStringAsync();
                var jsonSerializerx = new JavaScriptSerializer();
                this.cameras = jsonSerializerx.Deserialize<OutputReadCameras>(resultStr).results;
            }
        }

        private async Task Prepare() {
            /// 1) Read Config
            await this.ReadConfig();
            /// 2) Get Floors
            await this.ReadFloors();
            /// 3) Get Cameras
            await this.ReadCameras();
        }
    }
}
