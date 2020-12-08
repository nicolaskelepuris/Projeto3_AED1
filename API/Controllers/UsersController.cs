using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using API.Helpers;
using AutoMapper;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace API.Controllers
{
    [Route("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public UsersController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;

        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<Pagination<UserDto>>>> GetUsers([FromQuery] UsersSpecificationParams usersSpecificationParams)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin && !user.IsEmployee) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var spec = new UsersSpecification(usersSpecificationParams);

            var users = await UserSpecificationEvaluator<AppUser>.GetQuery(_userManager.Users, spec).ToListAsync();

            var usersDto = _mapper.Map<IReadOnlyList<AppUser>, IReadOnlyList<UserDto>>(users);

            var totalCountOfUsers = await _userManager.Users.Where(spec.Criteria).CountAsync();

            return new ResponseDto<Pagination<UserDto>>
            {
                Success = true,
                Data = new Pagination<UserDto>(usersSpecificationParams.PageIndex, usersSpecificationParams.PageSize, totalCountOfUsers, usersDto),
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpGet("currentUser")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> GetCurrentUser()
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
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

        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            var user = _mapper.Map<RegisterDto, AppUser>(registerDto);

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
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

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> GetUser([Required][FromRoute]string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin && !user.IsEmployee) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToReturn = await _userManager.FindByIdAsync(id);

            if (userToReturn == null) return NotFound(new ResponseDto<string>
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
                    User = _mapper.Map<AppUser, UserDto>(userToReturn),
                    Token = new TokenDto { Token = null },
                },
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserDto>>> DeleteUser([FromRoute] string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToBeDeleted = await _userManager.FindByIdAsync(id);

            if (userToBeDeleted == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var result = await _userManager.DeleteAsync(userToBeDeleted);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return StatusCode(204);
        }      

        [Authorize]
        [HttpPatch("{id}/updatephonenumber")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> UpdatePhoneNumber([FromBody] UpdateUserPhoneNumberDto updateUserPhoneNumberDto, [FromRoute]string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (user.Id != id) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var changePhoneToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, updateUserPhoneNumberDto.PhoneNumber);

            var result = await _userManager.ChangePhoneNumberAsync(user, updateUserPhoneNumberDto.PhoneNumber, changePhoneToken);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
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

        [Authorize]
        [HttpPatch("{id}/updateemail")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> UpdateEmail([FromBody] UpdateUserEmailDto updateUserEmailDto, [FromRoute]string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (user.Id != id) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(user, updateUserEmailDto.Email);

            var result = await _userManager.ChangeEmailAsync(user, updateUserEmailDto.Email, changeEmailToken);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
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

        [Authorize]
        [HttpPatch("{id}/updatepassword")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> UpdatePassword([FromBody] UpdateUserPasswordDto updateUserPasswordDto, [FromRoute]string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (user.Id != id) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var result = await _userManager.ChangePasswordAsync(user, updateUserPasswordDto.CurrentPassword, updateUserPasswordDto.NewPassword);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
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

        [Authorize]
        [HttpPatch("{id}/grantadminpermission")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> GrantAdminPermission([FromRoute] string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToReceiveAdmin = await _userManager.FindByIdAsync(id);

            if (userToReceiveAdmin == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (userToReceiveAdmin.IsAdmin) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            userToReceiveAdmin.IsAdmin = true;

            var result = await _userManager.UpdateAsync(userToReceiveAdmin);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<UserResponseDto>
            {
                Success = true,
                Data = new UserResponseDto
                {
                    User = _mapper.Map<AppUser, UserDto>(userToReceiveAdmin),
                    Token = new TokenDto { Token = null },
                },
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpPatch("{id}/removeadminpermission")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> RemoveAdminPermission([FromRoute] string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToRemoveAdmin = await _userManager.FindByIdAsync(id);

            if (userToRemoveAdmin == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!userToRemoveAdmin.IsAdmin) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            userToRemoveAdmin.IsAdmin = false;

            var result = await _userManager.UpdateAsync(userToRemoveAdmin);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<UserResponseDto>
            {
                Success = true,
                Data = new UserResponseDto
                {
                    User = _mapper.Map<AppUser, UserDto>(userToRemoveAdmin),
                    Token = new TokenDto { Token = null },
                },
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpPatch("{id}/grantemployeepermission")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> GrantEmployeePermission([FromRoute] string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToGrantEmployeePermission = await _userManager.FindByIdAsync(id);

            if (userToGrantEmployeePermission == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (userToGrantEmployeePermission.IsEmployee) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            userToGrantEmployeePermission.IsEmployee = true;

            var result = await _userManager.UpdateAsync(userToGrantEmployeePermission);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<UserResponseDto>
            {
                Success = true,
                Data = new UserResponseDto
                {
                    User = _mapper.Map<AppUser, UserDto>(userToGrantEmployeePermission),
                    Token = new TokenDto { Token = null },
                },
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpPatch("{id}/removeemployeepermissionfromuser")]
        [ProducesResponseType(typeof(ResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<UserResponseDto>>> RemoveEmployeePermissionFromUser([FromQuery] string id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            var userToRemoveEmployeePermission = await _userManager.FindByIdAsync(id);

            if (userToRemoveEmployeePermission == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!userToRemoveEmployeePermission.IsEmployee) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            userToRemoveEmployeePermission.IsEmployee = false;

            var result = await _userManager.UpdateAsync(userToRemoveEmployeePermission);

            if (!result.Succeeded) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<UserResponseDto>
            {
                Success = true,
                Data = new UserResponseDto
                {
                    User = _mapper.Map<AppUser, UserDto>(userToRemoveEmployeePermission),
                    Token = new TokenDto { Token = null },
                },
                Error = new ApiErrorResponse()
            };
        }        
    }
}