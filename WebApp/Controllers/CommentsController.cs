﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entities;

namespace WebApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Comments.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentEntity = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commentEntity == null)
            {
                return NotFound();
            }

            return View(commentEntity);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Created,Author,Content,AuthorId")] CommentEntity commentEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(commentEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(commentEntity);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentEntity = await _context.Comments.FindAsync(id);
            if (commentEntity == null)
            {
                return NotFound();
            }
            return View(commentEntity);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Created,Author,Content,AuthorId")] CommentEntity commentEntity)
        {
            if (id != commentEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(commentEntity);
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
                return RedirectToAction(nameof(Index));
            }
            return View(commentEntity);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentEntity = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commentEntity == null)
            {
                return NotFound();
            }

            return View(commentEntity);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commentEntity = await _context.Comments.FindAsync(id);
            _context.Comments.Remove(commentEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentEntityExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}