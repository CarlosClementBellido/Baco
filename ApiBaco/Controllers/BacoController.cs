using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Model;
using Service.Services.BacoService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;

namespace ApiBaco.Controllers
{
    [Route("api/[controller]")]
    public class BacoController : Controller
    {

        #region Properties

        private readonly IBacoService bacoService = new BacoService();

        #endregion

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("friends")]
        public IActionResult GetAllUsers()
        {
            return Json(bacoService.GetAllUsers());
        }

        [HttpGet("friends/{id}")]
        public IEnumerable<dynamic> GetFriendsById(int id)
        {
            return bacoService.GetFriendsById(id);
        }

        [HttpGet("groups")]
        public IEnumerable<dynamic> GetGroups()
        {
            return bacoService.GetGroups();
        }

        [HttpGet("groups/user/{id}")]
        public IEnumerable<dynamic> GetGroups(int id)
        {
            return bacoService.GetGroups(id);
        }

        [HttpGet("subscription/{id}")]
        public IEnumerable<dynamic> GetSubscriptionsById(int id)
        {
            return bacoService.GetSubscriptionsById(id);
        }

        [HttpGet("autentication/{nick}/{hash?}")]
        public bool CheckHash(string nick, string hash)
        {
            return bacoService.CheckHash(nick, hash);
        }

        [HttpGet("find/{nick?}")]
        public IEnumerable<dynamic> FindUser(string nick)
        {
            return bacoService.FindUser(nick);
        }

        [HttpGet("petitions/{id}")]
        public IEnumerable<dynamic> GetFriendPetitions(int id)
        {
            return bacoService.GetFriendPetitions(id);
        }

        [HttpGet("image/{nick}")]
        public FileStreamResult GetProfilePicture(string nick)
        {
            string path = $"Images\\{nick}.png";
            FileStream image;
            if (System.IO.File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path)))
                image = System.IO.File.OpenRead(path);
            else
                image = System.IO.File.OpenRead("Images/default.png");

            return File(image, "Image/jpeg");
        }

        [HttpGet("id/{nick}")]
        public int? GetId(string nick)
        {
            return bacoService.GetId(nick);
        }

        [HttpGet("mail/available/{mail}")]
        public bool MailAvailability(string mail)
        {
            return bacoService.MailAvailability(mail);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PostUser([FromBody]Users user)
        {
            Response response = bacoService.PostUser(user);
            if (response.Success)
                return Created(response.Message.ToString(), user);
            else
                return BadRequest(response.Message);
        }

        [HttpPost("friends/{idPetitioner}/{idRequested}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult PostNewFriend(int idPetitioner, int idRequested)
        {
            Response response = bacoService.PostFriendPetition(idPetitioner, idRequested);
            if (response.Success)
                return Ok(response.Message);
            else
                return Conflict(response.Message);
        }

        [HttpPut("friends/{idPetitioner}/{idRequested}/{accepted}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutFriendPetition(int idPetitioner, int idRequested, bool accepted)
        {
            Response response = bacoService.PutFriendPetition(idPetitioner, idRequested, accepted);
            if (response.Success)
                return Ok(response.Message);
            else
                return NotFound(response.Message);
        }

        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutUser(int id, [FromBody]Users user)
        {
            Response response = bacoService.PutUser(id, user);
            if (response.Success)
                return Ok(response.Message);
            else
                return NotFound(response.Message);
        }

        [HttpDelete("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteUser(int id)
        {
            Response response = bacoService.DeleteUser(id);
            if (response.Success)
                return Ok(response.Message);
            else
                return NotFound(response.Message);
        }

        [HttpPost("image/{nick}")]
        [HttpPut("image/{nick}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PostPrutProfilePicture(IFormFile image, string nick)
        {
            if (GetId(nick) != null)
            {
                string path = $"Images\\{nick}.png";
                try
                {
                    using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                    image.CopyTo(fs);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                return Ok("Profile picture updated");
            }
            return BadRequest("User not found");
        }

    }
}
