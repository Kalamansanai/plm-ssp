using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Line : ICHNode<OPU, Station>
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public List<Station> Children { get; private set; } = default!;

    public OPU Parent { get; private set; } = default!;
    public int ParentId { get; private set; }

    private Line() {}
    private Line(int id, string name, int parentId)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
    }

    public Line(string name)
    {
        Name = name;
    }

    public Result<Station> AddChildNode(string stationName,
        ICHNameUniquenessChecker<Line, Station> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(this, stationName, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var station = new Station(stationName);
        Children.Add(station);

        return Result.Ok(station);
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<OPU, Line> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }
}