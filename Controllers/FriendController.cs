using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.DAL.context;
using SocialMediaApp.DAL.Models;
using SocialMediaApp.UnitOfWorkContainer;
using SocialMediaAppp.BL.DtoModelsContainer;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SocialMediAppContext _context;

        //private readonly IUnitOfWork _unitOfWork;

        public FriendController(UserManager<AppUser> userManager , SocialMediAppContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        #region add frind



        [HttpGet]
        [Route("addFrind")]
        public async Task<IActionResult> AddFrind(string friendUserId)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            // Retrieve the friend user
            var friendUser = await _userManager.FindByIdAsync(friendUserId);
            if (friendUser == null)
            {
                return NotFound("Friend user not found");
            }

            // Create a new Friendship object
            var friendship = new Friendship
            {
                UserId = currentUser.Id,
                FriendId = friendUser.Id,
                CreatedAt = DateTime.Now,
                Status = FriendshipStatus.Pending // You can set the initial status as needed
            };


            currentUser.Friendships ??= new List<Friendship>();
            currentUser.Friendships.Add(friendship);

            friendUser.Friendships ??= new List<Friendship>();
            friendUser.Friendships.Add(friendship);

            _context.Set<Friendship>().Add(friendship);
            _context.SaveChanges();
            //await _context.SaveChangesAsync();

            return Ok();



        }

        #endregion


        [HttpGet]
        [Route("getAllFrinds")]
        public async Task<ActionResult<List<Friendship>>> GetAllFriends()
        {
            var currentUser = await _userManager.GetUserAsync(User);


            if (currentUser == null)
            {
                return BadRequest("can;t find this user ");
            }

            currentUser.Friendships ??= new List<Friendship>();

            var friendList = currentUser.Friendships.ToList();

            return friendList;
        }
    }
}
