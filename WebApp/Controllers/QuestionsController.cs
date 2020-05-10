﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entities;

namespace WebApp.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Questions.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionEntity = await _context.Questions
                .Include(x => x.Answers)
                .Include(x => x.Visitors)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (questionEntity == null)
            {
                return NotFound();
            }

            return View(questionEntity);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Id,Content,Tags")] QuestionEntity questionEntity)
        {
            if (ModelState.IsValid)
            {
                questionEntity = await CreateQuestion(questionEntity);
                _context.Add(questionEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(questionEntity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var questionEntity = await _context.Questions.FindAsync(id);
            var user = await GetCurentUser();
            if (user.Id != questionEntity.AuthorId && User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            if (questionEntity == null)
            {
                return NotFound();
            }
            return View(questionEntity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Id,Content,Tags")] QuestionEntity questionEntity)
        {
            if (id != questionEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionEntityExists(questionEntity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(questionEntity);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionEntity = await _context.Questions
                .FirstOrDefaultAsync(m => m.Id == id);
            var user = await GetCurentUser();
            if (user.Id != questionEntity.AuthorId && User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            if (questionEntity == null)
            {
                return NotFound();
            }

            return View(questionEntity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var questionEntity = await _context.Questions
                .Include(x => x.Answers)
                .Include(x => x.Visitors)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);
            foreach(var answer in questionEntity.Answers)
            {
                var answerEntity = await _context.Answers
                    .Include(x => x.Comments)
                    .FirstOrDefaultAsync(m => m.Id == id);
                _context.Comments.RemoveRange(answerEntity.Comments);
                _context.Answers.Remove(answerEntity);
            }
            _context.QuestionVisitors.RemoveRange(questionEntity.Visitors);
            _context.QuestionTags.RemoveRange(questionEntity.Tags);
            _context.Questions.Remove(questionEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionEntityExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }

        private async Task<QuestionEntity> CreateQuestion(QuestionEntity questionEntity)
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
    }
}
 