using BurebistaTipiHideBed;
int checks=0;
// Exhaust every inventory combination from every possible installed hide.
for(int current=-1;current<=3;current++)for(int mask=0;mask<16;mask++){
 int result=HideRules.Upgrade(current,i=>(mask&(1<<i))!=0);
 var possibilities=Enumerable.Range(0,4).Where(i=>i>current&&(mask&(1<<i))!=0).ToArray();
 int expected=possibilities.Length==0?-1:possibilities.Max();
 if(result!=expected)throw new Exception($"Upgrade {current}/{mask}: {result}");checks++;
 if(result>=0&&HideRules.Warmth[result]<= (current<0?0:HideRules.Warmth[current]))throw new Exception("Upgrade loses warmth");
}
int installed=-1,spent=0,returned=0;
foreach(int acquired in new[]{0,0,1,0,2,1,3,2,3}){
 int next=HideRules.Upgrade(installed,i=>i==acquired);
 if(next>=0){spent++;if(installed>=0)returned++;installed=next;}
}
if(installed!=3||spent!=4||returned!=3)throw new Exception("Upgrade sequence consumed duplicate skins");
if(HideRules.Upgrade(installed,_=>true)!=-1)throw new Exception("Bear must be final");
Console.WriteLine($"PASS: {checks} inventory combinations; upgrade sequence returns old skins and stops at bear.");
for(int rank=0;rank<4;rank++){
 if(!HideRules.Matches(HideRules.Keys[rank]+"(Clone)",rank))throw new Exception("Cloned hide not recognized");
 if(!HideRules.Matches(HideRules.Keys[rank]+"Data",rank))throw new Exception("Hide data not recognized");
 if(HideRules.Matches(HideRules.Keys[rank].Replace("Dried",""),rank))throw new Exception("Fresh hide accepted");
}
Console.WriteLine("PASS: cured hide names, clone names and data names accepted; fresh hides rejected.");
