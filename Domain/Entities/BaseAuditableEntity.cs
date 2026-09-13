using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class BaseAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
