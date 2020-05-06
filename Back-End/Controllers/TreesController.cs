using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back_End.Models;

namespace Back_End.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreesController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        public TreesController(DatabaseContext context)
        {
            _databaseContext = context;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Tree>> PostTree(Tree tree)
        {
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
