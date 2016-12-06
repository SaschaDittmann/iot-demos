using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace DeviceControlConsole
{
    class Program
    {
        private static readonly string EventHubName = System.Configuration.ConfigurationManager.AppSettings["EventHubName"];
        private static readonly string EventHubConnectionString = System.Configuration.ConfigurationManager.AppSettings["EventHubConnectionString"];
        private static readonly string IoTHubConnectionString = System.Configuration.ConfigurationManager.AppSettings["IoTHubServiceConnectionsString"];
        private static readonly List<DeviceAlert> DeviceAlerts = new List<DeviceAlert>();
        private static ServiceClient _serviceClient;
        private static EventHubReader _eventHubReader;

        static void Main(string[] args)
        {
            _eventHubReader = new EventHubReader("fancontrol", HandleDeviceEvents);
            _eventHubReader.Run(EventHubConnectionString, EventHubName, "");
            using (new Timer(HandleTimerEvent, null, 0, 30000))
            {
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
            }
        }

        private static void HandleTimerEvent(object obj)
        {
            foreach (var deviceAlert in DeviceAlerts)
            {
                if (DateTime.UtcNow.Subtract(deviceAlert.LastUpdate).TotalSeconds > 30
                    && deviceAlert.FanOn)
                {
                    PushPromo(deviceAlert.DeviceId, new DeviceMessage
                    {
                        Name = "TurnFanOff"
                    }).Wait();
                    deviceAlert.FanOn = false;
                    deviceAlert.LastUpdate = DateTime.UtcNow;
                }
            }
        }

        static void HandleDeviceEvents(string message)
        {
            var deviceEvents = JsonConvert.DeserializeObject<List<DeviceEvent>>(message);
            foreach (var deviceEvent in deviceEvents)
            {
                if (deviceEvent.RuleOutput == "AlarmTemp")
                {
                    var alert = DeviceAlerts.FirstOrDefault((a) => a.DeviceId == deviceEvent.DeviceId);
                    if (alert == null)
                    {
                        alert = new DeviceAlert
                        {
                            DeviceId = deviceEvent.DeviceId,
                            FanOn = true,
                            LastUpdate = DateTime.UtcNow
                        };
                        DeviceAlerts.Add(alert);
                    }
                    else
                    {
                        alert.FanOn = true;
                        alert.LastUpdate = DateTime.UtcNow;
                    }
                    PushPromo(deviceEvent.DeviceId, new DeviceMessage
                    {
                        Name = "TurnFanOn"
                    }).Wait();
                }
            }
        }

        static async Task PushPromo(string deviceId, DeviceMessage promoPackage)
        {
            // Create a Service Client instance provided the _IoTHubConnectionString
            _serviceClient = ServiceClient.CreateFromConnectionString(IoTHubConnectionString);

            var promoPackageJson = JsonConvert.SerializeObject(promoPackage);

            Console.WriteLine($"Sending Message to {deviceId}:");
            Console.WriteLine(promoPackageJson);

            var commandMessage = new Message(Encoding.ASCII.GetBytes(promoPackageJson));

            // Send the command
            await _serviceClient.SendAsync(deviceId, commandMessage);
        }
    }
}
