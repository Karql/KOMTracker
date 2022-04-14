using AutoMapper;
using KomTracker.API.Shared.Helpers;
using KomTracker.API.Shared.Models.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KomTracker.API.Controllers;

[ApiController]
public abstract class BaseApiController<T> : ControllerBase
{
    private IMediator? _mediatorInstance;
    private ILogger<T>? _loggerInstance;
    private IMapper _mapperInstance;
    private IHttpContextAccessor _httpContextAccessorInstance;
    protected IMediator _mediator => _mediatorInstance ??= HttpContext.RequestServices.GetService<IMediator>()!;
    protected ILogger<T> _logger => _loggerInstance ??= HttpContext.RequestServices.GetService<ILogger<T>>()!;
    protected IMapper _mapper => _mapperInstance ??= HttpContext.RequestServices.GetService<IMapper>()!;
    protected IHttpContextAccessor _httpContextAccessor => _httpContextAccessorInstance ??= HttpContext.RequestServices.GetService<IHttpContextAccessor>()!;

    protected UserModel? GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            return null;
        }

        return UserHelper.GetUserFromPrincipal(user);
    }
}
