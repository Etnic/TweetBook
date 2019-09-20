using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tweetbook.Domain;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Contracts.v1.Response;
using TweetBook.Contracts.V1;
using TweetBook.Data;
using TweetBook.Extensions;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{
   // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        public async Task<IActionResult> Update([FromBody] UpdatePostRequest updatePostRequest)
        {
            var userOwnsPost = await this.postService.UserOwnsPostAsync(updatePostRequest.Id, HttpContext.GetUserId());

            if (!userOwnsPost)
                            {
                return BadRequest("You do not own this post");
            }

            var post = new Post() { Id = updatePostRequest.Id, Name = updatePostRequest.Name };

            var result = await this.postService.UpdatePost(post);

            if (result)
                Ok(result);

            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            var userOwnsPost = await this.postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest("You do not own this post");
            }

            var result = await this.postService.DeletePost(postId);

            if (result)
                Ok(result);

            return NotFound();
        }

        [HttpPost(ApiRoutes.Posts.Create)]
        public bool Create([FromRoute] string postId,[FromBody] CreatePostRequest createPostRequest)
         {
            var newPost = Guid.NewGuid();

            var post = new Post()
            {   Id = newPost,
                Name = createPostRequest.Name,
                UserId = HttpContext.GetUserId()                
            };

            var result = this.postService.CreatePost(post);

            return result.Result;
        }
    }
}