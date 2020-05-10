using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entities;

namespace WebApp.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnswersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Answers.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] AnswerEntity answerEntity)
        {
            if (ModelState.IsValid)
            {
                answerEntity = await CreateAnswer(answerEntity);
                _context.Add(answerEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id) });
            }
            return View(answerEntity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answerEntity = await _context.Answers.FindAsync(id);
            var user = await GetCurentUser();
            if (user.Id != answerEntity.AuthorId && User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id) });
            }
            if (answerEntity == null)
            {
                return NotFound();
            }
            return View(answerEntity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content")] AnswerEntity answerEntity)
        {
            if (id != answerEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(answerEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnswerEntityExists(answerEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id) });
            }
            return View(answerEntity);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answerEntity = await _context.Answers
                .FirstOrDefaultAsync(m => m.Id == id);
            var user = await GetCurentUser();
            if (user.Id != answerEntity.AuthorId && User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id) });
            }
            if (answerEntity == null)
            {
                return NotFound();
            }

            return View(answerEntity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var answerEntity = await _context.Answers
                .Include(x => x.Comments)
                .Include(x => x.Visitors)
                .FirstOrDefaultAsync(x => x.Id == id);
            _context.AnswerVisitors.RemoveRange(answerEntity.Visitors);
            _context.Comments.RemoveRange(answerEntity.Comments);
            _context.Answers.Remove(answerEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Private methods
        private bool AnswerEntityExists(int id)
        {
            return _context.Answers.Any(e => e.Id == id);
        }
        private async Task<AnswerEntity> CreateAnswer(AnswerEntity questionEntity)
        {
            var user = await GetCurentUser();
            questionEntity.Author = user.UserName;
            questionEntity.AuthorId = user.Id;
            questionEntity.Created = DateTime.Now;
            return questionEntity;
        }
        private async Task<IdentityUser> GetCurentUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }
        private int GetQuestionId(int answerId)
        {
            return _context.Questions
                .Include(x => x.Answers)
                .FirstOrDefault(x => x.Answers.Any(a => a.Id == answerId))
                .Id;
        }
        #endregion
    }
}
