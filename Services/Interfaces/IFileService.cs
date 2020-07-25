using Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface IFileService
    {
        void InsertBanner(Banners banner);
        IEnumerable<Banners> GetBanners();
        void UpdateBanner(Banners banner);
    }
}
