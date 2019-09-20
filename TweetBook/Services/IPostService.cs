using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Domain;

namespace TweetBook.Services
{
    public interface IPostService
    {
        Task<IList<Post>> GetAllPosts();

        Post GetPostById(Guid postId);

        Task<bool> UpdatePost(Post post);

        Task<bool> DeletePost(Guid postId);

        Task<bool> CreatePost(Post post);

        Task<bool> UserOwnsPostAsync(Guid postId, string userId);
    }
}
