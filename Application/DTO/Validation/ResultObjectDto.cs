namespace IdentityX.Application.DTO.Validation
{
	public class ResultObjectDto<T>
	{
		public T Result { get; set; }

		public string Error { get; set; }
	}
}
