# Connect to Interactive Brokers' TWS API seamlessly from .NET #

Daemaged.IBNet allows you to easily connect to [Interactive Brokers](http://www.interactivebrokers.com)' [TWS](http://www.interactivebrokers.com/en/index.php?f=674) trading platform, from .NET applications and manupulate the [IB API](https://www.interactivebrokers.com/en/index.php?f=5041) to:
 
* Monitor your IB Account Value
* Read your portfolio's composition
* Discover trade-able financial instruments (Stocks/Forex/Futures/Options/Bonds)
* Subscribe to real-time market-data (Quotes/Market Depth/More)
* Place/Cancel/Replace Orders
* Request Live Order Status and track their state
* Get open orders from the TWS platform
* Get order execution history
* Download historical data
* Get fundamental data
* Get news bulletins
* Run market scanners

Daemaged.IBNet is a pure managed .NET implementation built from scratch that implements the entire protocol used by TWS to support the various clients

## Getting Daemaged.IBNet ##

The easiest method to get started with Daemaged.IBNet by far is to use [nuget](http://nuget.org/) to obtain and intall the [package](http://nuget.org/packages/Daemaged.IBNet/) into your project.

If for some reason that is not a possibility, source drops can be downloaded from [github](https://github.com/damageboy/daemaged.ibnet/archive/master.zip) or the entire [repository](https://github.com/damageboy/daemaged.ibnet) can be cloned and built locally on you machine.

## Connecting to TWS from .NET ##
Before connecting to TWS, and socket API must be enabled through TWS, and it is recommended that localhost (127.0.0.1) be added to the list of trusted ip addresses.
Here's a [video showing](http://www.youtube.com/watch?v=_8iKWWsK0uM) how to do that.
