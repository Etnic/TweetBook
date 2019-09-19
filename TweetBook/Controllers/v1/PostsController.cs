using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tweetbook.Domain;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Contracts.v1.Response;
using TweetBook.Contracts.V1;
using TweetBook.Data;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{
    public class PostsController : Controller
    {
        private readonly IPostService postService;

        public PostsController(IPostService postService)
        {
            this.postService = postService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IList<Post>> GetAll()
        {
            return await this.postService.GetAllPosts();
        }

        [HttpGet(ApiRoutes.Posts.GetPostById)]
        public IActionResult GetPostById([FromRoute] Guid postId)
        {
            var post = this.postService.GetPostById(postId);

            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public bool Update([FromBody] UpdatePostRequest updatePostRequest)
        {
            var post = new Post() { Id = updatePostRequest.Id, Name = updatePostRequest.Name };

            var result = this.postService.UpdatePost(post);

            return result.Result;
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public bool Delete([FromRoute] Guid postId)
        {
            var result = this.postService.DeletePost(postId);

            return result.Result;
        }

        [HttpPost(ApiRoutes.Posts.Create)]
        public bool Create([FromBody] CreatePostRequest createPostRequest)
        {
            var post = new Post() { Id = createPostRequest.Id, Name = createPostRequest.Name };

            var result = this.postService.CreatePost(post);

            return result.Result;
        }
    }
}