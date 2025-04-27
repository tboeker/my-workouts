using my_workouts;

// https://support.hammerhead.io/hc/en-us/articles/4416694669851-Dashboard-Importing-ZWO-FIT-based-Workouts

WorkoutBuilder[] workouts =
[
    WorkoutBuilder.Create("GA1 Ausdauer")
        .AddWarmup(TimeSpan.FromMinutes(5))
        .AddActive(TimeSpan.FromMinutes(160), HeartRateZone.Zone2)
        .AddCooldown(TimeSpan.FromMinutes(5)),    
    
    WorkoutBuilder.Create("GA2 Ausdauer")
        .AddWarmup(TimeSpan.FromMinutes(10))
        .AddActive(TimeSpan.FromMinutes(20), HeartRateZone.Zone3)
        .AddCooldown(TimeSpan.FromMinutes(5)),
    
    WorkoutBuilder.Create("Z3 Intervals 5min")
        .AddWarmup(TimeSpan.FromMinutes(10))
        .AddInterval(
            TimeSpan.FromMinutes(5),
            HeartRateZone.Zone3,
            TimeSpan.FromMinutes(3),
            HeartRateZone.Zone2,
            8)
        .AddCooldown(TimeSpan.FromMinutes(10)),
    
    WorkoutBuilder.Create("Z4 Intervals 4min")
        .AddWarmup(TimeSpan.FromMinutes(10))
        .AddInterval(
            TimeSpan.FromMinutes(4),
            HeartRateZone.Zone4,
            TimeSpan.FromMinutes(4),
            HeartRateZone.Zone2,
            5)
        .AddCooldown(TimeSpan.FromMinutes(10)),
    
    WorkoutBuilder.Create("Z5 Intervals 1min")
        .AddWarmup(TimeSpan.FromMinutes(10))
        .AddInterval(
            TimeSpan.FromMinutes(1),
            HeartRateZone.Zone5,
            TimeSpan.FromMinutes(2),
            HeartRateZone.Zone2,
            5)
        .AddCooldown(TimeSpan.FromMinutes(10))
];


foreach (var w in workouts)
{
    w.Write();
}