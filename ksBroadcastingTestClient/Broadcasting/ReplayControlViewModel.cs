using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ksBroadcastingNetwork;

namespace ksBroadcastingTestClient.Broadcasting
{
    public class ReplayControlViewModel : KSObservableObject
    {
        public KSRelayCommand PlayLiveReplay { get; }

        public int LiveReplaySecondsBack { get => Get<int>(); private set => Set(value); }
        public int LiveReplaySecondsPlaytime { get => Get<int>(); private set => Set(value); }
        public int CurrentSessionTime { get => Get<int>(); private set => Set(value); }

        private List<ACCUdpRemoteClient> _clients = new List<ACCUdpRemoteClient>();

        public ReplayControlViewModel()
        {
            PlayLiveReplay = new KSRelayCommand(OnLiveReplay);
            LiveReplaySecondsBack = 120;
            LiveReplaySecondsPlaytime = 10;
        }

        private void OnLiveReplay(object obj)
        {
            try
            {
                var secondsBack = Convert.ToInt32(obj);
                var requestedStartTime = CurrentSessionTime - (secondsBack * 1000);

                foreach (var client in _clients)
                {
                    client.MessageHandler.RequestInstantReplay(requestedStartTime, secondsBack * 1000.0f);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        internal void RegisterNewClient(ACCUdpRemoteClient newClient)
        {
            newClient.MessageHandler.OnRealtimeUpdate += MessageHandler_OnRealtimeUpdate;
            _clients.Add(newClient);
        }

        private void MessageHandler_OnRealtimeUpdate(string sender, ksBroadcastingNetwork.Structs.RealtimeUpdate update)
        {
            CurrentSessionTime = Convert.ToInt32(update.SessionTime.TotalMilliseconds);
        }
    }
}
