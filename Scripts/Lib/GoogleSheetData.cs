using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEditor;

namespace DVAH
{
    public static class GoogleSheetData
    {
        public static RequestBase getData( DVAH_Data DVAH_Data )
        { 

            RequestBase requestBase = new RequestBase(DVAH_Data.LinkGoogleSheet);
            _= requestBase.Send( result =>
            {
                if (!DVAH_Data)
                    return;
                JSONArray data = JSON.Parse(result.response)["values"].AsArray;

                List<JSONNode> rows = new List<JSONNode>();
                foreach (JSONNode item in data )
                {
                   
                    rows.Add(item.AsArray );
                    if (item.Count <= 1)
                        continue;
                    if (item.AsArray[0].ToString().ToLower().Contains("jus"))
                    {
                        DVAH_Data.Adjust_token = item.AsArray[1];
                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("FB") || item.AsArray[0].ToString().ToLower().Contains("ace"))
                    {
                        DVAH_Data.Facebook_AppID = item.AsArray[1];
                        if (item.AsArray > 2)
                            DVAH_Data.Facebook_AppID = item.AsArray[2];
                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("ann") )
                    {
                        DVAH_Data.AppLovin_BannerID = item.AsArray[1];
                         
                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("nte"))
                    {
                        DVAH_Data.AppLovin_InterID = item.AsArray[1];

                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("war"))
                    {
                        DVAH_Data.AppLovin_RewardID = item.AsArray[1];

                    }

                    if (item.AsArray[0].ToString().ToLower().Contains("pen"))
                    {
                        
                        string[] IDs = item.AsArray[1].ToString().Replace("\"","").Split("\\n");

                         
                        DVAH_Data.AppLovin_ADOpenIDs.Clear();

                        for (int i = 0; i< IDs.Length; i++)
                        {
                            Debug.LogError(i);
                            DVAH_Data.AppLovin_ADOpenIDs.Add(IDs[i]);
                        } 
                    }
                }
            });

            return requestBase;
        }
    }

}