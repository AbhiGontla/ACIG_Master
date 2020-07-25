using Core.Domain;
using Data;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWorks _unitOfWorks;
        public FileService(IUnitOfWorks unitOfWorks)
        {
            this._unitOfWorks = unitOfWorks;
        }
        public IEnumerable<Banners> GetBanners()
        {
            var query = _unitOfWorks.BannersRepository.GetDbSet();
            return query.Select(b => b);
        }

        public void InsertBanner(Banners banner)
        {
            _unitOfWorks.BannersRepository.Insert(banner);
            _unitOfWorks.Save();
        }
        public void UpdateBanner(Banners banner)
        {
            _unitOfWorks.BannersRepository.Update(banner);
            _unitOfWorks.Save();
        }
    }
}
