namespace Api.Services.DetectorController
{
    public class DetectorControllerOptions
    {
        public const string SectionName = "DetectorController";

        public int TimeoutMilliseconds { get; set; }
        public int PingMilliseconds { get; set; }
        public int ResponseBufferSize { get; set; }
        public int SnapshotBufferSize { get; set; }
    }
}