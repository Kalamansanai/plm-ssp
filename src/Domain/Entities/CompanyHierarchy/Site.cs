using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Site : ICHNodeWithChildren<OPU>
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public List<OPU> Children { get; private set; } = default!;

    private Site() {}

    private Site(int id, string name)
    {
        Id = id;
        Name = name;
    }

    private Site(string name)
    {
        Name = name;
    }

    public static Result<Site> New(string name,
        ICHNameUniquenessChecker<Site> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(name, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var site = new Site(name);
        return site;
    }

    public Result<OPU> AddChildNode(string opuName, ICHNameUniquenessChecker<Site, OPU> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(this, opuName, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var opu = new OPU(opuName);
        Children.Add(opu);

        return opu;
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<Site> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }
}