#region Copyright (c) 2007 by Dan Shechter

////////////////////////////////////////////////////////////////////////////////////////
////
//  IBNet, an Interactive Brokers TWS .NET Client & Server implmentation
//  by Dan Shechter
////////////////////////////////////////////////////////////////////////////////////////
//  License: MPL 1.1/GPL 2.0/LGPL 2.1
//  
//  The contents of this file are subject to the Mozilla Public License Version 
//  1.1 (the "License"); you may not use this file except in compliance with 
//  the License. You may obtain a copy of the License at 
//  http://www.mozilla.org/MPL/
//  
//  Software distributed under the License is distributed on an "AS IS" basis,
//  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
//  for the specific language governing rights and limitations under the
//  License.
//  
//  The Original Code is any part of this file that is not marked as a contribution.
//  
//  The Initial Developer of the Original Code is Dan Shecter.
//  Portions created by the Initial Developer are Copyright (C) 2007
//  the Initial Developer. All Rights Reserved.
//  
//  Contributor(s): None.
//  
//  Alternatively, the contents of this file may be used under the terms of
//  either the GNU General Public License Version 2 or later (the "GPL"), or
//  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
//  in which case the provisions of the GPL or the LGPL are applicable instead
//  of those above. If you wish to allow use of your version of this file only
//  under the terms of either the GPL or the LGPL, and not to allow others to
//  use your version of this file under the terms of the MPL, indicate your
//  decision by deleting the provisions above and replace them with the notice
//  and other provisions required by the GPL or the LGPL. If you do not delete
//  the provisions above, a recipient may use your version of this file under
//  the terms of any one of the MPL, the GPL or the LGPL.
////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.ComponentModel;
using System.Reflection;

namespace Daemaged.IBNet
{
  /// <summary>
  /// Flags enumeration type converter.
  /// </summary>
  public class FlagsEnumConverter : EnumConverter
  {
    #region Nested type: EnumFieldDescriptor

    /// <summary>
    /// This class represents an enumeration field in the property grid.
    /// </summary>
    protected class EnumFieldDescriptor : SimplePropertyDescriptor
    {
      #region Fields

      /// <summary>
      /// Stores the context which the enumeration field descriptor was created in.
      /// </summary>
      readonly ITypeDescriptorContext fContext;

      #endregion

      #region Methods

      /// <summary>
      /// Creates an instance of the enumeration field descriptor class.
      /// </summary>
      /// <param name="componentType">The type of the enumeration.</param>
      /// <param name="name">The name of the enumeration field.</param>
      /// <param name="context">The current context.</param>
      public EnumFieldDescriptor(Type componentType, string name, ITypeDescriptorContext context)
        : base(componentType, name, typeof (bool))
      {
        fContext = context;
      }

      /// <summary>
      /// Retrieves the value of the enumeration field.
      /// </summary>
      /// <param name="component">
      /// The instance of the enumeration type which to retrieve the field value for.
      /// </param>
      /// <returns>
      /// True if the enumeration field is included to the enumeration; 
      /// otherwise, False.
      /// </returns>
      public override object GetValue(object component)
      {
        return ((int) component & (int) Enum.Parse(ComponentType, Name)) != 0;
      }

      /// <summary>
      /// Sets the value of the enumeration field.
      /// </summary>
      /// <param name="component">
      /// The instance of the enumeration type which to set the field value to.
      /// </param>
      /// <param name="value">
      /// True if the enumeration field should included to the enumeration; 
      /// otherwise, False.
      /// </param>
      public override void SetValue(object component, object value)
      {
        var myValue = (bool) value;
        int myNewValue;
        if (myValue)
          myNewValue = ((int) component) | (int) Enum.Parse(ComponentType, Name);
        else
          myNewValue = ((int) component) & ~(int) Enum.Parse(ComponentType, Name);

        var myField = component.GetType().GetField("value__", BindingFlags.Instance | BindingFlags.Public);
        myField.SetValue(component, myNewValue);
        fContext.PropertyDescriptor.SetValue(fContext.Instance, component);
      }

      /// <summary>
      /// Retrieves a value indicating whether the enumeration 
      /// field is set to a non-default value.
      /// </summary>
      public override bool ShouldSerializeValue(object component)
      {
        return (bool) GetValue(component) != GetDefaultValue();
      }

      /// <summary>
      /// Resets the enumeration field to its default value.
      /// </summary>
      public override void ResetValue(object component)
      {
        SetValue(component, GetDefaultValue());
      }

      /// <summary>
      /// Retrieves a value indicating whether the enumeration 
      /// field can be reset to the default value.
      /// </summary>
      public override bool CanResetValue(object component)
      {
        return ShouldSerializeValue(component);
      }

      /// <summary>
      /// Retrieves the enumerations field�s default value.
      /// </summary>
      bool GetDefaultValue()
      {
        object myDefaultValue = null;
        var myPropertyName = fContext.PropertyDescriptor.Name;
        var myComponentType = fContext.PropertyDescriptor.ComponentType;

        // Get DefaultValueAttribute
        var myDefaultValueAttribute = (DefaultValueAttribute) Attribute.GetCustomAttribute(
                                                                myComponentType.GetProperty(myPropertyName,
                                                                                            BindingFlags.Instance |
                                                                                            BindingFlags.Public |
                                                                                            BindingFlags.NonPublic),
                                                                typeof (DefaultValueAttribute));
        if (myDefaultValueAttribute != null)
          myDefaultValue = myDefaultValueAttribute.Value;

        if (myDefaultValue != null)
          return ((int) myDefaultValue & (int) Enum.Parse(ComponentType, Name)) != 0;
        return false;
      }

      #endregion

      #region Properties

      public override AttributeCollection Attributes
      {
        get { return new AttributeCollection(new Attribute[] {RefreshPropertiesAttribute.Repaint}); }
      }

      #endregion
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an instance of the FlagsEnumConverter class.
    /// </summary>
    /// <param name="type">The type of the enumeration.</param>
    public FlagsEnumConverter(Type type) : base(type) {}

    /// <summary>
    /// Retrieves the property descriptors for the enumeration fields. 
    /// These property descriptors will be used by the property grid 
    /// to show separate enumeration fields.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <param name="value">A value of an enumeration type.</param>
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
                                                               Attribute[] attributes)
    {
      if (context != null) {
        var myType = value.GetType();
        var myNames = Enum.GetNames(myType);
        var myValues = Enum.GetValues(myType);
        if (myNames != null) {
          var myCollection = new PropertyDescriptorCollection(null);
          for (var i = 0; i < myNames.Length; i++) {
            if ((int) myValues.GetValue(i) != 0 && myNames[i] != "All")
              myCollection.Add(new EnumFieldDescriptor(myType, myNames[i], context));
          }
          return myCollection;
        }
      }
      return base.GetProperties(context, value, attributes);
    }

    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
      if (context != null) {
        return true;
      }
      return base.GetPropertiesSupported(context);
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return false;
    }

    #endregion
  }
}