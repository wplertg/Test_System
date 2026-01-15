using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.SignalRHub.Hubs;

namespace Tools.SignalRHub
{
    public class DeviceNotifyService
    {
        private readonly IHubContext<DeviceHub> _hub;

        public DeviceNotifyService(IHubContext<DeviceHub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// 推送设备消息（按设备名分组）
        /// </summary>
        public Task PushAsync(string deviceName, object data)
        {
            return _hub.Clients
                .Group(deviceName)
                .SendAsync("DeviceMessage", data);
        }

        /// <summary>
        /// 广播
        /// </summary>
        public Task BroadcastAsync(object data)
        {
            return _hub.Clients.All.SendAsync("Broadcast", data);
        }
    }
}
