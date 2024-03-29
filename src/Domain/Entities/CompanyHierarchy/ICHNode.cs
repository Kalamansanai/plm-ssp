namespace Domain.Entities.CompanyHierarchy;

public interface ICHNode<TParent, TChild>
    : ICHNodeWithChildren<TChild>, ICHNodeWithParent<TParent>
    where TParent : class, ICHNode
    where TChild : class, ICHNode
{
}

public interface ICHNodeWithParent<TParent> : ICHNode
    where TParent : class, ICHNode
{
    public TParent Parent { get; }
    public int ParentId { get; }
}

public interface ICHNodeWithChildren<TChild> : ICHNode
    where TChild : class, ICHNode
{
    public List<TChild> Children { get; }
}

public interface ICHNode : IBaseEntity
{
    public string Name { get; }

}