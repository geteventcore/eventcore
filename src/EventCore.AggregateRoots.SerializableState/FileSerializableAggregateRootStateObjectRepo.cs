using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace EventCore.AggregateRoots.SerializableState
{
	public class FileSerializableAggregateRootStateObjectRepo : ISerializableAggregateRootStateObjectRepo
	{
		private readonly string _basePath;

		public string BasePath { get => _basePath; }

		public FileSerializableAggregateRootStateObjectRepo(string basePath)
		{
			_basePath = Path.GetFullPath(basePath);

			if (!Directory.Exists(_basePath))
			{
				Directory.CreateDirectory(_basePath);
			}
		}

		public Task<SerializableAggregateRootStateObject<TInternalState>> LoadAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId)
		{
			SerializableAggregateRootStateObject<TInternalState> stateObj = null;
			
			var stateFilePath = BuildStateFilePath(_basePath, regionId, context, aggregateRootName, aggregateRootId);
			if (File.Exists(stateFilePath))
			{
				var json = File.ReadAllText(stateFilePath);
				stateObj = (SerializableAggregateRootStateObject<TInternalState>)JsonConvert.DeserializeObject(json, typeof(SerializableAggregateRootStateObject<TInternalState>));
			}

			return Task.FromResult<SerializableAggregateRootStateObject<TInternalState>>(stateObj);
		}

		public Task SaveAsync<TInternalState>(string regionId, string context, string aggregateRootName, string aggregateRootId, SerializableAggregateRootStateObject<TInternalState> stateObject)
		{
			var json = JsonConvert.SerializeObject(stateObject);
			File.WriteAllText(BuildStateFilePath(_basePath, regionId, context, aggregateRootName, aggregateRootId), json);
			return Task.CompletedTask;
		}


		public static string BuildStateFilePath(string basePath, string regionId, string context, string aggregateRootName, string aggregateRootId)
		{
			return Path.Combine(basePath, string.Join("-", regionId, context, aggregateRootName, aggregateRootId));
		}
	}
}
