namespace Bacera
{
	public interface IEntityCriteria<T, TPk> : ICriteria where T : class, IEntity<TPk> where TPk : IEquatable<TPk>
	{
		TPk Id { get; set; }
		IList<TPk> Ids { get; set; }
	}
}
