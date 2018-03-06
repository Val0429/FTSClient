using System;
using System.Timers;
using WebSocketSharp;
using Constants;
using Newtonsoft.Json.Linq;

namespace WebSocketListener
{
    public class Listener
    {
        public String URI { get; set; }
        public String Query { get; set; }

        private LogFile LogFile = new LogFile();

        public event EventHandler<EventArgs<Boolean>> OnConnectionStateChange;
        protected void RaiseOnConnectionStateChange(Boolean status)
        {
            if (OnConnectionStateChange != null)
                OnConnectionStateChange(this, new EventArgs<Boolean>(status));
        }

        public event EventHandler<EventArgs<String>> OnMessageReceive;
        protected void RaiseOnMessageReceive(String Message)
        {
            if (OnMessageReceive != null)
                OnMessageReceive(this, new EventArgs<String>(Message));
        }

        public event EventHandler<EventArgs<String>> OnDropMessage;
        protected void RaiseOnDropMessage(String Message)
        {
            if (OnDropMessage != null)
                OnDropMessage(this, new EventArgs<String>(Message));
        }

        public Listener() { }
        public Listener(String uri, String query)
        {
            URI = uri;
            Query = query;
        }

        private Timer _timer = new Timer();
        private WebSocket _listenerClient;
        public Boolean Start()
        {

            if (String.IsNullOrEmpty(URI) || String.IsNullOrEmpty(Query))
                return false;

            _listenerClient = new WebSocket(URI);

            _listenerClient.OnMessage -= _listenerClient_OnMessage;
            _listenerClient.OnMessage += _listenerClient_OnMessage;

            _listenerClient.OnError -= _listenerClient_OnError;
            _listenerClient.OnError += _listenerClient_OnError;

            _listenerClient.OnClose -= _listenerClient_OnClose;
            _listenerClient.OnClose += _listenerClient_OnClose;

            _listenerClient.Connect();

            LogFile.WriteToLogFile("_listenerClient IsAlive " + (_listenerClient.IsAlive ? "true" : "false"));

            if (_listenerClient.IsAlive)
            {
                LogFile.WriteToLogFile("_listenerClient Query " + Query);

                RaiseOnConnectionStateChange(_listenerClient.IsAlive);

                _listenerClient.Send(Query);

                _timer.Interval = 1000;
                _timer.Elapsed += _timer_Elapsed;

                _timer.Enabled = false;
                _timer.Enabled = true;
                return true;
            }
            return false;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            if (!_listenerClient.IsAlive)
            {
                _listenerClient.Close();
                _listenerClient = null;

                Start();
            }
            _timer.Enabled = true;
        }

        private void _listenerClient_OnClose(object sender, CloseEventArgs e)
        {
            LogFile.WriteToLogFile("_listenerClient OnClose " + e.Reason);

            RaiseOnConnectionStateChange(_listenerClient.IsAlive);
        }

        private void _listenerClient_OnError(object sender, ErrorEventArgs e)
        {
            LogFile.WriteToLogFile("_listenerClient OnError " + e.Exception);

            RaiseOnConnectionStateChange(_listenerClient.IsAlive);
        }

        private void _listenerClient_OnMessage(object sender, MessageEventArgs e)
        {
            LogFile.WriteToLogFile(e.Data);

            if((e.Data.IndexOf("start") >= 0) || (e.Data.IndexOf("error") >= 0) || (e.Data.IndexOf("stop") >= 0))
            { 
                return;
            }

            JObject _jsonObj = JObject.Parse(e.Data);

            var w = Convert.ToDouble(_jsonObj["facewidth"].ToString());
            var h = Convert.ToDouble(_jsonObj["faceheight"].ToString());

            if ( (w >=10) || (h >= 10))
                RaiseOnMessageReceive(e.Data);
            else
                RaiseOnDropMessage(e.Data);
        }

        public void Stop()
        {
            OnMessageReceive = null; 
            _listenerClient.OnMessage -= _listenerClient_OnMessage;
            _listenerClient.Close();
            _listenerClient = null;
        }
    }
}
