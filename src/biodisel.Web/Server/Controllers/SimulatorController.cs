using System;
using System.Threading;
using System.Threading.Tasks;
using biodisel.core;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("simulator")]
    public class SimulatorController: ControllerBase
    {
        private readonly Worker _worker;
        private readonly CancellationTokenSource _cancellationTokenSource;
        public SimulatorController(Worker worker)
        {
            _worker = worker;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(TimeSpan.FromHours(1));
        }


        [HttpPost("start")]
        public async Task StartSimulation()
        {
            await _worker.StartAsync(_cancellationTokenSource.Token);
        }

        [HttpPost("stop")]
        public async Task StopSimulation()
        {
            await _worker.StopAsync(new CancellationToken(true));
        }
    }
}