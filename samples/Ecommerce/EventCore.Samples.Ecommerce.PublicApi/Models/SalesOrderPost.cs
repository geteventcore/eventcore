namespace EventCore.Samples.Ecommerce.PublicApi.Models
{
	public class SalesOrderPost
	{
		public readonly string CustomerName;
		public readonly decimal TotalPrice;

		public SalesOrderPost(string customerName, decimal totalPrice)
		{
		}
	}
}
