namespace EventCore.Samples.EmailSystem.WebApi.Models
{
	public class SalesOrder
	{
		public readonly string SalesOrderId;
		public readonly string CustomerName;
		public readonly decimal TotalPrice;

		public SalesOrder(string salesOrderId, string customerName, decimal totalPrice)
		{
		}
	}
}
