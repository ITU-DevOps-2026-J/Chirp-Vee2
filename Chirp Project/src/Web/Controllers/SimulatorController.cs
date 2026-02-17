using Microsoft.AspNetCore.Mvc;
using Core.DTO;
using Core.Interfaces;
using Core.Model;
using Microsoft.IdentityModel.Tokens;

namespace Web.Controllers
{
    [ApiController]
    public class SimulatorController : ControllerBase
    {
        private readonly ICheepService _cheepService;
        private readonly ILatestsRepository _latestsRepository;

        public SimulatorController(ICheepService cheepService, ILatestsRepository latestsRepository)
        {
            _cheepService = cheepService;
            _latestsRepository = latestsRepository;
        }

        [HttpGet]
        [Route("fllws/{username}")]
        public async Task<IActionResult> GetUserFollowsAsync(
            string username, 
            [FromQuery] int? latest, 
            [FromHeader(Name = "Authorization")] 
            string authorization, 
            [FromQuery] int? no = 100)
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
                return StatusCode(403, new
                {
                    status = 403,
                    error_message = "You are not authorized to use this resource!"
                });

            try
            {
                await _cheepService.GetAuthorFromName(username, 0);
            }
            catch (Exception _)
            {
                return StatusCode(404);
            }
               
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);
            var followers = await _cheepService.GetFollowerViewModelByUsername(username, no);
            return StatusCode(200, new
            {
                follows = followers
            });
        }

        [HttpPost]
        [Route("fllws/{username}")]
        public async Task<IActionResult> PostFollowUserAsync(
            string username, 
            [FromQuery] int? latest, 
            [FromHeader(Name = "Authorization")]
            string authorization,
            [FromBody] FollowAction payload)
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
                return StatusCode(403, new
                {
                    status = 403,
                    error_message = "You are not authorized to use this resource!"
                });

            Author author;
            try
            {
                author = await _cheepService.GetAuthorFromName(username, 0);
            }
            catch (Exception _)
            {
                return StatusCode(404);
            }
            
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);

            if (!payload.follow.IsNullOrEmpty())
            {
                var authorToFollow = await _cheepService.GetAuthorFromName(payload.follow, 0);
                _cheepService.AddFollowerId(author, authorToFollow.AuthorId);
            }
            
            if (!payload.unfollow.IsNullOrEmpty())
            {
                var authorToUnFollow = await _cheepService.GetAuthorFromName(payload.unfollow, 0);
                _cheepService.RemoveFollowerId(author, authorToUnFollow.AuthorId);
            }

            return StatusCode(204);
        }
        
        [HttpGet]
        [Route("latest")]
        public async Task<IActionResult> GetLatestAsync()
        {
            var latestId = await _latestsRepository.GetLatestId();
            return StatusCode(200, new
            {
                latest = latestId
            });
        }

        [HttpGet]
        [Route("msgs")]
        public async Task<IActionResult> GetMessagesAsync(
            [FromQuery] int? latest,
            [FromHeader(Name = "Authorization")] string authorization,
            [FromQuery] int? no = 100
            )
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
                return StatusCode(403, new
                {
                    status = 403,
                    error_message = "You are not authorized to use this resource!"
                });
            
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);
            
            var cmessages = await _cheepService.GetXAmountOfCheeps(no!.Value);
            return StatusCode(200, new
            {
                messages = cmessages
            });
        }
        
        [HttpGet]
        [Route("msgs/{username}")]
        public async Task<IActionResult> GetUserMessagesAsync(
            string username,
            [FromQuery] int? latest,
            [FromHeader(Name = "Authorization")] string authorization,
            [FromQuery] int? no = 100
        )
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
                return StatusCode(403, new
                {
                    status = 403,
                    error_message = "You are not authorized to use this resource!"
                });
            
            try
            {
                await _cheepService.GetAuthorFromName(username, 0);
            }
            catch (Exception _)
            {
                return StatusCode(404);
            }
            
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);
            
            var cmessages = _cheepService.GetXAmountUserCheepsByUsername(username, no!.Value);
            return StatusCode(200, new
            {
                messages = cmessages
            });
        }
        
        [HttpPost]
        [Route("msgs/{username}")]
        public async Task<IActionResult> PostUserMessagesAsync(
            string username,
            [FromQuery] int? latest,
            [FromHeader(Name = "Authorization")] string authorization,
            [FromBody] PostMessage payload,
            [FromQuery] int? no = 100
        )
        {
            if (authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
                return StatusCode(403, new
                {
                    status = 403,
                    error_message = "You are not authorized to use this resource!"
                });
            
            try
            {
                await _cheepService.GetAuthorFromName(username, 0);
            }
            catch (Exception _)
            {
                return StatusCode(404);
            }
            
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);

            var author = await _cheepService.GetAuthorFromName(username, 0);
            await _cheepService.CreateCheep(author.Email, payload.Content);
            
            return StatusCode(204);
        }
        
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> PostRegisterUserAsync(
            [FromQuery] int? latest,
            [FromBody] RegisterRequest payload
        )
        {
            if (latest.HasValue)
                _latestsRepository.AddLatest(latest);

            if (payload.Username.IsNullOrEmpty() ||
                payload.Email.IsNullOrEmpty() ||
                !payload.Email.Contains("@") ||
                payload.Pwd.IsNullOrEmpty())
                return StatusCode(400);
            
            //todo implement register
            
            return StatusCode(204);
        }
    }
}