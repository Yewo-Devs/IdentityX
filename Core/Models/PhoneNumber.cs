namespace IdentityX.Core.Models
{
	public class PhoneNumber
	{
		public string CountryCode { get; set; }
		public string AreaCode { get; set; }
		public string Number { get; set; }

		public string GetNumber => string.IsNullOrEmpty(AreaCode)? $"+{CountryCode}-{Number}" : $"+{CountryCode}-{AreaCode}-{Number}";
	}
}