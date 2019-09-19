using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1
{
    public static class ApiRoutes
    {
        private const string Root = "api";
        private const string Version = "v1";
        private const string Base = Root + "/" + Version;

        public static class Posts
        {
            public const string GetAll = Base + "/" + "posts";
            public const string GetPostById = Base + "/" + "posts/{postId}";
            public const string Create = Base + "/" + "create/{postId}";
            public const string Update = Base + "/" + "update/{postId}";
            public const string Delete = Base + "/" + "delete/{postId}";
        }
    }
}
