using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Domain.Entities;

public partial class Ticket:BaseEntity
{
    public string TicketType { get; set; }
    public string Title { get; set; } 
    public string Description { get; set; }
    public string Status { get; set; }
    public Guid RenterId { get; set; }
    public Guid StaffId { get; set; }

    //relationship
    [ForeignKey(nameof(RenterId))]
    public Renter Renter { get; set; } = null!;
    [ForeignKey(nameof(StaffId))]
    public Staff Staff { get; set; } = null!;
}
