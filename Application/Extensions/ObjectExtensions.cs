using Newtonsoft.Json;
using System.Reflection;

namespace IdentityX.Application.Extensions
{
    public static class ObjectExtensions
    {
        public static T ToObject<T>(this IDictionary<string, object> source)
        {
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IDictionary<string, object> AsDictionary(this object source)
        {
            var jString = JsonConvert.SerializeObject(source);

			return JsonConvert.DeserializeObject<Dictionary<string, object>>(jString);
        }

        /// <summary>
        /// Maps object to T and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Map<T>(this object data) where T : new()
        {
            Dictionary<string, object> valPairs = (Dictionary<string, object>)((new T()).AsDictionary());

            IDictionary<string, object> keyValuePairs = data.AsDictionary();

            foreach (var item in keyValuePairs)
            {
                foreach (var key in valPairs)
                {
                    if (item.Key.ToLower() == key.Key.ToLower())
                    {
                        valPairs[key.Key] = item.Value;
                        continue;
                    }
                }
            }

            return valPairs.ToObject<T>();
        }
	}
}
