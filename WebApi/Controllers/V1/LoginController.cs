using Application.Common.Interfaces;
using Application.Login.Commands.LoginCommand;
using Backend.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.V1;

namespace Backend.WebApi.Controllers.V1;

public class LoginController : V1ApiController
{
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ILoginResult> Create([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var loginCommand = new LoginCommand
        {
            UserName = dto.UserName,
            Password = dto.Password
        };
        return await Mediator.Send(loginCommand, cancellationToken);
    }
}