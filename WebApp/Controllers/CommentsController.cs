using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entities;

namespace WebApp.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Comments.ToListAsync());
        }

        public IActionResult Create(int id)
        {
            return View(new CommentEntity() { Id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] CommentEntity commentEntity)
        {
            var answerId = commentEntity.Id;
            if (ModelState.IsValid)
            {
                var comment = await CreateComment(commentEntity);
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                var answerEntity = _context.Answers
                    .Include(x => x.Comments)
                    .FirstOrDefault(x => x.Id == answerId);
                answerEntity.Comments.Add(comment);
                _context.Answers.Update(answerEntity);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(comment.Id), addView = false});
            }
            commentEntity.Id = answerId;
            return View(commentEntity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentEntity = await _context.Comments.FindAsync(id);

            var user = await GetCurentUser();
            if (user.Id != commentEntity.AuthorId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(commentEntity.Id) });
            }

            if (commentEntity == null)
            {
                return NotFound();
            }
            return View(commentEntity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content")] CommentEntity commentEntity)
        {
            if (id != commentEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentEntity.Id);
                    comment.Content = commentEntity.Content;
                    _context.Comments.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentEntityExists(commentEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(commentEntity.Id) });
            }
            return View(commentEntity);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentEntity = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);

            var user = await GetCurentUser();
            if (user.Id != commentEntity.AuthorId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(commentEntity.Id) });
            }

            if (commentEntity == null)
            {
                return NotFound();
            }

            return View(commentEntity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commentEntity = await _context.Comments.FindAsync(id);
            _context.Comments.Remove(commentEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Private methods
        private bool CommentEntityExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
        private async Task<CommentEntity> CreateComment(CommentEntity commentEntity)
        {
            var user = await GetCurentUser();
            commentEntity.Author = user.UserName;
            commentEntity.AuthorId = user.Id;
            commentEntity.Created = DateTime.Now;
            commentEntity.Id = 0;
            return commentEntity;
        }
        private async Task<IdentityUser> GetCurentUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }
        private int GetQuestionId(int commentId)
        {
            var answerId = _context.Answers
                .Include(x => x.Comments)
                .FirstOrDefault(x => x.Comments.Any(a => a.Id == commentId))
                .Id;

            return _context.Questions
                .Include(x => x.Answers)
                .FirstOrDefault(x => x.Answers.Any(a => a.Id == answerId))
                .Id;
        }
        #endregion
    }
}
