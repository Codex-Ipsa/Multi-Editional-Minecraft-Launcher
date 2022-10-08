﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MCLauncher
{
    public class AssetIndex
    {
        public static bool isLegacy;

        public static List<string> nameList = new List<string>();
        public static List<string> hashList = new List<string>();

        public static void start(string indexUrl, string indexName)
        {
            if (indexName.Contains("legacy"))
            {
                isLegacy = true;
                Logger.logMessage("[AssetIndex]", "isLegacy returned true!");
            }
            else
            {
                isLegacy = false;
            }

            WebClient client = new WebClient();
            Directory.CreateDirectory($"{Globals.currentPath}\\.codexipsa\\assets\\indexes\\");
            if (!File.Exists($"{Globals.currentPath}\\.codexipsa\\assets\\indexes\\{indexName}.json"))
            {
                client.DownloadFile(indexUrl, $"{Globals.currentPath}\\.codexipsa\\assets\\indexes\\{indexName}.json");
            }
            string origJson = client.DownloadString(indexUrl);

            JObject origObj = JsonConvert.DeserializeObject<JObject>(origJson);
            var origProps = origObj.Properties();

            foreach (var oProp in origProps)
            {
                string oKey = oProp.Name;
                string oVal = oProp.Value.ToString();

                if (oKey == "objects")
                {
                    string indexJson = oVal;
                    JObject assetObj = JsonConvert.DeserializeObject<JObject>(indexJson);
                    var assetProps = assetObj.Properties();
                    foreach (var aProp in assetProps)
                    {
                        string aKey = aProp.Name;
                        object aVal = aProp.Value;
                        JObject itemObj = JsonConvert.DeserializeObject<JObject>(aVal.ToString());
                        var itemProps = itemObj.Properties();
                        foreach (var iProp in itemProps)
                        {
                            if(iProp.Name == "hash")
                            {
                                object iVal = iProp.Value;
                                Logger.logMessage("[AssetIndex]", $"Name: {aKey}; hash: {iVal}");
                                nameList.Add(aKey);
                                hashList.Add(iVal.ToString());
                            }
                        }
                    }
                }
                else
                {
                    Logger.logError("[AssetIndex]", $"Unknown key: {oKey}, with value: {oVal}");
                }

                Logger.logError("[AssetIndex]", $"isLegacy: {isLegacy}");
                if (isLegacy == true)
                {
                    Directory.CreateDirectory($"{Globals.currentPath}\\.codexipsa\\assets\\virtual\\{indexName}\\");
                    int indexInt = 0;
                    WebClient wc = new WebClient();
                    foreach (var name in nameList)
                    {
                        string fullHash = hashList[indexInt];
                        string firstTwo = fullHash.Substring(0, 2);

                        string fileDirectory = "/" + name;
                        int index = fileDirectory.LastIndexOf("/");
                        if (index >= 0)
                            fileDirectory = fileDirectory.Substring(0, index);

                        string fileName = name;
                        int index2 = fileName.IndexOf("/");
                        if (index2 >= 0)
                            fileName = fileName.Substring(fileName.LastIndexOf("/"));


                        if (!File.Exists($"{Globals.currentPath}\\.codexipsa\\assets\\virtual\\{indexName}\\{fileDirectory}\\{fileName}"))
                        {
                            Directory.CreateDirectory($"{Globals.currentPath}\\.codexipsa\\assets\\virtual\\{indexName}\\{fileDirectory}");
                            wc.DownloadFile($"http://resources.download.minecraft.net/{firstTwo}/{fullHash}", $"{Globals.currentPath}\\.codexipsa\\assets\\virtual\\{indexName}\\{fileDirectory}\\{fileName}");
                            Logger.logMessage("[AssetIndex]", $"Downloaded {fileName} to {fileDirectory}");
                        }

                        indexInt++;
                    }
                    isLegacy = false;
                    indexInt = 0;
                    hashList.Clear();
                    nameList.Clear();
                }
                else if (isLegacy == false)
                {
                    Directory.CreateDirectory($"{Globals.currentPath}\\.codexipsa\\assets\\objects\\");

                    int indexInt = 0;
                    WebClient wc = new WebClient();
                    foreach (var name in nameList)
                    {
                        string fullHash = hashList[indexInt];
                        string firstTwo = fullHash.Substring(0, 2);

                        if (!File.Exists($"{Globals.currentPath}\\.codexipsa\\assets\\objects\\{firstTwo}\\{fullHash}"))
                        {
                            Directory.CreateDirectory($"{Globals.currentPath}\\.codexipsa\\assets\\objects\\{firstTwo}");
                            wc.DownloadFile($"http://resources.download.minecraft.net/{firstTwo}/{fullHash}", $"{Globals.currentPath}\\.codexipsa\\assets\\objects\\{firstTwo}\\{fullHash}");
                            Logger.logMessage("[AssetIndex]", $"Downloaded {name} to {firstTwo}/{fullHash}");
                        }

                        indexInt++;
                    }
                    isLegacy = false;
                    indexInt = 0;
                    hashList.Clear();
                    nameList.Clear();
                }
            }
        }
    }
}
