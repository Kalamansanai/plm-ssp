using Domain.Common;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Api.Endpoints.Tasks;

public class GetObjectsAndSteps : Endpoint<GetObjectsAndSteps.Req, GetObjectsAndSteps.Res>
{
    public IRepository<Task> TaskRepo { get; set; } = default!;
    public class Req
    {
        public int TaskId { get; set; }
    }

    public class Res
    {
        public IEnumerable<ResObject> Objects { get; set; } = default!;
        public IEnumerable<ResStep> Steps { get; set; } = default!;
        public IEnumerable<ResCoordinate> MarkerCoordinates { get; set; } = default!;

        public record ResObject(int Id, string Name, ObjectCoordinates Coordinates);

        public record ResStep(int Id, int? OrderNum, TemplateState ExpectedInitialState,
            TemplateState ExpectedSubsequentState, int ObjectId);

        public record ResCoordinate(int X, int Y);


    }

    private static Res MapOut(Task task)
    {
        return new Res
        {
            Objects = task.Objects.Select(o => new Res.ResObject(o.Id, o.Name, o.Coordinates)),
            Steps = task.Steps.Select(s => new Res.ResStep(s.Id, s.OrderNum, s.ExpectedInitialState, s.ExpectedSubsequentState, s.ObjectId)),
            MarkerCoordinates = task.MarkerCoordinates.Select(c => new Res.ResCoordinate(X: c.X, Y: c.Y))
        };
    }

    public override void Configure()
    {
        Get(Api.Routes.Tasks.GetObjectsAndEvents);
        AllowAnonymous();
        Options(x => x.WithTags("Tasks"));
    }

    public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
    {
        var task = await TaskRepo.FirstOrDefaultAsync(new TaskWithChildrenSpec(req.TaskId), ct);

        if (task is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        if (task.MarkerCoordinates is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(task);
        await SendOkAsync(res, ct);
    }
}