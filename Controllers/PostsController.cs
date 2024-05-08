using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAppp.BL.DtoModelsContainer;
using SocialMediaAppp.BL.MangersContainer.PostMangerContainer;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostManger _postManger;

        public PostsController(IPostManger postManger)
        {
            _postManger = postManger;
        }

        [HttpGet]
        [Route("GetUserPosts")]
        [Authorize]
        public ActionResult<List<PostDto>> GetUserPosts()
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var Posts =   _postManger.GetUserAllPosts(userId).ToList();
            return Posts; 
        }


        [HttpPost]
        [Route("creatPost")]
        [Authorize]
        public async Task<ActionResult> CreatPost( [FromForm] CreatPostDto postDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(" the tittle is required  or something went wrong ");
            }
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            //var userId = "78d837ec-1ada-4f0d-a36c-1c9d7daae22a";

          var result = await  _postManger.CreatPost(postDto, userId);
            if (result.SuccessFlag== false)
            {
                return BadRequest(result.Message);
            }
            return Created();
        }
    }
}
