namespace IdentityX.Application.Interfaces
{
	public interface IDataService
	{
		Task DeleteData(string collection, string id);
		Task<IEnumerable<T>> GetCollectionOfType<T>(string collection);
		Task<T> GetInstanceOfType<T>(string collection, string accountId);
		Task StoreData(string collection, object data, string id);
		Task UpdateData(string collection, string path, object data);
	}
}
