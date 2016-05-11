﻿using System;
using System.Linq;
using System.Text;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class TeamCityListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Message("testSuiteStarted name='{0}'", message.Name);
        }

        public void Handle(CaseSkipped message)
        {
            Message("testIgnored name='{0}' message='{1}'", message.Name, message.SkipReason);
        }

        public void Handle(CasePassed message)
        {
            Message("testStarted name='{0}'", message.Name);
            Output(message);
            Message("testFinished name='{0}' duration='{1}'", message.Name, DurationInMilliseconds(message.Duration));
        }

        public void Handle(CaseFailed message)
        {
            Message("testStarted name='{0}'", message.Name);
            Output(message);
            Message("testFailed name='{0}' message='{1}' details='{2}'", message.Name, message.Exceptions.Message, message.Exceptions.CompoundStackTrace);
            Message("testFinished name='{0}' duration='{1}'", message.Name, DurationInMilliseconds(message.Duration));
        }

        public void Handle(AssemblyCompleted message)
        {
            Message("testSuiteFinished name='{0}'", message.Name);
        }

        static void Message(string format, params string[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity[" + format + "]", encodedArgs);
        }

        static void Output(CaseCompleted message)
        {
            if (!String.IsNullOrEmpty(message.Output))
                Message("testStdOut name='{0}' out='{1}'", message.Name, message.Output);
        }

        static string Encode(string value)
        {
            if (value == null)
                return "";

            var builder = new StringBuilder();

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '|': builder.Append("||"); break;
                    case '\'': builder.Append("|'"); break;
                    case '[': builder.Append("|["); break;
                    case ']': builder.Append("|]"); break;
                    case '\n': builder.Append("|n"); break; // Line Feed
                    case '\r': builder.Append("|r"); break; // Carriage Return
                    case '\u0085': builder.Append("|x"); break; // Next Line
                    case '\u2028': builder.Append("|l"); break; // Line Separator
                    case '\u2029': builder.Append("|p"); break; // Paragraph Separator
                    default: builder.Append(ch); break;
                }
            }

            return builder.ToString();
        }

        static string DurationInMilliseconds(TimeSpan duration)
        {
            return ((int)Math.Ceiling(duration.TotalMilliseconds)).ToString();
        }
    }
}