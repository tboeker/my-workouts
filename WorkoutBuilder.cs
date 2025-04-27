using Dynastream.Fit;
using File = Dynastream.Fit.File;

// ReSharper disable MemberCanBePrivate.Global

namespace my_workouts;

public enum HeartRateZone
{
    Resting = 0,
    Zone1 = 1, // ~50-60% of max HR
    Zone2 = 2, // ~60-70% of max HR
    Zone3 = 3, // ~70-80% of max HR
    Zone4 = 4, // ~80-90% of max HR
    Zone5 = 5 // ~90-100% of max HR
}

public class WorkoutBuilder
{
    private readonly List<WorkoutStepMesg?> workoutSteps = [];
    private readonly WorkoutMesg workoutMesg = new WorkoutMesg();


    private static uint HRLow(HeartRateZone zone)
    {
        switch (zone)
        {
            case HeartRateZone.Resting:
                return 0;
            case HeartRateZone.Zone1:
                return 50;
            case HeartRateZone.Zone2:
                return 60;
            case HeartRateZone.Zone3:
                return 70;
            case HeartRateZone.Zone4:
                return 80;
            case HeartRateZone.Zone5:
                return 90;
            default:
                throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
        }
    }

    private static uint HRHigh(HeartRateZone zone)
    {
        switch (zone)
        {
            case HeartRateZone.Resting:
                return 0;
            case HeartRateZone.Zone1:
                return 60;
            case HeartRateZone.Zone2:
                return 70;
            case HeartRateZone.Zone3:
                return 80;
            case HeartRateZone.Zone4:
                return 90;
            case HeartRateZone.Zone5:
                return 100;
            default:
                throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
        }
    }

    private WorkoutBuilder(string workoutName)
    {
        WorkoutName = workoutName;
    }

    public static WorkoutBuilder Create(string workoutName)
    {
        var builder = new WorkoutBuilder(workoutName);
        return builder;
    }

    public string WorkoutName { get; init; }

    public void Write()
    {
        workoutMesg.SetWktName(WorkoutName);
        workoutMesg.SetSport(Sport.Cycling);
        workoutMesg.SetSubSport(SubSport.Invalid);
        workoutMesg.SetNumValidSteps((ushort)workoutSteps.Count);
        CreateWorkout(workoutMesg, workoutSteps);
    }

    static void CreateWorkout(WorkoutMesg workoutMesg, List<WorkoutStepMesg?> workoutSteps)
    {
        // The combination of file type, manufacturer id, product id, and serial number should be unique.
        // When available, a non-random serial number should be used.
        const File fileType = File.Workout;
        const ushort manufacturerId = Manufacturer.Development;
        const ushort productId = 0;
        var random = new Random();
        var serialNumber = (uint)random.Next();

        // Every FIT file MUST contain a File ID message
        var fileIdMesg = new FileIdMesg();
        fileIdMesg.SetType(fileType);
        fileIdMesg.SetManufacturer(manufacturerId);
        fileIdMesg.SetProduct(productId);
        fileIdMesg.SetTimeCreated(new Dynastream.Fit.DateTime(System.DateTime.UtcNow));
        fileIdMesg.SetSerialNumber(serialNumber);

        // Create the output stream, this can be any type of stream, including a file or memory stream. Must have read/write access
        var fitDest = new FileStream($"{workoutMesg.GetWktNameAsString().Replace(' ', '_')}.fit",
            FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        // Create a FIT Encode object
        var encoder = new Encode(ProtocolVersion.V10);

        // Write the FIT header to the output stream
        encoder.Open(fitDest);

        // Write the messages to the file, in the proper sequence
        encoder.Write(fileIdMesg);
        encoder.Write(workoutMesg);

        foreach (var workoutStep in workoutSteps)
        {
            encoder.Write(workoutStep);
        }

        // Update the data size in the header and calculate the CRC
        encoder.Close();

        // Close the output stream
        fitDest.Close();

        Console.WriteLine($"Encoded FIT file {fitDest.Name}");
    }

    private WorkoutStepMesg AddStep(TimeSpan duration, Intensity intensity,
        HeartRateZone heartRateZone = HeartRateZone.Zone1,
        string? name = null, string? notes = null)
    {
        var step = new WorkoutStepMesg();
        step.SetMessageIndex((ushort)workoutSteps.Count);

        if (name != null)
        {
            step.SetWktStepName(name);
        }

        if (notes != null)
        {
            step.SetNotes(notes);
        }

        step.SetIntensity(intensity);

        step.SetDurationType(WktStepDuration.Time);
        step.SetDurationValue(Convert.ToUInt32(duration.TotalMilliseconds));

        step.SetTargetType(WktStepTarget.HeartRate);
        step.SetTargetValue(0);

        step.SetCustomTargetValueLow(HRLow(heartRateZone));
        step.SetCustomTargetValueHigh(HRHigh(heartRateZone));

        // step.SetSecondaryCustomTargetValueLow(HRHigh(heartRateZone - 1));
        // step.SetSecondaryCustomTargetValueHigh(HRLow(heartRateZone + 1));

        workoutSteps.Add(step);
        return step;
    }

    public WorkoutBuilder AddWarmup(TimeSpan duration, HeartRateZone heartRateZone = HeartRateZone.Zone1,
        string? name = "Warmup", string? notes = null)
    {
        AddStep(duration, Intensity.Warmup, heartRateZone, name, notes);
        return this;
    }

    public WorkoutBuilder AddCooldown(TimeSpan duration, HeartRateZone heartRateZone = HeartRateZone.Zone1,
        string? name = "Cooldown", string? notes = null)
    {
        AddStep(duration, Intensity.Cooldown, heartRateZone, name, notes);
        return this;
    }

    public WorkoutBuilder AddActive(TimeSpan duration, HeartRateZone heartRateZone = HeartRateZone.Zone3,
        string? name = "Aktiv", string? notes = null)
    {
        AddStep(duration, Intensity.Active, heartRateZone, name, notes);
        return this;
    }

    public WorkoutBuilder AddInterval(
        TimeSpan duration1,
        HeartRateZone heartRateZone1,
        TimeSpan duration2,
        HeartRateZone heartRateZone2,
        int repeats,
        string? name1 = null, string? notes1 = null,
        string? name2 = null, string? notes2 = null)
    {
        var step1 = AddStep(duration1, Intensity.Interval, heartRateZone1, name1, notes1);
        AddStep(duration2, Intensity.Interval, heartRateZone2, name2, notes2);

        var interv = new WorkoutStepMesg();
        interv.SetMessageIndex((ushort)workoutSteps.Count);

        interv.SetDurationType(WktStepDuration.RepeatUntilStepsCmplt);
        interv.SetDurationValue(step1.GetMessageIndex());

        interv.SetTargetType(WktStepTarget.Open);
        interv.SetTargetValue(Convert.ToUInt32(repeats));
        workoutSteps.Add(interv);

        return this;
    }
}