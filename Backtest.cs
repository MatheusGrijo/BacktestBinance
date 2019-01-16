using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Backtest
{
    System.Data.DataTable dt = new System.Data.DataTable();

    public void create()
    {
        dt = new System.Data.DataTable();
        dt.Columns.Add("Date");
        dt.Columns.Add("Buy");
        dt.Columns.Add("Amount");
        dt.Columns.Add("Sell");
        dt.Columns.Add("Perc");
        dt.Columns.Add("Status");
        dt.Columns.Add("Stoploss");
        dt.Columns.Add("DateFinal");
        dt.Columns.Add("TotalTime");
        dt.Columns.Add("Diff");
    }

    public class ReturnDataArray
    {
        public double[] arrayPriceClose = null;
        public double[] arrayPriceHigh = null;
        public double[] arrayPriceLow = null;
        public double[] arrayPriceOpen = null;
        public double[] arrayVolume = null;
        public double[] arrayDate = null;
        public double[] arrayQuoteVolume = null;

    }
    public static ReturnDataArray getDataArray(string coin, string timeGraph)
    {
        int i = 0;
        try
        {

            ReturnDataArray returnDataArray = new ReturnDataArray();



            String jsonAsStringRSI = "";
            if (source == "CACHE")
            {
                DateTime begin = DateTime.Parse("2018-01-01");
                if (!System.IO.File.Exists(Program.location + @"\cache\" + coin + timeGraph + ".txt"))
                {
                    System.IO.StreamWriter w = new System.IO.StreamWriter(Program.location + @"\cache\" + coin + timeGraph + ".txt", true);
                    while (begin != DateTime.Parse("2018-12-29"))
                    {
                        jsonAsStringRSI = Http.get("https://api.binance.com/api/v1/klines?symbol=" + coin + "&interval=" + timeGraph + "&startTime=" + DatetimeToUnix(DateTime.Parse(begin.ToString("yyyy-MM-dd") + " 00:00:00")) + "&endTime=" + DatetimeToUnix(DateTime.Parse(begin.ToString("yyyy-MM-dd") + " 12:59:59")), false).Replace("[[", "[").Replace("]]", "]") + ",";
                        jsonAsStringRSI += Http.get("https://api.binance.com/api/v1/klines?symbol=" + coin + "&interval=" + timeGraph + "&startTime=" + DatetimeToUnix(DateTime.Parse(begin.ToString("yyyy-MM-dd") + " 13:00:00")) + "&endTime=" + DatetimeToUnix(DateTime.Parse(begin.ToString("yyyy-MM-dd") + " 23:59:59")), false).Replace("[[", "[").Replace("]]", "]") + ",";

                        w.Write(jsonAsStringRSI);

                        begin = begin.AddDays(1);
                        System.Threading.Thread.Sleep(1000);
                    }
                    w.Close();
                    w.Dispose();
                    jsonAsStringRSI = "[" + System.IO.File.ReadAllText(Program.location + @"\cache\" + coin + timeGraph + ".txt") + "]";
                    jsonAsStringRSI = jsonAsStringRSI.Substring(0, jsonAsStringRSI.Length - 1);
                    System.IO.File.Delete(Program.location + @"\cache\" + coin + timeGraph + ".txt");

                    w = new System.IO.StreamWriter(Program.location + @"\cache\" + coin + timeGraph + ".txt", true);
                    w.Write(jsonAsStringRSI);
                    w.Close();
                    w.Dispose();

                }


                jsonAsStringRSI = System.IO.File.ReadAllText(Program.location + @"\cache\" + coin + timeGraph + ".txt");
            }
            else
                jsonAsStringRSI = Http.get("https://api.binance.com/api/v1/klines?symbol=" + coin + "&interval=" + timeGraph + "&limit=1000", true);



            Newtonsoft.Json.Linq.JContainer jsonRSI = (Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(jsonAsStringRSI);

            i = 0;
            foreach (JContainer element in jsonRSI.Children())
                i++;

            returnDataArray.arrayPriceClose = new double[i];
            returnDataArray.arrayPriceHigh = new double[i];
            returnDataArray.arrayPriceLow = new double[i];
            returnDataArray.arrayPriceOpen = new double[i];
            returnDataArray.arrayVolume = new double[i];
            returnDataArray.arrayDate = new double[i];
            returnDataArray.arrayQuoteVolume = new double[i];

            i = 0;
            foreach (JContainer element in jsonRSI.Children())
            {

                returnDataArray.arrayPriceClose[i] = double.Parse(element[4].ToString().Replace(".", ","));
                returnDataArray.arrayPriceHigh[i] = double.Parse(element[2].ToString().Replace(".", ","));
                returnDataArray.arrayPriceLow[i] = double.Parse(element[3].ToString().Replace(".", ","));
                returnDataArray.arrayPriceOpen[i] = double.Parse(element[1].ToString().Replace(".", ","));
                returnDataArray.arrayVolume[i] = double.Parse(element[5].ToString().Replace(".", ","));
                returnDataArray.arrayQuoteVolume[i] = double.Parse(element[7].ToString().Replace(".", ","));
                returnDataArray.arrayDate[i] = double.Parse(element[6].ToString().Replace(".", ","));
                i++;
            }

            return returnDataArray;
        }
        catch (Exception ex)
        {
            return null;
        }

    }
    public static ReturnDataArray getDataArrayCW(string coin, string timeGraph)
    {
        ReturnDataArray returnDataArray = new ReturnDataArray();
        String jsonAsStringRSI = Http.get("https://api.cryptowat.ch/markets/binance/" + coin + "/ohlc?periods=" + (int.Parse(timeGraph.Replace("m", "")) * 60) + "&after=1514764800", false);
        Newtonsoft.Json.Linq.JContainer jsonRSI = (Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(jsonAsStringRSI);

        string auxarray = "300";
        if (timeGraph == "5m")
            auxarray = "300";
        if (timeGraph == "15m")
            auxarray = "900";

        int count = 0;
        foreach (JContainer element in jsonRSI["result"][auxarray])
            count++;

        returnDataArray.arrayPriceClose = new double[count];
        returnDataArray.arrayPriceHigh = new double[count];
        returnDataArray.arrayPriceLow = new double[count];
        returnDataArray.arrayPriceOpen = new double[count];
        returnDataArray.arrayVolume = new double[count];
        returnDataArray.arrayDate = new double[count];
        returnDataArray.arrayQuoteVolume = new double[count];
        int i = 0;
        foreach (JContainer element in jsonRSI["result"][auxarray])
        {
            returnDataArray.arrayPriceClose[i] = double.Parse(element[4].ToString().Replace(".", ","));
            returnDataArray.arrayPriceHigh[i] = double.Parse(element[2].ToString().Replace(".", ","));
            returnDataArray.arrayPriceLow[i] = double.Parse(element[3].ToString().Replace(".", ","));
            returnDataArray.arrayPriceOpen[i] = double.Parse(element[1].ToString().Replace(".", ","));
            returnDataArray.arrayVolume[i] = double.Parse(element[5].ToString().Replace(".", ","));
            //returnDataArray.arrayQuoteVolume[i] = double.Parse(element[7].ToString().Replace(".", ","));
            returnDataArray.arrayDate[i] = double.Parse(element[0].ToString().Replace(".", ","));
            i++;
        }

        return returnDataArray;
    }

    public static void log(string value)
    {
        Console.WriteLine(value);
    }

    public double[] arrayCut(double[] array, int _i)
    {
        try

        {
            double[] r = new double[_i + 1];

            for (int i = 0; i < array.Length; i++)
            {
                r[i] = array[i];
                if (_i == i)
                    break;
            }
            return r;
        }
        catch (Exception ex)
        {
            return null;
        }

    }

    public static void ToPrintConsole(DataTable dataTable)
    {
        // Print top line
        Console.WriteLine(new string('-', 200));

        // Print col headers
        var colHeaders = dataTable.Columns.Cast<DataColumn>().Select(arg => arg.ColumnName);
        foreach (String s in colHeaders)
        {
            Console.Write("| {0,-20}", s);
        }
        Console.WriteLine();

        // Print line below col headers
        Console.WriteLine(new string('-', 200));

        // Print rows
        foreach (DataRow row in dataTable.Rows)
        {
            foreach (Object o in row.ItemArray)
            {
                Console.Write("| {0,-20}", o.ToString());
            }
            log("");
        }

        // Print bottom line
        log(new string('-', 200));
    }



    public String Strategy2(double[] arrayPriceOpen, double[] arrayPriceClose, double[] arrayPriceLow, double[] arrayPriceHigh, double[] arrayVolume, int i)
    {
        try
        {

            int outBegidx, outNbElement;
            int outBegidx1, outNbElement1;
            int outBegidx2, outNbElement2;
            int outBegidx3, outNbElement3;
            int outBegidx4, outNbElement4;
            int outBegidx5, outNbElement5;
            int outBegidx6, outNbElement6;

            int outBegidx7, outNbElement7;

            int outBegidx8, outNbElement8;

            int outBegidx9, outNbElement9;
            int outBegidx10, outNbElement10;

            arrayPriceOpen = arrayCut(arrayPriceOpen, i);
            arrayPriceClose = arrayCut(arrayPriceClose, i);
            arrayPriceLow = arrayCut(arrayPriceLow, i);
            arrayPriceHigh = arrayCut(arrayPriceHigh, i);
            arrayVolume = arrayCut(arrayVolume, i);


            double[] arrayresultEMA = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 26, TicTacTec.TA.Library.Core.MAType.Dema, out outBegidx, out outNbElement, arrayresultEMA);

            double[] arrayresultEMA30 = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 100, TicTacTec.TA.Library.Core.MAType.Dema, out outBegidx1, out outNbElement1, arrayresultEMA30);

            double[] arrayresultEMA200 = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 200, TicTacTec.TA.Library.Core.MAType.Dema, out outBegidx2, out outNbElement2, arrayresultEMA200);

            double[] arrayresultADX = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.Adx(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, 20, out outBegidx3, out outNbElement3, arrayresultADX);

            if (arrayPriceOpen[arrayPriceClose.Length - 1] > arrayresultEMA200[outNbElement1 - 1])
                if (arrayPriceHigh[arrayPriceClose.Length - 2] < arrayresultEMA200[outNbElement1 - 2])
                    return "buy";

            //if (arrayresultADX[outNbElement3 - 1] > arrayresultADX[outNbElement3 - 2] && arrayresultADX[outNbElement3 - 2] > arrayresultADX[outNbElement3 - 3])
            if (arrayresultEMA200[outNbElement2 - 1] > arrayresultEMA200[outNbElement2 - 2] && arrayresultEMA200[outNbElement2 - 2] > arrayresultEMA200[outNbElement2 - 3] && arrayresultEMA200[outNbElement2 - 3] > arrayresultEMA200[outNbElement2 - 4] && arrayresultEMA200[outNbElement2 - 4] > arrayresultEMA200[outNbElement2 - 5]
            && arrayresultEMA200[outNbElement2 - 5] > arrayresultEMA200[outNbElement2 - 6] && arrayresultEMA200[outNbElement2 - 6] > arrayresultEMA200[outNbElement2 - 7] && arrayresultEMA200[outNbElement2 - 7] > arrayresultEMA200[outNbElement2 - 8] && arrayresultEMA200[outNbElement2 - 8] > arrayresultEMA200[outNbElement2 - 9]
            )
                if (arrayresultEMA30[outNbElement2 - 1] > arrayresultEMA30[outNbElement2 - 2] && arrayresultEMA30[outNbElement2 - 2] > arrayresultEMA30[outNbElement2 - 3] && arrayresultEMA30[outNbElement2 - 3] > arrayresultEMA30[outNbElement2 - 4] && arrayresultEMA30[outNbElement2 - 4] > arrayresultEMA30[outNbElement2 - 5])

                    if (arrayresultEMA[outNbElement - 1] > arrayresultEMA200[outNbElement2 - 1] && arrayresultEMA30[outNbElement1 - 1] > arrayresultEMA200[outNbElement2 - 1])
                        if (arrayresultEMA[outNbElement - 4] < arrayresultEMA30[outNbElement1 - 4])
                            if (arrayresultEMA[outNbElement - 3] < arrayresultEMA30[outNbElement1 - 3])
                                if (arrayresultEMA[outNbElement - 2] < arrayresultEMA30[outNbElement1 - 2])
                                    if (arrayresultEMA[outNbElement - 1] > arrayresultEMA30[outNbElement1 - 1])
                                        if (arrayPriceClose[arrayPriceClose.Length - 1] > arrayresultEMA200[outNbElement1 - 1])
                                            if (arrayPriceClose[arrayPriceClose.Length - 1] > arrayresultEMA30[outNbElement1 - 1])
                                            {
                                                return "buay";
                                            }


            //double[] arrayresultRSI = new double[arrayPriceClose.Length];
            //TicTacTec.TA.Library.Core.Rsi(0, arrayPriceClose.Length - 1, arrayPriceClose, 20, out outBegidx, out outNbElement, arrayresultRSI);
            //int[] arrayresultBaby = new int[arrayPriceClose.Length];
            //int[] arrayresultBaby2 = new int[arrayPriceClose.Length];
            //TicTacTec.TA.Library.Core.CdlLongLine(0, arrayPriceClose.Length - 1, arrayPriceOpen, arrayPriceHigh, arrayPriceLow, arrayPriceClose, out outBegidx7, out outNbElement7, arrayresultBaby);
            //TicTacTec.TA.Library.Core.Cdl3LineStrike(0, arrayPriceClose.Length - 1, arrayPriceOpen, arrayPriceHigh, arrayPriceLow, arrayPriceClose, out outBegidx8, out outNbElement8, arrayresultBaby2);
            //arrayresultADX = new double[arrayPriceClose.Length];
            //TicTacTec.TA.Library.Core.Adx(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, 20, out outBegidx3, out outNbElement3, arrayresultADX);
            //if (arrayresultADX[outNbElement3 - 1] > arrayresultADX[outNbElement3 - 2] && arrayresultADX[outNbElement3 - 2] > arrayresultADX[outNbElement3 - 3]
            //    && arrayresultADX[outNbElement3 - 3] > arrayresultADX[outNbElement3 - 4] && arrayresultADX[outNbElement3 - 4] > arrayresultADX[outNbElement3 - 5]
            //    && arrayresultADX[outNbElement3 - 5] > arrayresultADX[outNbElement3 - 6] && arrayresultADX[outNbElement3 - 6] > arrayresultADX[outNbElement3 - 7]
            //    )
            //{
            //    if (arrayresultADX[outNbElement3 - 1] > 40)
            //        //if (arrayPriceHigh[arrayPriceClose.Length - 1] > arrayPriceHigh[arrayPriceClose.Length - 3] && arrayPriceHigh[arrayPriceClose.Length - 1] > arrayPriceHigh[arrayPriceClose.Length - 2])
            //        //if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayPriceClose[arrayPriceClose.Length - 4] && arrayPriceClose[arrayPriceClose.Length - 4] < arrayPriceClose[arrayPriceClose.Length - 5])
            //        //if (arrayresultRSI[outNbElement - 1] > 30 && arrayresultRSI[outNbElement - 2] < 30)

            //        if (arrayresultBaby[outNbElement7 - 1] > 0)
            //        {
            //            return "buy1";
            //        }
            //}
            //if (arrayresultBaby2[outNbElement8 - 1] > 0)
            //{
            //    return "buya";
            //}
            //if (arrayresultRSI[outNbElement - 1] > 30 && arrayresultRSI[outNbElement - 2] < 30 && arrayresultRSI[outNbElement - 3] < 30
            //    //&& (arrayresultRSI[outNbElement - 1] - arrayresultRSI[outNbElement - 2]) < 10
            //    )
            //{
            //    if (arrayresultBaby[outNbElement7 - 1] > 0)
            //        return "buy";
            //}


            double[] arrayresultRSI = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.Rsi(0, arrayPriceClose.Length - 1, arrayPriceClose, 20, out outBegidx, out outNbElement, arrayresultRSI);
            int[] arrayresultBaby = new int[arrayPriceClose.Length];
            int[] arrayresultBaby2 = new int[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.CdlHammer(0, arrayPriceClose.Length - 1, arrayPriceOpen, arrayPriceHigh, arrayPriceLow, arrayPriceClose, out outBegidx7, out outNbElement7, arrayresultBaby);
            TicTacTec.TA.Library.Core.CdlDoji(0, arrayPriceClose.Length - 1, arrayPriceOpen, arrayPriceHigh, arrayPriceLow, arrayPriceClose, out outBegidx8, out outNbElement8, arrayresultBaby2);
            arrayresultADX = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.Adx(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, 20, out outBegidx3, out outNbElement3, arrayresultADX);
            if (arrayresultADX[outNbElement3 - 1] > arrayresultADX[outNbElement3 - 2] && arrayresultADX[outNbElement3 - 2] > arrayresultADX[outNbElement3 - 3]
                && arrayresultADX[outNbElement3 - 3] > arrayresultADX[outNbElement3 - 4] && arrayresultADX[outNbElement3 - 4] > arrayresultADX[outNbElement3 - 5]
                && arrayresultADX[outNbElement3 - 5] > arrayresultADX[outNbElement3 - 6] && arrayresultADX[outNbElement3 - 6] > arrayresultADX[outNbElement3 - 7]
                )
            {
                if (arrayresultADX[outNbElement3 - 1] > 40)
                    //if (arrayPriceHigh[arrayPriceClose.Length - 1] > arrayPriceHigh[arrayPriceClose.Length - 3] && arrayPriceHigh[arrayPriceClose.Length - 1] > arrayPriceHigh[arrayPriceClose.Length - 2])
                    //if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayPriceClose[arrayPriceClose.Length - 4] && arrayPriceClose[arrayPriceClose.Length - 4] < arrayPriceClose[arrayPriceClose.Length - 5])
                    //if (arrayresultRSI[outNbElement - 1] > 30 && arrayresultRSI[outNbElement - 2] < 30)

                    if (arrayresultBaby[outNbElement7 - 1] > 0)
                    {
                        return "buy1";
                    }
            }
            {
                if (arrayresultBaby2[outNbElement8 - 2] > 0)
                    if (arrayresultBaby2[outNbElement8 - 3] > 0)
                        if (arrayresultBaby[outNbElement7 - 1] > 0)
                            return "buya";
            }


            arrayresultEMA200 = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 144, TicTacTec.TA.Library.Core.MAType.Ema, out outBegidx2, out outNbElement2, arrayresultEMA200);
            arrayresultADX = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.Adx(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, 20, out outBegidx3, out outNbElement3, arrayresultADX);
            TicTacTec.TA.Library.Core.CdlEngulfing(0, arrayPriceClose.Length - 1, arrayPriceOpen, arrayPriceHigh, arrayPriceLow, arrayPriceClose, out outBegidx7, out outNbElement7, arrayresultBaby);
            //if (arrayresultADX[outNbElement3 - 1] > arrayresultADX[outNbElement3 - 2])
            //  if (arrayresultEMA200[outNbElement2 - 1] >= arrayresultEMA200[outNbElement2 - 2] && arrayresultEMA200[outNbElement2 - 2] >= arrayresultEMA200[outNbElement2 - 3] && arrayresultEMA200[outNbElement2 - 3] >= arrayresultEMA200[outNbElement2 - 4])
            //    if (arrayPriceClose[arrayPriceClose.Length - 2] >= arrayPriceClose[arrayPriceClose.Length - 3] && arrayPriceClose[arrayPriceClose.Length - 3] >= arrayPriceClose[arrayPriceClose.Length - 4])
            if (arrayresultBaby[outNbElement7 - 1] > 0)
            {
                return "buy";
            }


            double[] arrayMACD = new double[arrayPriceClose.Length];
            double[] arrayMACDSignal = new double[arrayPriceClose.Length];
            double[] arrayMACDHist = new double[arrayPriceClose.Length];
            double[] arrayresultMFI = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.Mfi(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, arrayVolume, 20, out outBegidx4, out outNbElement4, arrayresultMFI);
            TicTacTec.TA.Library.Core.Macd(0, arrayPriceClose.Length - 1, arrayPriceClose, 9, 15, 7, out outBegidx10, out outNbElement10, arrayMACD, arrayMACDSignal, arrayMACDHist);
            if ((arrayresultMFI[outNbElement4 - 1] > 30 && arrayresultMFI[outNbElement4 - 2] < 30) && arrayresultMFI[outNbElement4 - 2] > arrayresultMFI[outNbElement4 - 3])
                //if ((arrayresultMFI[outNbElement4 - 1] - arrayresultMFI[outNbElement4 - 2] < 10))
                if (arrayMACDHist[outNbElement10 - 2] < 0 && arrayMACDHist[outNbElement10 - 3] < 0 && arrayMACDHist[outNbElement10 - 4] < 0 && arrayMACDHist[outNbElement10 - 5] < 0 && arrayMACDHist[outNbElement10 - 6] < 0)
                    //&& arrayMACDHist[outNbElement10 - 7] < 0 && arrayMACDHist[outNbElement10 - 8] < 0 && arrayMACDHist[outNbElement10 - 9] < 0 && arrayMACDHist[outNbElement10 - 10] < 0)
                    ////&& arrayMACDHist[outNbElement10 - 11] < 0 && arrayMACDHist[outNbElement10 - 12] < 0 && arrayMACDHist[outNbElement10 - 13] < 0 && arrayMACDHist[outNbElement10 - 14] < 0
                    if (arrayMACDHist[outNbElement10 - 1] > 0
                    )
                    {
                        //if (arrayMACD[outNbElement10 - 1] > 0 && arrayMACDSignal[outNbElement10 - 1] > 0 && arrayMACD[outNbElement10 - 2] > 0 && arrayMACDSignal[outNbElement10 - 2] > 0)
                        return "buya";
                    }


            return "nothing";
        }
        catch
        {
            return "nothing";
        }
    }

    public String StrategyG(double[] arrayPriceOpen, double[] arrayPriceClose, double[] arrayPriceLow, double[] arrayPriceHigh, double[] arrayVolume, int i)
    {
        try
        {

            int outBegidx, outNbElement;

            arrayPriceOpen = arrayCut(arrayPriceOpen, i);
            arrayPriceClose = arrayCut(arrayPriceClose, i);
            arrayPriceLow = arrayCut(arrayPriceLow, i);
            arrayPriceHigh = arrayCut(arrayPriceHigh, i);
            arrayVolume = arrayCut(arrayVolume, i);


            double[] arrayresultEMA = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 26, TicTacTec.TA.Library.Core.MAType.Ema, out outBegidx, out outNbElement, arrayresultEMA);



            if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayPriceClose[arrayPriceClose.Length - 2] && arrayPriceClose[arrayPriceClose.Length - 2] < arrayPriceClose[arrayPriceClose.Length - 1])
                if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayresultEMA[outNbElement - 3] && arrayPriceClose[arrayPriceClose.Length - 2] < arrayresultEMA[outNbElement - 2])
                    if (arrayPriceClose[arrayPriceClose.Length - 1] > arrayresultEMA[outNbElement - 1])
                    {
                        double[] arrayresultRSI = new double[arrayPriceClose.Length];
                        TicTacTec.TA.Library.Core.Rsi(0, arrayPriceClose.Length - 1, arrayPriceClose, 14, out outBegidx, out outNbElement, arrayresultRSI);

                        if (arrayresultRSI[outNbElement - 1] > 50 && arrayresultRSI[outNbElement - 2] < 50)
                        {
                            double[] arrayresultADX = new double[arrayPriceClose.Length];
                            TicTacTec.TA.Library.Core.Adx(0, arrayPriceClose.Length - 1, arrayPriceHigh, arrayPriceLow, arrayPriceClose, 20, out outNbElement, out outNbElement, arrayresultADX);
                            if (arrayresultADX[outNbElement - 1] > 30)
                                return "buy";
                        }
                    }

            if (arrayPriceClose[arrayPriceClose.Length - 2] > arrayresultEMA[outNbElement - 2])
                if (arrayPriceClose[arrayPriceClose.Length - 1] < arrayresultEMA[outNbElement - 1])
                    if (arrayPriceLow[arrayPriceClose.Length - 1] < arrayPriceLow[outNbElement - 1])
                        return "sell";






            return "nothing";
        }
        catch
        {
            return "nothing";
        }
    }

    public String Strategy9(double[] arrayPriceOpen, double[] arrayPriceClose, double[] arrayPriceLow, double[] arrayPriceHigh, double[] arrayVolume, int i)
    {
        try
        {

            int outBegidx, outNbElement;

            arrayPriceOpen = arrayCut(arrayPriceOpen, i);
            arrayPriceClose = arrayCut(arrayPriceClose, i);
            arrayPriceLow = arrayCut(arrayPriceLow, i);
            arrayPriceHigh = arrayCut(arrayPriceHigh, i);
            arrayVolume = arrayCut(arrayVolume, i);


            double[] arrayresultEMA = new double[arrayPriceClose.Length];
            TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 9, TicTacTec.TA.Library.Core.MAType.Ema, out outBegidx, out outNbElement, arrayresultEMA);

            if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayPriceClose[arrayPriceClose.Length - 2] && arrayPriceClose[arrayPriceClose.Length - 2] < arrayPriceClose[arrayPriceClose.Length - 1])
                if (arrayPriceHigh[arrayPriceClose.Length - 3] < arrayPriceHigh[arrayPriceClose.Length - 2] && arrayPriceHigh[arrayPriceClose.Length - 2] < arrayPriceHigh[arrayPriceClose.Length - 1])
                    if (arrayPriceLow[arrayPriceClose.Length - 3] < arrayPriceLow[arrayPriceClose.Length - 2] && arrayPriceLow[arrayPriceClose.Length - 2] < arrayPriceLow[arrayPriceClose.Length - 1])
                        if (arrayPriceClose[arrayPriceClose.Length - 3] < arrayresultEMA[outNbElement - 3] && arrayPriceClose[arrayPriceClose.Length - 2] < arrayresultEMA[outNbElement - 2])
                            if (arrayPriceClose[arrayPriceClose.Length - 1] > arrayresultEMA[outNbElement - 1])
                                if (arrayPriceHigh[arrayPriceClose.Length - 1] > arrayPriceHigh[outNbElement - 1])
                                    return "buy";

            if (arrayPriceClose[arrayPriceClose.Length - 2] > arrayresultEMA[outNbElement - 2])
                if (arrayPriceClose[arrayPriceClose.Length - 1] < arrayresultEMA[outNbElement - 1])
                    if (arrayPriceLow[arrayPriceClose.Length - 1] < arrayPriceLow[outNbElement - 1])
                        return "sell";






            return "nothing";
        }
        catch
        {
            return "nothing";
        }
    }


    public double _stop = 2;
    public double _profit = 0.35;
    public int ganancia = 33;
    public int _volume =100;
    public static string source = "CW";
    public static string pair = "BTC";

    public static string timegraph = "5m";//Console.ReadLine();
    public static string _begin = "2019-01-01 00:00:00";
    public static string _end = "2019-01-16 23:59:59";

    public void run(String coin, DateTime begin, DateTime final, decimal totalBtc = 2, double profit = 0.4, double stop = 0.6, double investPerOperation = 0.05, string ___timegraph = "5m")
    {
        try
        {


            //log("Profit(0.3): ");
            //profit = double.Parse(Console.ReadLine());
            profit = _profit;
            //log("Sstop(0.3): ");
            stop = _stop;// double.Parse(Console.ReadLine());
            //log("Time Frame(5m): ");
            
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./time_" + timegraph + "_log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            //Console.SetOut(writer);


            String _coins = "";
            String jsonTicker = Http.get("https://api.binance.com/api/v1/ticker/24hr", false);
            JContainer jContainer = (JContainer)JsonConvert.DeserializeObject(jsonTicker, (typeof(JContainer)));
            foreach (var item in jContainer)
            {
                if (item["symbol"].ToString().Substring(item["symbol"].ToString().Length - pair.Length, pair.Length).Trim().ToUpper() == pair)
                {

                    decimal volume = decimal.Parse(item["quoteVolume"].ToString().Replace(".", ","));
                    decimal priceChange = decimal.Parse(item["priceChange"].ToString().Replace(".", ","));
                    if (volume >= _volume)
                    {
                       //if (item["symbol"].ToString().IndexOf("BCH") < 0)
                            if (item["symbol"].ToString().IndexOf("BNB") < 0)
                                //if (item["symbol"].ToString().IndexOf("VET") < 0)
                                   // if (item["symbol"].ToString().IndexOf("USD") < 0)
                                    if (decimal.Parse(item["lastPrice"].ToString().Replace(".", ",")) > 0.00000200m)
                                        if (decimal.Parse(item["bidPrice"].ToString().Replace(".", ",")) > 0)
                                        //if (( decimal.Parse(item["priceChangePercent"].ToString().Replace(".", ","))) > -10 && (decimal.Parse(item["priceChangePercent"].ToString().Replace(".", ","))) < 10)                                            
                                            _coins += item["symbol"].ToString() + ";";
                    }
                }
            }
            _coins = _coins.Substring(0, _coins.Length - 1);


            //_coins = "BTCUSDT;XRPUSDT;ETHUSDT;TRXUSDT;EOSUSDT;BNBUSDT;ADAUSDT;LTCUSDT;ETCUSDT;IOTAUSDT;NEOUSDT;ONTUSDT;QTUMUSDT;XLMUSDT;VETUSDT";
            //_coins = "ETHBTC;ETCBTC;NEOBTC;XRPBTC;XLMBTC;EOSBTC;TUSDBTC;XMRBTC;NANOBTC;IOTABTC";

            ReturnDataArray returnDataArray = null;
            StringBuilder sb = new StringBuilder();
            String[] arrayPair = _coins.Split(';');
            double gainTotal = 0;
            double lossTotal = 0;
            int openTotal = 0;
            double perc = 0;
            for (int zx = 0; zx < arrayPair.Length; zx++)
            {
                try
                {


                    //log("Pair(XMRBTC): ");
                    coin = arrayPair[zx];// Console.ReadLine();



                    create();
                    log("Backteste " + coin);

                    if(source == "CW")
                        returnDataArray = getDataArrayCW(coin, timegraph);
                    else
                        returnDataArray = getDataArray(coin, timegraph);

                    int gain = 0;
                    int loss = 0;

                    for (int i = 0; i < returnDataArray.arrayDate.Length; i++)
                    {
                        if (UnixTimeStampToDateTime(returnDataArray.arrayDate[i]) > DateTime.Parse(_begin) && UnixTimeStampToDateTime(returnDataArray.arrayDate[i]) < DateTime.Parse(_end))
                        {
                            Console.Title = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]).ToString("dd/MM/yyyy HH:mm:ss") + "| GAIN: " + gain + "| LOSS: " + loss + "| Perc " + perc;
                            double stopValue = 0;
                            double targetValue = 0;
                            double diff = 0;
                            String ret = Strategy(returnDataArray.arrayDate, returnDataArray.arrayPriceOpen, returnDataArray.arrayPriceClose, returnDataArray.arrayPriceLow, returnDataArray.arrayPriceHigh, returnDataArray.arrayVolume, i, out stopValue, out targetValue, out diff);


                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (dt.Rows[x]["Status"].ToString() == "open")
                                {
                                    //if (1 == 1)
                                    {

                                    }
                                    //   else
                                    {

                                        if (returnDataArray.arrayPriceHigh[i] > double.Parse(dt.Rows[x]["Sell"].ToString()))
                                        {

                                            dt.Rows[x]["Status"] = "gain";
                                            dt.Rows[x]["DateFinal"] = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]);
                                            TimeSpan ts = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]) - DateTime.Parse(dt.Rows[x]["Date"].ToString());
                                            dt.Rows[x]["TotalTime"] = ts.TotalMinutes;
                                            double auxPerc = ((double.Parse(dt.Rows[x]["Sell"].ToString()) * 100) / double.Parse(dt.Rows[x]["Buy"].ToString()) - 100);
                                            //if (auxPerc > profit)
                                              //  auxPerc = profit;

                                            auxPerc = profit - 0.2;

                                            perc += auxPerc;
                                            dt.Rows[x]["Perc"] = auxPerc;
                                            gain++;
                                        }
                                        else if (double.Parse(dt.Rows[x]["Stoploss"].ToString()) >= returnDataArray.arrayPriceLow[i])
                                        {
                                            dt.Rows[x]["Status"] = "loss";
                                            dt.Rows[x]["DateFinal"] = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]);
                                            TimeSpan ts = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]) - DateTime.Parse(dt.Rows[x]["Date"].ToString());
                                            dt.Rows[x]["TotalTime"] = ts.TotalMinutes;
                                            double auxPerc = ((double.Parse(dt.Rows[x]["Stoploss"].ToString()) * 100) / double.Parse(dt.Rows[x]["Buy"].ToString()) - 100) ;
                                           // if (auxPerc < -1 * stop)
                                                auxPerc += -1 * (0.2);
                                            perc += auxPerc;
                                            dt.Rows[x]["Perc"] = auxPerc;
                                            loss++;
                                        }
                                        else
                                        {
                                            if (ret == "sell")
                                            {
                                                if (returnDataArray.arrayPriceClose[i] > double.Parse(dt.Rows[x]["Buy"].ToString()))
                                                {
                                                    gain++;
                                                    dt.Rows[x]["Status"] = "gain";
                                                }
                                                else
                                                {
                                                    loss++;
                                                    dt.Rows[x]["Status"] = "loss";
                                                }

                                                dt.Rows[x]["DateFinal"] = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]);
                                                TimeSpan ts = UnixTimeStampToDateTime(returnDataArray.arrayDate[i]) - DateTime.Parse(dt.Rows[x]["Date"].ToString());
                                                dt.Rows[x]["TotalTime"] = ts.TotalMinutes;
                                                double auxPerc = ((returnDataArray.arrayPriceClose[i] * 100) / double.Parse(dt.Rows[x]["Buy"].ToString()) - 100) - 0.2;
                                                //if (auxPerc > 5)
                                                //auxPerc = 0;

                                                perc += auxPerc;
                                                dt.Rows[x]["Perc"] = auxPerc;
                                            }

                                        }
                                    }
                                }
                            }


                            if (ret == "buy")
                            {
                                //Console.WriteLine(UnixTimeStampToDateTime(returnDataArray.arrayDate[i]).ToString());
                                //Console.WriteLine((returnDataArray.arrayPriceOpen[i]).ToString());
                                //Console.WriteLine((returnDataArray.arrayPriceClose[i]).ToString());
                                //Console.WriteLine((returnDataArray.arrayPriceHigh[i]).ToString());
                                //Console.WriteLine((returnDataArray.arrayPriceLow[i]).ToString());
                                double sell = targetValue;// ((returnDataArray.arrayPriceClose[i] * profit) / 100) + returnDataArray.arrayPriceClose[i];
                                double stoploss = returnDataArray.arrayPriceClose[i] - ((returnDataArray.arrayPriceClose[i] * stop) / 100);
                                if (stopValue != 0)
                                    stoploss = stopValue;
                                dt.Rows.Add(UnixTimeStampToDateTime(returnDataArray.arrayDate[i]).ToString(), Math.Round(returnDataArray.arrayPriceClose[i], 8), Math.Round((investPerOperation / returnDataArray.arrayPriceClose[i]), 8), Math.Round(sell, 8), 0, "open", Math.Round(stoploss, 8), DateTime.Now.AddDays(-5000), "",diff);
                            }

                        }
                    }



                    sb.AppendLine(coin);
                    sb.AppendLine("GAIN: " + gain + " | " + (gain * profit) + "%");
                    sb.AppendLine("LOSS: " + loss + " | " + (loss * stop) + "%");
                    sb.AppendLine("RESULT FINAL: " + ((gain * profit) - (loss * stop)) + "%");

                    gainTotal += (gain);
                    lossTotal += (loss);

                    for (int x = 0; x < dt.Rows.Count; x++)
                        if (dt.Rows[x]["Status"].ToString().Trim().ToLower().IndexOf("open") >= 0)
                        {
                            openTotal++;
                            dt.Rows[x]["DateFinal"] = UnixTimeStampToDateTime(returnDataArray.arrayDate[returnDataArray.arrayDate.Length - 1]);
                            TimeSpan ts = UnixTimeStampToDateTime(returnDataArray.arrayDate[returnDataArray.arrayDate.Length - 1]) - DateTime.Parse(dt.Rows[x]["Date"].ToString());
                            dt.Rows[x]["TotalTime"] = ts.TotalMinutes;
                            double auxPerc = ((returnDataArray.arrayPriceClose[returnDataArray.arrayDate.Length - 1] * 100) / double.Parse(dt.Rows[x]["Buy"].ToString()) - 100) - 0.2;
                            //if (auxPerc > 5)
                            //  auxPerc = 0;
                            dt.Rows[x]["Status"] = "force";
                            //perc += auxPerc;
                            dt.Rows[x]["Perc"] = auxPerc;
                        }

                    ToPrintConsole(dt);

                    sb.AppendLine("===========================");
                    //Console.ReadLine();

                    Console.Title = "GAIN: " + gainTotal + "| LOSS: " + lossTotal + "| Perc " + perc;
                }
                catch
                { }
            }






            log(sb.ToString());
            log(UnixTimeStampToDateTime(returnDataArray.arrayDate[0]).ToString());
            log(UnixTimeStampToDateTime(returnDataArray.arrayDate[returnDataArray.arrayDate.Length - 1]).ToString());
            log("TRADE OPEN: " + openTotal);
            log("TRADE GAIN: " + gainTotal);
            log("TRADE LOSS: " + lossTotal);
            log("RESULT GAIN(GAIN-LOSS): " + (gainTotal - lossTotal) + "");
            log("RESULT GAIN %: " + (gainTotal * 100) / ((gainTotal + lossTotal)) + "%");
            log("PERC PROFIT-LOSS: " + perc + "%");

            //Console.SetOut(oldOut);
            //writer.Close();
            //ostrm.Close();

            log("Press enter to exit...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            log("ERROR " + ex.Message + ex.StackTrace);
            Console.ReadLine();
            Environment.Exit(0);
        }
    }


    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        if (source == "CW")
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        else
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    public static Double DatetimeToUnix(DateTime date)
    {
        if (source == "CW")
            return (date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        else
            return (date.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }
}

