using Core.Content;
using Data;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services
{
    public class ContentService : IContentService
    {
        private readonly IUnitOfWorks _unitOfWorks;

        public ContentService(IUnitOfWorks unitOfWorks)
        {
            _unitOfWorks = unitOfWorks;
        }
        public Articles GetArticleById(int id)
        {
            var query = _unitOfWorks.ArticlesRepository.GetDbSet();
            return query.Where(a => a.Id.Equals(id)).FirstOrDefault();
        }

        public IEnumerable<Articles> GetArticles()
        {
            var query = _unitOfWorks.ArticlesRepository.GetDbSet();
            return query.Select(a => a);
        }

        public void InsertArticle(Articles article)
        {
            _unitOfWorks.ArticlesRepository.Insert(article);
            _unitOfWorks.Save();
        }

        public void UpdateArticle(Articles article)
        {
            _unitOfWorks.ArticlesRepository.Update(article);
            _unitOfWorks.Save();
        }
    }
}
