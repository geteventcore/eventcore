using EventCore.AggregateRoots.SerializableState;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace EventCore.Samples.EmailSystem.DomainApi.Infrastructure
{
	public class SerializableAggregateRootStateObjectRepo : ISerializableAggregateRootStateObjectRepo
	{
		private readonly string _basePath;

		public string BasePath { get => _basePath; }

		public SerializableAggregateRootStateObjectRepo(string basePath)
		{
			_basePath = Path.GetFullPath(basePath);

			if (!Directory.Exists(_basePath))
			{
				Directory.CreateDirectory(_basePath);
			}
		}

		public async Task<SerializableAggregateRootStateObject<TInternalState>> LoadAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId)
		{
			var json = await File.ReadAllTextAsync(BuildStateFilePath(_basePath, regionId, context, aggregateRootName, aggregateRootId));
			return (SerializableAggregateRootStateObject<TInternalState>)JsonConvert.DeserializeObject(json, typeof(SerializableAggregateRootStateObject<TInternalState>));
		}

		public async Task SaveAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId, SerializableAggregateRootStateObject<TInternalState> stateObject)
		{
			var json = JsonConvert.SerializeObject(stateObject);
			await File.WriteAllTextAsync(BuildStateFilePath(_basePath, regionId, context, aggregateRootName, aggregateRootId), json);
		}


		public static string BuildStateFilePath(string basePath, string regionId, string context, string aggregateRootName, string aggregateRootId)
		{
			return Path.Join(basePath, string.Join("-", regionId, context, aggregateRootName, aggregateRootId));
		}
	}
}
