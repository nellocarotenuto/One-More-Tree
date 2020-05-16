using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Back_End.Models;

namespace Back_End.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TreesController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly BlobServiceClient _blobServiceClient;

        public TreesController(DatabaseContext context, BlobServiceClient blobServiceClient)
        {
            _databaseContext = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: api/Trees
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TreeResponseDTO>>> GetTrees()
        {
            List<TreeResponseDTO> trees = new List<TreeResponseDTO>();

            foreach (Tree tree in _databaseContext.Trees)
            {
                trees.Add(TreeToDTO(tree));
            }

            return trees.ToList();
        }

        // GET: api/Trees/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TreeResponseDTO>> GetTree(long id)
        {
            Tree tree = await _databaseContext.Trees.FindAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            return TreeToDTO(tree);
        }

        // PUT: api/Trees/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTree(long id, TreePutRequestDTO request)
        {
            long userId = long.Parse(User.Claims.Where(claim => claim.Type == JwtRegisteredClaimNames.Sub).First().Value);
            Tree tree = await _databaseContext.Trees.FindAsync(id);

            if (id != request.Id)
            {
                return BadRequest();
            }

            if (tree.User.Id != userId)
            {
                return Unauthorized();
            }

            tree.Description = request.Description;
            _databaseContext.Entry(tree).State = EntityState.Modified;

            try
            {
                await _databaseContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TreeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Trees
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<TreeResponseDTO>> PostTree([FromForm] TreePostRequestDTO request)
        {
            // Upload the photo to Azure Blobs
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("trees");
            BlobClient blobClient = blobContainerClient.GetBlobClient(string.Format(@"{0}.png", Guid.NewGuid()));

            await blobClient.UploadAsync(request.Photo.OpenReadStream(), new BlobHttpHeaders() { ContentType = "image/png" });

            // Retrieve the user from the database
            long userId = long.Parse(User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value);

            IEnumerable<User> users = from user in _databaseContext.Users
                                      where user.Id == userId
                                      select user;

            // Set tree properties
            Tree tree = new Tree() {
                Photo = blobClient.Uri.ToString(),
                Description = request.Description,
                Latitude = Double.Parse(request.Coordinates.Split(",")[0], CultureInfo.InvariantCulture),
                Longitude = Double.Parse(request.Coordinates.Split(",")[1], CultureInfo.InvariantCulture),
                State = request.State,
                City = request.City,
                Date = DateTime.Now,
                User = users.First()
            };

            // Save the tree to the database
            _databaseContext.Trees.Add(tree);
            await _databaseContext.SaveChangesAsync();

            return CreatedAtAction("GetTree", new { id = tree.Id }, TreeToDTO(tree));
        }

        // DELETE: api/Trees/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tree>> DeleteTree(long id)
        {
            long userId = long.Parse(User.Claims.Where(claim => claim.Type == JwtRegisteredClaimNames.Sub).First().Value);
            
            Tree tree = await _databaseContext.Trees.FindAsync(id);
            
            if (tree == null)
            {
                return NotFound();
            }

            if (tree.User.Id != userId)
            {
                return Unauthorized();
            }

            _databaseContext.Trees.Remove(tree);
            await _databaseContext.SaveChangesAsync();

            return tree;
        }

        [HttpGet("/api/users/{id}/trees")]
        [AllowAnonymous]
        public async Task<ActionResult<TreeResponseDTO>> GetUserTrees(long id)
        {
            User user = await _databaseContext.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            List<TreeResponseDTO> trees = new List<TreeResponseDTO>();

            foreach (Tree tree in _databaseContext.Trees.Where(tree => tree.User.Id == id))
            {
                trees.Add(TreeToDTO(tree));
            }

            return Ok(trees);
        }

        private bool TreeExists(long id)
        {
            return _databaseContext.Trees.Any(e => e.Id == id);
        }
        private bool UserExists(long id)
        {
            return _databaseContext.Users.Any(e => e.Id == id);
        }

        private TreeResponseDTO TreeToDTO(Tree tree)
        {
            return new TreeResponseDTO
            {
                Id = tree.Id,
                Description = tree.Description,
                Photo = tree.Photo,
                Latitude = tree.Latitude,
                Longitude = tree.Longitude,
                City = tree.City,
                State = tree.State,
                Date = tree.Date,
                User = new UserResponseDTO
                {
                    Id = tree.User.Id,
                    Name = tree.User.Name,
                    Picture = tree.User.Picture
                }
            };
        }
    }
}
