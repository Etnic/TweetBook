using System;
using System.Collections.Generic;
using Tweetbook.Domain;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Data;
using Microsoft.EntityFrameworkCore;

namespace TweetBook.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext dataContext;
        
        public PostService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<bool> UpdatePost(Post post)
        {
            this.dataContext.dbPosts.Update(post);

            var result = await this.dataContext.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeletePost(Guid postId)
        {
            var post = await this.dataContext.dbPosts.FindAsync(postId);

            if (post != null)
            {
                this.dataContext.dbPosts.Remove(post);

                var result = await this.dataContext.SaveChangesAsync();

                return result > 0;
            }

            return false;
        }

        public async Task<bool> CreatePost(Post post)
        {
            await this.dataContext.dbPosts.AddAsync(post);

            var result = await this.dataContext.SaveChangesAsync();

            return result > 0;
        }

        public Post GetPostById(Guid postId)
        {
            return this.dataContext.dbPosts.SingleOrDefault(x => x.Id == postId);
        }
        
        public async Task<IList<Post>> GetAllPosts()
        {
            return await this.dataContext.dbPosts.ToListAsync();
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            var post = await this.dataContext.dbPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }

            if (post.UserId != userId)
                return false;

            return true;
        }
    }
}