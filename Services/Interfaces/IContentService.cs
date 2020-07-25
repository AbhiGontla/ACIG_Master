using Core.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface IContentService
    {
        void InsertArticle(Articles Article);
        void UpdateArticle(Articles article);
        Articles GetArticleById(int id);
        IEnumerable<Articles> GetArticles();
    }
}
