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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using IBNet.Util;

namespace Daemaged.IBNet
{
  public class TWSEncoding : ITWSEncoding
  {
    class EnumEncDec
    {
      readonly IDictionary<int, string> _enumSerializares;
      readonly IDictionary<string, int> _enumDeserializares;

      internal EnumEncDec(Type t)
      {
        ValidateAllValuesAreMapped(t);
        _enumSerializares = GenerateSerializationDictionary(t);
        _enumDeserializares = GenerateDeserializationDictionary(t);
      }

      public IDictionary<string, int> EnumDeserializares => _enumDeserializares;
      public IDictionary<int, string> EnumSerializares => _enumSerializares;

      static IDictionary<string, int> GenerateDeserializationDictionary(Type e)
      {
        var map =
          Enum.GetValues(e).Cast<int>().Zip(Enum.GetNames(e), (Value, Name) => new { Value, Name }).ToDictionary(
          v => e.GetMember(v.Name).Single().GetCustomAttribute<StringSerializerAttribute>().Value,
          v => v.Value);

        return new CompiledStaticDictionary<string, int>(map);

      }

      static IDictionary<int, string> GenerateSerializationDictionary(Type e)
      {
        return Enum.GetValues(e).Cast<int>().Zip(Enum.GetNames(e), (Value, Name) => new { Value, Name }).ToDictionary(
          v => v.Value,
          v => e.GetMember(v.Name).Single().GetCustomAttribute<StringSerializerAttribute>().Value);
      }
    }

    const string IB_EXPIRY_DATE_FORMAT = "yyyyMMdd";

    static readonly Dictionary<Type, EnumEncDec> _enumDecoders;

    protected Stream Stream;

    protected bool ExpectForServerMessage, ExpectForClientMessage;
    protected ServerMessage LastServerMessageEncoded;
    protected ClientMessage LastClientMessageDecoded;

    static TWSEncoding()
    {
      var enums =
        from t in Assembly.GetExecutingAssembly().GetTypes()
        where t.IsEnum && t.GetCustomAttribute<StringSerializableAttribute>() != null
        select t;


      _enumDecoders = enums.ToDictionary(x => x, x => new EnumEncDec(x));
    }

    static void ValidateAllValuesAreMapped(Type e)
    {
      var fields = e.GetFields(BindingFlags.Public | BindingFlags.Static);
      var q =
        from v in fields
        where v.GetCustomAttribute<StringSerializerAttribute>() != null
        select v;

      if (fields.Length != q.Count())
        throw new ArgumentException($"Type {e} doesn't have seriazliation value for each member");
    }

    public TWSEncoding()
    {
      Stream = null;
    }

    public TWSEncoding(Stream stream)
    {
      Stream = stream;
    }

    public void SetStream(Stream stream)
    {
      if (Stream != null)
        throw new Exception("Stream is already initialized!");

      Stream = stream;
    }

    #region Encode Wrappers

    public virtual void Encode(TWSClientInfo v)
    {
      Encode(v.Version);
    }

    public virtual void Encode(TWSServerInfo v)
    {
      Encode(v.Version);
    }

    public virtual void Encode(TWSClientId id)
    {
      Encode(id.Id);
    }

    public virtual void EncodeExpiryDate(DateTime expiry)
    {
      Encode(expiry.ToString(IB_EXPIRY_DATE_FORMAT));
    }

    static class IntCaster<T>
    {
      static int Identity(int x){return x;}
      static Func<int, int> _identity = Identity;

      static IntCaster()
      {
        ToInt = Delegate.CreateDelegate(typeof(Func<T, int>), _identity.Method) as Func<T, int>;
        ToT   = Delegate.CreateDelegate(typeof(Func<int, T>), _identity.Method) as Func<int, T>;
      }
      public static Func<T, int> ToInt { get; private set; }
      public static Func<int, T> ToT { get; private set; }
    }

    public void Encode<T>(T value) where T : struct, IConvertible
    {
      var t = typeof (T);

      if (_enumDecoders.ContainsKey(t))
        Encode(_enumDecoders[t].EnumSerializares[IntCaster<T>.ToInt(value)]);
      else
        Encode(IntCaster<T>.ToInt(value));
    }

    public virtual void Encode(bool value)
    {
      Encode(value ? 1 : 0);
    }

    public virtual void Encode(double value)
    {
      Encode(value.ToString().Replace(',', '.'));
    }

    public virtual void Encode(int value)
    {
      Encode(value.ToString());
    }

    public virtual void EncodeMax(double value)
    {
      if (value == double.MaxValue)
        Encode((string) null);
      else
        Encode(value);
    }

    public void Flush()
    {
      Stream.Flush();
    }

    public virtual void EncodeMax(int value)
    {
      if (value == 0x7fffffff)
        Encode((string) null);
      else
        Encode(value);
    }

    #endregion

    #region Decode Wrappers

    public virtual DateTime DecodeExpiryDate()
    {
      var expiryString = DecodeString();
      if (expiryString != null && expiryString.Length > 0)
        return DateTime.ParseExact(expiryString, IB_EXPIRY_DATE_FORMAT, CultureInfo.InvariantCulture);

      return new DateTime();
    }


    public virtual TWSServerInfo DecodeServerInfo()
    {
      return new TWSServerInfo(DecodeInt());
    }

    public virtual TWSClientInfo DecodeClientInfo()
    {
      return new TWSClientInfo(DecodeInt());
    }

    public virtual TWSClientId DecodeClientId()
    {
      return new TWSClientId(DecodeInt());
    }

    public virtual bool DecodeBool()
    {
      return (DecodeInt() == 1);
    }

    public virtual double DecodeDouble()
    {
      var txt = DecodeString();
      return txt == null ? 0 : double.Parse(txt);
      //txt = txt.Replace(".", NUMBER_DECIMAL_SEPARATOR);
      //txt = txt.Replace(",", NUMBER_DECIMAL_SEPARATOR);
    }
    public double DecodeDoubleMax()
    {
      var txt = DecodeString();
      return txt == null ? Double.MaxValue : double.Parse(txt);
    }


    public virtual int DecodeInt()
    {
      var txt = DecodeString();
      return txt == null ? 0 : Int32.Parse(txt);
    }

    public int DecodeIntMax()
    {
      var txt = DecodeString();
      return txt == null ? Int32.MaxValue : Int32.Parse(txt);

    }

    public virtual long DecodeLong()
    {
      var txt = DecodeString();
      return txt != null ? Int32.Parse(txt) : 0;
    }

    #endregion

    #region String Encoding/Decoding

    public virtual void Encode(string text)
    {
      Debug.Assert(!ExpectForServerMessage);

      if (!String.IsNullOrEmpty(text)) {
        var bytes = Encoding.UTF8.GetBytes(text);
        Stream.Write(bytes, 0, bytes.Length);
      }

      Stream.WriteByte(0);
      Stream.Flush();
    }

    public virtual string DecodeString()
    {
      Debug.Assert(!ExpectForClientMessage);

      var sb = new StringBuilder();

      while (true) {
        var b = Stream.ReadByte();
        if ((b == 0) || (b == -1))
          goto decode_string_finished;
        sb.Append((char) b);
      }
      decode_string_finished:
      var decodeString = sb.Length != 0 ? sb.ToString() : null;

      return decodeString;
    }

    public T DecodeEnum<T>() where T : struct, IConvertible
    {
      var t = typeof (T);
      var intValue = _enumDecoders.ContainsKey(t) ?
        _enumDecoders[t].EnumDeserializares[DecodeString() ?? String.Empty] :
        DecodeInt();
      // Is this a TWS string based enum?
      return IntCaster<T>.ToT(intValue);
    }

    public ClientMessage DecodeClientMessage()
    {
      Debug.Assert(ExpectForClientMessage);
      ExpectForClientMessage = false;
      var clientMsg = DecodeEnum<ClientMessage>();
      LastClientMessageDecoded = clientMsg;
      return clientMsg;
    }

    public virtual void BeginDecodeMessage()
    {
      LastClientMessageDecoded = ClientMessage.Undefined;
      ExpectForClientMessage = true;
     }

    public virtual void EndDecodeMessage() {}

    public void Encode(ServerMessage value)
    {
      Debug.Assert(ExpectForServerMessage);
      ExpectForServerMessage = false;
      Encode(IntCaster<ServerMessage>.ToInt(value));
      LastServerMessageEncoded = value;
    }

    public virtual void BeginEncodeMessage()
    {
      LastServerMessageEncoded = ServerMessage.Undefined;
      ExpectForServerMessage = true;
    }

    public virtual void EndEncodeMessage() {}

    #endregion
  }
}