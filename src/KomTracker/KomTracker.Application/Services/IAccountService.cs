using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface IAccountService
{
    Task<Result> Connect(string code, string scope);
}
