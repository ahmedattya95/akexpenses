﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkExpenses.Api.Data;
using AkExpenses.Api.Utitlity;
using AkExpenses.Models;
using AkExpenses.Models.Shared;
using AkExpenses.Models.Shared.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkExpenses.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OutcomesController : ControllerBase
    {
        private const int PAGE_SIZE = 10;
        private readonly ApplicationDbContext db;

        public OutcomesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        #region Get

        //Returns all outcomes for the logged in user
        [HttpGet]
        public async Task<IActionResult> Get(string query, int? page)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "";
            }

            if (!page.HasValue || page <= 0)
            {
                page = 1;
            }

            //Get the logged in account
            var account = await getAccount();
            var totalOutcomes = db.Outcomes.Where(o => o.AccountId == account.Id).Count();

            //Get all outcomes
            var outcomes = db.Outcomes.Where(o => o.AccountId == account.Id)
                .Include(o => o.MoneyType)
                .Include(o => o.Category)
                .OrderByDescending(o => o.PayDate)
                .ThenByDescending(o => o.CreatedDate)
                .Skip(PAGE_SIZE * (page.Value - 1))
                .Take(PAGE_SIZE);


            return Ok(new HttpCollectionResponse<object>
            {
                IsSuccess = true,
                Message = "Outcomes have been retrieved successfully.",
                Count = totalOutcomes,
                Page = page.Value,
                PageSize = PAGE_SIZE,
                SearchQuery = query,
                TotalPages = DataHelper.GetTotalPages(totalOutcomes, PAGE_SIZE),
                Values = outcomes.Select(o => new
                {
                    o.Id,
                    o.Title,
                    o.Description,
                    o.PayDate,
                    o.MoneyTypeId,
                    MoneyType = o.MoneyType.Name,
                    Category = o.Category.Name,
                    o.CategoryId,
                    o.BillId
                })
            });
        }

        //Returns a specific outcome by id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            //Validate the id
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var outcome = await db.Outcomes.FindAsync(id);

            if (outcome == null)
            {
                return NotFound();
            }

            return Ok(new HttpSingleResponse<Outcome>
            {
                IsSuccess = true,
                Message = "Outcome has been retrieved successfully.",
                Value = outcome
            });
        }

        #endregion

        #region Post

        //Adds a new outcome to the database
        [HttpPost]
        public async Task<IActionResult> Post(OutcomeViewModel model)
        {
            //Validate the model
            if (ModelState.IsValid)
            {
                //Get the logged in account
                var account = await getAccount();

                //Get the money type
                var moneyType = await db.MoneyTypes.FindAsync(model.MoneyTypeId);

                if (moneyType == null)
                {
                    return NotFound();
                }

                //Get the category
                var category = await db.Categories.FindAsync(model.CategoryId);

                if (category == null)
                {
                    return NotFound();
                }

                //Get the bill
                var bill = await db.Bills.FindAsync(model.BillId);

                //Create new outcome
                var newOutcome = new Outcome
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Description = model.Description,
                    Amount = model.Amount,
                    PayDate = model.PayDate,
                    BillId = model.BillId != null ? model.BillId : null,
                    CategoryId = model.CategoryId,
                    MoneyTypeId = model.MoneyTypeId,
                    AccountId = account.Id
                };

                //Add the new outcome to the database
                await db.Outcomes.AddAsync(newOutcome);
                await db.SaveChangesAsync();

                return Ok(new HttpSingleResponse<Outcome>
                {
                    IsSuccess = true,
                    Message = "Outcome has been added successfully",
                    Value = newOutcome
                });
            }

            return this.FixedBadRequest("Model sent has some errors.");
        }

        #endregion

        #region Put

        //Edits the info of a specific outcome
        [HttpPut]
        public async Task<IActionResult> Put(OutcomeViewModel model)
        {
            //Validate the model
            if (ModelState.IsValid)
            {
                //Get the outcome
                var outcome = await db.Outcomes.FindAsync(model.Id);

                if (outcome == null)
                {
                    return NotFound();
                }

                //Get the money type
                var moneyType = await db.MoneyTypes.FindAsync(model.MoneyTypeId);

                if (moneyType == null)
                {
                    return NotFound();
                }

                //Get the category
                var category = await db.Categories.FindAsync(model.CategoryId);

                if (category == null)
                {
                    return NotFound();
                }

                //Get the bill
                var bill = await db.Bills.FindAsync(model.BillId);

                //Update outcome info
                outcome.Title = model.Title;
                outcome.Description = model.Description;
                outcome.Amount = model.Amount;
                outcome.PayDate = model.PayDate;
                outcome.MoneyTypeId = model.MoneyTypeId;
                outcome.CategoryId = model.CategoryId;
                outcome.BillId = model.BillId;

                return Ok(new HttpSingleResponse<object>
                {
                    IsSuccess = true,
                    Message = "Outcome has been udpated successfully.",
                    Value = outcome
                });
            }

            return this.FixedBadRequest("Outcome has been edited successfully.");
        }

        #endregion

        #region Delete

        //Delete a specific outcome by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //Validate the id
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            //Get the outcome
            var outcome = await db.Outcomes.FindAsync(id);

            if (outcome == null)
            {
                return NotFound();
            }

            //Delete the outcome
            db.Outcomes.Remove(outcome);

            //Save
            await db.SaveChangesAsync();

            return Ok(new HttpSingleResponse<Outcome>
            {
                IsSuccess = true,
                Message = "Outcome has been deleted successfully.",
                Value = outcome
            });
        }

        #endregion

        #region Helper Functions

        //Returns the current logged in account
        private async Task<Account> getAccount()
        {
            var id = User.Claims.SingleOrDefault(c => c.Type == "AccountId").Value;
            return await db.Accounts.FindAsync(id);
        }

        #endregion
    }
}