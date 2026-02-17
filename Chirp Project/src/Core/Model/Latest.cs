using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Model;

public class Latest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LatestEntryId { get; set; }
    public required int LatestCommandId { get; set; }
    public required DateTime UpdatedDate { get; set; }
    public required DateTime CreatedDate { get; set; }
}