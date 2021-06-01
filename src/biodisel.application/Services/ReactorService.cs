using System.Threading.Tasks;
using biodisel.domain.Interfaces;
using Microsoft.Extensions.Logging;
using biodisel.domain.Models;
using biodisel.domain.Models.Enums;
using System;
using biodisel.queues.Publishers;

namespace biodisel.application.Services
{
    public class ReactorService : IReactorService
    {
        private decimal Volume => OilVolume + EthanolVolume + NaOHVolume;
        private decimal NaOHVolume = 0;
        private decimal EthanolVolume = 0;
        private decimal Cycles = 0;
        private decimal OilVolume = 0;
        private readonly ILogger<ReactorService> _logger;
        private readonly ReactorPublisher _publisher;
        private readonly VolumePublisher _publisherReport;
        public bool IsResting { get; private set; }

        public ReactorService(ILogger<ReactorService> logger, ReactorPublisher publisher, VolumePublisher publisherReport)
        {
            _logger = logger;
            _publisher = publisher;
            _publisherReport = publisherReport;
        }

        public async Task Fill(ResidueMessage residueMessage)
        {
            switch (residueMessage.IntegrationSystem)
            {
                case IntegrationSystem.Oil:
                    OilVolume += residueMessage.Volume;
                    _logger.LogInformation($"{OilVolume} oil volume total, received {residueMessage.Volume}");
                    break;
                case IntegrationSystem.NaOH:
                    NaOHVolume += residueMessage.Volume;
                    _logger.LogInformation($"{NaOHVolume} NaOH volume total, received {residueMessage.Volume}");
                    break;
                case IntegrationSystem.Ethanol:
                    EthanolVolume += residueMessage.Volume;
                    _logger.LogInformation($"{EthanolVolume} Ethanol volume total, received {residueMessage.Volume}");
                    break;
                default:
                    break;
            }

            SendVolumesReport();

            if (CanBeActivated())
            {
                await Activate();
            };
        }

        private void SendVolumesReport(){
            _publisherReport.SendVolumeReport(IntegrationSystem.Reactor,Volume);
            _publisherReport.SendVolumeReport(IntegrationSystem.Oil,OilVolume);
            _publisherReport.SendVolumeReport(IntegrationSystem.Ethanol,EthanolVolume);
            _publisherReport.SendVolumeReport(IntegrationSystem.NaOH,NaOHVolume);
        }

        private void SendCycleReport()
        {
            _publisherReport.SendVolumeReport(IntegrationSystem.ReactorCycle, Cycles);
        }

        public async Task SendResidueToDecanter(decimal residueVolume)
        {
            _logger.LogWarning($"Volume {residueVolume} to send");
            var message = new ResidueMessage{
                Volume= residueVolume,
                IntegrationSystem= IntegrationSystem.Reactor
            };
            await Task.Run(() => _publisher.Send(message));
            _logger.LogWarning($"Volume sent to decanter");
            OilVolume = Math.Max(OilVolume - residueVolume * 0.2m,0);
            NaOHVolume = Math.Max(NaOHVolume - residueVolume * 0.2m,0);
            EthanolVolume =  Math.Max(EthanolVolume - residueVolume * 0.4m,0);
        }

        public async Task Rest(double volumeSent)
        {
            _logger.LogDebug($"Rest wil be of {TimeSpan.FromSeconds(5 * volumeSent / 3).TotalSeconds}");
            _logger.LogCritical("Starting to rest!");
            IsResting = true;
            await Task.Delay( TimeSpan.FromSeconds(5 * volumeSent / 3) );
            IsResting = false;
            _logger.LogCritical("Rested!");
        }

        public async Task Activate()
        {
            _logger.LogInformation("Activation being done");
            var i = Volume;
            decimal volumeSent = 0;
            for (; i >= 0; i -= 5)
            {
                await SendResidueToDecanter(5);
            SendVolumesReport();
            await Task.Delay(TimeSpan.FromSeconds(1));
                volumeSent += 5;
            }
            _logger.LogInformation($"Residue sent! Actual volume {Volume}");
            Cycles += 1;
            SendCycleReport();
            Rest(Convert.ToDouble(volumeSent));
        }

        public bool CanBeActivated()
        {
            if (OilVolume >= 1.25m && NaOHVolume >= 1.25m && EthanolVolume >= 2.5m && !IsResting)
                return true;
            return false;
        }
    }
}