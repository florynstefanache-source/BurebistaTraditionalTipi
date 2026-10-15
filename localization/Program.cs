using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Runtime.Loader;
string Arg(string key,string fallback){int index=Array.IndexOf(args,key);return index>=0&&index+1<args.Length?args[index+1]:fallback;}
var game=Arg("--game-dir","C:/Program Files (x86)/Steam/steamapps/common/TheLongDark");
var resolver=new DefaultAssemblyResolver();foreach(var d in new[]{"MelonLoader/Il2CppAssemblies","MelonLoader/net6","Mods"})resolver.AddSearchDirectory(game+"/"+d);
var options=new ReaderParameters{AssemblyResolver=resolver};
var originalDirectory=Arg("--base-dir",args.Contains("--publication")?"work/tipi-language/publication-original":"work/tipi-language/original");
if(args.Contains("--compare-repository")){
 foreach(var name in new[]{"BurebistaTraditionalTipi","BurebistaTipiCuringRack","BurebistaTipiWorkbench"}){
  using var installed=AssemblyDefinition.ReadAssembly("work/tipi-language/original/"+name+".dll",options);using var published=AssemblyDefinition.ReadAssembly("work/publication/BurebistaTraditionalTipi/release/Mods/"+name+".dll",options);
  Console.WriteLine(name+" repository version "+published.CustomAttributes.First(a=>a.AttributeType.Name=="MelonInfoAttribute").ConstructorArguments[2].Value);
  var publishedMethods=All(published.MainModule.Types).SelectMany(t=>t.Methods).Where(m=>m.HasBody).ToDictionary(m=>m.FullName);
  foreach(var method in All(installed.MainModule.Types).SelectMany(t=>t.Methods).Where(m=>m.HasBody)){if(!publishedMethods.TryGetValue(method.FullName,out var other)||!method.Body.Instructions.Select(i=>i.ToString()).SequenceEqual(other.Body.Instructions.Select(i=>i.ToString())))Console.WriteLine("Different method: "+method.FullName);}
 }return;
}
using var native=AssemblyDefinition.ReadAssembly(game+"/MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll",options);
var getter=native.MainModule.Types.Single(t=>t.Name=="Localization").Methods.Single(m=>m.Name=="get_Language");
var catalog=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Arg("--catalog","work/tipi-language/translations.json")))!;
IEnumerable<TypeDefinition> All(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in All(t.NestedTypes))yield return n;}}
var output=Arg("--output-dir","outputs/BurebistaTipi-3Addons-ES-EN/Mods");Directory.CreateDirectory(output);
var reportDir=Arg("--report-dir","work/tipi-language");Directory.CreateDirectory(reportDir);
int checks=0;var report=new List<string>();
foreach(var name in new[]{"BurebistaTraditionalTipi","BurebistaTipiCuringRack","BurebistaTipiWorkbench","BurebistaTipiHideBed"}){
 using var assembly=AssemblyDefinition.ReadAssembly(originalDirectory+"/"+name+".dll",options);var module=assembly.MainModule;
 var helper=new TypeDefinition("Burebista","TipiDisplayLanguage",TypeAttributes.NotPublic|TypeAttributes.Abstract|TypeAttributes.Sealed,module.TypeSystem.Object);module.Types.Add(helper);
 var methods=new Dictionary<string,MethodDefinition>();
 MethodDefinition Translate(string es){
  if(methods.TryGetValue(es,out var existing))return existing;
  var method=new MethodDefinition("Text"+methods.Count,MethodAttributes.Assembly|MethodAttributes.Static,module.TypeSystem.String);helper.Methods.Add(method);methods.Add(es,method);
  method.Body.InitLocals=true;method.Body.Variables.Add(new VariableDefinition(module.TypeSystem.String));
  var il=method.Body.GetILProcessor();var start=Instruction.Create(OpCodes.Call,module.ImportReference(getter));var present=Instruction.Create(OpCodes.Ldstr,"English");var spanish=Instruction.Create(OpCodes.Ldstr,es);var caught=Instruction.Create(OpCodes.Pop);var done=Instruction.Create(OpCodes.Ldloc_0);
  il.Append(start);il.Emit(OpCodes.Dup);il.Emit(OpCodes.Brtrue,present);il.Emit(OpCodes.Pop);il.Emit(OpCodes.Br,spanish);il.Append(present);
  var starts=new MethodReference("StartsWith",module.TypeSystem.Boolean,module.TypeSystem.String){HasThis=true};starts.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));il.Emit(OpCodes.Callvirt,starts);il.Emit(OpCodes.Brfalse,spanish);il.Emit(OpCodes.Ldstr,catalog[es]);il.Emit(OpCodes.Stloc_0);il.Emit(OpCodes.Leave,done);
  il.Append(spanish);il.Emit(OpCodes.Stloc_0);il.Emit(OpCodes.Leave,done);il.Append(caught);il.Emit(OpCodes.Ldstr,es);il.Emit(OpCodes.Stloc_0);il.Emit(OpCodes.Leave,done);il.Append(done);il.Emit(OpCodes.Ret);
  method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch){CatchType=new TypeReference("System","Exception",module,module.TypeSystem.Object.Scope),TryStart=start,TryEnd=caught,HandlerStart=caught,HandlerEnd=done});return method;
 }
 int uses=0,animals=0;
 foreach(var type in All(module.Types).Where(t=>t!=helper).ToArray())foreach(var method in type.Methods.Where(m=>m.HasBody))foreach(var ins in method.Body.Instructions.ToArray()){
  // Keep saved keys, gear IDs, hierarchy names and static arrays unchanged.
  if(method.Name==".cctor")continue;
  if(ins.OpCode==OpCodes.Ldstr&&ins.Operand is string es&&catalog.ContainsKey(es)){ins.OpCode=OpCodes.Call;ins.Operand=Translate(es);uses++;}
 }
 if(name=="BurebistaTipiHideBed"){
  // Translate animal names at display time rather than during static startup.
  var animal=new MethodDefinition("Animal",MethodAttributes.Assembly|MethodAttributes.Static,module.TypeSystem.String);animal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));helper.Methods.Add(animal);var il=animal.Body.GetILProcessor();
  var equals=new MethodReference("op_Equality",module.TypeSystem.Boolean,module.TypeSystem.String);equals.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));equals.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
  foreach(var es in new[]{"ciervo","lobo","alce","oso"}){var next=Instruction.Create(OpCodes.Nop);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldstr,es);il.Emit(OpCodes.Call,equals);il.Emit(OpCodes.Brfalse,next);il.Emit(OpCodes.Call,Translate(es));il.Emit(OpCodes.Ret);il.Append(next);}il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ret);
  foreach(var m in All(module.Types).Where(t=>t!=helper).SelectMany(t=>t.Methods).Where(m=>m.HasBody&&(m.Name=="OnGUI"||m.Name=="Improve"))){bool names=false;foreach(var i in m.Body.Instructions.ToArray()){
   if(i.OpCode==OpCodes.Ldsfld&&i.Operand is FieldReference field&&field.Name=="Names")names=true;
   if(names&&i.OpCode==OpCodes.Ldelem_Ref){m.Body.GetILProcessor().InsertAfter(i,Instruction.Create(OpCodes.Call,animal));names=false;animals++;}
  }
  foreach(var i in m.Body.Instructions)if(i.OpCode.OperandType==OperandType.ShortInlineBrTarget)i.OpCode=LongBranch(i.OpCode);
  }if(animals!=2)throw new Exception("Expected two animal display sites, found "+animals);
 }
 if(uses==0)throw new Exception("No translations for "+name);
 var path=output+"/"+name+".dll";assembly.Write(path);
 using var check=AssemblyDefinition.ReadAssembly(path,options);using var original=AssemblyDefinition.ReadAssembly(originalDirectory+"/"+name+".dll",options);
 var generated=check.MainModule.Types.Single(t=>t.Name==helper.Name);
 foreach(var entry in methods){var method=generated.Methods.Single(m=>m.Name==entry.Value.Name);foreach(var language in new string?[]{"English","English (Canada)","Spanish (Spain)","Spanish",null}){if(Evaluate(method,language)!=((language?.StartsWith("English")??false)?catalog[entry.Key]:entry.Key))throw new Exception("Language branch failure");checks++;}if(method.Body.ExceptionHandlers.Count!=1)throw new Exception("Missing startup fallback");checks++;}
 foreach(var before in All(original.MainModule.Types).SelectMany(t=>t.Methods).Where(m=>m.HasBody)){
  var after=All(check.MainModule.Types).SelectMany(t=>t.Methods).Single(m=>m.FullName==before.FullName);
  var filtered=after.Body.Instructions.Where(i=>!(i.OpCode==OpCodes.Call&&i.Operand is MethodReference r&&r.DeclaringType.FullName==helper.FullName&&r.Name=="Animal")).ToList();
  if(before.Body.Instructions.Count!=filtered.Count||before.Body.ExceptionHandlers.Count!=after.Body.ExceptionHandlers.Count||before.Body.Variables.Count!=after.Body.Variables.Count)throw new Exception("Changed structure: "+before.FullName);
  for(int n=0;n<filtered.Count;n++){
   var a=before.Body.Instructions[n];var b=filtered[n];
   if(a.OpCode==OpCodes.Ldstr&&b.OpCode==OpCodes.Call&&b.Operand is MethodReference r&&r.DeclaringType.FullName==helper.FullName){if(!catalog.ContainsKey((string)a.Operand))throw new Exception("Unexpected string replacement");continue;}
   if(Normal(a.OpCode)!=Normal(b.OpCode)||Operand(a.Operand,before.Body.Instructions.ToList())!=Operand(b.Operand,filtered))throw new Exception("Unexpected behavior change in "+before.FullName+" instruction "+n);
  }checks++;
 }
 if(original.MainModule.Resources.Count!=check.MainModule.Resources.Count)throw new Exception("Resource count changed");
 foreach(var resource in original.MainModule.Resources.OfType<EmbeddedResource>()){var rebuilt=check.MainModule.Resources.OfType<EmbeddedResource>().Single(r=>r.Name==resource.Name);if(!resource.GetResourceData().SequenceEqual(rebuilt.GetResourceData()))throw new Exception("Changed asset");checks++;}
 if(original.MainModule.AssemblyReferences.Select(r=>r.FullName).Except(check.MainModule.AssemblyReferences.Select(r=>r.FullName)).Any())throw new Exception("Missing dependency");
 // JIT-test the actual generated methods using a managed localization stub.
 // This validates CLR method bodies and exception handling without calling Unity.
 var stub=new TypeDefinition("Burebista","LanguageTestStub",TypeAttributes.Public|TypeAttributes.Abstract|TypeAttributes.Sealed,module.TypeSystem.Object);module.Types.Add(stub);
 var langField=new FieldDefinition("Language",FieldAttributes.Public|FieldAttributes.Static,module.TypeSystem.String);stub.Fields.Add(langField);
 var throwField=new FieldDefinition("Throw",FieldAttributes.Public|FieldAttributes.Static,module.TypeSystem.Boolean);stub.Fields.Add(throwField);
 var get=new MethodDefinition("Get",MethodAttributes.Public|MethodAttributes.Static,module.TypeSystem.String);stub.Methods.Add(get);var gil=get.Body.GetILProcessor();var ok=Instruction.Create(OpCodes.Ldsfld,langField);gil.Emit(OpCodes.Ldsfld,throwField);gil.Emit(OpCodes.Brfalse,ok);
 var exType=new TypeReference("System","InvalidOperationException",module,module.TypeSystem.Object.Scope);var ctor=new MethodReference(".ctor",module.TypeSystem.Void,exType){HasThis=true};gil.Emit(OpCodes.Newobj,ctor);gil.Emit(OpCodes.Throw);gil.Append(ok);gil.Emit(OpCodes.Ret);
 foreach(var method in helper.Methods)foreach(var i in method.Body.Instructions)if(i.Operand is MethodReference mr&&mr.Name=="get_Language")i.Operand=get;
 Directory.CreateDirectory(reportDir+"/jit-tests");var testPath=Path.GetFullPath(reportDir+"/jit-tests/"+name+".dll");assembly.Write(testPath);
 var context=new AssemblyLoadContext("tipi-test-"+name,true);try{
  var managed=context.LoadFromAssemblyPath(testPath);var texts=managed.GetType(helper.FullName,true)!;var state=managed.GetType(stub.FullName,true)!;
  foreach(var language in new string?[]{"English","English (Canada)","Spanish (Spain)",null}){state.GetField("Language")!.SetValue(null,language);foreach(var entry in methods){var result=(string)texts.GetMethod(entry.Value.Name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,null)!;if(result!=((language?.StartsWith("English")??false)?catalog[entry.Key]:entry.Key))throw new Exception("JIT language failure");checks++;}}
  state.GetField("Throw")!.SetValue(null,true);foreach(var entry in methods){var result=(string)texts.GetMethod(entry.Value.Name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,null)!;if(result!=entry.Key)throw new Exception("JIT fallback failure");checks++;}state.GetField("Throw")!.SetValue(null,false);
  if(animals>0)foreach(var language in new[]{"English","Spanish (Spain)"}){state.GetField("Language")!.SetValue(null,language);foreach(var animalName in new[]{"ciervo","lobo","alce","oso"}){var result=(string)texts.GetMethod("Animal",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!.Invoke(null,new object[]{animalName})!;if(result!=(language=="English"?catalog[animalName]:animalName))throw new Exception("Animal JIT failure");checks++;}}
 }finally{context.Unload();}
 report.Add(name+": "+uses+" text occurrences; "+methods.Count+" bilingual phrases; original behavior, assets and managed JIT checked.");Console.WriteLine(report[^1]);
}
File.WriteAllLines(reportDir+"/VERIFICATION.txt",report.Append(checks+" checks passed. Game not executed."));Console.WriteLine(checks+" checks passed. Game not executed.");
static OpCode LongBranch(OpCode op)=>op.Code switch {Code.Br_S=>OpCodes.Br,Code.Brfalse_S=>OpCodes.Brfalse,Code.Brtrue_S=>OpCodes.Brtrue,Code.Beq_S=>OpCodes.Beq,Code.Bge_S=>OpCodes.Bge,Code.Bgt_S=>OpCodes.Bgt,Code.Ble_S=>OpCodes.Ble,Code.Blt_S=>OpCodes.Blt,Code.Bne_Un_S=>OpCodes.Bne_Un,Code.Bge_Un_S=>OpCodes.Bge_Un,Code.Bgt_Un_S=>OpCodes.Bgt_Un,Code.Ble_Un_S=>OpCodes.Ble_Un,Code.Blt_Un_S=>OpCodes.Blt_Un,Code.Leave_S=>OpCodes.Leave,_=>throw new Exception("Unknown short branch")};
static string Normal(OpCode op)=>(op.OperandType==OperandType.ShortInlineBrTarget?LongBranch(op):op).Name;
static string Operand(object value,List<Instruction> code)=>value switch {null=>"",Instruction i=>"target:"+code.IndexOf(i),Instruction[] a=>string.Join(",",a.Select(i=>code.IndexOf(i))),_=>value.ToString()!};
static string Evaluate(MethodDefinition method,string? language){var stack=new Stack<object?>();object? result=null;var code=method.Body.Instructions;int pc=0;while(pc<code.Count){var i=code[pc++];switch(i.OpCode.Code){case Code.Call:stack.Push(language);break;case Code.Dup:stack.Push(stack.Peek());break;case Code.Pop:stack.Pop();break;case Code.Brtrue:if(stack.Pop()!=null)pc=code.IndexOf((Instruction)i.Operand);break;case Code.Br:case Code.Leave:pc=code.IndexOf((Instruction)i.Operand);break;case Code.Ldstr:stack.Push(i.Operand);break;case Code.Callvirt:var prefix=(string)stack.Pop()!;var text=(string)stack.Pop()!;stack.Push(text.StartsWith(prefix));break;case Code.Brfalse:if(!(bool)stack.Pop()!)pc=code.IndexOf((Instruction)i.Operand);break;case Code.Stloc_0:result=stack.Pop();break;case Code.Ldloc_0:stack.Push(result);break;case Code.Ret:return (string)stack.Pop()!;default:throw new Exception("Unexpected generated IL");}}throw new Exception("No return");}
