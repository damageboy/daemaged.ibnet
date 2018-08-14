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
using System.Net;

namespace Daemaged.IBNet.Server
{
  public class TWSServerEventArgs : EventArgs
  {
    public TWSServerEventArgs(TWSServer server)
    {
      Server = server;
    }

    public TWSServerEventArgs(TWSServerClientHandler clientState)
    {
      ClientState = clientState;
      Server = clientState.Server;
    }

    public TWSServer Server { get; private set; }
    public TWSServerClientHandler ClientState { get; private set; }
  }

  public class TWSServerLoginEventArgs : TWSServerEventArgs
  {
    public TWSServerLoginEventArgs(TWSServer server) : base(server) {}
    public TWSServerLoginEventArgs(TWSServerClientHandler clientState) : base(clientState) {}
  }

  public class TWSServerErrorEventArgs : TWSServerEventArgs
  {
    public TWSServerErrorEventArgs(TWSServer server, TWSError error)
      : base(server)
    {
      Error = error;
    }

    public TWSServerErrorEventArgs(TWSServerClientHandler clientState, TWSError error)
      : base(clientState)
    {
      Error = error;
    }


    public TWSError Error { get; private set; }
  }

  public class TWSTcpClientConnectedEventArgs : TWSServerEventArgs
  {
    public TWSTcpClientConnectedEventArgs(TWSServer server, IPEndPoint endPoint)
      : base(server)
    {
      EndPoint = endPoint;
    }

    public IPEndPoint EndPoint { get; set; }
    public bool Reject { get; set; }
  }

  public class TWSMarketDataRequestEventArgs : TWSServerEventArgs
  {
    public TWSMarketDataRequestEventArgs(TWSServerClientHandler clientState, int reqId, IBContract contract)
      : base(clientState)
    {
      Contract = contract;
      ReqId = reqId;
    }

    public int ReqId { get; private set; }
    public IBContract Contract { get; private set; }
  }

  public class TWSMarketDepthRequestEventArgs : TWSMarketDataRequestEventArgs
  {
    public TWSMarketDepthRequestEventArgs(TWSServerClientHandler clientState, int reqId, IBContract contract,
                                          int numRows)
      : base(clientState, reqId, contract)
    {
      numRows = numRows;
    }

    public int NumRows { get; private set; }
  }


  public class TWSMarketDataCancelEventArgs : TWSServerEventArgs
  {
    public TWSMarketDataCancelEventArgs(TWSServerClientHandler clientState, int reqId, IBContract contract)
      : base(clientState)
    {
      Contract = contract;
      ReqId = reqId;
    }

    public int ReqId { get; private set; }
    public IBContract Contract { get; private set; }
  }
}