using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.DAL.Models;
using SocialMediaAppp.BL.DtoModelsContainer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialMediaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManger;
        private readonly IConfiguration _configration;

        public AccountController(UserManager<AppUser> userManager , IConfiguration configuration)
        {
            _userManger = userManager;
            _configration = configuration;
        }

        #region user register 

        [HttpPost]
        [Route("UserRegister")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string FileNAme;
            try
            {
                if (registerDto.UserImage == null || registerDto.UserImage.Length == 0)
                {
                    FileNAme = "User.jpg";
                }
                else
                {
                    if (!registerDto.UserImage.ContentType.StartsWith("image/"))
                    {
                        return BadRequest("only image file are allowed");
                    }

                    var FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploades");
                    FileNAme = Guid.NewGuid().ToString() + Path.GetExtension(registerDto.UserImage.FileName);
                    var FullPath = Path.Combine(FolderPath, FileNAme);

                    using (var stream = new FileStream(FullPath, FileMode.Create))
                    {

                        await registerDto.UserImage.CopyToAsync(stream);
                    };
                }

                var userAddress = new UserAddress
                {
                    City = registerDto.City,
                    Country = registerDto.Country,
                };


                var NewUser = new AppUser()
                {
                    UserName = registerDto.FullName,
                    Email = registerDto.Email,
                    Image = FileNAme,
                    UserAddress = userAddress,
                    PhoneNumber = registerDto.PhoneNumber,

                };

                //store the new user 
                var result = await _userManger.CreateAsync(NewUser, registerDto.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                var Claims = new List<Claim>
                    {
                        new (ClaimTypes.NameIdentifier , NewUser.Id.ToString()),
                        new (ClaimTypes.Email , registerDto.Email),
                        new (ClaimTypes.StreetAddress , NewUser.UserAddress.ToString()),
                        new Claim(ClaimTypes.Role , "NormalUser"),
                        new Claim("UserImage", NewUser.Image),
                        new Claim("UserFullName", NewUser.UserName),
                        new Claim("PhoneNumber", NewUser.PhoneNumber),
                    };

                await _userManger.AddClaimsAsync(NewUser, Claims);
                return Created();


            }
            catch (Exception ex)
            {
                return StatusCode(500, $" internal server error {ex.Message}");
            }
        }
        #endregion

        #region  user login 
        [HttpPost]
        [Route("userLogin")]
        public async Task<ActionResult<TokenDto>> UserLogin(UserLoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("plz enter email and passord ");
            }

            var user = await _userManger.FindByEmailAsync(loginDto.Email); 
            if (user == null )
            {
                return BadRequest("plz enter valid  email and passord ");
            }

            var auth = await _userManger.CheckPasswordAsync(user, loginDto.Password);
            if (!auth)
            {
                return BadRequest("plz enter valid  email and passord ");
            }

            var userClaims = await _userManger.GetClaimsAsync(user);

            //generat user token 
            return generateToken((List<Claim>)userClaims); 
        }
        #endregion

        #region generat user token 

        private ActionResult<TokenDto> generateToken(List<Claim> userClaims)
        {
            var secretKey = _configration.GetValue<string>("TokenSecret");
            var Keybits = Encoding.ASCII.GetBytes(secretKey);
            var SymanticaKey = new SymmetricSecurityKey(Keybits);

            DateTime ExpireDate = DateTime.Now.AddDays(2);


            var MysigningCredentials = new SigningCredentials(SymanticaKey, SecurityAlgorithms.HmacSha256Signature);
            var jwt = new JwtSecurityToken(

               claims: userClaims,
               expires: ExpireDate,
               signingCredentials: MysigningCredentials
                 );
            var JwtString = new JwtSecurityTokenHandler().WriteToken(jwt);
                
            
            return new TokenDto { TokenString =  JwtString , ExpireDate = ExpireDate};
        }

        #endregion

        #region usersData

        [HttpGet]
        [Route("allusers")] 
        public ActionResult<List<FrinendDto>> allUser()
        {
            var users = _userManger.Users.Select(x => new FrinendDto
            {
                id = x.Id,
                name = x.UserName,
                Image = x.Image
            }).ToList();
            return users; 
        }




        #endregion

    }
}
