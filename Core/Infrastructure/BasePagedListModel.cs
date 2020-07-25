using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
    public abstract partial class BasePagedListModel<T> where T : class
    {
        public BasePagedListModel()
        {
            this.Meta = new PageListMetadata();
        }

        /// <summary>
        /// Gets or sets data records
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        public PageListMetadata Meta { get; set; }
    }
}
