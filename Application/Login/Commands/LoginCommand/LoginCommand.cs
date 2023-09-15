using Application.Common.Interfaces;
using MediatR;

namespace Application.Login.Commands.LoginCommand;

public class LoginCommand : IRequest<ILoginResult>
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ILoginResult>
{
    private readonly ILoginService _loginService;

    public LoginCommandHandler(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<ILoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _loginService.Login(request.UserName, request.Password, cancellationToken);
    }
}