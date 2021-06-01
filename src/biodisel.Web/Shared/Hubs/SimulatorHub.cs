using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using biodisel.domain.Models.Enums;

namespace biodisel.Web.Shared.Hubs
{
    public class SimulatorHub : Hub
    {
        public async Task SendMessage(IntegrationSystem integrationSystem, decimal volume)
        {
            await Clients.All.SendAsync("ReceiveMessage",integrationSystem,volume);
        }
    }
}