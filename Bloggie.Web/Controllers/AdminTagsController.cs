using System.Security.Cryptography.X509Certificates;
using Bloggie.Web.Data;
using Bloggie.Web.Models.Domain;
using Bloggie.Web.Models.ViewModels;
using Bloggie.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.Web.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminTagsController : Controller
    {
        public ITagRepository tagRepository;

        public AdminTagsController(ITagRepository tagRepository)
        {
            this.tagRepository = tagRepository;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        
        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(AddTagRequest addTagRequest)
        {
            ValidateAddTagRequest(addTagRequest);

            if (!ModelState.IsValid)
            {
                return View();
            }
            // here doing mapping
            var tag = new Tag
            {
                Name = addTagRequest.Name,
                DisplayName = addTagRequest.DisplayName
            };

            await tagRepository.AddAsync(tag);

            return RedirectToAction("List");
        }

        [HttpGet]
        [ActionName("List")]      // if not define this, then by default taken function name 
        public async Task<IActionResult> List(
            string? searchQuery, 
            string? sortBy, 
            string? sortDirection,
            int pageSize = 3,
            int pageNumber = 1
            )
        {

            var totalRecords = await tagRepository.CountAsync();
            var totalPages = Math.Ceiling((decimal)totalRecords / pageSize);

            if(pageNumber > totalPages)
            {
                pageNumber--;
            }

            if(pageNumber < 1)
            {
                pageNumber++;
            }

            ViewBag.TotalPages = totalPages;

            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.PageSize = pageSize;
            ViewBag.PageNumber = pageNumber;

            // use dbContext to read tags
            var tags = await tagRepository.GetAllAsync(searchQuery, sortBy, sortDirection, pageNumber, pageSize);
            return View(tags);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // we need to take id from which clicked
            var existingTag = await tagRepository.GetAsync(id);

            // now need to pass that in view model
            if (existingTag != null)
            {
                var editTagRequest = new EditTagRequest
                {
                    Id = existingTag.Id,
                    Name = existingTag.Name,
                    DisplayName = existingTag.DisplayName
                };

                return View(editTagRequest);
            }

            return View(null);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditTagRequest editTagRequest)
        {
            var tag = new Tag
            {
                Id = editTagRequest.Id,
                Name = editTagRequest.Name,
                DisplayName = editTagRequest.DisplayName
            };

            // need to call to update
            var updatedTag = await tagRepository.UpdateAsync(tag);
            if(updatedTag != null)
            {
                // show success notification
                return RedirectToAction("List");
            }
            
            // show error notification
            return RedirectToAction("Edit", new { id = editTagRequest.Id });

        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deletedTag = await tagRepository.DeleteAsync(id);
            if(deletedTag != null)
            {
                // show success message
                return RedirectToAction("List");
            }
            

            // show error message
            return View();
        }

        public void ValidateAddTagRequest(AddTagRequest addTagRequest)
        {
            if(addTagRequest.Name is not null && addTagRequest.DisplayName is not null)
            {
                if(addTagRequest.Name == addTagRequest.DisplayName)
                {
                    ModelState.AddModelError("DisplayName", "Display name cannot be same as Name");
                }
            }
        }



    }
}
