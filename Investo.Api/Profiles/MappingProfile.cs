namespace Investo.Api.Profiles;

using AutoMapper;
using Investo.Api.ViewModels;
using Investo.BusinessLogic.Models;
using Investo.DataAccess.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        this.CreateMap<UserCreateViewModel, UserCreateModel>();
        this.CreateMap<UserCreateModel, User>();
        this.CreateMap<UserLoginViewModel, UserLoginModel>();
        this.CreateMap<UserCreateViewModel, UserLoginModel>();
        this.CreateMap<User, UserModel>();
        this.CreateMap<UserUpdateViewModel, UserUpdateModel>();
        this.CreateMap<UserResetPasswordCodeViewModel, UserResetPasswordCodeModel>();
        this.CreateMap<UserResetPasswordCodeModel, UserResetPasswordCode>();
        this.CreateMap<PasswordResetCodeViewModel, PasswordResetCodeModel>();
    }
}