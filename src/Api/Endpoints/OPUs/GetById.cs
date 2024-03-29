using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.OPUs;

public class GetById : Endpoint<GetById.Req, GetById.Res>
{
    public IRepository<OPU> OPURepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public IEnumerable<LineRes> Lines { get; set; } = default!;

        public record LineRes(int Id, string Name);
    }

    private static Res MapOut(OPU o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Lines = o.Children.Select(l => new Res.LineRes(l.Id, l.Name))
    };

    public override void Configure()
    {
        Get(Api.Routes.OPUs.GetById);
        AllowAnonymous();
        Options(x => x.WithTags("OPUs"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var opu = await OPURepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<OPU, Line>(req.Id), ct);

        if (opu is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = MapOut(opu);
        await SendOkAsync(res, ct);
    }
}