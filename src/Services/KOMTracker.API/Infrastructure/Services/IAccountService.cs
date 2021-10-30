using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public interface IAccountService
    {
        Task<Result> Connect(string code, string scope);
    }
}
