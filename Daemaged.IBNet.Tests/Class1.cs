using System;
using System.Linq;
using System.Reflection;
using Daemaged.IBNet.Client;
using Daemaged.IBNet.Dsl;
using Xunit;

namespace Daemaged.IBNet.Tests
{
  /// <summary>
  /// The project uses many reflection / expressions based constructs which get 
  /// dynamically compiled into expression trees and lambda functions, this test 
  /// verifies that these internal "compilers" don't bomb flat out @ runtime
  /// </summary>
  public class InternalCodeGenerationTests
  {

    [Fact]
    public void TestEnumEncodersDecodersCompilation()
    {
      var encdec = TWSEncoding._enumDecoders;
      Console.WriteLine("Found {0} enum encoders", encdec.Count);
    }

    [Fact]
    public void TestEnumEncoders()
    {
      var encdec = TWSEncoding._enumDecoders;

      var enums =
        from t in Assembly.GetAssembly(typeof(TWSClient)).GetTypes()
        where t.IsEnum && t.GetCustomAttribute<StringSerializableAttribute>() != null
        select t;

      foreach (var e in enums) {
        var x = encdec[e];
        Console.WriteLine("{0}", e.Name);
        foreach (int v in Enum.GetValues(e)) {
          var str = x.EnumSerializares[v];
          Console.WriteLine(" >{0}=\"{1}\"", v, str);
          Assert.NotNull(str);
        }
      }      
    }

    [Fact]
    public void TestEnumDecoders()
    {
      var encdec = TWSEncoding._enumDecoders;

      var enums =
        from t in Assembly.GetAssembly(typeof(TWSClient)).GetTypes()
        where t.IsEnum && t.GetCustomAttribute<StringSerializableAttribute>() != null
        select t;

      foreach (var e in enums)
      {
        var x = encdec[e];
        Console.WriteLine("{0}", e.Name);
        foreach (string n in Enum.GetNames(e)) {
          var str = e.GetMember(n).Single().GetCustomAttribute<StringSerializerAttribute>().Value;
          Assert.NotNull(str);
          var v = x.EnumDeserializares[str];

          Console.WriteLine(" >\"{0}\"={1}", str, v);          
        }
      }
    }

    [Fact]
    public void TestMessageEncoders()
    {
      var templates =
        from t in typeof(TWSMsgDefs).GetFields(BindingFlags.NonPublic | BindingFlags.Static) 
        where t.FieldType.IsGenericType && 
              t.FieldType.GetGenericTypeDefinition() == typeof(TWSMessage<>)
        select t;
      
      var method = typeof(TWSEncoderGenerator).GetMethod("GetEncoderFunc");
      foreach (var t in templates) {
        var generic = method.MakeGenericMethod(t.FieldType.GetGenericArguments());
        generic.Invoke(null, new[] {t.GetValue(null)});
      }
      
    }
  }
}
