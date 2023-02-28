//-----------------------------------------------------------------------
// <copyright file="MailCoreServiceErrorValue.cs" company="Beckhoff Automation GmbH & Co. KG">
//     Copyright (c) Beckhoff Automation GmbH & Co. KG. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace MailCoreService
{
    // Contains extension specific error values.
    public static class MailCoreServiceErrorValue
    {
        public static readonly uint Success = 0;
        public static readonly uint Fail = 1;

        public static readonly uint SendMailFail = 10;
        public static readonly uint DataWrongTypeOrEmpty = 11;
    }
}
