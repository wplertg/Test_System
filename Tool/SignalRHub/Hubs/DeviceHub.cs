using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.SignalRHub.Hubs
{
    public class DeviceHub: Hub
    {
        /// <summary>
        /// 前端连接成功
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 前端订阅某个设备
        /// </summary>
        public async Task Subscribe(string deviceName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, deviceName);
        }

        /// <summary>
        /// 前端取消订阅
        /// </summary>
        public async Task UnSubscribe(string deviceName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceName);
        }
    }
}
