using System;

namespace UserAccessManagement.BackgroundTasks
{
    public class BenefitsEnrollmentWorkerOptions
    {
        public int MaxDegreeOfParallelism { get; init; }
        public TimeSpan NoAvailableSlotsQueueDelay { get; init; }
    }
}
