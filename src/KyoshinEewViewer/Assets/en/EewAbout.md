Earthquake Early Warning (EEW) has inherent characteristics and limitations. For details, please refer to [the JMA website](https://www.data.jma.go.jp/svd/eew/data/nc/shikumi/whats-eew.html).

When receiving via the Kyoshin Monitor (default), only one EEW can be processed at a time. However, this software supports pseudo-simultaneous reception of multiple EEWs.  
When doing so, please note the following points based on Kyoshin Monitor specifications:

- Events with very deep hypocenters, or updates where the hypocenter becomes deeper in subsequent reports, cannot be received.
- Cancel reports and out-of-range conditions may not be handled properly.
- Some announcements may be missed, and the latest state may not always be maintained.

### Feature Matrix

SNP: SignalNowProfessional  
DM (Forecast): DM-D.S.S EEW (Forecast)  
DM (Warning): DM-D.S.S EEW (Warning)

|Feature \ Source|Kyoshin Monitor|SNP|DM (Forecast)|DM (Warning)|AXIS|
|---|---|---|---|---|---|
|**Price**|Free|Paid|Paid|Paid|Free|
|**Speed**|Slow|Fast|Fast|Fast|Fast|
|**Cancel Report**|No\*|Yes|Yes|Yes|Yes|
|**Deep Earthquake**|No|Yes|Yes|Yes|Yes|
|**Forecast**|Yes|Yes|Yes|No|Yes|
|**Max Intensity / Epicenter Name**|Yes|No|Yes|Yes|Yes|
|**Accuracy Info**|No|Yes|Yes|Yes|No|
|**Warning Area**|No|Yes|Yes|Yes|No|
|**Regional Max Intensities**|No|No|Yes|No|Yes|

\* The Kyoshin Monitor has a pseudo-cancel processing feature.
