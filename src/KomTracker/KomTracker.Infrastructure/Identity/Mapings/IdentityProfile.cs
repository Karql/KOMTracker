using AutoMapper;
using KomTracker.Application.Models.Identity;
using KomTracker.Infrastructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Mapings;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<UserEntity, UserModel>();
    }
}