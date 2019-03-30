#region Copyright 
// Copyright 2017 HS Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HS.Microcore.Interfaces.Logging;

namespace HS.Microcore.SharedLogic.Logging
{
    public class LogCallSiteInfo
    {
        public Type ReflectedType;
        public string ClassName;
        public string AssemblyName;
        public string AssemblyVersion;
        public string MethodName;
        public string FileName;
        public int LineNumber;
        public string BuildTime;
    }

    public abstract class LogBase : ILog
    {

        protected LogCallSiteInfo CallSiteInfoTemplate { get; set; }


        public void Write(TraceEventType level, Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            if (MinimumTraceLevel < level)
                return;

            var logCallSiteInfo = new LogCallSiteInfo
            {
                ReflectedType = CallSiteInfoTemplate?.ReflectedType,
                ClassName = CallSiteInfoTemplate?.ClassName,
                AssemblyName = CallSiteInfoTemplate?.AssemblyName,
                AssemblyVersion = CallSiteInfoTemplate?.AssemblyVersion,
                MethodName = method,
                FileName = file,
                LineNumber = line,
                BuildTime = CallSiteInfoTemplate?.BuildTime
            };

            try
            {
                log((message, encryptedTags, unencryptedTags, exception, includeStack) =>
                    {
                        var stackTrace = includeStack ? Environment.StackTrace : null;

                        //Some time people make mistake between encryptedTags and exception fields.
                        if (encryptedTags is Exception && exception == null)
                        {
                            exception = (Exception)encryptedTags;
                            encryptedTags = null;
                        }

                        var unencTags = TagsExtractor.GetTagsFromObject(unencryptedTags)
                                                     .Concat(exception.GetUnencryptedTags())
                                                     .Where(_ => _.Value != null)
                                                     .FormatTagsWithTypeSuffix()
                                                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        var encTags = TagsExtractor.GetTagsFromObject(encryptedTags)
                                                   .Concat(exception.GetEncryptedTagsAndExtendedProperties())
                                                   .Where(_ => _.Value != null)
                                                   .FormatTagsWithoutTypeSuffix()
                                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        WriteLog(level, logCallSiteInfo, message, encTags, unencTags, exception, stackTrace);
                    });
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Programmatic error while logging: {ex}");
            }
        }


        protected abstract Task<bool> WriteLog(TraceEventType level, LogCallSiteInfo logCallSiteInfo, string message, IDictionary<string, string> encryptedTags, IDictionary<string, string> unencryptedTags, Exception exception = null, string stackTrace = null);

        public abstract TraceEventType? MinimumTraceLevel { get; set; }


        public void Debug(Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            Write(TraceEventType.Verbose, log, file, line, method);
        }

        public void Info(Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            Write(TraceEventType.Information, log, file, line, method);
        }

        public void Warn(Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            Write(TraceEventType.Warning, log, file, line, method);
        }


        public void Warn(string message, object encryptedTags = null, object unencryptedTags = null, Exception exception = null, bool includeStack = false, string file = "", int line = 0, string method = null)
        {
            Write(TraceEventType.Warning, _ => _(message, encryptedTags, unencryptedTags, exception, includeStack), file, line, method);
        }


        public void Error(Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            Write(TraceEventType.Error, log, file, line, method);
        }


        public void Error(string message, object encryptedTags = null, object unencryptedTags = null, Exception exception = null, bool includeStack = false, string file = "", int line = 0, string method = null)
        {
            Write(TraceEventType.Error, _ => _(message, encryptedTags, unencryptedTags, exception, includeStack), file, line, method);
        }


        public void Critical(Action<LogDelegate> log, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string method = null)
        {
            Write(TraceEventType.Critical, log, file, line, method);
        }


        public void Critical(string message, object encryptedTags = null, object unencryptedTags = null, Exception exception = null, bool includeStack = false, string file = "", int line = 0, string method = null)
        {
            Write(TraceEventType.Critical, _ => _(message, encryptedTags, unencryptedTags, exception, includeStack), file, line, method);
        }

    }
}