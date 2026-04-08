using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GMPS.API.Hubs
{
    [Authorize]
    public class CommentHub : Hub
    {
        public Task JoinOrderCommentGroup(int orderId)
        {
            if (orderId <= 0)
            {
                throw new HubException("OrderId phải lớn hơn 0.");
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, GetOrderGroupName(orderId));
        }

        public Task LeaveOrderCommentGroup(int orderId)
        {
            if (orderId <= 0)
            {
                throw new HubException("OrderId phải lớn hơn 0.");
            }

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetOrderGroupName(orderId));
        }

        public static string GetOrderGroupName(int orderId) => $"order-{orderId}-comments";
    }

}
