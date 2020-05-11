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

        public IActionResult Create(int id)
        {
            return View(new AnswerEntity() { Id = id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] AnswerEntity answerEntity)
        {
            var id = answerEntity.Id;
            if (ModelState.IsValid)
            {
                var answer = await CreateAnswer(answerEntity);
                _context.Answers.Add(answer);
                await _context.SaveChangesAsync();

                var questionEntity = _context.Questions
                    .Include(x => x.Answers)
                    .FirstOrDefault(x => x.Id == id);
                questionEntity.Answers.Add(answer);
                _context.Questions.Update(questionEntity);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Questions", new { id = id, addView = false });
            }
            answerEntity.Id = id;
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
            if (user.Id != answerEntity.AuthorId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id), addView = false });
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
                    var answer = await _context.Answers.FirstOrDefaultAsync(x => x.Id == answerEntity.Id);
                    answer.Content = answerEntity.Content;
                    _context.Answers.Update(answer);
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
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id), addView = false });
            }
            return View(answerEntity);
        }

        public async Task<IActionResult> ChangeVotes(int id)
        {
            var answerEntity = await _context.Answers
                .Include(x => x.Visitors)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (answerEntity == null)
            {
                return NotFound();
            }

            var user = await GetCurentUser();
            var visitor = answerEntity.Visitors.FirstOrDefault(x => x.CustomerId == user.Id);
            if (visitor == null)
            {
                var visitorEntity = CreateVisitor(id, user.Id);
                _context.AnswerVisitors.Add(visitorEntity);
                await _context.SaveChangesAsync();

                answerEntity.Visitors.Add(visitorEntity);
                _context.Answers.Update(answerEntity);
            }
            else
            {
                visitor.IsVotes = !visitor.IsVotes;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id), addView = false });
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
            if (user.Id != answerEntity.AuthorId && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Questions", new { id = GetQuestionId(answerEntity.Id), addView = false });
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
            var questionId = GetQuestionId(id);
            _context.AnswerVisitors.RemoveRange(answerEntity.Visitors);
            _context.Comments.RemoveRange(answerEntity.Comments);
            _context.Answers.Remove(answerEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Questions", new { id = questionId, addView = false });
        }

        #region Private methods
        private bool AnswerEntityExists(int id)
        {
            return _context.Answers.Any(e => e.Id == id);
        }
        private async Task<AnswerEntity> CreateAnswer(AnswerEntity answerEntity)
        {
            var user = await GetCurentUser();
            answerEntity.Author = user.UserName;
            answerEntity.AuthorId = user.Id;
            answerEntity.Created = DateTime.Now;
            answerEntity.Id = 0;
            return answerEntity;
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
        private AnswerVisitorEntity CreateVisitor(int answerId, string userId)
        {
            var visitor = new AnswerVisitorEntity();
            visitor.Id = 0;
            visitor.CustomerId = userId;
            visitor.IsVotes = true;
            visitor.AnswerId = answerId;
            return visitor;
        }
        #endregion
    }
}
