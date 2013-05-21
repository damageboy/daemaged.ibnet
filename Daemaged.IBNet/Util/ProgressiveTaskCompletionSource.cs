using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if NET_4_5
namespace Daemaged.IBNet.Util
{
  internal class ProgressiveTaskCompletionSource<T> : TaskCompletionSource<T>, IFaultable
  {    
    public T Value { get; set; }

    public void SetCompleted()
    {
      SetResult(Value);
    }
  }

  internal interface IFaultable
  {
    bool TrySetException(Exception e);
  }
}
#endif