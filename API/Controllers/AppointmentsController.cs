using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/appointments")]
    public class AppointmentsController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public AppointmentsController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(ResponseDto<Pagination<AppointmentToReturnDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseDto<Pagination<AppointmentToReturnDto>>>> GetAppointments([FromQuery] AppointmentsSpecificationParams appointmentsSpecificationParams)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var spec = new AppointmentsWithAppUserSpecification(appointmentsSpecificationParams, user);

            var countSpec = new AppointmentsWithFiltersForCountSpecification(spec.Criteria);

            var totalItems = await _unitOfWork.Repository<Appointment>().CountAsync(countSpec);

            var appointments = await _unitOfWork.Repository<Appointment>().ListAsyncWithSpec(spec);

            if (appointments.Count == 0) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var appointmentsData = _mapper.Map<IReadOnlyList<Appointment>, IReadOnlyList<AppointmentToReturnDto>>(appointments);

            return new ResponseDto<Pagination<AppointmentToReturnDto>>
            {
                Success = true,
                Data = new Pagination<AppointmentToReturnDto>
                (appointmentsSpecificationParams.PageIndex, appointmentsSpecificationParams.PageSize, totalItems, appointmentsData),
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseDto<AppointmentToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<AppointmentToReturnDto>>> GetAppointment([FromRoute] int id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityByIdAsync(id);

            if (appointment == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (user.IsAdmin || user.IsEmployee || appointment.AppUserEmail == user.Email)
            {
                return new ResponseDto<AppointmentToReturnDto>
                {
                    Success = true,
                    Data = _mapper.Map<Appointment, AppointmentToReturnDto>(appointment),
                    Error = new ApiErrorResponse()
                };
            }
            else
            {
                return StatusCode(403, new ResponseDto<string>
                {
                    Success = false,
                    Data = null,
                    Error = new ApiErrorResponse(403)
                });
            }
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDto<AppointmentToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<AppointmentToReturnDto>>> CreateAppointment([FromBody] CreateOrUpdateAppointmentDto createAppointmentDto)
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

            var appointment = _mapper.Map<CreateOrUpdateAppointmentDto, Appointment>(createAppointmentDto);

            _unitOfWork.Repository<Appointment>().AddEntity(appointment);

            var result = await _unitOfWork.Complete();

            if (result <= 0) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<AppointmentToReturnDto>
            {
                Success = true,
                Data = _mapper.Map<Appointment, AppointmentToReturnDto>(appointment),
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseDto<AppointmentToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<AppointmentToReturnDto>>> UpdateAppointment([FromBody] CreateOrUpdateAppointmentDto updateAppointmentDto, [FromRoute] int id)
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

            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityByIdAsync(id);

            if (appointment == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            _mapper.Map<CreateOrUpdateAppointmentDto, Appointment>(updateAppointmentDto, appointment);

            _unitOfWork.Repository<Appointment>().UpdateEntity(appointment);

            var result = await _unitOfWork.Complete();

            if (result <= 0) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<AppointmentToReturnDto>
            {
                Success = true,
                Data = _mapper.Map<Appointment, AppointmentToReturnDto>(appointment),
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(ResponseDto<AppointmentToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<AppointmentToReturnDto>>> CancelAppointment([FromRoute] int id)
        {
            var user = await _userManager.FindUserByEmailAsyncFromClaimsPrincipal(HttpContext.User);

            if (user == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityByIdAsync(id);

            if (appointment == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            if (!user.IsAdmin && !user.IsEmployee && user.Email != appointment.AppUserEmail) return StatusCode(403, new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(403)
            });

            appointment.IsCancelled = true;

            _unitOfWork.Repository<Appointment>().UpdateEntity(appointment);

            var result = await _unitOfWork.Complete();

            if (result <= 0) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return new ResponseDto<AppointmentToReturnDto>
            {
                Success = true,
                Data = _mapper.Map<Appointment, AppointmentToReturnDto>(appointment),
                Error = new ApiErrorResponse()
            };
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseDto<AppointmentToReturnDto>>> DeleteAppointment([FromRoute] int id)
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

            var appointment = await _unitOfWork.Repository<Appointment>().GetEntityByIdAsync(id);

            if (appointment == null) return NotFound(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(404)
            });

            _unitOfWork.Repository<Appointment>().DeleteEntity(appointment);

            var result = await _unitOfWork.Complete();

            if (result <= 0) return BadRequest(new ResponseDto<string>
            {
                Success = false,
                Data = null,
                Error = new ApiErrorResponse(400)
            });

            return StatusCode(204);
        }
    }
}