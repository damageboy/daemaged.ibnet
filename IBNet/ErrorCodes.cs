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
using System.Linq;
using System.Reflection;

namespace Daemaged.IBNet
{
  public class TWSError
  {
    public TWSError(int code, string message, bool dropDead = false)
    {
      Code = code;
      Message = message;
      DropDead = dropDead;
    }

    public int Code { get; private set; }
    public string Message { get; private set; }
    public bool DropDead { get; private set; }
    public override string ToString()
    {
      return Message;
    }
  }
  /// <summary>
  /// All known TWS Error codes, taken from http://www.interactivebrokers.com/php/apiguide/interoperability/socket_client_c++/errors.htm
  /// </summary>
  public class TWSErrors
  {
    public const int NO_VALID_CODE = -1;
    public const int NO_VALID_ID = -1;
    public static readonly TWSError ACCOUNT_NOT_FA;

    public static readonly TWSError ALREADY_CONNECTED;
    public static readonly TWSError BLOCK_TOO_SMALL;
    public static readonly TWSError BUY_PRICE;
    public static readonly TWSError CLIENT_NOT_ZERO;
    public static readonly TWSError CONNECT_FAIL;
    public static readonly TWSError CONNECTION_DROPPED;
    public static readonly TWSError CONTRACT_MIN_PRICE;
    public static readonly TWSError DEAD_EXCHANGE;
    public static readonly TWSError DUPLICATE_ORDER_ID;
    public static readonly TWSError DUPLICATE_TICKER_ID;
    public static readonly TWSError ERROR_126;
    public static readonly TWSError ERROR_127;
    public static readonly TWSError ERROR_128;
    public static readonly TWSError ERROR_129;
    public static readonly TWSError ERROR_130;
    public static readonly TWSError ERROR_131;
    public static readonly TWSError ERROR_132;
    public static readonly TWSError ERROR_133;
    public static readonly TWSError ERROR_135;
    public static readonly TWSError ERROR_136;
    public static readonly TWSError ERROR_137;
    public static readonly TWSError ERROR_138;
    public static readonly TWSError ERROR_142;
    public static readonly TWSError ERROR_144;
    public static readonly TWSError ERROR_147;
    public static readonly TWSError ERROR_148;
    public static readonly TWSError ERROR_151;
    public static readonly TWSError ERROR_152;
    public static readonly TWSError ERROR_153;
    public static readonly TWSError ERROR_154;
    public static readonly TWSError ERROR_155;
    public static readonly TWSError ERROR_156;
    public static readonly TWSError ERROR_157;
    public static readonly TWSError ERROR_158;
    public static readonly TWSError ERROR_159;
    public static readonly TWSError ERROR_161;
    public static readonly TWSError ERROR_200;
    public static readonly TWSError ERROR_203;
    public static readonly TWSError ERROR_2100;
    public static readonly TWSError ERROR_2101;
    public static readonly TWSError ERROR_2102;
    public static readonly TWSError ERROR_300;
    public static readonly TWSError ERROR_301;
    public static readonly TWSError ERROR_302;
    public static readonly TWSError ERROR_303;
    public static readonly TWSError ERROR_304;
    public static readonly TWSError ERROR_305;
    public static readonly TWSError ERROR_306;
    public static readonly TWSError ERROR_307;
    public static readonly TWSError ERROR_314;
    public static readonly TWSError ERROR_315;
    public static readonly TWSError ERROR_318;
    public static readonly TWSError ERROR_319;
    public static readonly TWSError ERROR_325;
    public static readonly TWSError ERROR_326;
    public static readonly TWSError ERROR_328;
    public static readonly TWSError ERROR_330;
    public static readonly TWSError ERROR_331;
    public static readonly TWSError ERROR_332;
    public static readonly TWSError ERROR_333;
    public static readonly TWSError ERROR_334;
    public static readonly TWSError ERROR_335;
    public static readonly TWSError ERROR_336;
    public static readonly TWSError ERROR_337;
    public static readonly TWSError ERROR_338;
    public static readonly TWSError ERROR_339;
    public static readonly TWSError ERROR_340;
    public static readonly TWSError ERROR_365;
    public static readonly TWSError ERROR_366;
    public static readonly TWSError ERROR_367;
    public static readonly TWSError ERROR_368;
    public static readonly TWSError ERROR_369;
    public static readonly TWSError ERROR_370;
    public static readonly TWSError ERROR_371;
    public static readonly TWSError ERROR_372;
    public static readonly TWSError ERROR_373;
    public static readonly TWSError ERROR_374;
    public static readonly TWSError ERROR_375;
    public static readonly TWSError ERROR_CLIENT_VERSION;
    public static readonly TWSError FAIL_API_PAGE;
    public static readonly TWSError FAIL_CHANGE_ORDER_TYPE;
    public static readonly TWSError FAIL_MATCH_ORDER;
    public static readonly TWSError FAIL_MODIFY_FILLED;
    public static readonly TWSError FAIL_ORDER_INCOMPLETE;
    public static readonly TWSError FAIL_SEND_ACCT;
    public static readonly TWSError FAIL_SEND_CANHISTDATA;
    public static readonly TWSError FAIL_SEND_CANMKT;
    public static readonly TWSError FAIL_SEND_CANMKTDEPTH;
    public static readonly TWSError FAIL_SEND_CANRTBARS;
    public static readonly TWSError FAIL_SEND_CANSCANNER;
    public static readonly TWSError FAIL_SEND_CORDER;
    public static readonly TWSError FAIL_SEND_EXEC;
    public static readonly TWSError FAIL_SEND_FA_REPLACE;
    public static readonly TWSError FAIL_SEND_FA_REQUEST;
    public static readonly TWSError FAIL_SEND_OORDER;
    public static readonly TWSError FAIL_SEND_ORDER;
    public static readonly TWSError FAIL_SEND_REQCONTRACT;
    public static readonly TWSError FAIL_SEND_REQCURRTIME;
    public static readonly TWSError FAIL_SEND_REQHISTDATA;
    public static readonly TWSError FAIL_SEND_REQMKT;
    public static readonly TWSError FAIL_SEND_REQMKTDEPTH;
    public static readonly TWSError FAIL_SEND_REQRTBARS;
    public static readonly TWSError FAIL_SEND_REQSCANNER;
    public static readonly TWSError FAIL_SEND_REQSCANNERPARAMETERS;
    public static readonly TWSError FAIL_SEND_SERVER_LOG_LEVEL;
    public static readonly TWSError FAIL_TRANSMIT_ORDER;
    public static readonly TWSError HISTORICAL_DATA_ERROR;
    public static readonly TWSError HISTORICAL_DATA_FARM_CONNECTED;
    public static readonly TWSError HISTORICAL_DATA_FARM_DISCONNECTED;
    public static readonly TWSError HISTORICAL_DATA_FARM_INACTIVE;
    public static readonly TWSError HISTORICAL_DATA_QUERY;
    public static readonly TWSError IB_TWS_CONNECTION_LOST;
    public static readonly TWSError IB_TWS_CONNECTION_LOST_DATA_LOST;
    public static readonly TWSError IB_TWS_CONNECTION_LOST_DATA_OK;
    public static readonly TWSError INVALID_BD;
    public static readonly TWSError INVALID_COMBO;
    public static readonly TWSError INVALID_COMBO_LEG;
    public static readonly TWSError INVALID_DELTA;
    public static readonly TWSError INVALID_GTD;
    public static readonly TWSError INVALID_MARKET_DEPTH_ID;
    public static readonly TWSError INVALID_ORIGIN;
    public static readonly TWSError INVALID_PEG;
    public static readonly TWSError INVALID_TRIGGER;
    public static readonly TWSError MARKET_DATA_FARM_CONNECTED;
    public static readonly TWSError MARKET_DATA_FARM_DISCONNECTED;
    public static readonly TWSError MARKET_DATA_FARM_INACTIVE;
    public static readonly TWSError MARKET_DEPTH_HALTED;
    public static readonly TWSError MARKET_DEPTH_RESET;
    public static readonly TWSError MAX_MARKET_DEPTH;
    public static readonly TWSError MAX_RATE_EXCEEDED;
    public static readonly TWSError MAX_TICKERS_REACHED;
    public static readonly TWSError MODIFY_ORDER_FAIL;
    public static readonly TWSError NO_MARKET_RULE;
    public static readonly TWSError NO_RECORD;
    public static readonly TWSError NO_REQUEST_TAG;
    public static readonly TWSError NOT_CONNECTED;
    public static readonly TWSError ORDER_CANCELLED;
    public static readonly TWSError ORDER_REJECTED;
    public static readonly TWSError PARSING_ERROR;
    public static readonly TWSError RELATIVE_EXCHANGE;
    public static readonly TWSError RELATIVE_STK_ONLY;
    public static readonly TWSError REQ_ID_NOT_INTEGER;
    public static readonly TWSError SERVER_API_ERROR;
    public static readonly TWSError SERVER_API_PROCESSING;
    public static readonly TWSError SERVER_API_VALIDATION;
    public static readonly TWSError SERVER_DDE_ERROR;
    public static readonly TWSError SIZE_NOT_DOUBLE;
    public static readonly TWSError SIZE_NOT_INTEGER;
    public static readonly TWSError SIZE_NOT_ZERO;
    public static readonly TWSError TIF_ERROR;
    public static readonly TWSError TIF_TYPE_INCOMPATIBLE;
    public static readonly TWSError TWS_PRICE_MISMATCH;
    public static readonly TWSError UNKNOWN_CONTRACT;
    public static readonly TWSError UNKNOWN_ID;
    public static readonly TWSError UNSUPPORTED_ORDER_TYPE;
    public static readonly TWSError UPDATE_TWS;
    public static readonly TWSError VALIDATION_ERROR;
    public static readonly TWSError VWAP_EXCHANGE_ONLY;
    public static readonly TWSError VWAP_ORDER_ONLY;
    public static readonly TWSError VWAP_TOO_LATE;
    public static readonly TWSError OUTSIDE_RTH_IGNORES;
    public static readonly TWSError ERROR_168;
    public static readonly TWSError ERROR_167;
    public static readonly TWSError ERROR_166;

    public static Dictionary<int, TWSError> Errors;
    public static readonly TWSError  ERROR_376;
    public static readonly TWSError  ERROR_377;
    public static readonly TWSError  ERROR_378;
    public static readonly TWSError  ERROR_379;
    public static readonly TWSError  ERROR_380;
    public static readonly TWSError  ERROR_381;
    public static readonly TWSError  ERROR_382;
    public static readonly TWSError  ERROR_383;
    public static readonly TWSError  ERROR_384;
    public static readonly TWSError  ERROR_385;
    public static readonly TWSError  ERROR_386;
    public static readonly TWSError  ERROR_387;
    public static readonly TWSError  ERROR_388;
    public static readonly TWSError  ERROR_389;
    public static readonly TWSError  ERROR_390;
    public static readonly TWSError  ERROR_391;
    public static readonly TWSError  ERROR_392;
    public static readonly TWSError  ERROR_393;
    public static readonly TWSError  ERROR_394;
    public static readonly TWSError  ERROR_395;
    public static readonly TWSError  ERROR_396;
    public static readonly TWSError  ERROR_397;
    public static readonly TWSError  ERROR_398;
    public static readonly TWSError  ERROR_399;

    public static readonly TWSError ERROR_400;
    public static readonly TWSError ERROR_401;
    public static readonly TWSError ERROR_402;
    public static readonly TWSError ERROR_403;
    public static readonly TWSError ERROR_404;
    public static readonly TWSError ERROR_405;
    public static readonly TWSError ERROR_406;
    public static readonly TWSError ERROR_407;
    public static readonly TWSError ERROR_408;
    public static readonly TWSError ERROR_409;
    public static readonly TWSError ERROR_410;
    public static readonly TWSError ERROR_411;
    public static readonly TWSError ERROR_412;
    public static readonly TWSError ERROR_413;
    public static readonly TWSError ERROR_414;
    public static readonly TWSError ERROR_415;
    public static readonly TWSError ERROR_416;
    public static readonly TWSError ERROR_417;
    public static readonly TWSError ERROR_418;
    public static readonly TWSError ERROR_419;
    public static readonly TWSError ERROR_420;
    public static readonly TWSError ERROR_421;
    public static readonly TWSError ERROR_422;


    static TWSErrors()
    {
      MAX_RATE_EXCEEDED = new TWSError(100, @"Max rate of messages per second has been exceeded.");
      MAX_TICKERS_REACHED = new TWSError(101, @"Max number of tickers has been reached.");
      DUPLICATE_TICKER_ID = new TWSError(102, @"Duplicate ticker ID.");
      DUPLICATE_ORDER_ID = new TWSError(103, @"Duplicate order ID.");
      FAIL_MODIFY_FILLED = new TWSError(104, @"Can't modify a filled order.");
      FAIL_MATCH_ORDER = new TWSError(105, @"Order being modified does not match original order.");
      FAIL_TRANSMIT_ORDER = new TWSError(106, @"Can't transmit order ID.");
      FAIL_ORDER_INCOMPLETE = new TWSError(107, @"Can't transmit incomplete order.");
      TWS_PRICE_MISMATCH = new TWSError(109,
                                        @"Price is out of the range defined by the Percent Setting in TWS. The order will not be transmitted.");
      CONTRACT_MIN_PRICE = new TWSError(110,
                                        @"The price does not conform to the minimum price variation for this contract.");
      TIF_TYPE_INCOMPATIBLE = new TWSError(111, @"The tif type and the order type are incompatible.");
      UNSUPPORTED_ORDER_TYPE = new TWSError(112,
                                            @"Unsupported order type (order type) has been selected for the exchange (exch name).");
      TIF_ERROR = new TWSError(113, @"The Tif option should be set to DAY for MOC and LOC orders.");
      RELATIVE_STK_ONLY = new TWSError(114, @"Relative orders are only valid for stock orders.");
      RELATIVE_EXCHANGE = new TWSError(115,
                                       @"Relative orders for US stocks must be submitted to SMART, INSTINET, or PRIMEX.");
      DEAD_EXCHANGE = new TWSError(116, @"The order cannot be transmitted to a dead exchange.");
      BLOCK_TOO_SMALL = new TWSError(117, @"The block order size must be at least 50.");
      VWAP_EXCHANGE_ONLY = new TWSError(118, @"VWAP orders must be routed through the VWAP exchange.");
      VWAP_ORDER_ONLY = new TWSError(119, @"Only VWAP orders may be placed on the VWAP exchange.");
      VWAP_TOO_LATE = new TWSError(120, @"It is too late to place a VWAP order for today.");
      INVALID_BD = new TWSError(121, @"Invalid BD flag for the order. Check destination and BD flag at TWS.");
      NO_REQUEST_TAG = new TWSError(122, @"No request tag has been found for order.");
      NO_RECORD = new TWSError(123, @"No record is available for conid.");
      NO_MARKET_RULE = new TWSError(124, @"No market rule is available for conid.");
      BUY_PRICE = new TWSError(125, @"Buy price must be the same as the best asking price.");
      ERROR_126 = new TWSError(126, @"Sell price must be the same as the best bidding price.");
      ERROR_127 = new TWSError(127, @"Linkage orders are not supported on this exchange.");
      ERROR_128 = new TWSError(128, @"Linkage customers cannot submit non-linkage orders foroptions.");
      ERROR_129 = new TWSError(129, @"VWAP orders must be submitted at least three minutes before the start time.");
      ERROR_130 = new TWSError(130, @"The display size is not supported for this order and will be ignored.");
      ERROR_131 = new TWSError(131,
                               @"The sweep-to-fill flag and display size are only valid for US stocks routed through SMART, and will be ignored otherwise.");
      ERROR_132 = new TWSError(132, @"This order cannot be transmitted without an account number.");
      ERROR_133 = new TWSError(133, @"Submit new order failed.");
      MODIFY_ORDER_FAIL = new TWSError(134, @"Modify order failed.");
      ERROR_135 = new TWSError(135, @"Can't find order with ID equal to");
      ERROR_136 = new TWSError(136, @"This order cannot be cancelled.");
      ERROR_137 = new TWSError(137, @"VWAP orders can only be cancelled up to three minutes before the start time.");
      ERROR_138 = new TWSError(138, @"Could not parse ticker request.");
      PARSING_ERROR = new TWSError(139, @"Parsing error:");
      SIZE_NOT_INTEGER = new TWSError(140, @"The size value should be an integer:");
      SIZE_NOT_DOUBLE = new TWSError(141, @"The price value should be a double:");
      ERROR_142 = new TWSError(142, @"Institutional customer account does not have account info");
      REQ_ID_NOT_INTEGER = new TWSError(143, @"Requested ID is not an integer number.");
      ERROR_144 = new TWSError(144, @"Order size does not match total share allocation.");
      VALIDATION_ERROR = new TWSError(145, @"Error in validating entry fields.");
      INVALID_TRIGGER = new TWSError(146, @"Invalid trigger method.");
      ERROR_147 = new TWSError(147, @"The conditional contract info is incomplete.");
      ERROR_148 = new TWSError(148,
                               @"A conditional order can only be submitted when the order type is set to limit or market.");
      ERROR_151 = new TWSError(151, @"This order cannot be transmitted without a user name.");
      ERROR_152 = new TWSError(152, @"The hidden order attribute may only be specified for orders on INET (Island).");
      ERROR_153 = new TWSError(153, @"EFP orders can only be a limit order.");
      ERROR_154 = new TWSError(154, @"Orders cannot be transmitted for a halted security.");
      ERROR_155 = new TWSError(155, @"A sizeOp order must have a username and account.");
      ERROR_156 = new TWSError(156, @"A SizeOp order must go to IBSX");
      ERROR_157 = new TWSError(157,
                               @"An order can be EITHER Iceberg or Discretionary. Please remove either the Discretionary amount or the Display size.");
      ERROR_158 = new TWSError(158, @"You must specify an offset amount or a percent offset value.");
      ERROR_159 = new TWSError(159, @"The percent offset value must be between 0% and 100%.");
      SIZE_NOT_ZERO = new TWSError(160, @"The size value cannot be zero.");
      ERROR_161 = new TWSError(161, @"Cancel attempted when order is not in a cancellable state. Order permId =");
      HISTORICAL_DATA_ERROR = new TWSError(162, @"Historical market data Service error message.");
      HISTORICAL_DATA_QUERY = new TWSError(165, @"Historical market Data Service query message.");
      ERROR_166 = new TWSError(166, "HMDS Expired Contract Violation.");
      ERROR_167 = new TWSError(167, "VWAP order time must be in the future.");
      ERROR_168 = new TWSError(168, "Discretionary amount does not conform to the minimum price variation for this contract.");

      ERROR_200 = new TWSError(200, @"No security definition has been found for the request");
      ORDER_REJECTED = new TWSError(201, @"Order rejected - reason:");
      ORDER_CANCELLED = new TWSError(202, @"Order Cancelled - reason:");
      ERROR_203 = new TWSError(203, @"The security (security) is not available or allowed for this account.");


      ERROR_300 = new TWSError(300, @"Can't find EId with ticker Id:");
      ERROR_301 = new TWSError(301, @"Invalid ticker action:");
      ERROR_302 = new TWSError(302, @"Error parsing stop ticker string");
      ERROR_303 = new TWSError(303, @"Invalid action:");
      ERROR_304 = new TWSError(304, @"Invalid acct. value action:");
      ERROR_305 = new TWSError(305, @"Request parsing error, the request has been ignored.");
      ERROR_306 = new TWSError(306, @"Error processing DDE request.");
      ERROR_307 = new TWSError(307, @"Invalid request topic.");
      FAIL_API_PAGE = new TWSError(308,
                                   @"Unable to create the API page in TWS as the maximum number of pages already exists.");
      MAX_MARKET_DEPTH = new TWSError(309, @"Max number (3) of market depth requests has been reached");
      INVALID_MARKET_DEPTH_ID = new TWSError(310, @"Can't find the subscribed market depth with tickerId:");
      INVALID_ORIGIN = new TWSError(311, @"The origin is invalid.");
      INVALID_COMBO = new TWSError(312, @"The combo details are invalid.");
      INVALID_COMBO_LEG = new TWSError(313, @"The combo details for leg '<leg number>' are invalid.");
      ERROR_314 = new TWSError(314, @"Security type BAG requires combo leg details.");
      ERROR_315 = new TWSError(315, @"Stock combo legs are restricted to SMART order routing.");
      MARKET_DEPTH_HALTED = new TWSError(316, @"Market depth data has been HALTED. Please re-subscribe.", true);
      MARKET_DEPTH_RESET = new TWSError(317,
                                        @"Market depth data has been RESET. Please empty deep book contents before applying any new entries.",
                                        true);
      ERROR_318 = new TWSError(318, @"Advisors cannot modify partially filled orders.");
      ERROR_319 = new TWSError(319, @"Attempt to set the server log level failed as the log level was invalid.");
      SERVER_API_ERROR = new TWSError(320, @"Server error when reading an API client request.");
      SERVER_API_VALIDATION = new TWSError(321, @"Server error when validating an API client request.");
      SERVER_API_PROCESSING = new TWSError(322, @"Server error when processing an API client request.");
      SERVER_DDE_ERROR = new TWSError(324, @"Server error when reading a DDE client request - missing data.");
      ERROR_325 = new TWSError(325,
                               @"Discretionary orders are not supported for this combination of exchange and order type.");
      ERROR_326 = new TWSError(326, @"Unable connect as the client id is already in use. Retry with a unique client id.");
      CLIENT_NOT_ZERO = new TWSError(327,
                                     @"Only API connections with clientId set to 0 can set the auto bind TWS orders property.");
      ERROR_328 = new TWSError(328, @"Only support trailing stop order on limit or stop limit order.");
      FAIL_CHANGE_ORDER_TYPE = new TWSError(329, @"Order modify failed. Cannot change to the new order type.");
      ERROR_330 = new TWSError(330, @"Only FA customers can request managed accounts list.");
      ERROR_331 = new TWSError(331, @"Internal error. FA does not have any managed accounts.");
      ERROR_332 = new TWSError(332, @"The account codes for the order profile are invalid.");
      ERROR_333 = new TWSError(333, @"Invalid share allocation syntax.");
      ERROR_334 = new TWSError(334, @"Invalid Good Till Date order");
      ERROR_335 = new TWSError(335, @"Invalid delta: The delta must be between 0 and 100.");
      ERROR_336 = new TWSError(336, @"Invalid Expiration Time");
      ERROR_337 = new TWSError(337, @"Invalid Good After Time");
      ERROR_338 = new TWSError(338, @"Good After Time Disabled");
      ERROR_339 = new TWSError(339, @"Futures Spread Not Supported Any More");
      ERROR_340 = new TWSError(340, @"The account logged into is not an Advisor account.");
      INVALID_DELTA = new TWSError(341, @"Invalid delta");
      INVALID_PEG = new TWSError(342, @"Invalid Peg");
      INVALID_GTD = new TWSError(343, @"Invalid Good Till Date");
      ACCOUNT_NOT_FA = new TWSError(344, @"The account is not a financial advisor account");
      ERROR_CLIENT_VERSION = new TWSError(357, @"Your client version is too low for");
      ERROR_365 = new TWSError(365, @"No scanner subscription found for ticker id:");
      ERROR_366 = new TWSError(366, @"No historical data query found for ticker id:");
      ERROR_367 = new TWSError(367, @"Volatility type if set must be 1 or 2 for VOL orders.");
      ERROR_368 = new TWSError(368, @"Reference Price Type must be 1 or 2 for dynamic volatility management.");
      ERROR_369 = new TWSError(369, @"Volatility orders are only valid for US options.");
      ERROR_370 = new TWSError(370,
                               @"Dynamic Volatility orders must be SMART routed, or trade on a Price Improvement Exchange.");
      ERROR_371 = new TWSError(371, @"VOL order requires positive floating point value for volatility.");
      ERROR_372 = new TWSError(372, @"Cannot set dynamic VOL attribute on non-VOL order.");
      ERROR_373 = new TWSError(373, @"Can only set stock range attribute on VOL or PEGGED TO STOCK order.");
      ERROR_374 = new TWSError(374,
                               @"If both are set, the lower stock range attribute must be less than the upper stock range attribute.");
      ERROR_375 = new TWSError(375, @"Stock range attributes cannot be negative.");
      ERROR_376 = new TWSError(376, "The order is not eligible for continuous update. The option must trade on a cheap-to-reroute exchange.");
      ERROR_377 = new TWSError(377, "Must specify valid delta hedge order aux. price.");
      ERROR_378 = new TWSError(378, "Delta hedge order type requires delta hedge aux. price to be specified.");
      ERROR_379 = new TWSError(379, "Delta hedge order type requires that no delta hedge aux. price be specified.");
      ERROR_380 = new TWSError(380, "This order type is not allowed for delta hedge orders.");
      ERROR_381 = new TWSError(381, "Your DDE.dll needs to be upgraded.");
      ERROR_382 = new TWSError(382, "The price specified violates the number of ticks constraint specified in the default order settings.");
      ERROR_383 = new TWSError(383, "The size specified violates the size constraint specified in the default order settings.");
      ERROR_384 = new TWSError(384, "Invalid DDE array request.");
      ERROR_385 = new TWSError(385, "Duplicate ticker ID for API scanner subscription.");
      ERROR_386 = new TWSError(386, "Duplicate ticker ID for API historical data query.");
      ERROR_387 = new TWSError(387, "Unsupported order type for this exchange and security type.");
      ERROR_388 = new TWSError(388, "Order size is smaller than the minimum requirement.");
      ERROR_389 = new TWSError(389, "Supplied routed order ID is not unique.");
      ERROR_390 = new TWSError(390, "Supplied routed order ID is invalid.");
      ERROR_391 = new TWSError(391, "Invalid date for GTD.");
      ERROR_392 = new TWSError(392, "This order is invalid since is uses an expired contract.");
      ERROR_393 = new TWSError(393, "The short slot order has not delta hedge order type.");
      ERROR_394 = new TWSError(394, "Invalid process time.");
      ERROR_395 = new TWSError(395, "Direct routed OCA error.");
      ERROR_396 = new TWSError(396, "Direct routed order type error.");
      ERROR_397 = new TWSError(397, "Region order type error.");
      ERROR_398 = new TWSError(398, "Direct routed condition error placeholder.");
      ERROR_399 = new TWSError(399, "Order message error");

      ERROR_400 = new TWSError(400, "Algo order error.");
      ERROR_401 = new TWSError(401, "Length restriction.");
      ERROR_402 = new TWSError(402, "Conditions are not allowed for this contract.");
      ERROR_403 = new TWSError(403, "Invalid stop price.");
      ERROR_404 = new TWSError(404, "Allowed quantity message.");
      ERROR_405 = new TWSError(405, "The child order quantity should be equivalent to the parent order size.");
      ERROR_406 = new TWSError(406, "The currency (  ) is not allowed.");
      ERROR_407 = new TWSError(407, "The symbol should contain valid non-unicode characters only.");
      ERROR_408 = new TWSError(408, "Invalid scale order increment.");
      ERROR_409 = new TWSError(409, "Invalid scale order. You must specify order component size.");
      ERROR_410 = new TWSError(410, "Invalid subsequent component size for scale order.");
      ERROR_411 = new TWSError(411, "Invalid outside RTH.");
      ERROR_412 = new TWSError(412, "The contract is not available for trading.");
      ERROR_413 = new TWSError(413, "What-if order should have the transmit flag set to true.");
      ERROR_414 = new TWSError(414, "Do not use generic ticks for snapshot market data.");
      ERROR_415 = new TWSError(415, "RFQ already sent for the conid.");
      ERROR_416 = new TWSError(416, "RFQ not applicable for the contract. Order ID:");
      ERROR_417 = new TWSError(417, "Invalid initial component size for scale order.");
      ERROR_418 = new TWSError(418, "Invalid scale order profit offset.");
      ERROR_419 = new TWSError(419, "Missing initial component size for scale order.");
      ERROR_420 = new TWSError(420, "Invalid real-time query.");
      ERROR_421 = new TWSError(421, "Invalid route.");
      ERROR_422 = new TWSError(422, "Invalid OMS component attributes.");



      ALREADY_CONNECTED = new TWSError(501, @"Already connected.");
      CONNECT_FAIL = new TWSError(502,
                                  @"Couldn't connect to TWS.  Confirm that 'Enable ActiveX and Socket Clients' is enabled on the TWS 'Configure->API' menu.");
      UPDATE_TWS = new TWSError(503, @"The TWS is out of date and must be upgraded.");
      NOT_CONNECTED = new TWSError(504, @"Not connected");
      UNKNOWN_ID = new TWSError(505, @"Fatal Error: Error message id.");
      FAIL_SEND_REQMKT = new TWSError(510, @"Request Market Data Sending Error - ");
      FAIL_SEND_CANMKT = new TWSError(511, @"Cancel Market Data Sending Error - ");
      FAIL_SEND_ORDER = new TWSError(512, @"Order Sending Error - ");
      FAIL_SEND_ACCT = new TWSError(513, @"Account Update Request Sending Error -");
      FAIL_SEND_EXEC = new TWSError(514, @"Request For Executions Sending Error -");
      FAIL_SEND_CORDER = new TWSError(515, @"Cancel Order Sending Error -");
      FAIL_SEND_OORDER = new TWSError(516, @"Request Open Order Sending Error -");
      UNKNOWN_CONTRACT = new TWSError(517, @"Error contract. Verify the contract details supplied.");
      FAIL_SEND_REQCONTRACT = new TWSError(518, @"Request Contract Data Sending Error - ");
      FAIL_SEND_REQMKTDEPTH = new TWSError(519, @"Request Market Depth Sending Error - ");
      FAIL_SEND_CANMKTDEPTH = new TWSError(520, @"Cancel Market Depth Sending Error - ");
      FAIL_SEND_SERVER_LOG_LEVEL = new TWSError(521, @"Set Server Log Level Sending Error - ");
      FAIL_SEND_FA_REQUEST = new TWSError(522, @"FA Information Request Sending Error - ");
      FAIL_SEND_FA_REPLACE = new TWSError(523, @"FA Information Replace Sending Error - ");
      FAIL_SEND_REQSCANNER = new TWSError(524, @"Request Scanner Subscription Sending Error - ");
      FAIL_SEND_CANSCANNER = new TWSError(525, @"Cancel Scanner Subscription Sending Error - ");
      FAIL_SEND_REQSCANNERPARAMETERS = new TWSError(526, @"Request Scanner Parameter Sending Error - ");
      FAIL_SEND_REQHISTDATA = new TWSError(527, "@Request Historical Data Sending Error - ");
      FAIL_SEND_CANHISTDATA = new TWSError(528, "@Request Historical Data Sending Error - ");
      FAIL_SEND_REQRTBARS = new TWSError(529, "@Request Real-time Bar Data Sending Error - ");
      FAIL_SEND_CANRTBARS = new TWSError(530, "@Cancel Real-time Bar Data Sending Error - ");
      FAIL_SEND_REQCURRTIME = new TWSError(531, "@Request Current Time Sending Error - ");

      IB_TWS_CONNECTION_LOST = new TWSError(1100, @"Connectivity between IB and TWS has been lost.");
      IB_TWS_CONNECTION_LOST_DATA_LOST = new TWSError(1101,
                                                      @"Connectivity between IB and TWS has been lost - data lost.");
      IB_TWS_CONNECTION_LOST_DATA_OK = new TWSError(1102,
                                                    @"Connectivity between IB and TWS has been lost - data maintained.");
      CONNECTION_DROPPED = new TWSError(1300,
                                        @"TWS socket port has been reset and this connection is being dropped.Please reconnect on the new port - <port_num>",
                                        true);
      ERROR_2100 = new TWSError(2100,
                                @"New account data requested from TWS.  API client has been unsubscribed from account data.");
      ERROR_2101 = new TWSError(2101,
                                @"Unable to subscribe to account as the following clients are subscribed to a different account.");
      ERROR_2102 = new TWSError(2102, @"Unable to modify this order as it is still being processed.");
      MARKET_DATA_FARM_DISCONNECTED = new TWSError(2103, @"A market data farm is disconnected.");
      MARKET_DATA_FARM_CONNECTED = new TWSError(2104, @"A market data farm is connected.");
      HISTORICAL_DATA_FARM_DISCONNECTED = new TWSError(2105, @"A historical data farm is disconnected.");
      HISTORICAL_DATA_FARM_CONNECTED = new TWSError(2106, @"A historical data farm is connected.");
      HISTORICAL_DATA_FARM_INACTIVE = new TWSError(2107,
                                                   @"A historical data farm connection has become inactive but should be available upon demand.");
      MARKET_DATA_FARM_INACTIVE = new TWSError(2108,
                                               @"A market data farm connection has become inactive but should be available upon demand.");
      OUTSIDE_RTH_IGNORES = new TWSError(2109, "Order Event Warning: Attribute 'Outside Regular Trading Hours' is ignored based on the order type and destination. PlaceOrder is now processed");

      var q =
        from m in typeof (TWSErrors).GetMembers(BindingFlags.Public | BindingFlags.Static)
        where m.ReflectedType == typeof (TWSError)
        select m;


    }
  }
}