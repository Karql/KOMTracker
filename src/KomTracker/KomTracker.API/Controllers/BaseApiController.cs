using AutoMapper;
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
    protected IMediator _mediator => _mediatorInstance ??= HttpContext.RequestServices.GetService<IMediator>()!;
    protected ILogger<T> _logger => _loggerInstance ??= HttpContext.RequestServices.GetService<ILogger<T>>()!;
    protected IMapper _mapper => _mapperInstance ??= HttpContext.RequestServices.GetService<IMapper>()!;

}
