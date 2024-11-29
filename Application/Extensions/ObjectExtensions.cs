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

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }

        /// <summary>
        /// Maps T2 to T and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Map<T, T2>(this object data) where T : new()
        {
            Dictionary<string, object> valPairs = (Dictionary<string, object>)((new T()).AsDictionary());

            IDictionary<string, object> keyValuePairs = ((T2)data).AsDictionary();

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
