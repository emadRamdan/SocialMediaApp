using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAppp.BL.DtoModelsContainer;
using SocialMediaAppp.BL.MangersContainer.CommentMangerContainer;
using System.Security.Claims;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentManger _commentManger;

        public CommentsController(ICommentManger commentManger)
        {
            _commentManger = commentManger;
        }

        [HttpGet]
        [Route("GetAllComments")]
        public ActionResult<List<CommentDto>> GetAllComment(int PostId) {
           return _commentManger.GetAllComments(PostId).ToList();
        }


        [HttpPost]
        [Route("AddComment")]
        [Authorize]
        public ActionResult AddComment(AddCommentDto comment )
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            _commentManger.AddComment(comment, userId);
            return Created();
        }
    }
}
