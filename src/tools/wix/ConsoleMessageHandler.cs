//-------------------------------------------------------------------------------------------------
// <copyright file="ConsoleMessageHandler.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
//
// <summary>
// Message handler for console.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Message handler for console applications.
    /// </summary>
    public class ConsoleMessageHandler : MessageHandler, IMessageHandler
    {
        private const int SuccessErrorNumber = 0;

        private int lastErrorNumber;
        private string shortAppName;
        private string longAppName;

        /// <summary>
        /// Create a new console message handler.
        /// </summary>
        /// <param name="shortAppName">Short application name; usually 4 uppercase characters.</param>
        /// <param name="longAppName">Long application name; usually the executable name.</param>
        public ConsoleMessageHandler(string shortAppName, string longAppName)
        {
            this.shortAppName = shortAppName;
            this.longAppName = longAppName;
        }

        /// <summary>
        /// Gets the last error code encountered by the message handler.
        /// </summary>
        /// <value>The exit code for the process.</value>
        public int LastErrorNumber
        {
            get { return this.lastErrorNumber; }
        }

        /// <summary>
        /// Display a message to the console.
        /// </summary>
        /// <param name="sender">Sender of the message.</param>
        /// <param name="mea">Arguments for the message event.</param>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public virtual void Display(object sender, MessageEventArgs mea)
        {
            string message = this.GetMessageString(sender, mea);

            if (null != message)
            {
#if DEBUG
                Debugger.Log((int)mea.Level, this.shortAppName, string.Concat(message, "\n"));
#endif
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Implements IMessageHandler to display error messages.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            this.Display(this, mea);
        }

        /// <summary>
        /// Creates a properly formatted message string.
        /// </summary>
        /// <param name="messageLevel">Level of the message, as generated by MessageLevel(MessageEventArgs).</param>
        /// <param name="mea">Event arguments for the message.</param>
        /// <returns>String containing the formatted message.</returns>
        protected override string GenerateMessageString(MessageLevel messageLevel, MessageEventArgs mea)
        {
            string message = base.GenerateMessageString(messageLevel, mea);

            if (null == message)
            {
                return null;
            }

            StringBuilder messageBuilder = new StringBuilder();
            ArrayList fileNames = new ArrayList();
            string errorFileName = this.longAppName;
            string messageType = String.Empty;

            if (MessageLevel.Warning == messageLevel)
            {
                messageType = WixStrings.MessageType_Warning;
            }
            else if (MessageLevel.Error == messageLevel)
            {
                this.lastErrorNumber = mea.Id;
                messageType = WixStrings.MessageType_Error;
            }

            if (null != mea.SourceLineNumbers && 0 < mea.SourceLineNumbers.Count)
            {
                bool first = true;
                foreach (SourceLineNumber sln in mea.SourceLineNumbers)
                {
                    if (sln.HasLineNumber)
                    {
                        if (first)
                        {
                            first = false;
                            errorFileName = String.Format(CultureInfo.CurrentUICulture, WixStrings.Format_FirstLineNumber, sln.FileName, sln.LineNumber);
                        }

                        fileNames.Add(String.Format(CultureInfo.CurrentUICulture, WixStrings.Format_LineNumber, sln.FileName, sln.LineNumber));
                    }
                    else
                    {
                        if (first)
                        {
                            first = false;
                            errorFileName = sln.FileName;
                        }

                        fileNames.Add(sln.FileName);
                    }
                }
            }

            if (MessageLevel.Information == messageLevel)
            {
                messageBuilder.AppendFormat(WixStrings.Format_InfoMessage, message);
            }
            else
            {
                messageBuilder.AppendFormat(WixStrings.Format_NonInfoMessage, errorFileName, messageType, this.shortAppName, mea.Id, message);
            }

            if (this.SourceTrace)
            {
                if (0 == fileNames.Count)
                {
                    messageBuilder.AppendFormat(WixStrings.INF_SourceTraceUnavailable, Environment.NewLine);
                }
                else
                {
                    messageBuilder.AppendFormat(WixStrings.INF_SourceTrace, Environment.NewLine);
                    foreach (string fileName in fileNames)
                    {
                        messageBuilder.AppendFormat(WixStrings.INF_SourceTraceLocation, fileName, Environment.NewLine);
                    }
                }

                messageBuilder.Append(Environment.NewLine);
            }

            return messageBuilder.ToString();
        }
    }
}
