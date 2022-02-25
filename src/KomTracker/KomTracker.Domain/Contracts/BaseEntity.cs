using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Domain.Contracts;

public class BaseEntity
{
    public DateTime AuditCD { get; set; }
    public DateTime? AuditMD { get; set; }
}
