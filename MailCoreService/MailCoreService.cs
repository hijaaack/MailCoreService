//-----------------------------------------------------------------------
// <copyright file="MailCoreService.cs" company="Beckhoff Automation GmbH & Co. KG">
//     Copyright (c) Beckhoff Automation GmbH & Co. KG. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------

using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using System;
using System.Net;
using System.Net.Mail;
using TcHmiSrv.Core;
using TcHmiSrv.Core.General;
using TcHmiSrv.Core.Listeners;
using TcHmiSrv.Core.Tools.Management;
using TcHmiSrv.Core.Tools.Settings;

namespace MailCoreService
{
    // Represents the default type of the TwinCAT HMI server extension.
    public class MailCoreService : IServerExtension
    {
        private readonly RequestListener _requestListener = new RequestListener();
        private readonly ConfigListener _configListener = new ConfigListener();
        private readonly ShutdownListener _shutdownListener = new ShutdownListener();
        private string _smtpServer;
        private int _port;
        private bool _enableSsl;
        private string _username;
        private string _password;
        private SmtpSender _sender;

        // Called after the TwinCAT HMI server loaded the server extension.
        public ErrorValue Init()
        {
            // Wait for a debugger to be attached to the current process and signal a
            // breakpoint to the attached debugger in Init
            //TcHmiApplication.AsyncDebugHost.WaitForDebugger(true);

            //Event registers
            _requestListener.OnRequest += OnRequest;
            _configListener.OnChange += OnChange;
            _shutdownListener.OnShutdown += OnShutdown;

            //set up the config listener
            var settings = new ConfigListenerSettings();
            var filter = new ConfigListenerSettingsFilter(
                ConfigChangeType.OnChange, new string[] { "SMTPServer", "Port", "EnableSSL", "Username", "Password" }
            );
            settings.Filters.Add(filter);
            TcHmiApplication.AsyncHost.RegisterListener(TcHmiApplication.Context, _configListener, settings);

            //get config values
            _smtpServer = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "SMTPServer");
            _port = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Port");
            _enableSsl = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "EnableSSL");
            _username = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Username");
            _password = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Password");

            //Initialize smtpclient
            _sender = new SmtpSender(() => new SmtpClient(_smtpServer)
            {
                EnableSsl = _enableSsl,
                Port = _port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_username, _password)
            });

            //Log
            TcHmiAsyncLogger.Send(Severity.Info, "MESSAGE_INIT", "");

            return ErrorValue.HMI_SUCCESS;
        }

        // Called when the extension gets disabled or the TwinCAT HMI server shutdown/reboots 
        private void OnShutdown(object sender, TcHmiSrv.Core.Listeners.ShutdownListenerEventArgs.OnShutdownEventArgs e)
        {
            //Log
            TcHmiAsyncLogger.Send(Severity.Info, "MESSAGE_SHUTDOWN", "");

            //Unregister listeners
            TcHmiApplication.AsyncHost.UnregisterListener(TcHmiApplication.Context, _requestListener);
            TcHmiApplication.AsyncHost.UnregisterListener(TcHmiApplication.Context, _shutdownListener);
            TcHmiApplication.AsyncHost.UnregisterListener(TcHmiApplication.Context, _configListener);
        }

        // Called when the user changes data in the config-page of the extension. Also called on extension init. 
        private void OnChange(object sender, TcHmiSrv.Core.Listeners.ConfigListenerEventArgs.OnChangeEventArgs e)
        {
            //Retrieve ConfigPage Values
            var smtpServerValue = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "SMTPServer");
            var portValue = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Port");
            var enableSslValue = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "EnableSSL");
            var username = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Username");
            var password = TcHmiApplication.AsyncHost.GetConfigValue(TcHmiApplication.Context, "Password");

            //Update value if new
            if (smtpServerValue != _smtpServer || portValue != _port || enableSslValue != _enableSsl || username != _username || password != _password)
            {
                _smtpServer = smtpServerValue;
                _port = portValue;
                _enableSsl = enableSslValue;
                _username = username;
                _password = password;

                //Update SmtpSender
                _sender = new SmtpSender(() => new SmtpClient(_smtpServer)
                {
                    EnableSsl = _enableSsl,
                    Port = _port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(_username, _password)
                });

                //Log
                TcHmiAsyncLogger.Send(e.Context, Severity.Info, "NEW_CONFIG", "");
            }
        }

        // Called when a client requests a symbol from the domain of the TwinCAT HMI server extension.
        private void OnRequest(object sender, TcHmiSrv.Core.Listeners.RequestListenerEventArgs.OnRequestEventArgs e)
        {
            try
            {
                e.Commands.Result = MailCoreServiceErrorValue.Success;

                foreach (Command command in e.Commands)
                {
                    try
                    {
                        // Use the mapping to check which command is requested
                        switch (command.Mapping)
                        {
                            case "SendMail":
                                SendMail(command, e.Context);
                                break;

                            default:
                                command.ExtensionResult = MailCoreServiceErrorValue.Fail;
                                command.ResultString = "Unknown command '" + command.Mapping + "' not handled.";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        command.ExtensionResult = MailCoreServiceErrorValue.Fail;
                        command.ResultString = "Calling command '" + command.Mapping + "' failed! Additional information: " + ex.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                TcHmiAsyncLogger.Send(e.Context, Severity.Error, "ERROR_CALL_COMMAND", new string[] { ex.Message });
            }
        }

        private ErrorValue SendMail(Command command, Context context)
        {
            if (command.WriteValue != null && command.WriteValue.Type == TcHmiSrv.Core.ValueType.Struct)
            {
                try
                {
                    var writeValue = command.WriteValue;
                    var from = writeValue["from"];
                    var to = writeValue["to"];
                    var subject = writeValue["subject"];
                    var body = writeValue["body"];
                    var htmlBody = writeValue["htmlBody"];

                    Email.DefaultSender = _sender;
                    Email.DefaultRenderer = new RazorRenderer();

                    var email = Email
                        .From(from)
                        .To(to)
                        .Subject(subject)
                        .Body(body, htmlBody)
                        .Send();

                    if (email.Successful)
                    {
                        return ErrorValue.HMI_SUCCESS;
                    }
                    else
                    {
                        command.ExtensionResult = MailCoreServiceErrorValue.SendMailFail;
                        //List<string> list = new List<string>();
                        foreach (string error in email.ErrorMessages)
                        {
                            //list.Add(error);
                            TcHmiAsyncLogger.Send(context, Severity.Error, "ERROR_MAIL", error);
                        }
                        //string[] stringArray = list.ConvertAll(x => x.ToString()).ToArray();
                        //TcHmiAsyncLogger.Send(context, Severity.Error, "ERROR_MAIL", stringArray);
                    }
                }
                catch (Exception ex)
                {
                    command.ExtensionResult = MailCoreServiceErrorValue.SendMailFail;
                    TcHmiAsyncLogger.Send(context, Severity.Error, "ERROR_REQUEST", new string[] { ex.Message });
                }
            }
            else
            {
                command.ExtensionResult = MailCoreServiceErrorValue.DataWrongTypeOrEmpty;
            }

            return ErrorValue.HMI_SUCCESS;
        }

    }
}
