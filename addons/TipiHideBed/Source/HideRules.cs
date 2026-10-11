namespace BurebistaTipiHideBed;
public static class HideRules {
 public static readonly string[] Keys={"GEAR_LeatherHideDried","GEAR_WolfPeltDried","GEAR_MooseHideDried","GEAR_BearHideDried"};
 public static readonly string[] Names={"ciervo","lobo","alce","oso"};
 public static readonly float[] Warmth={5,7,10,12};
 public static bool Matches(string name,int rank){if(string.IsNullOrWhiteSpace(name))return false;var clean=name.Replace("(Clone)","").Trim();return clean.Equals(Keys[rank],StringComparison.OrdinalIgnoreCase)||clean.Equals(Keys[rank]+"Data",StringComparison.OrdinalIgnoreCase);}
 public static int Upgrade(int current,Func<int,bool> available){for(int i=3;i>current;i--)if(available(i))return i;return -1;}
}

