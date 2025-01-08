using Newtonsoft.Json;

namespace IdentityX.Application.DTO.Validation
{
	public class ResultObjectDto<T>
	{
		public T Result { get; set; }

		private string _error;
		public string Error 
		{ 
			get { return JsonConvert.SerializeObject(_error); } 
			set { _error = value; } 
		}
	}
}
