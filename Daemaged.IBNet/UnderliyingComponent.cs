using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemaged.IBNet
{
  public class UnderliyingComponent
  {
    /// <summary>
    /// Contract Id
    /// </summary>
    public int ConId { get; set; }

    /// <summary>
    /// Delta Value
    /// </summary>
    public double Delta { get; set; }

    /// <summary>
    /// Price
    /// </summary>
    public double Price { get; set; }

  }
}
