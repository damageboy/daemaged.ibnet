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

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Daemaged.IBNet.Client
{
  public class TWSSocketFactory
  {
    static readonly Dictionary<TWSClient, int> _refCount;
    static readonly Dictionary<IPEndPoint, TWSClient> _sockets;

    static TWSSocketFactory()
    {
      _sockets = new Dictionary<IPEndPoint, TWSClient>();
      _refCount = new Dictionary<TWSClient, int>();
    }

    public static TWSClient GetSocket(string host, int port)
    {
      var address = IPAddress.Loopback;
      var localAddress = IPAddress.Loopback;
      foreach (var a in Dns.GetHostEntry(host).AddressList) {
        if (a.AddressFamily == AddressFamily.InterNetwork)
          address = a;
        else
          continue;

        if (a == IPAddress.Loopback)
          break;
      }


      var endPoint = new IPEndPoint(address, port);
      TWSClient socket;

      lock (_refCount) {
        if (!_sockets.TryGetValue(endPoint, out socket)) {
          _sockets.Add(endPoint, socket = new TWSClient(endPoint));
          _refCount.Add(socket, 0);
        }

        _refCount[socket]++;
      }
      return socket;
    }

    public static void ReleaseSocket(TWSClient client)
    {
      lock (_refCount) {
        _refCount[client]--;
        if (_refCount[client] != 0) return;
        client.Disconnect();
        _refCount.Remove(client);
        _sockets.Remove(client.EndPoint);
      }
    }
  }
}