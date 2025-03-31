using Bloggie.Web.Data;
using Bloggie.Web.Models.Domain;
using Bloggie.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.Web.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly BloggieDbContext bloggieDbContext;
        public TagRepository(BloggieDbContext bloggieDbContext) {

            this.bloggieDbContext = bloggieDbContext;
        }

        public BloggieDbContext BloggieDbContext { get; }

        public async Task<Tag> AddAsync(Tag tag)
        {
            await bloggieDbContext.Tags.AddAsync(tag);
            await bloggieDbContext.SaveChangesAsync();  // mandatory to save in db at last
            return tag;
        }

        public async Task<int> CountAsync()
        {
            return await bloggieDbContext.Tags.CountAsync();
        }

        public async Task<Tag?> DeleteAsync(Guid id)
        {
            var existingTag = await bloggieDbContext.Tags.FindAsync(id);
            if (existingTag != null)
            {
                bloggieDbContext.Tags.Remove(existingTag);
                await bloggieDbContext.SaveChangesAsync();
                return existingTag;
            }

            return null;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync(
            string? searchQuery, 
            string? sortBy, 
            string? sortDirection,
            int pageNumber = 1,
            int pageSize = 100)
        {
            var query = bloggieDbContext.Tags.AsQueryable();

            // filtering
            if (string.IsNullOrWhiteSpace(searchQuery) == false)
            {
                query = query.Where(x => x.Name.Contains(searchQuery)
                        || x.DisplayName.Contains(searchQuery));

            }

                // sorting
            if(string.IsNullOrWhiteSpace(sortBy) == false)
            {
                var isDesc = string.Equals(sortDirection, "Dsc", StringComparison.OrdinalIgnoreCase);

                if(string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
                }

                if(string.Equals(sortBy, "DisplayName", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(x => x.DisplayName) : query.OrderBy(x => x.DisplayName);
                }
            }

            //pagination
            // skip 0 take 5 -> page 1 of 5 result
            // skip 5 take next 5 -> page 2 of 5 result

            var skipResult = (pageNumber - 1) * pageSize;
            query = query.Skip(skipResult).Take(pageSize);

                return await query.ToListAsync();
            //return await bloggieDbContext.Tags.ToListAsync();
        }

        public async Task<Tag> GetAsync(Guid id)
        {
           var tag =  await bloggieDbContext.Tags.FirstOrDefaultAsync(x => x.Id == id);
            return tag;
        }

        public async Task<Tag?> UpdateAsync(Tag tag)
        {
            // take it out existing one
            var existingTag = await GetAsync(tag.Id);
            // now need to update
            if (existingTag != null)
            {
                existingTag.Name = tag.Name;
                existingTag.DisplayName = tag.DisplayName;

                // now save it
                await bloggieDbContext.SaveChangesAsync();
                return existingTag;
            }
            return null;

        }
    }
}
