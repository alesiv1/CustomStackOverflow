using AutoMapper;
using System;

namespace WebApp.Models.AutoMapper
{
	public class AutoMapping : Profile
	{
		public AutoMapping()
		{
			//CreateMap<User, UserDTO>(); - means you want to map from User to UserDTO
			//User user = new User();
			//var userDTO = _mapper.Map<UserDTO>(user);
		}
	}
}
