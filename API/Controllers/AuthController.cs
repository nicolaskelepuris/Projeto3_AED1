using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Login);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

            if (!result.Succeeded) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            return new ResponseDto<UserResponseDto>
            {
                Success = true,
                Data = new UserResponseDto
                {
                    User = _mapper.Map<AppUser, UserDto>(user),
                    Token = new TokenDto { Token = _tokenService.CreateToken(user) },
                },
                Error = new ApiErrorResponse()
            };
        }
    }
}