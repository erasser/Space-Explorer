/*// UNCOMMENT THIS CODE TO CHECK MISSING ASSEMBLY FILES
using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Editor {
    public class AssemblyCompilationReporter {
    
        [InitializeOnLoadMethod]
        private static void Init() {
            CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
        }

        private static void CompilationPipelineOnAssemblyCompilationFinished(string s, CompilerMessage[] compilerMessages) {
            var startTimeInTicks = PlayerPrefs.GetString($"CompileStartTime{s}");
            var startTime = new DateTime(Convert.ToInt64(startTimeInTicks));

            var compileTime = DateTime.Now - startTime;
      
            Debug.Log($"=== CompilationPipeline Assembly Finished {s} ({compileTime.ToString("s\\.fff")}s)");

            foreach (var compilerMessage in compilerMessages) {
                switch (compilerMessage.type) {
                    case CompilerMessageType.Error:
                        Debug.LogError($"==== {compilerMessage.file}[{compilerMessage.line}:{compilerMessage.column}] {compilerMessage.message}");
                        break;
                    case CompilerMessageType.Warning:
                        Debug.LogWarning($"==== {compilerMessage.file}[{compilerMessage.line}:{compilerMessage.column}] {compilerMessage.message}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(string s) {      
            PlayerPrefs.SetString($"CompileStartTime{s}", Convert.ToString(DateTime.Now.Ticks));
        }
    }
}*/