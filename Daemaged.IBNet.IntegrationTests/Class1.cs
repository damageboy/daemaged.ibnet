using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daemaged.IBNet.Client;
using Xunit;
using Xunit.Extensions;

namespace Daemaged.IBNet.IntegrationTests
{
  public class ClientIntegrationTests : IDisposable
  {
    private TWSClient _client;
    public ClientIntegrationTests()
    {
      _client = new TWSClient("127.0.0.1", 7496);
      _client.Connect();
    }

    public void Dispose()
    {
      _client.Disconnect();
      _client = null;
    }

    [Fact]
    public void RequestContractDetailsTest()
    {
      var contract = new IBContract {
        Symbol = "AAPL",
        Exchange = "SMART",
        Currency = "USD",
        SecurityType = IBSecurityType.Stock,
      };

      var id = _client.RequestContractDetails(contract);
    }

  }
}
