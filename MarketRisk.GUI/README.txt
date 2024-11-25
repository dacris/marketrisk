MarketRisk
(C) Dacris Software Inc.
www.dacris.com
---

1. Disclaimer
---
This software can be used to estimate the market risk involved in a specific investment decision.
This software does not provide financial advice. You should use it only as a rough guideline as part of a broader investment strategy.
We do not, in any way, guarantee that the returns or risks estimated by this software are accurate or will manifest.
Please consult with a qualified financial advisor before making your investment decision.

2. Running
---
Execute "MarketRisk.GUI.exe" to start the application.
Requires Windows 10 v1909 or later.
If you get a Windows warning, click "More Info" and then click "Run".

3. Description of Assets
---
There are 10 assets that this software enables you to explore (Assets menu).
1. USBond - This is the 10-year Treasury Bond (Asset Class: Government Bonds)
2. Dow - This is the Dow Jones Industrial Average (Asset Class: Stocks)
3. SP500 - This is the S&P 500 Index (Asset Class: Stocks)
4. Gold - This is the price of an ounce of gold (Asset Class: Bullion)
5. Silver - This is the price of an ounce of silver (Asset Class: Bullion)
6. Palladium - This is the price of an ounce of palladium (Asset Class: Bullion)
7. Platinum - This is the price of an ounce of platinum (Asset Class: Bullion)
8. TSX - This is the S&P/TSX Composite Index (Asset Class: Stocks)
9. CABond - This is the 10-year Canada Savings Bond (Asset Class: Government Bonds)
10. Bitcoin - This is the popular cryptocurrency (Asset Class: Crypto)

Choose "Best Configuration" for the configuration with the highest risk-adjusted-return.
Otherwise, you can build your own by checking & unchecking the menu items.

The asset prices & rates of return are configured in the "Data" folder and your own price data can be added there.

4. Position Recommendation Algorithms
---
The software uses a proprietary risk estimation algorithm to recommend your invested position size. For details, see #9.
The software is optimized to produce the highest return possible, with a constant limit of risk.
It limits the maximum sale to 20% of your position by default, to avoid paying lots of capital gains taxes.
This limit can be changed by adding a config setting to MarketRisk.GUI.exe.config: MaxSalePercent,
e.g. <add key="MaxSalePercent" value="30" />

5. Stats
---
The software shows you 50-year portfolio stats for a demo portfolio started in 1929 with a starting risk limit of $1000 1929 dollars.
The risk limit grows by a default real rate of 4% per year. This can be changed with MarketRisk.GUI.exe.config setting "RealRiskGrowthRatePercent".
The demo portfolio metrics are measured from 1970 to 2020, i.e. the last 50 years.

For reference:
Long-term inflation rate is 3.8%.
Money supply growth rate is 6.5%.

6. How Much to Invest
---
In addition to stats, the software can also tell you how much you should invest, based on your total risk tolerance.
Enter your risk amount in the top text box - e.g. 10000. This amount should be in present-day US dollars.

We recommend that you grow your risk amount by 3-8% in real terms every year, depending on your savings situation.
For instance, if you are saving at a real rate of 10% per year, you should aim for higher risk growth (8%).
If you are saving at only 5% per year, a lower number would be better (3%).

Next, you will need to enter the price of every asset, or in the case of bonds, the yields to date.
Optionally, enter the position size you currently have in your portfolio, in $. For precious metals, use spot prices per ounce.
Once you've entered your prices / yields, click on the asset you wish to find out about, and click "Calculate".
Your total position size and risk amount for that asset will be shown in the Portfolio Recommendations section.
Note: "Amount to Invest" is a total maximum amount. If you want to invest less, feel free to do so. You are not required to risk any $.
You can also create a report (after clicking Calculate) by going to File - Export HTML Report.

7. Adjustment Frequency
---
For best results, and throughout the application, portfolio adjustment frequency should be every year.
We recommend you run the application early in the year, e.g. in February, and then take the rest of the year to adjust your portfolio.
Trading frequently incurs penalties in the form of fees and taxes. This is why you should keep the frequency to yearly or less frequent.
Demo portfolio returns are for yearly adjustment, at the year's average price.

8. Taxes
---
In the calculated returns, capital gains & other taxes are not taken into consideration.
You should consider tax consequences if you plan to sell a large portion of your holdings or earn dividend income.
You would need to weigh your total risk against your total capital gains taxes and make a determination on whether it is better to sell or hold.
The only consideration we give is to capital gains taxes by limiting the amount you can sell in a given year to 20% of your position (see #4).

9. How We Calculate Risk
---
Risk is loosely defined as the ratio of expected bottom price:M2 to average price:M2 for a 90+ year historical period.
We have implemented several formulae including linear, quadratic, exponential, and reciprocal. The most successful ones are quadratic and reciprocal, of which we use the reciprocal formula as of build #10.
Not all formulae are listed here, but we will list the reciprocal formula below:
RiskRatio = MIN (0.975, averageRisk / 4.0 + (1.0 - averageRisk / 4.0) - (1.0 - averageRisk / 4.0) / MAX(1.0, ((price * MarginOfSafety * DeviationFromMinPrice / m2) / (1 / averageM2toPriceRatio))))
RiskRatio is the fraction of your position that is at risk of being lost, or the greatest potential loss (fraction lost) (within 1 year) that can be experienced by holding this investment asset.
For example, if my asset returns 5% in one year and my expected rate of return is 15.5%, the risk (loss) experienced in that year is 5 - 15.5 or -10.5%. As a risk ratio, this would be 0.105.
The above is further adjusted by making RiskRatio = RiskRatio ^ (1/4) if the asset is overvalued by 25% or more versus the average historical price:M2 ratio.
The averageRisk in our formula is determined by the average one-year decline in the asset price over its history, versus an expected 9% real rate of return. E.g. 0.05 means a 5% difference from a real return of 9%, or 4% real return.
AverageRisk/4.0 is our minimum risk. How little of a decline are we expecting in the best case scenario? This is most likely overestimated as there can be years when there is zero risk. This is our first margin of safety. For example, if averageRisk is 20%, minimum risk is 5%. Risk ratio cannot drop below that.
MarginOfSafety is 1.2 or 20% margin, meaning that the current price is determined to be 20% higher than its actual value, to allow a 20% margin of safety. This is the second margin of safety.
DeviationFromMinPrice is the median ratio between the average price:M2 ratio of the asset and the minimum price:M2 ratio. It is set to 1.5. It answers the question, what is the half-amplitude of the price wave (geometric amplitude)?
The above formula determines the risk ratio (i.e. the fraction of your investment that is at risk of loss).
Note - We consider any return lower than our expected return to be a risk. That is, even if the asset returned 6% in a given year, because it's under the 9% real return, there was actually a risk associated with holding the asset.
The only risk-free periods are when the asset outperforms our expected return. For example, if the asset returns 10% in real terms in a given year, we consider that year to be risk-free.
Using this risk ratio, we then find the recommended position by limiting the total amount of risk for the asset ($) to a fraction of your total desired portfolio risk.
In our design, we err on the side of caution by over-estimating the risk at several stages. For instance, we raise the risk ratio to a power of 1/4 if the asset is overvalued, we include a margin of safety, and we select a 9% real rate of return (which is very high) as our baseline.
Overall, the risk we estimate is almost always going to be higher than the actual risk. The intention is to have a 1 in 100 years chance that the estimated risk ratio will be exceeded by actual price action. A true once-in-a-century market crash would be required in order to produce a loss equal to what is estimated here.

10. What is Risk-Centric Investing?
---
MarketRisk is a software application that can be used for risk-centric investing.
Risk-centric investing is an investing technique that focuses on keeping risk constant and limited throughout your investing journey.
Risk is defined as the maximum one-year potential decline in your portfolio's value, throughout your portfolio's history.
Risk often occurs due to overvaluation, i.e. when an asset price is way above its historical average.
Risk is reduced when an asset is undervalued, i.e. its price is way below its historical average.
In a way, risk-centric investing is similar to value investing, in that it seeks to maximize investment into undervalued assets.
Depending on how quickly you grow your risk tolerance, you can use MarketRisk for growth investing, blend, or income investing.
For example, if your risk tolerance only grows by the inflation rate, you are doing income investing.
In income investing, most of your portfolio's gains are realized immediately and turned into income.
If your risk tolerance grows by a large amount, say 8% in real terms per year, you are doing growth investing.
In growth investing, most of your portfolio's gains are reinvested and grown over time, not realized immediately.
There are advantages and disadvantages to every investing technique.
The beauty of risk-centric investing is that you know how much you are risking, so in theory your money is safer over time.