using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatBackend.Hubs
{
    /// <summary>
    /// นี่คือ WebSocket Hub หลักของเรา (Phase 3)
    /// เราต้องเพิ่ม [Authorize] เพื่อบังคับให้เฉพาะผู้ใช้ที่ Login แล้ว (มี JWT)
    /// เท่านั้นที่สามารถเชื่อมต่อ WebSocket นี้ได้
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        // เมื่อ Client เชื่อมต่อสำเร็จ
        public override async Task OnConnectedAsync()
        {
            // (ในอนาคต) เราจะเพิ่ม Logic ที่นี่
            // เช่น การดึง User ID มา Map กับ ConnectionId
            // var userId = Context.UserIdentifier;
            
            // (ตัวอย่าง) ส่งข้อความกลับไปหาคนที่เพิ่งต่อเข้ามา
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome! You are connected.");

            await base.OnConnectedAsync();
        }

        // เมื่อ Client หลุดการเชื่อมต่อ
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            // (ในอนาคต) เราจะเพิ่ม Logic การ cleanup ที่นี่
            await base.OnDisconnectedAsync(exception);
        }

        // (นี่เป็นแค่ตัวอย่าง Method ที่ Client เรียกได้)
        // public async Task SendMessage(string message)
        // {
        //     var userName = Context.User.Identity.Name;
        //     await Clients.All.SendAsync("ReceiveMessage", userName, message);
        // }
    }
}