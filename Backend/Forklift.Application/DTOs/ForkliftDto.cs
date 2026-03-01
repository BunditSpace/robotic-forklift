using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forklift.Application.DTOs;

/// <summary>
/// forklift data transfer object conssisting of name, model number and manufacturing date
/// </summary>
public class ForkliftDto
{
    /// unique identifier of the forklift
    public Guid Id { get; set; }
    
    /// name of the forklift
    public string Name { get; set; } = string.Empty;

    /// model number of the forklift
    public string ModelNumber { get; set; } = string.Empty;

    /// manufacturing date of the forklift
    public DateTime? ManufacturingDate { get; set; }
}
