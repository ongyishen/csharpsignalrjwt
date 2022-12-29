using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WebApiSignalRClient.Core;
using WebApiSignalRClient.Models;

namespace WebApiSignalRClient.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SignalRClientController : ControllerBase
	{
		[HttpPost("SendMessage")]
		public async Task<IActionResult> SendMessage(ChatForm param)
		{
			LoginRequest loginParam = new LoginRequest();
			loginParam.Username = param.User;
			loginParam.Password = param.Password;

			var connection = new HubConnectionBuilder()
				.WithUrl(GlobalContext.SystemConfig.ChatHubUrl, options =>
				{
					options.AccessTokenProvider = async () =>
					{
						var stringData = JsonConvert.SerializeObject(loginParam);
						var content = new StringContent(stringData);
						content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
						HttpClient httpClient = new HttpClient();
						var response = await httpClient.PostAsync(GlobalContext.SystemConfig.JwtTokenUrl, content);
						response.EnsureSuccessStatusCode();
						return await response.Content.ReadAsStringAsync();
					};
				})
				.WithAutomaticReconnect()
				.Build();

			try
			{
				await connection.StartAsync();
				await connection.InvokeAsync("SendMessage", param.User, param.Message);
				await connection.StopAsync();
			}
			catch (Exception ex)
			{
				return Ok(ex.Message);
			}

			return Ok();
		}
	}
}