using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Lines;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<Line> LineRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set; }
    }

    public override void Configure()
    {
        Delete(Api.Routes.Lines.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Lines"));
        Description(x => x
                .Accepts<Req>("application/json")
                .Produces(204)
                .Produces(404),
            clearDefaults: true);
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = await LineRepo.GetByIdAsync(req.Id, ct);

        if (line is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await LineRepo.DeleteAsync(line, ct);

        await SendNoContentAsync(ct);
    }
}