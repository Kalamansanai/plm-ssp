using System.Text.Json.Serialization;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Services;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;
using System;
using System.Diagnostics;
using Infrastructure.Logging;

namespace Api.Endpoints.Detectors;

public class Command : Endpoint<Command.Req, EmptyResponse>
{
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public DetectorCommandService CommandService { get; set; } = default!;
    public INotifyChannel NotifyChannel { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
        public DetectorCommand Command { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Detectors.Command);
        AllowAnonymous();
        Options(x => x.WithTags("Detectors"));
        Summary(x => x.ExampleRequest = new Dictionary<string, object>
        {
            { "Command", new DetectorCommand.StartDetection(2) }
        });
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Stopwatch q = new Stopwatch();
        q.Start();

        var detector = await DetectorRepo.GetByIdAsync(req.Id, ct);
        
        q.Stop();
        PlmLogger.Log("***************************");
        PlmLogger.Log("Command Query Time");
        PlmLogger.Log($"{sw.Elapsed.TotalSeconds}");

        if (detector is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var result = await CommandService.HandleCommand(detector, req.Command, ct);
        result.Unwrap();

        await DetectorRepo.SaveChangesAsync(ct);
        
        sw.Stop();
        PlmLogger.Log("Command Time");
        PlmLogger.Log($"{sw.Elapsed.TotalSeconds}");

        //SSE
        NotifyChannel.AddNotify(detector.Location.Id);
        await SendNoContentAsync(ct);
    }
}
