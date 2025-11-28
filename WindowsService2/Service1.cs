using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _timer;
        private ServiceController _controller;
        private readonly string _logPath = @"C:\Users\luuti\source\repos\WindowsService2\TestServiceLog.txt";
        private EventLog _eventLog;

        public Service1()
        {
            InitializeComponent();
            this.AutoLog = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
        }
        private void InitEventLog()
        {
            // Tạo event log source nếu chưa có
            if (!EventLog.SourceExists("DinhCD_ServiceSource"))
            {
                EventLog.CreateEventSource("DinhCD_ServiceSource", "DinhCD_ServiceLog");
            }
            _eventLog = new EventLog
            {
                Source = "DinhCD_ServiceSource",
                Log = "DinhCD_ServiceLog"
            };
        }

        private void InitTimer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += new ElapsedEventHandler(this.WriteLog);
            _timer.Interval = 1000;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }
        private void WriteLog(object sender, EventArgs e)
        {
            WLog("Service Running");
        }
        protected override void OnStart(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            if (!this.AutoLog)
            {
                InitEventLog();
            }
            _controller = new ServiceController(this.ServiceName);
            WLog("OnStart: START_PENDING");

            InitTimer();

            WLog("OnStart: RUNNING");
        }


        private void WriteLog(object sender, ElapsedEventArgs e)
        {
            WLog("Heartbeat: Service Running");
            WEvent("Event log");
        }
        private void WEvent(string message, EventLogEntryType type = EventLogEntryType.Information)
        {
            try
            {
                _eventLog.WriteEntry(message, type);
            }
            catch (Exception ex)
            {
                File.AppendAllText(_logPath, $"EVENT LOG ERROR: {ex.Message}\n");
            }
        }

        private void WLog(string content)
        {
            string status = "UNKNOWN";

            try
            {
                // Khi service dừng thì serviceController có thể null hoặc throw
                if (_controller != null)
                {
                    _controller.Refresh();
                    status = _controller.Status.ToString();
                }
            }
            catch
            {
                status = "ERROR_GETTING_STATUS";
            }

            File.AppendAllText(_logPath,
                $"{DateTime.Now:HH:mm:ss} | Status={status} | {content}{Environment.NewLine}");
        }

        protected override void OnStop()
        {
            WLog("OnStop: STOP_PENDING");

            if (_timer != null)
                _timer.Stop();

            WLog("OnStop: STOPPED");
        }

        protected override void OnPause()
        {
            WLog("OnPause: PAUSE_PENDING");
            _timer?.Stop();
            WLog("OnPause: PAUSED");
            base.OnPause();
        }

        protected override void OnContinue()
        {
            WLog("OnContinue: CONTINUE_PENDING");
            _timer?.Start();
            WLog("OnContinue: RUNNING");
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            WLog("OnShutdown: Called (Windows shutting down)");
            base.OnShutdown();
        }
        
    }
}
