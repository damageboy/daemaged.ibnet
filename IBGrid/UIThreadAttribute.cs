using System;
using System.Reflection;
using System.Windows.Forms;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Message = PostSharp.Extensibility.Message;

namespace IBGrid
{
  [Serializable]
  public sealed class UIThreadAttribute : MethodInterceptionAspect
  {
    public override void OnInvoke(MethodInterceptionArgs args)
    {
      Control c = null;
      if (args.Instance is Control)
        c = (Control) args.Instance;
      else if (args.Instance is ListViewItem)
        c = ((ListViewItem) args.Instance).ListView;
      else if (args.Instance is DataGridViewRow)
        c = ((DataGridViewRow)args.Instance).DataGridView;
      if (c.InvokeRequired)
        c.BeginInvoke(new Action(args.Proceed));
      else
        args.Proceed();
    }

 
    /// <summary>
    /// The method needs to be applied control side (target site).
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (method.IsStatic)
        Message.Write(new Message(SeverityType.Error,
                                  "AG0001",
                                  String.Format("Error in the custom attribute UIThreadAttribute on type '{0}', method '{1}': the method cannot be static", method.DeclaringType, method.Name),
                                  "UIThreadAttribute"));

      // Ensure method is declared within a Control-derived class
      if (!typeof(Control).IsAssignableFrom(method.DeclaringType) &&
          !typeof(ListViewItem).IsAssignableFrom(method.DeclaringType) &&
          !typeof(DataGridViewRow).IsAssignableFrom(method.DeclaringType))
        Message.Write(new Message(SeverityType.Error, 
                                  "AG0002",
                                  String.Format("Error in the custom attribute UIThreadAttribute on type '{0}', method '{1}': the argument '{0}' must derive from System.Windows.Forms.Control.", method.DeclaringType, method.Name),
                                  "UIThreadAttribute"));

      return base.CompileTimeValidate(method);
    }
  }
}