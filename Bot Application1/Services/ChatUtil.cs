using System;
using System.Collections.Generic;
using System.Device.Location;

namespace Bot_Application1
{
	public static class ChatUtil
	{
		public static int LevenshteinDistance(string s, string t)
		{
			if (string.IsNullOrEmpty(s))
			{
				if (string.IsNullOrEmpty(t))
					return 0;
				return t.Length;
			}

			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 1; j <= m; d[0, j] = j++) ;

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}

		public static Dictionary<string, string> GetNearestLocation(double lat, double lon, Dictionary<string, string[]> source) {
            Dictionary<string, string> site = new Dictionary<string, string>();
            var sourceCoord = new GeoCoordinate(lat, lon);

            Dictionary<string, string[]>.Enumerator enumerator = source.GetEnumerator();

            double distant = 9999999999.0;
            string choosenKey = null;
            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                var eCoord = new GeoCoordinate(Convert.ToDouble(current.Value[1]), Convert.ToDouble(current.Value[2]));
                if (sourceCoord.GetDistanceTo(eCoord) < distant) {
                    distant = sourceCoord.GetDistanceTo(eCoord);
                    choosenKey = current.Key;
                }
            }

            site.Add("name", source[choosenKey].GetValue(0).ToString());
            site.Add("lat", source[choosenKey].GetValue(1).ToString());
			site.Add("lon", source[choosenKey].GetValue(2).ToString());
            site.Add("email", source[choosenKey].GetValue(3).ToString());
            site.Add("phone", source[choosenKey].GetValue(4).ToString());
            site.Add("address", source[choosenKey].GetValue(5).ToString());
            site.Add("dist", distant.ToString());

			return site;
        }
	}
}
