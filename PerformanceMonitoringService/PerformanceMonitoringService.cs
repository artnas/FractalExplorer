using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NamedPipeWrapper;
using ProjektGraficzny;
using static System.Diagnostics.EventLog;

namespace PerformanceMonitoringService
{
    public partial class PerformanceMonitoringService : ServiceBase
    {
        private readonly EventLog eventLog;
        private readonly string pipeName = "ProjektGraficznyPipe";

        public PerformanceMonitoringService()
        {
            InitializeComponent();

            eventLog = new EventLog();
            if (!SourceExists("ProjektGraficznySource"))
            {
                CreateEventSource(
                    "ProjektGraficznySource", "ProjektGraficznyLog");
            }
            eventLog.Source = "ProjektGraficznySource";
            eventLog.Log = "ProjektGraficznyLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Rozpoczęcie działania");

            var client = new NamedPipeClient<PipeMessage>(pipeName);

            client.ServerMessage += delegate (NamedPipeConnection<PipeMessage, PipeMessage> conn, PipeMessage message)
            {
                string multiLineString = message.content.Replace('#', '\n');

                switch (message.messageType)
                {
                    case 0:
                        eventLog.WriteEntry($"{multiLineString}");
                        break;
                    case 1:
                        eventLog.WriteEntry($"Statystyki:\n{multiLineString}");
                        break;
                    default:
                        eventLog.WriteEntry($"Nieznany typ wiadomości: {message.messageType}, treść: {message.content}");
                        break;
                }
            };

            client.Start();
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Zakończenie działania");
        }

    }
}
