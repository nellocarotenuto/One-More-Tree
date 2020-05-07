using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back_End.Models;
using Azure.Storage.Blobs;
using System.Globalization;

namespace Back_End.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<Tree>>> GetTrees()
        {
            return await _databaseContext.Trees.ToListAsync();
        }

        // GET: api/Trees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tree>> GetTree(long id)
        {
            var tree = await _databaseContext.Trees.FindAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            return tree;
        }

        // PUT: api/Trees/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTree(long id, Tree tree)
        {
            if (id != tree.ID)
            {
                return BadRequest();
            }

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
        public async Task<ActionResult<Tree>> PostTree([FromForm] TreeApiPostRequest request)
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("trees");
            BlobClient blobClient = blobContainerClient.GetBlobClient(string.Format(@"{0}.png", Guid.NewGuid()));
            
            await blobClient.UploadAsync(request.Photo.OpenReadStream(), true);

            Tree tree = new Tree() {
                Photo = blobClient.Uri.ToString(),
                Description = request.Description,
                Latitude = Double.Parse(request.Coordinates.Split(",")[0], CultureInfo.InvariantCulture),
                Longitude = Double.Parse(request.Coordinates.Split(",")[1], CultureInfo.InvariantCulture),
                State = request.State,
                City = request.City,
                Date = DateTime.Now
            };
            
            _databaseContext.Trees.Add(tree);
            await _databaseContext.SaveChangesAsync();

            return CreatedAtAction("GetTree", new { id = tree.ID }, tree);
        }

        // DELETE: api/Trees/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tree>> DeleteTree(long id)
        {
            var tree = await _databaseContext.Trees.FindAsync(id);
            if (tree == null)
            {
                return NotFound();
            }

            _databaseContext.Trees.Remove(tree);
            await _databaseContext.SaveChangesAsync();

            return tree;
        }

        private bool TreeExists(long id)
        {
            return _databaseContext.Trees.Any(e => e.ID == id);
        }
    }
}
