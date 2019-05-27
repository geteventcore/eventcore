namespace EventCore.Samples.Ecommerce.PublicApi.Models
{
	public class NewSalesOrder
	{
		public readonly string CustomerName;
		public readonly string CustomerEmail;
		public readonly decimal TotalPrice;

		public NewSalesOrder(string customerName, string customerEmail, decimal totalPrice)
		{
			CustomerName = customerName;
			CustomerEmail = customerEmail;
			TotalPrice = totalPrice;
		}
	}
}
