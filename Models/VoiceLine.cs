using Melanchall.DryWetMidi;
using Melanchall.DryWetMidi.Smf.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Counterpoint.Models
{
    /// <summary>
    /// Single voice line composed of collection of bars
    /// </summary>
    internal class VoiceLine
    {
        public List<Note> NotesUngrouped;
        public List<Bar> Bars { get; set; }
        public int VoiceId { get; set; }
        public List<int> HorizontalIntervals;
        /// <summary>
        /// VerticalIntervals[i] represents
        /// intervals between current and VoiceLines[i]
        /// </summary>
        public List<List<int>> VerticalIntervals;



        public VoiceLine(List<Bar> bars, int voiceId, List<Note> notes)
        {
            this.Bars = bars;
            VoiceId = voiceId;
            NotesUngrouped = notes;
        }

        /// <summary>
        /// Indexer for Bars
        /// </summary>

        public Bar this[int i]
        {
            get
            {
                return Bars[i];
            }
        }


        public void PopulateHorizontalIntervals()
        {
            this.HorizontalIntervals = new List<int>();

            for (int j = 0; j < Bars.Count; j++)
            {
                for (int i = 0; i < Bars[j].notes.Count; i++)
                {
                    if (i + 1 < Bars[j].notes.Count)
                    {
                        HorizontalIntervals.Add(CounterpointAnalysis.GetIntervalInSemitones(Bars[j].notes[i], Bars[j].notes[i + 1]));
                    }
                    else
                    {
                        if (j + 1 < Bars.Count)
                            HorizontalIntervals.Add(CounterpointAnalysis.GetIntervalInSemitones(Bars[j].notes[i], Bars[j + 1].notes[0]));
                    }
                }

            }

        }

    }
}
