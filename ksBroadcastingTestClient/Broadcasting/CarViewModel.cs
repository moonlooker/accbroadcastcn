using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ksBroadcastingNetwork;
using ksBroadcastingNetwork.Structs;

namespace ksBroadcastingTestClient.Broadcasting
{
    public class CarViewModel : KSObservableObject
    {
        public int CarIndex { get; }
        public int RaceNumber { get => Get<int>(); private set => Set(value); }
        public int CarModelEnum { get => Get<int>(); private set => Set(value); }
        public string TeamName { get => Get<string>(); private set => Set(value); }
        public string TeamCarName { get => Get<string>(); private set => Set(value); }
        public string CarModelName { get => Get<string>(); private set => Set(value); }
        public int CupCategoryEnum { get => Get<int>(); private set => Set(value); }
        public DriverViewModel CurrentDriver { get => Get<DriverViewModel>(); private set => Set(value); }

        public IEnumerable<DriverViewModel> InactiveDrivers { get { return Drivers.Where(x => x.DriverIndex != CurrentDriver?.DriverIndex); } }
        public ObservableCollection<DriverViewModel> Drivers { get; } = new ObservableCollection<DriverViewModel>();

        public CarLocationEnum CarLocation { get => Get<CarLocationEnum>(); private set => Set(value); }
        public int Delta { get => Get<int>(); private set => Set(value); }
        public string DeltaString { get => Get<string>(); private set => Set(value); }
        public Brush DeltaColor { get => Get<Brush>(); private set => Set(value); }
        public int Gear { get => Get<int>(); private set => Set(value); }
        public int Kmh { get => Get<int>(); private set => Set(value); }
        public int Position { get => Get<int>(); private set => Set(value); }
        public int CupPosition { get => Get<int>(); private set => Set(value); }
        public int TrackPosition { get => Get<int>(); private set => Set(value); }
        public float SplinePosition { get => Get<float>(); private set => Set(value); }
        public float WorldX { get => Get<float>(); private set => Set(value); }
        public float WorldY { get => Get<float>(); private set => Set(value); }
        public float Yaw { get => Get<float>(); private set => Set(value); }
        public int Laps { get => Get<int>(); private set => Set(value); }
        public LapViewModel BestLap { get => Get<LapViewModel>(); private set => Set(value); }
        public LapViewModel LastLap { get => Get<LapViewModel>(); private set => Set(value); }
        public LapViewModel CurrentLap { get => Get<LapViewModel>(); private set => Set(value); }
        public string LocationHint { get => Get<string>(); private set => Set(value); }

        public Brush RowForeground { get => Get<Brush>(); private set => Set(value); }
        public Brush RowBackground { get => Get<Brush>(); private set => Set(value); }


        public CarViewModel(ushort carIndex)
        {
            CarIndex = carIndex;
        }

        internal void Update(CarInfo carUpdate)
        {
            RaceNumber = carUpdate.RaceNumber;
            CarModelEnum = carUpdate.CarModelType;
            TeamName = carUpdate.TeamName;
            TeamCarName = carUpdate.TeamCarName;
            CarModelName = carUpdate.DisplayName;
            CupCategoryEnum = carUpdate.CupCategory;

            foreach (var driverUpdate in carUpdate.Drivers)
            {
                var driverVM = Drivers.SingleOrDefault(x => x.DriverIndex == driverUpdate.DriverIndex);
                if (driverVM == null)
                {
                    // This one is new!
                    driverVM = new DriverViewModel(driverUpdate.DriverIndex);
                    Drivers.Add(driverVM);
                    NotifyUpdate(nameof(InactiveDrivers));
                }

                driverVM.Update(driverUpdate);
            }
        }

        internal void Update(RealtimeCarUpdate carUpdate)
        {
            if (carUpdate.CarIndex != CarIndex)
            {
                System.Diagnostics.Debug.WriteLine($"Wrong {nameof(RealtimeCarUpdate)}.CarIndex {carUpdate.CarIndex} for {nameof(CarViewModel)}.CarIndex {CarIndex}");
                return;
            }

            if (CurrentDriver?.DriverIndex != carUpdate.DriverIndex)
            {
                // The driver has changed!
                CurrentDriver = Drivers.SingleOrDefault(x => x.DriverIndex == carUpdate.DriverIndex);
                NotifyUpdate(nameof(InactiveDrivers));
            }

            CarLocation = carUpdate.CarLocation;
            Delta = carUpdate.Delta;
            DeltaString = $"{TimeSpan.FromMilliseconds(Delta):ss\\.f}";
            if (Delta < -100)
                DeltaColor = Brushes.Green;
            else if (Delta > 100)
                DeltaColor = Brushes.Red;
            else
                DeltaColor = null;

            Gear = carUpdate.Gear + 1;
            Kmh = carUpdate.Kmh;
            Position = carUpdate.Position;
            CupPosition = carUpdate.CupPosition;
            TrackPosition = carUpdate.TrackPosition;
            SplinePosition = carUpdate.SplinePosition;
            WorldX = carUpdate.WorldPosX;
            WorldY = carUpdate.WorldPosY;
            Yaw = carUpdate.Yaw;
            Laps = carUpdate.Laps;

            if(BestLap == null && carUpdate.BestSessionLap != null)
                BestLap = new LapViewModel();
            if (carUpdate.BestSessionLap != null)
                BestLap.Update(carUpdate.BestSessionLap);

            if (LastLap == null && carUpdate.LastLap != null)
                LastLap = new LapViewModel();
            if (carUpdate.LastLap != null)
                LastLap.Update(carUpdate.LastLap);

            if (CurrentLap == null && carUpdate.CurrentLap != null)
                CurrentLap = new LapViewModel();
            if (carUpdate.CurrentLap != null)
                CurrentLap.Update(carUpdate.CurrentLap);

            // The location hint will combine stuff like pits, in/outlap
            if (CarLocation == CarLocationEnum.PitEntry)
                LocationHint = "InPit";
            else if (CarLocation == CarLocationEnum.Pitlane)
                LocationHint = "PIT";
            else if (CarLocation == CarLocationEnum.PitExit)
                LocationHint = "OutPit";
            else if (CarLocation == CarLocationEnum.Track)
                LocationHint = "OnTrack";
            else
                LocationHint = CurrentLap?.LapHint;
        }

        internal void SetFocused(int focusedCarIndex)
        {
            if (CarIndex == focusedCarIndex)
            {
                RowForeground = Brushes.Yellow;
                RowBackground = Brushes.Black;
            }
            else
            {
                RowForeground = Brushes.Black;
                RowBackground = null;
            }
        }
    }
}
