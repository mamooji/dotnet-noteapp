using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class V1ApiController : ApiController
{
}