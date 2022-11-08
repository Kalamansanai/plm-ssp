using System.Net;
using System.Net.NetworkInformation;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public class Detector : IBaseEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public PhysicalAddress MacAddress { get; private set; } = default!;
    public IPAddress IpAddress { get; private set; } = default!;
    public DetectorState State { get; private set; }
    public List<HeartBeatLog> HeartBeatLogs { get; private set; } = default!;

    public Location? Location { get; private set; }
    public int? LocationId { get; private set; }

    [Owned]
    public class HeartBeatLog
    {
        public int Id { get; set; }
        public int Temperature { get; set; }
        public int FreeStoragePercentage { get; set; }
        public int Uptime { get; set; }
    }

    private Detector() { }

    public static Result<Detector> Create(string newName, PhysicalAddress newMacAddress, IPAddress newAddress, Location location)
    {
        var detector = new Detector
        {
            Name = newName,
            MacAddress = newMacAddress,
            IpAddress = newAddress,
            HeartBeatLogs = new List<HeartBeatLog>()
        };

        return location.AttachDetector(detector).ToResult(detector);
    }
}