using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Detectors;

public class ReCalibrate : Endpoint<ReCalibrate.Req, EmptyResponse>
{
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public IDetectorConnection DetectorConnection { get; set; } = default!;
    public class Req
    {
        public int LocationId { get; set; }
        public int[]? NewTrayCoordinates { get; set; }
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Detectors.ReCalibrate);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(req.LocationId), ct);

        if (location is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (location.Detector is null)
        {
            ThrowError("This location has no active detector!");
            return;
        }

        //get the old coordinates for the difference calculation
        var old = location.GetCoordinates();
        old.Unwrap();
        
        //send to the RPI and get back the current coordinates
        var result = await location.Detector.SendRecalibrate(old.Value, DetectorConnection, req.NewTrayCoordinates);
        result.Unwrap();

        //set the coordinates with the new ones
        location.Coordinates = result.Value;
        
        await SendNoContentAsync(ct);
    }
}