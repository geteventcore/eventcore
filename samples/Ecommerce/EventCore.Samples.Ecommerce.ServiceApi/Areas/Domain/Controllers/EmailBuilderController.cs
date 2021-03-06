﻿using EventCore.Samples.Ecommerce.Domain.EmailBuilder;
using EventCore.Samples.Ecommerce.Domain.EmailBuilder.Commands;
using EventCore.Samples.Ecommerce.ServiceApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventCore.Samples.Ecommerce.ServiceApi.Areas.Domain.Controllers
{
	[Route("[area]/[controller]")]
	[ApiController]
	[Area("domain")]
	public class EmailBuilderController : ControllerBase
	{
		private readonly EmailBuilderAggregate _ar;

		public EmailBuilderController(EmailBuilderAggregate ar)
		{
			_ar = ar;
		}

		[HttpPost("BuildSalesOrderEmail")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<IActionResult> BuildSalesOrderEmailAsync([FromBody] BuildSalesReceiptEmailCommand c) => CommandProcessor.ProcessCommandAsync(_ar, c);
	}
}
