## What is [Project DM-D.S.S](https://dmdata.jp/)?

A paid service that delivers real-time earthquake information and other data via WebSocket.  
By default, KyoshinEewViewer for ingen uses the [JMA Disaster Prevention XML (PULL)](http://xml.kishou.go.jp/xmlpull.html) feed to receive earthquake information, which introduces a latency of **approximately 1-3 minutes** from announcement to reception.  
Using DM-D.S.S reduces this to **just a few seconds**, enabling reception speeds equal to or faster than those of TV broadcasters and corporate apps.

> [!WARNING]
> Under individual-use plans, it is prohibited to provide APIs that publicly redistribute EEW (forecast) content received from DM-D.S.S, or to post such content via automated bots on social media.

If you are using workflow integrations, please exercise caution.  
[See here for details](https://dmdata.jp/docs/eew#%E5%86%8D%E9%85%8D%E4%BF%A1%E3%83%9D%E3%83%AA%E3%82%B7%E3%83%BC).

## Pricing

|Category|Max Monthly|Daily Prorated|
|---|---|---|
|EEW (Forecast)|¥1,650/mo|¥75/day|
|EEW (Warning)|¥440/mo|¥20/day|
|Earthquake/Tsunami|¥550/mo|¥25/day|

(As of July 2022 / only the plans usable with this software are listed)
