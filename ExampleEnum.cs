using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _04_interactions_framework
{
    public enum ExampleEnum
    {
        First,
        Second,
        Third,
        Fourth,
        //[ChoiceDisplay("Twenty First")]
        TwentyFirst
    }
    public enum TimestampFormat
    {
        SecondsPassed,
        MinutesPassed,
        HoursPassed,
        DaysPassed,
        WeeksPassed,
        MonthsPassed,
        YearsPassed,
        DecadesPassed
    }

}
