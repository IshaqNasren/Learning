using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Data;
using TodoApi.Dtos;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using TodoApi.Helpers;
using TodoApi.Model;

namespace TodoApi.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams){
            
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                
            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if(string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            
            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserFotListDto>>(users);

            Response.AddPagination(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);

            return Ok(usersToReturn);
            
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id){

            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDeatailDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto UserForUpdateDto)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);  

            _mapper.Map(UserForUpdateDto,userFromRepo);

            if(await _repo.SaveAll())
                return NoContent();
            
            throw new Exception($"Updating user {id} failed on save!");

        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId){

             if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();   

            var like = await _repo.GetLike(id,recipientId);

            if(like != null)
                return BadRequest("You already like this user");

            if(await _repo.GetUser(recipientId) == null)
                return NotFound();

            like = new Like{LikerId = id , LikeeId = recipientId};

            _repo.Add<Like>(like);

            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");                                
        }    
        

    }
}