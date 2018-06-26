using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.Models
{
    public static class CounterpointSpecieExtensions
    {
        public static string ToUserFriendlyString(this CounterpointSpecie specie)
        {
            switch (specie)
            {
                case CounterpointSpecie.First:
                    return "First spiecie";
                case CounterpointSpecie.Second:
                    return "Second spiecie";
                case CounterpointSpecie.Third:
                    return "Third spiecie";
                case CounterpointSpecie.Fourth:
                    return "Fourth spiecie";
                case CounterpointSpecie.Fifth:
                    return "Fifth spiecie";
                default:
                    return "";
            }
        }
        public static CounterpointSpecie FromUserFriendlyString(this string specie)
        {
            switch (specie)
            {
                case "First spiecie":
                    return CounterpointSpecie.First;

                case "Second spiecie":
                    return CounterpointSpecie.Second;
                case "Third spiecie":
                    return CounterpointSpecie.Third;
                case "Fourth spiecie":
                    return CounterpointSpecie.Fourth;
                case "Fifth spiecie":
                    return CounterpointSpecie.Fifth;
                default:
                    return CounterpointSpecie.First;
            }
        }
    }
}
