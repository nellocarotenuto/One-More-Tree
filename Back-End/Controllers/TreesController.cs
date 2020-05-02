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
        private readonly OneMoreTreeContext _context;

        public TreesController(OneMoreTreeContext context)
        {
            _context = context;
        }

        // GET: api/Trees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tree>>> GetTrees()
        {
            return await _context.Trees.ToListAsync();
        }

        // GET: api/Trees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tree>> GetTree(long id)
        {
            var tree = await _context.Trees.FindAsync(id);

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

            _context.Entry(tree).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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
            _context.Trees.Add(tree);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTree", new { id = tree.ID }, tree);
        }

        // DELETE: api/Trees/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tree>> DeleteTree(long id)
        {
            var tree = await _context.Trees.FindAsync(id);
            if (tree == null)
            {
                return NotFound();
            }

            _context.Trees.Remove(tree);
            await _context.SaveChangesAsync();

            return tree;
        }

        private bool TreeExists(long id)
        {
            return _context.Trees.Any(e => e.ID == id);
        }
    }
}
